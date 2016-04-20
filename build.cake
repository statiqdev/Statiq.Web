// The following environment variables need to be set for Publish target:
// NUGET_API_KEY
// WYAM_GITHUB_TOKEN

// The following environment variables need to be set for Publish-MyGet target:
// MYGET_API_KEY

// Publishing workflow:
// - Update ReleaseNotes.md and RELEASE in develop branch
// - Run a normal build with Cake to set SolutionInfo.cs in the repo and run through unit tests ("build.cmd")
// - Push to develop and fast-forward merge to master
// - Switch to master
// - Run a Publish build with Cake ("build.cmd --target Publish")
// - No need to add a version tag to the repo - added by GitHub on publish
// - Switch back to develop branch

#addin "Cake.FileHelpers"
#addin "Octokit"
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
var semVersion = version + (isLocal ? "-beta" : string.Concat("-build-", buildNumber));

var buildDir = Directory("./src/Wyam/bin") + Directory(configuration);
var buildResultDir = Directory("./build") + Directory(semVersion);
var nugetRoot = buildResultDir + Directory("nuget");
var binDir = buildResultDir + Directory("bin");

var zipFile = "Wyam-v" + semVersion + ".zip";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(() =>
{
    Information("Building version {0} of Wyam.", semVersion);
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
    {
        CleanDirectories(new DirectoryPath[] { buildDir, buildResultDir, binDir, nugetRoot });
    });

Task("Restore-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        // Note: specify additional package sources here if needed
        NuGetRestore("./src/Wyam.sln");
    });

Task("Patch-Assembly-Info")
    .IsDependentOn("Restore-Packages")
    .Does(() =>
    {
        var file = "./src/SolutionInfo.cs";
        CreateAssemblyInfo(file, new AssemblyInfoSettings {
            Product = "Wyam",
            Copyright = "Copyright \xa9 Wyam Contributors",
            Version = version,
            FileVersion = version,
            InformationalVersion = semVersion
        });
    });

Task("Build")
    .IsDependentOn("Patch-Assembly-Info")
    .Does(() =>
    {
        if(isRunningOnWindows)
        {
            MSBuild("./src/Wyam.sln", new MSBuildSettings()
                .SetConfiguration(configuration)
                .SetVerbosity(Verbosity.Minimal)
            );
        }
        else
        {
            XBuild("./src/Wyam.sln", new XBuildSettings()
                .SetConfiguration(configuration)
                .SetVerbosity(Verbosity.Minimal)
            );
        }
    });

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
    {
        var settings = new NUnit3Settings
        {
            Work = buildResultDir.Path.FullPath
        };
        if(isRunningOnAppVeyor)
        {
            settings.Where = "cat != ExcludeFromAppVeyor";
        }
        NUnit3("./src/**/bin/" + configuration + "/*.Tests.dll", settings);
    });

Task("Copy-Files")
    .IsDependentOn("Build")
    .Does(() =>
    {
        CopyDirectory(buildDir, binDir);
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
        var nugetExe = GetFiles("./tools/**/nuget.exe").FirstOrDefault()
            ?? (isRunningOnAppVeyor ? GetFiles("C:\\Tools\\NuGet3\\nuget.exe").FirstOrDefault() : null);
        if(nugetExe == null)
        {            
            throw new InvalidOperationException("Could not find nuget.exe.");
        }
        
        // Package all nuspecs (except the tools and all modules packages)
        foreach(var nuspec in GetFiles("./src/**/Wyam.*/*.nuspec")
            .Where(x => x.GetDirectory().GetDirectoryName() != "Wyam.Modules.All"))
        {
            NuGetPack(nuspec.ChangeExtension(".csproj"), new NuGetPackSettings
            {
                Version = semVersion,
                BasePath = nuspec.GetDirectory(),
                OutputDirectory = nugetRoot,
                Symbols = false
            });
        }
    });
    
Task("Create-AllModules-Package")
    .IsDependentOn("Build")
    .Does(() =>
    {        
        var nuspec = GetFiles("./src/Wyam.Modules.All/*.nuspec").FirstOrDefault();
        if(nuspec == null)
        {            
            throw new InvalidOperationException("Could not find all modules nuspec.");
        }
        
        // Add dependencies for all module libraries
        List<NuSpecDependency> dependencies = new List<NuSpecDependency>();
        foreach(var moduleNuspec in GetFiles("./src/**/Wyam.Modules.*/*.nuspec")
            .Where(x => x.GetDirectory().GetDirectoryName() != "Wyam.Modules.All"))
        {
            dependencies.Add(new NuSpecDependency
            {
                Id = moduleNuspec.GetDirectory().GetDirectoryName(),
                Version = semVersion
            });
        }
        
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
        var nuspec = GetFiles("./src/Wyam/*.nuspec").FirstOrDefault();
        if(nuspec == null)
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
    
Task("Publish-MyGet")
    .IsDependentOn("Create-Packages")
    .WithCriteria(() => !isLocal)
    .WithCriteria(() => !isPullRequest)
    .Does(() =>
    {
        // Resolve the API key.
        var apiKey = EnvironmentVariable("MYGET_API_KEY");
        if(string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("Could not resolve MyGet API key.");
        }

        foreach(var nupkg in GetFiles(nugetRoot.Path.FullPath + "/*.nupkg"))
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
        if(string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("Could not resolve NuGet API key.");
        }

        foreach(var nupkg in GetFiles(nugetRoot.Path.FullPath + "/*.nupkg"))
        {
            NuGetPush(nupkg, new NuGetPushSettings 
            {
                ApiKey = apiKey
            });
        }
    });
    
Task("Publish-Release")
    .IsDependentOn("Zip-Files")
    .WithCriteria(() => isLocal)
    // TODO: Add criteria that makes sure this is the master branch
    .Does(() =>
    {
        var githubToken = EnvironmentVariable("WYAM_GITHUB_TOKEN");
        if(string.IsNullOrEmpty(githubToken)) {
            throw new InvalidOperationException("Could not resolve Wyam GitHub token.");
        }
        
        var github = new GitHubClient(new ProductHeaderValue("WyamCakeBuild"))
        {
            Credentials = new Credentials(githubToken)
        };
        var release = github.Release.Create("Wyamio", "Wyam", new NewRelease("v" + semVersion) 
        {
            Name = semVersion,
            Body = string.Join(Environment.NewLine, releaseNotes.Notes) + Environment.NewLine + Environment.NewLine
                + @"### Note that you may need to right-click the zip file after download and select ""Unblock"" in the Security section of the properties dialog, otherwise you could get strange errors when using the application.",
            Prerelease = true,
            TargetCommitish = "master"
        }).Result; 
        var zipPath = buildResultDir + File(zipFile);
        using(var zipStream = System.IO.File.OpenRead(zipPath.Path.FullPath))
        {
            var releaseAsset = github.Release.UploadAsset(release, new ReleaseAssetUpload(zipFile, "application/zip", zipStream, null)).Result;
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
    .IsDependentOn("Create-AllModules-Package")    
    .IsDependentOn("Create-Tools-Package");
    
Task("Package")
    .IsDependentOn("Run-Unit-Tests")
    .IsDependentOn("Zip-Files")
    .IsDependentOn("Create-Packages");

Task("Default")
    .IsDependentOn("Package");    

Task("Publish")
    .IsDependentOn("Publish-Packages")
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
