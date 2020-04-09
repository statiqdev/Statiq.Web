// The following environment variables need to be set for Publish target:
// STATIQ_NUGET_API_KEY
// STATIQ_GITHUB_TOKEN

// The following environment variables need to be set for Sign-Packages target:
// STATIQ_CERTPASS

#addin "Cake.FileHelpers"
#addin "Octokit"
#tool "nuget:?package=NUnit.ConsoleRunner&version=3.7.0"
#tool "nuget:?package=NuGet.CommandLine&version=4.9.2"
#tool "AzurePipelines.TestLogger&version=1.0.2"

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
var isRunningOnBuildServer = !string.IsNullOrEmpty(EnvironmentVariable("AGENT_NAME")); // See https://github.com/cake-build/cake/issues/1684#issuecomment-397682686
var isPullRequest = !string.IsNullOrWhiteSpace(EnvironmentVariable("SYSTEM_PULLREQUEST_PULLREQUESTID"));  // See https://github.com/cake-build/cake/issues/2149
var buildNumber = TFBuild.Environment.Build.Number.Replace('.', '-');
var branch = TFBuild.Environment.Repository.Branch;

var releaseNotes = ParseReleaseNotes("./RELEASE.md");

var version = releaseNotes.Version.ToString();
var semVersion = releaseNotes.RawVersionLine.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]
    + (isLocal ? string.Empty : string.Concat("-build-", buildNumber));

var msBuildSettings = new DotNetCoreMSBuildSettings()
    .WithProperty("Version", semVersion)
    .WithProperty("AssemblyVersion", version)
    .WithProperty("FileVersion", version);

var buildDir = Directory("./build");
var nugetRoot = buildDir + Directory("nuget");
var binDir = buildDir + Directory("bin");

var zipFile = "StatiqWeb-v" + semVersion + ".zip";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(context =>
{
    Information("Building version {0} (semver {1}) of Statiq.", version, semVersion);
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
    {
        CleanDirectories(new DirectoryPath[] { buildDir, binDir, nugetRoot });
    });

Task("Restore-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        DotNetCoreRestore("./Statiq.Web.sln", new DotNetCoreRestoreSettings
        {
            MSBuildSettings = msBuildSettings
        });
    });

Task("Build")
    .IsDependentOn("Restore-Packages")
    .Does(() =>
    {
        DotNetCoreBuild("./Statiq.Web.sln", new DotNetCoreBuildSettings
        {
            Configuration = configuration,
            NoRestore = true,
            MSBuildSettings = msBuildSettings
        });
    });

