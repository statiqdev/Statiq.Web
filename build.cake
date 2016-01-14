// The following environment variables need to be set for Publish target:
// NUGET_API_KEY
// WYAM_GITHUB_TOKEN

#addin "Cake.FileHelpers"

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

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        // Note: specify additional package sources here if needed
        NuGetRestore("./src/Wyam.sln");
    });

Task("Patch-Assembly-Info")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
    {
        var file = "./src/SolutionInfo.cs";
        CreateAssemblyInfo(file, new AssemblyInfoSettings {
            Product = "Wyam",
            Copyright = "Copyright (c) Wyam Contributors",
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

Task("Create-NuGet-Packages")
    .IsDependentOn("Build")
    .Does(() =>
    {
        foreach(var nuspec in GetFiles("./src/**/*.nuspec"))
        {
            StartProcess("./tools/nuget.exe", new ProcessSettings()
                .WithArguments(x => x
                    .Append("pack")
                    .AppendQuoted(nuspec.ChangeExtension(".csproj").FullPath)
                    .AppendSwitch("-Version", semVersion)
                    .AppendSwitch("-OutputDirectory", nugetRoot.Path.FullPath)
                )
            );
        
            /*
            NuGetPack(nuspec.ChangeExtension(".csproj"), new NuGetPackSettings
            {
                Version = semVersion,
                BasePath = nuspec.GetDirectory(),
                OutputDirectory = nugetRoot,
                Symbols = false
            });
            */
        }
    });
    
Task("Publish-NuGet")
    .IsDependentOn("Create-NuGet-Packages")
    .WithCriteria(() => isLocal)
    // TODO: Add criteria that makes sure this is the master branch
    .Does(() =>
    {
        var apiKey = EnvironmentVariable("NUGET_API_KEY");
        if(string.IsNullOrEmpty(apiKey)) {
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
        
        var github = new GitHubClient(new ProductHeaderValue("Wyam Cake Build"))
        {
            Credentials = new Credentials("46353799916dbc5e3ab31a6bd596ea59ba2cd99f")
        }
        var release = github.Release.Create("Wyamio", "Wyam", new NewRelease("v" + semVersion) 
        {
            Name = semVersion,
            Body = string.Join(Environment.NewLine, releaseNotes.Notes),
            Prerelease = true,
            TargetCommitish = "master"
        }).Result; 
        var zipPath = buildResultDir + File(zipFile);
        using(var zipStream = zipPath.OpenRead())
        {
            github.Release.UploadAsset(release, new ReleaseAssetUpload(zipFile, "application/zip", zipStream, null)).Result;
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

Task("Package")
    .IsDependentOn("Run-Unit-Tests")
    .IsDependentOn("Zip-Files")
    .IsDependentOn("Create-NuGet-Packages");

Task("Default")
    .IsDependentOn("Package");    

Task("Publish")
    .IsDependentOn("Publish-NuGet")
    .IsDependentOn("Publish-Release");
    
Task("AppVeyor")
    .IsDependentOn("Update-AppVeyor-Build-Number")
    .IsDependentOn("Upload-AppVeyor-Artifacts");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
