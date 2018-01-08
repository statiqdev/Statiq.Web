// The following environment variables need to be set for Publish target:
// NUGET_API_KEY
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
// - Run a build on Wyam.Web from CI to verify final release (first make sure NuGet Gallery has updated packages by searching for "wyam")

#addin "Cake.FileHelpers"
#addin "Octokit"
#addin "Cake.Squirrel"
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
var isRunningOnAppVeyor = AppVeyor.IsRunningOnAppVeyor;
var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;
var buildNumber = AppVeyor.Environment.Build.Number;

var releaseNotes = ParseReleaseNotes("./ReleaseNotes.md");

var version = releaseNotes.Version.ToString();
var semVersion = version + (isLocal ? string.Empty : string.Concat("-build-", buildNumber));

var buildDir = Directory("./src/clients/Wyam/bin") + Directory(configuration);
var buildResultDir = Directory("./build") + Directory(semVersion);
var nugetRoot = buildResultDir + Directory("nuget");
var chocoRoot = buildResultDir + Directory("choco");
var binDir = buildResultDir + Directory("bin");
var windowsDir = buildResultDir + Directory("windows");

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
        CleanDirectories(new DirectoryPath[] { buildDir, buildResultDir, binDir, nugetRoot, chocoRoot, windowsDir });
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
        NuGetRestore("./Wyam.sln");
        if (isRunningOnWindows)
        {
            NuGetRestore("./Wyam.Windows.sln");
        }

        // Need to pass version to restore so assets file is updated
        // See https://github.com/NuGet/Home/issues/4790
        // and https://github.com/NuGet/Home/issues/5371
        MSBuild("./Wyam.sln", new MSBuildSettings()
            .SetConfiguration(configuration)
            .WithTarget("restore")
            .WithProperty("Version", $"\"{semVersion}\"")
            .WithProperty("WarningLevel", "0")
            .SetMaxCpuCount(0)
            .SetVerbosity(Verbosity.Minimal)
        );
    });

Task("Build")
    .IsDependentOn("Restore-Packages")
    .Does(() =>
    {
        MSBuild("./Wyam.sln", new MSBuildSettings()
            .SetConfiguration(configuration)
            .SetMaxCpuCount(0)
            .SetVerbosity(Verbosity.Minimal)
            .WithProperty("WarningLevel", "0")  
        );
        MSBuild("./Wyam.Windows.sln", new MSBuildSettings()
            .SetConfiguration(configuration)
            .SetMaxCpuCount(0)
            .SetVerbosity(Verbosity.Minimal)
            .WithProperty("WarningLevel", "0")
        );
    });

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
    {
        DotNetCoreTestSettings testSettings = new DotNetCoreTestSettings()
        {
            NoBuild = true,
            ArgumentCustomization = x => x.Append("--no-restore"),
            Configuration = configuration
        };
        if (isRunningOnAppVeyor)
        {
            testSettings.Filter = "TestCategory!=ExcludeFromAppVeyor";
        }

        foreach (var project in GetFiles("./tests/**/*.csproj"))
        {
            Information($"Running tests in {project}");
            DotNetCoreTest(MakeAbsolute(project).ToString(), testSettings);
        }
    });

Task("Copy-Files")
    .IsDependentOn("Build")
    .Does(() =>
    {
        CopyDirectory(buildDir.Path.FullPath + "/net462", binDir);
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
    .Does(() =>
    {        
        // Get the set of projects to package
        List<FilePath> projects = new List<FilePath>(GetFiles("./src/**/*.csproj"));

        // Wyam client and Wyam.Windows is packaged separatly
        projects.RemoveAll(x => x.GetDirectory().GetDirectoryName() == "Wyam");
        projects.RemoveAll(x => x.GetDirectory().GetDirectoryName() == "Wyam.Windows");
        
        // Package all nuspecs
        foreach (var project in projects)
        {
            MSBuild(project, new MSBuildSettings()
                .SetConfiguration(configuration)
                .WithTarget("pack")
                .WithProperty("PackageOutputPath", $"\"{MakeAbsolute(nugetRoot)}\"")
                .WithProperty("PackageVersion", $"\"{semVersion}\"")
            );
        }
    });

Task("Create-Theme-Packages")
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
                Owners = new [] { "Dave Glick" },
                Description = "A theme for the Wyam " + segments[segments.Length - 2] + " recipe.",
                ProjectUrl = new Uri("https://wyam.io"),
                IconUrl = new Uri("https://wyam.io/assets/img/logo-square-64.png"),
                LicenseUrl = new Uri("https://github.com/Wyamio/Wyam/blob/master/LICENSE"),
                Copyright = "Copyright 2017",
                Tags = new [] { "Wyam", "Theme", "Static", "StaticContent", "StaticSite" },
                RequireLicenseAcceptance = false,
                Symbols = false,
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
    .IsDependentOn("Build")
    .Does(() =>
    {        
        var nuspec = GetFiles("./src/clients/Wyam/*.nuspec").FirstOrDefault();
        if (nuspec == null)
        {            
            throw new InvalidOperationException("Could not find tools nuspec.");
        }
        var pattern = string.Format("bin\\{0}\\**\\*", configuration);  // This is needed to get around a Mono scripting issue (see #246, #248, #249)
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
                    Target = "tools"
                } 
            }
        });
    });

