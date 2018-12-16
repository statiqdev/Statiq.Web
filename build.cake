// The following environment variables need to be set for Build target:
// SIGNTOOL (to sign the executable)

// The following environment variables need to be set for Publish target:
// WYAM_NUGET_API_KEY
// WYAM_GITHUB_TOKEN

// The following environment variables need to be set for Publish-MyGet target:
// MYGET_API_KEY

// The following environment variables need to be set for Publish-Chocolatey target:
// CHOCOLATEY_API_KEY

// Publishing workflow:
// - Update ReleaseNotes.md and RELEASE in develop branch
// - Run a normal build with Cake to set SolutionInfo.cs in the repo and run through unit tests (`build.cmd`)
// - Push to develop and fast-forward merge to master
// - Switch to master
// - Wait for CI to complete build and publish to MyGet
// - Run a local prerelease build of Wyam.Web to verify release (`build -Script "prerelease.cake"` from Wyam.Web folder)
// - Run a Publish build with Cake (`build -target Publish`)
// - No need to add a version tag to the repo - added by GitHub on publish
// - Switch back to develop branch
// - Add a blog post to Wyam.Web about the release
// - Run a build on Wyam.Web from CI to verify final release (first make sure NuGet Gallery has updated packages by searching for "wyam")

#addin "Cake.FileHelpers"
#addin "Octokit"
#tool "nuget:?package=NUnit.ConsoleRunner&version=3.7.0"
#tool "nuget:?package=NuGet.CommandLine&version=4.7.1"
#tool "AzurePipelines.TestLogger&version=0.4.7"

using Octokit;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var isLocal = BuildSystem.IsLocalBuild;
var isRunningOnUnix = IsRunningOnUnix();
var isRunningOnWindows = IsRunningOnWindows();
var isRunningOnBuildServer = TFBuild.IsRunningOnVSTS;
var isPullRequest = !string.IsNullOrWhiteSpace(EnvironmentVariable("SYSTEM_PULLREQUEST_PULLREQUESTID"));  // See https://github.com/cake-build/cake/issues/2149
var buildNumber = TFBuild.Environment.Build.Number.Replace('.', '-');
var branch = TFBuild.Environment.Repository.Branch;

var releaseNotes = ParseReleaseNotes("./ReleaseNotes.md");

var version = releaseNotes.Version.ToString();
var semVersion = version + (isLocal ? string.Empty : string.Concat("-build-", buildNumber));

var msBuildSettings = new DotNetCoreMSBuildSettings()
    .WithProperty("Version", semVersion)
    .WithProperty("AssemblyVersion", version)
    .WithProperty("FileVersion", version);

var buildDir = Directory("./src/clients/Wyam/bin") + Directory(configuration);
var buildResultDir = Directory("./build");
var nugetRoot = buildResultDir + Directory("nuget");
var chocoRoot = buildResultDir + Directory("choco");
var binDir = buildResultDir + Directory("bin");

var zipFile = "Wyam-v" + semVersion + ".zip";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(context =>
{
    Information("Building version {0} of Wyam.", semVersion);
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
    {
        CleanDirectories(new DirectoryPath[] { buildDir, buildResultDir, binDir, nugetRoot, chocoRoot });
    });

Task("Patch-Assembly-Info")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        var file = "./SolutionInfo.cs";
        CreateAssemblyInfo(file, new AssemblyInfoSettings {
            Product = "Wyam",
            Copyright = "Copyright \xa9 Wyam Contributors",
            Version = version,
            FileVersion = version,
            InformationalVersion = semVersion
        });
    });

Task("Restore-Packages")
    .IsDependentOn("Patch-Assembly-Info")
    .Does(() =>
    {
        DotNetCoreRestore("./Wyam.sln", new DotNetCoreRestoreSettings
        {
            MSBuildSettings = msBuildSettings
        });
    });

Task("Build")
    .IsDependentOn("Restore-Packages")
    .Does(() =>
    {
        DotNetCoreBuild("./Wyam.sln", new DotNetCoreBuildSettings
        {
            Configuration = configuration,
            NoRestore = true,
            MSBuildSettings = msBuildSettings
        });
    });

Task("Publish-Client")
    .IsDependentOn("Build")
    .Does(() =>
    {
        DotNetCorePublish("./src/clients/Wyam/Wyam.csproj", new DotNetCorePublishSettings
        {
            Configuration = configuration,
            NoBuild = true,
            NoRestore = true
        });
    });

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .DoesForEach(GetFiles("./tests/**/*.csproj"), project =>
    {
        DotNetCoreTestSettings testSettings = new DotNetCoreTestSettings()
        {
            NoBuild = true,
            NoRestore = true,
            Configuration = configuration
        };
        if (isRunningOnBuildServer)
        {
            testSettings.Filter = "TestCategory!=ExcludeFromBuildServer";
            testSettings.Logger = "AzurePipelines";
            testSettings.TestAdapterPath = GetDirectories($"./tools/AzurePipelines.TestLogger.*/contentFiles/any/any").First();
        }

        Information($"Running tests in {project}");
        DotNetCoreTest(MakeAbsolute(project).ToString(), testSettings);
    })
    .DeferOnError();

