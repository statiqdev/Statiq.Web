using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Octokit;
using Statiq.App;
using Statiq.Common;
using Statiq.Core;

namespace Statiq.Web.Build
{
    public class Program
    {
        private static readonly NormalizedPath ArtifactsFolder = "artifacts";
        private static readonly string GitHubOwner = "statiqdev";
        private static readonly string GitHubName = "Statiq.Web";
        private static readonly string BuildServer = nameof(BuildServer);
        private static readonly string ProjectPattern = "src/**/{!templates,}/**/*.csproj";

        public static async Task<int> Main(string[] args) => await Bootstrapper
            .Factory
            .CreateDefault(args)
            .ConfigureEngine(x =>
            {
                x.FileSystem.RootPath = new NormalizedPath(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.Parent.Parent;
                x.FileSystem.OutputPath = x.FileSystem.RootPath / ArtifactsFolder;
                x.FileSystem.InputPaths.Clear();
                x.FileSystem.InputPaths.Add(x.FileSystem.RootPath);
            })
            .ConfigureSettings(settings => settings.Add(BuildServer, settings.ContainsAnyKeys("GITHUB_ACTIONS", "TF_BUILD")))
            .AddPipelines<Program>()
            .RunAsync();

        public class Build : Pipeline
        {
            public Build()
            {
                ExecutionPolicy = ExecutionPolicy.Manual;

                ProcessModules = new ModuleList
                {
                    new ReadFiles(ProjectPattern),
                    new StartProcess("dotnet")
                        .WithArgument("build")
                        .WithArgument(Config.FromContext(context =>
                            context.GetBool(BuildServer) || context.ExecutingPipelines.ContainsKey(nameof(Publish))
                                ? "-p:ContinuousIntegrationBuild=\"true\"" // Perform a deterministic build if on the CI server or publishing
                                : null))
                        .WithArgument(Config.FromDocument(doc => doc.Source.FullPath), true)
                        .WithParallelExecution(false)
                        .LogOutput()
                };
            }
        }

        public class Test : Pipeline
        {
            public Test()
            {
                ExecutionPolicy = ExecutionPolicy.Manual;

                ProcessModules = new ModuleList
                {
                    new ReadFiles("tests/**/*.csproj"),
                    new StartProcess("dotnet")
                        .WithArgument("test")
                        .WithArgument(Config.FromDocument(doc => doc.Source.FullPath), true)
                        .WithParallelExecution(false)
                        .LogOutput()
                };
            }
        }

        public class Pack : Pipeline
        {
            public Pack()
            {
                ExecutionPolicy = ExecutionPolicy.Manual;

                Dependencies.Add(nameof(Build));

                ProcessModules = new ModuleList
                {
                    new ThrowExceptionIf(Config.ContainsSettings("DAVIDGLICK_CERTPASS").IsFalse(), "DAVIDGLICK_CERTPASS setting missing"),
                    new ReadFiles(ProjectPattern),
                    new StartProcess("dotnet")
                        .WithArgument("pack")
                        .WithArgument("--no-build")
                        .WithArgument("--no-restore")
                        .WithArgument("-o", Config.FromContext(ctx => ctx.FileSystem.GetOutputPath().FullPath), true)
                        .WithArgument(Config.FromDocument(doc => doc.Source.FullPath), true)
                        .WithParallelExecution(false)
                        .LogOutput(),
                    new ReadFiles(Config.FromContext(ctx => ctx.FileSystem.GetOutputPath("*.nupkg").FullPath)),
                    new StartProcess("nuget")
                        .WithArgument("sign")
                        .WithArgument(Config.FromDocument(doc => doc.Source.FullPath), true)
                        .WithArgument("-CertificatePath", Config.FromContext(ctx => ctx.FileSystem.GetRootFile("davidglick.pfx").Path.FullPath), true)
                        .WithArgument("-CertificatePassword", Config.FromSetting("DAVIDGLICK_CERTPASS"), true)
                        .WithArgument("-Timestamper", "http://timestamp.digicert.com", true)
                        .WithArgument("-NonInteractive")
                        .WithParallelExecution(false)
                        .LogOutput()
                };
            }
        }

        public class Publish : Pipeline
        {
            public Publish()
            {
                ExecutionPolicy = ExecutionPolicy.Manual;

                Dependencies.Add(nameof(Pack));

                ProcessModules = new ModuleList
                {
                    new ThrowExceptionIf(Config.ContainsSettings("STATIQ_NUGET_API_KEY").IsFalse(), "STATIQ_NUGET_API_KEY setting missing"),
                    new ThrowExceptionIf(Config.ContainsSettings("STATIQ_GITHUB_TOKEN").IsFalse(), "STATIQ_GITHUB_TOKEN setting missing"),
                    new ReadFiles(Config.FromContext(ctx => ctx.FileSystem.GetOutputPath("*.nupkg").FullPath)),
                    new StartProcess("nuget")
                        .WithArgument("push")
                        .WithArgument(Config.FromDocument(doc => doc.Source.FullPath), true)
                        .WithArgument("-ApiKey", Config.FromSetting("STATIQ_NUGET_API_KEY"), true)
                        .WithArgument("-Source", "https://api.nuget.org/v3/index.json", true)
                        .WithParallelExecution(false)
                        .LogOutput(),
                    new ExecuteConfig(Config.FromContext(async ctx =>
                    {
                        // Get release notes
                        IFile releaseNotesFile = ctx.FileSystem.GetInputFile("RELEASE.md");
                        string releaseNotes = await releaseNotesFile.ReadAllTextAsync();
                        string[] lines = releaseNotes.Trim().Split("\n#", StringSplitOptions.RemoveEmptyEntries)[0].Trim().Split("\n").Select(x => x.Trim()).ToArray();
                        string version = lines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries)[1];
                        string notes = string.Join(Environment.NewLine, lines.Skip(1).SkipWhile(x => string.IsNullOrWhiteSpace(x)));

                        ctx.LogInformation("Version " + version);
                        ctx.LogInformation("Notes " + Environment.NewLine + notes);

                        // Add release to GitHub
                        GitHubClient github = new GitHubClient(new ProductHeaderValue("Statiq"))
                        {
                            Credentials = new Credentials(ctx.GetString("STATIQ_GITHUB_TOKEN"))
                        };
                        Release release = await github.Repository.Release.Create(GitHubOwner, GitHubName, new NewRelease("v" + version)
                        {
                            Name = version,
                            Body = string.Join(Environment.NewLine, notes),
                            TargetCommitish = "main",
                            Prerelease = version.Contains('-')
                        });
                        ctx.LogInformation($"Added release {release.Name} to GitHub");
                    }))
                };
            }
        }
    }
}
