#addin "Cake.FileHelpers"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var version = FileReadLines("./VERSION")[0];
var semVersion = version + "-beta";
var isLocal = BuildSystem.IsLocalBuild;
var isRunningOnUnix = IsRunningOnUnix();
var isRunningOnWindows = IsRunningOnWindows();
var isRunningOnAppVeyor = AppVeyor.IsRunningOnAppVeyor;
var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;
var buildNumber = AppVeyor.Environment.Build.Number;
var buildDir = Directory("./src/Wyam/bin") + Directory(configuration);
var buildResultDir = Directory("./build") + Directory(semVersion);
var nugetRoot = buildResultDir + Directory("nuget");
var binDir = buildResultDir + Directory("bin");

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
    
// TODO: Create RELEASE file

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

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