Task("Copy-Files")
    .IsDependentOn("Publish-Client")
    .Does(() =>
    {
        CopyDirectory(buildDir.Path.FullPath + "/netcoreapp2.1/publish", binDir);
        CopyFiles(new FilePath[] { "LICENSE", "README.md", "ReleaseNotes.md" }, binDir);
    });

Task("Zip-Files")
    .IsDependentOn("Copy-Files")
    .Does(() =>
    {
        var zipPath = buildResultDir + File(zipFile);
        var files = GetFiles(binDir.Path.FullPath + "/**/*");
        Zip(binDir, zipPath, files);
    });

Task("Create-Library-Packages")
    .IsDependentOn("Build")
    .IsDependentOn("Publish-Client")
    .Does(() =>
    {        
        // Get the set of projects to package
        List<FilePath> projects = new List<FilePath>(GetFiles("./src/**/*.csproj"));
        
        // Package all nuspecs
        foreach (var project in projects)
        {
            DotNetCorePack(
                MakeAbsolute(project).ToString(),
                new DotNetCorePackSettings
                {
                    Configuration = configuration,
                    NoBuild = true,
                    NoRestore = true,
                    OutputDirectory = nugetRoot,
                    MSBuildSettings = msBuildSettings
                });
        }
    });

Task("Create-Theme-Packages")
    .WithCriteria(() => isRunningOnWindows)
    .Does(() =>
    {        
        // All themes must be under the themes folder in a NameOfRecipe/NameOfTheme subfolder
        var themeDirectories = GetDirectories("./themes/*/*");
        
        // Package all themes
        foreach (var themeDirectory in themeDirectories)
        {
            string[] segments = themeDirectory.Segments;
            string id = "Wyam." + segments[segments.Length - 2] + "." + segments[segments.Length - 1];
            NuGetPack(new NuGetPackSettings
            {
                Id = id,
                Version = semVersion,
                Title = id,
                Authors = new [] { "Dave Glick" },
                Owners = new [] { "Dave Glick", "wyam" },
                Description = "A theme for the Wyam " + segments[segments.Length - 2] + " recipe.",
                ProjectUrl = new Uri("https://wyam.io"),
                IconUrl = new Uri("https://wyam.io/assets/img/logo-square-64.png"),
                LicenseUrl = new Uri("https://github.com/Wyamio/Wyam/blob/master/LICENSE"),
                Copyright = "Copyright 2017",
                Tags = new [] { "Wyam", "Theme", "Static", "StaticContent", "StaticSite" },
                RequireLicenseAcceptance = false,
                Symbols = false,
                Repository = new NuGetRepository {
                    Type = "git",
                    Url = "https://github.com/Wyamio/Wyam.git"
                },
                Files = new []
                {
                    new NuSpecContent 
                    { 
                        Source = "**/*",
                        Target = "content"
                    }                     
                },
                BasePath = themeDirectory,
                OutputDirectory = nugetRoot
            });
        }
    });
    
Task("Create-AllModules-Package")
    .IsDependentOn("Build")
    .WithCriteria(() => isRunningOnWindows)
    .Does(() =>
    {        
        var nuspec = GetFiles("./src/extensions/Wyam.All/*.nuspec").FirstOrDefault();
        if (nuspec == null)
        {            
            throw new InvalidOperationException("Could not find all modules nuspec.");
        }
        
        // Add dependencies for all module libraries
        List<FilePath> nuspecs = new List<FilePath>(GetFiles("./src/extensions/**/*.nuspec"));
        nuspecs.RemoveAll(x => x.GetDirectory().GetDirectoryName() == "Wyam.All");
        List<NuSpecDependency> dependencies = new List<NuSpecDependency>(
            nuspecs
                .Select(x => new NuSpecDependency
                    {
                        Id = x.GetDirectory().GetDirectoryName(),
                        Version = semVersion
                    })
        );
        
        // Pack the all modules package
        NuGetPack(nuspec, new NuGetPackSettings
        {
            Version = semVersion,
            BasePath = nuspec.GetDirectory(),
            OutputDirectory = nugetRoot,
            Symbols = false,
            Dependencies = dependencies
        });
    });
    
Task("Create-Tools-Package")
    .IsDependentOn("Publish-Client")
    .WithCriteria(() => isRunningOnWindows)
    .Does(() =>
    {        
        var nuspec = GetFiles("./src/clients/Wyam/*.nuspec").FirstOrDefault();
        if (nuspec == null)
        {            
            throw new InvalidOperationException("Could not find tools nuspec.");
        }
        var pattern = string.Format("bin\\{0}\\netcoreapp2.1\\publish\\**\\*", configuration);  // This is needed to get around a Mono scripting issue (see #246, #248, #249)
        NuGetPack(nuspec, new NuGetPackSettings
        {
            Version = semVersion,
            BasePath = nuspec.GetDirectory(),
            OutputDirectory = nugetRoot,
            Symbols = false,
            Files = new [] 
            { 
                new NuSpecContent 
                { 
                    Source = pattern,
                    Target = "tools\\netcoreapp2.1"
                } 
            }
        });
    });