Task("Copy-Files")
    .IsDependentOn("Build")
    .Does(() =>
    {
        CopyFiles("./src/**/bin/" + configuration + "/*/*", binDir);
        CopyFiles(new FilePath[] { "LICENSE.md", "LICENSE-FAQ.md", "README.md", "RELEASE.md" }, buildDir);
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

Task("Create-Packages")
    .IsDependentOn("Build")
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
    
Task("Publish-Prerelease-Packages")
    .IsDependentOn("Create-Packages")
    .WithCriteria(() => !isLocal)
    .WithCriteria(() => !isPullRequest)
    .WithCriteria(() => isRunningOnWindows)
    .WithCriteria(() => branch == "master")
    .Does(() =>
    {
        /*
        // Publish to the Wyam MyGet feed for now...
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

        return;

        // Eventually publish to GitHub Package Registry (or a public Azure Artifacts feed?)
        var githubToken = EnvironmentVariable("STATIQ_GITHUB_TOKEN");
        if (string.IsNullOrEmpty(githubToken))
        {
            throw new InvalidOperationException("Could not resolve GitHub token.");
        }

        // Add the authenticated feed source (remove any existing ones first to reset the access token)
        if(NuGetHasSource("https://nuget.pkg.github.com/statiqdev/index.json"))
        {
            NuGetRemoveSource(
                "GitHubStatiq",
                "https://nuget.pkg.github.com/statiqdev/index.json");
        }
        NuGetAddSource(
            "GitHubStatiq",
            "https://nuget.pkg.github.com/statiqdev/index.json",
            new NuGetSourcesSettings
            {
                UserName = "daveaglick",
                Password = githubToken
            });

        foreach (var nupkg in GetFiles(nugetRoot.Path.FullPath + "/*.nupkg"))
        {
            NuGetPush(nupkg, new NuGetPushSettings 
            {
                ApiKey = githubToken,
                Source = "https://nuget.pkg.github.com/statiqdev/index.json"
            });
        }
        */
    });    

Task("Sign-Packages")
    .IsDependentOn("Create-Packages")
    .WithCriteria(() => isLocal)
    .Does(() =>
    {
        var certPass = EnvironmentVariable("DAVIDGLICK_CERTPASS");
        if (string.IsNullOrEmpty(certPass))
        {
            throw new InvalidOperationException("Could not resolve certificate password.");
        }

        foreach (var nupkg in GetFiles(nugetRoot.Path.FullPath + "/*.nupkg"))
        {
            StartProcess("nuget", "sign \"" + nupkg.ToString() + "\" -CertificatePath \"davidglick.pfx\" -CertificatePassword \"" + certPass + "\" -Timestamper \"http://timestamp.digicert.com\" -NonInteractive");
        }        
    });

Task("Zip-Files")
    .IsDependentOn("Copy-Files")
    .IsDependentOn("Sign-Packages")
    .WithCriteria(() => isLocal)
    .WithCriteria(() => isRunningOnWindows)
    .Does(() =>
    {
        var zipPath = buildDir + File(zipFile);
        var files = GetFiles(buildDir.Path.FullPath + "/**/*");
        Zip(buildDir, zipPath, files);
    });
    
Task("Publish-Packages")
    .IsDependentOn("Sign-Packages")
    .WithCriteria(() => isLocal)
    .WithCriteria(() => isRunningOnWindows)
    .Does(() =>
    {
        var apiKey = EnvironmentVariable("STATIQ_NUGET_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("Could not resolve NuGet API key.");
        }

        foreach (var nupkg in GetFiles(nugetRoot.Path.FullPath + "/*.nupkg"))
        {
            try
            {
                NuGetPush(nupkg, new NuGetPushSettings 
                {
                    ApiKey = apiKey,
                    Source = "https://api.nuget.org/v3/index.json"
                });
            }
            catch(Exception ex)
            {
                Error(ex.Message);
            }
        }
    });

Task("Publish-Release")
    .IsDependentOn("Zip-Files")
    .WithCriteria(() => isLocal)
    .WithCriteria(() => isRunningOnWindows)
    .Does(() =>
    {
        var githubToken = EnvironmentVariable("STATIQ_GITHUB_TOKEN");
        if (string.IsNullOrEmpty(githubToken))
        {
            throw new InvalidOperationException("Could not resolve GitHub token.");
        }
        
        var github = new GitHubClient(new ProductHeaderValue("Cake"))
        {
            Credentials = new Credentials(githubToken)
        };
        var release = github.Repository.Release.Create("statiqdev", "Statiq.Web", new NewRelease("v" + semVersion) 
        {
            Name = semVersion,
            Body = string.Join(Environment.NewLine, releaseNotes.Notes),
            TargetCommitish = "master",
            Prerelease = semVersion.Contains('-')
        }).Result;
        
        var zipPath = buildDir + File(zipFile);
        using (var zipStream = System.IO.File.OpenRead(zipPath.Path.FullPath))
        {
            var releaseAsset = github.Repository.Release.UploadAsset(release, new ReleaseAssetUpload(zipFile, "application/zip", zipStream, null)).Result;
        }
    });
    
//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////
 
Task("Package")
    .IsDependentOn("Run-Unit-Tests")
    .IsDependentOn("Create-Packages");

Task("Default")
    .IsDependentOn("Package");    

Task("Publish")
    .IsDependentOn("Publish-Packages")
    .IsDependentOn("Publish-Release");
    
Task("BuildServer")
    .IsDependentOn("Run-Unit-Tests")
    .IsDependentOn("Publish-Prerelease-Packages");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