Task("Create-Chocolatey-Package")
    .IsDependentOn("Build")
    .Does(() => {
        var nuspecFile = GetFiles("./nuspec/chocolatey/**/*.nuspec").FirstOrDefault();
        ChocolateyPack(nuspecFile, new ChocolateyPackSettings {
            Version = semVersion,
            OutputDirectory = string.Format("./build/{0}/choco", semVersion),
            WorkingDirectory = string.Format("./build/{0}", semVersion)
        });
    });

// Note that we're not creating a differential release files since we're using a new releases folder per-version
// That's by design - in order to distribute diffs from GitHub and have them get picked up by Squirrel, *all* prior
// versions have to be included in *every* GitHub release. That stinks, and we're not going to do it. Since Squirrel
// won't do incremental updates if we don't upload everything, it serves no purpose to create the diffs. 
Task("Create-Windows")
    .IsDependentOn("Copy-Files")
    .Does(() => {        
        if(isRunningOnWindows)
        {
            var nuspec = GetFiles("./src/clients/Wyam.Windows/*.nuspec").FirstOrDefault();
            if (nuspec == null)
            {            
                throw new InvalidOperationException("Could not find installer nuspec.");
            }       
            var packageDir = nuspec.GetDirectory() + ("/bin/" + configuration);
            CopyDirectory(binDir, packageDir);  // Copy everything from main Wyam bin to Wyam.Windows bin prior to packaging
            var pattern = string.Format("bin\\{0}\\**\\*", configuration);  // This is needed to get around a Mono scripting issue (see #246, #248, #249)
            NuGetPack(nuspec, new NuGetPackSettings
            {
                Version = semVersion,
                BasePath = nuspec.GetDirectory(),
                OutputDirectory = packageDir,
                Symbols = false,
                Files = new [] 
                { 
                    new NuSpecContent 
                    { 
                        Source = pattern,
                        Target = "lib/net45"
                    }
                }
            });
            var package = (packageDir + "/") + File("Wyam.Windows." + semVersion + ".nupkg");            
            Squirrel(package, new SquirrelSettings
            {
                Silent = true,
                NoMsi = true,
                ReleaseDirectory = windowsDir,
                SetupIcon = GetFiles("./src/clients/Wyam.Windows/wyam.ico").First().FullPath
            });
            DeleteFile(package);
        }
    });
    
Task("Publish-MyGet")
    .IsDependentOn("Create-Packages")
    .WithCriteria(() => !isLocal)
    .WithCriteria(() => !isPullRequest)
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
                ApiKey = apiKey
            });
        }
    });
    
Task("Publish-Packages")
    .IsDependentOn("Create-Packages")
    .WithCriteria(() => isLocal)
    // TODO: Add criteria that makes sure this is the master branch
    .Does(() =>
    {
        var apiKey = EnvironmentVariable("NUGET_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("Could not resolve NuGet API key.");
        }

        foreach (var nupkg in GetFiles(nugetRoot.Path.FullPath + "/*.nupkg"))
        {
            NuGetPush(nupkg, new NuGetPushSettings 
            {
                ApiKey = apiKey,
                Source = "https://nuget.org/api/v2/package"
            });
        }
    });

Task("Publish-Chocolatey-Package")
    .IsDependentOn("Create-Chocolatey-Package")
    .WithCriteria(()=> isLocal)
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
    .IsDependentOn("Create-Windows")
    .WithCriteria(() => isLocal)
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
        
        var windowsFiles = GetFiles(windowsDir.Path.FullPath + "/*");
        foreach (var windowsFile in windowsFiles)
        {
            using (var contentStream = System.IO.File.OpenRead(windowsFile.FullPath))
            {
                var fileName = windowsFile.GetFilename().ToString();
                var releaseAsset = github.Repository.Release.UploadAsset(release, new ReleaseAssetUpload(fileName, "application/binary", contentStream, null)).Result;
            }
        }
    });
    
Task("Update-AppVeyor-Build-Number")
    .WithCriteria(() => isRunningOnAppVeyor)
    .Does(() =>
    {
        AppVeyor.UpdateBuildVersion(semVersion);
    });

Task("Upload-AppVeyor-Artifacts")
    .IsDependentOn("Zip-Files")
    .WithCriteria(() => isRunningOnAppVeyor)
    .Does(() =>
    {
        var artifact = buildResultDir + File(zipFile);
        AppVeyor.UploadArtifact(artifact);
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
    .IsDependentOn("Create-Windows")
    .IsDependentOn("Create-Packages");

Task("Default")
    .IsDependentOn("Package");    

Task("Publish")
    .IsDependentOn("Publish-Packages")
    .IsDependentOn("Publish-Chocolatey-Package")
    .IsDependentOn("Publish-Release");
    
Task("AppVeyor")
    .IsDependentOn("Run-Unit-Tests")
    .IsDependentOn("Publish-MyGet")
    .IsDependentOn("Update-AppVeyor-Build-Number")
    .IsDependentOn("Upload-AppVeyor-Artifacts");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