Task("Create-Chocolatey-Package")
    .IsDependentOn("Copy-Files")
    .WithCriteria(() => isRunningOnWindows)
    .Does(() => {
        var nuspecFile = GetFiles("./src/clients/Chocolatey/*.nuspec").FirstOrDefault();
        ChocolateyPack(nuspecFile, new ChocolateyPackSettings {
            Version = semVersion,
            OutputDirectory = "./build/choco",
            WorkingDirectory = "./build"
        });
    });
    
Task("Publish-MyGet")
    .IsDependentOn("Create-Packages")
    .WithCriteria(() => !isLocal)
    .WithCriteria(() => !isPullRequest)
    .WithCriteria(() => isRunningOnWindows)
    .WithCriteria(() => branch == "develop")
    .Does(() =>
    {
        // Resolve the API key.
        var apiKey = EnvironmentVariable("MYGET_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("Could not resolve MyGet API key.");
        }

        foreach (var nupkg in GetFiles(nugetRoot.Path.FullPath + "/*.nupkg"))
        {
            NuGetPush(nupkg, new NuGetPushSettings 
            {
                Source = "https://www.myget.org/F/wyam/api/v2/package",
                ApiKey = apiKey,
                Timeout = TimeSpan.FromSeconds(600)
            });
        }
    });
    
Task("Publish-Packages")
    .IsDependentOn("Create-Packages")
    .WithCriteria(() => isLocal)
    .WithCriteria(() => isRunningOnWindows)
    // TODO: Add criteria that makes sure this is the master branch
    .Does(() =>
    {
        var apiKey = EnvironmentVariable("WYAM_NUGET_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("Could not resolve NuGet API key.");
        }

        foreach (var nupkg in GetFiles(nugetRoot.Path.FullPath + "/*.nupkg"))
        {
            NuGetPush(nupkg, new NuGetPushSettings 
            {
                ApiKey = apiKey,
                Source = "https://api.nuget.org/v3/index.json"
            });
        }
    });

Task("Publish-Chocolatey-Package")
    .IsDependentOn("Create-Chocolatey-Package")
    .WithCriteria(()=> isLocal)
    .WithCriteria(() => isRunningOnWindows)
    .Does(()=> 
    {
        var chocolateyApiKey = EnvironmentVariable("CHOCOLATEY_API_KEY");
        if (string.IsNullOrEmpty(chocolateyApiKey))
        {
            throw new InvalidOperationException("Could not resolve Chocolatey API key.");
        }

        foreach(var chocoPkg in GetFiles(chocoRoot.Path.FullPath + "/*.nupkg"))
        {
            ChocolateyPush(chocoPkg, new ChocolateyPushSettings {
                ApiKey = chocolateyApiKey,
                Source = "https://push.chocolatey.org/"
            });
        }
    });

Task("Publish-Release")
    .IsDependentOn("Zip-Files")
    .WithCriteria(() => isLocal)
    .WithCriteria(() => isRunningOnWindows)
    // TODO: Add criteria that makes sure this is the master branch
    .Does(() =>
    {
        var githubToken = EnvironmentVariable("WYAM_GITHUB_TOKEN");
        if (string.IsNullOrEmpty(githubToken))
        {
            throw new InvalidOperationException("Could not resolve Wyam GitHub token.");
        }
        
        var github = new GitHubClient(new ProductHeaderValue("WyamCakeBuild"))
        {
            Credentials = new Credentials(githubToken)
        };
        var release = github.Repository.Release.Create("Wyamio", "Wyam", new NewRelease("v" + semVersion) 
        {
            Name = semVersion,
            Body = string.Join(Environment.NewLine, releaseNotes.Notes) + Environment.NewLine + Environment.NewLine
                + @"### Please see https://wyam.io/docs/usage/obtaining for important notes about downloading and installing.",
            TargetCommitish = "master"
        }).Result; 
        
        var zipPath = buildResultDir + File(zipFile);
        using (var zipStream = System.IO.File.OpenRead(zipPath.Path.FullPath))
        {
            var releaseAsset = github.Repository.Release.UploadAsset(release, new ReleaseAssetUpload(zipFile, "application/zip", zipStream, null)).Result;
        }
    });
    
//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Create-Packages")
    .IsDependentOn("Create-Library-Packages")
    .IsDependentOn("Create-Theme-Packages")   
    .IsDependentOn("Create-AllModules-Package")    
    .IsDependentOn("Create-Tools-Package")
    .IsDependentOn("Create-Chocolatey-Package");
    
Task("Package")
    .IsDependentOn("Run-Unit-Tests")
    .IsDependentOn("Zip-Files")
    .IsDependentOn("Create-Packages");

Task("Default")
    .IsDependentOn("Package");    

Task("Publish")
    .IsDependentOn("Publish-Packages")
    .IsDependentOn("Publish-Chocolatey-Package")
    .IsDependentOn("Publish-Release");
    
Task("BuildServer")
    .IsDependentOn("Run-Unit-Tests")
    .IsDependentOn("Publish-MyGet");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
