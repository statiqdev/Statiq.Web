using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Squirrel;

namespace Wyam.Windows
{
    public class Program
    {
        // This is a list of .exe files that will have proxy .cmd files created
        private static readonly string[] _exeFiles = { "wyam.exe", "wyam.windows.exe" };

        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionEvent;
            Program program = new Program();
            program.Run(args);
        }

        private static void UnhandledExceptionEvent(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());
            Environment.Exit(1);
        }

        private void Run(string[] args)
        {
            // Output version info
            AssemblyInformationalVersionAttribute versionAttribute
                = Attribute.GetCustomAttribute(typeof(Program).Assembly, typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
            Console.WriteLine("Wyam version {0}", versionAttribute == null ? "unknown" : versionAttribute.InformationalVersion);

            // Parse the command arguments
            Settings settings = new Settings();
            bool hasErrors;
            if (!settings.ParseArgs(args, out hasErrors))
            {
                return;
            }

            // Process args
            switch (settings.Command)
            {
                case Settings.CommandEnum.SquirrelInstall:
                    SquirrelInstall(settings.SquirrelVersion);
                    return;
                case Settings.CommandEnum.SquirrelUninstall:
                    SquirrelUninstall(settings.SquirrelVersion);
                    return;
                case Settings.CommandEnum.SquirrelObsolete:
                    SquirrelObsolete(settings.SquirrelVersion);
                    return;
                case Settings.CommandEnum.SquirrelUpdated:
                    SquirrelUpdated(settings.SquirrelVersion);
                    return;
                case Settings.CommandEnum.SquirrelFirstRun:
                    SquirrelFirstRun();
                    return;
                case Settings.CommandEnum.Update:
                    Update();
                    break;
                case Settings.CommandEnum.AddPath:
                    AddPath();
                    break;
                case Settings.CommandEnum.RemovePath:
                    RemovePath();
                    break;
            }
        }

        private static void SquirrelInstall(string version)
        {
            CreateCmdFiles(version);
            CreateShortcuts();
        }

        private static void SquirrelUpdated(string version)
        {
            CreateCmdFiles(version);
            CreateShortcuts();
        }

        private static void SquirrelUninstall(string version)
        {
            RemoveShortcuts();
        }

        private static void SquirrelObsolete(string version)
        {
        }

        private static void SquirrelFirstRun()
        {
        }

        private static void CreateCmdFiles(string version)
        {
            string currentDirectory = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            foreach (string exeFile in _exeFiles)
            {
                string cmdContent = $@"@echo off{Environment.NewLine}{version}\{exeFile} %*";
                string cmdPath = Path.Combine(currentDirectory, "..", Path.ChangeExtension(exeFile, ".cmd"));
                File.WriteAllText(cmdPath, cmdContent);
            }
        }

        private static void CreateShortcuts()
        {
            // Uncomment when ready to create shortcuts
            //using (UpdateManager manager = GetUpdateManager())
            //{
            //    manager.CreateShortcutForThisExe();
            //}
        }

        private static void RemoveShortcuts()
        {
            // Uncomment when ready to create shortcuts
            //using (UpdateManager manager = GetUpdateManager())
            //{
            //    manager.RemoveShortcutForThisExe();
            //}
        }

        private static void Update()
        {
            using (UpdateManager manager = GetUpdateManager())
            {
                Console.Write("Checking for updates... ");
                UpdateInfo updates = manager.CheckForUpdate().Result;
                List<ReleaseEntry> releases = updates.ReleasesToApply;
                if (releases.Count > 0)
                {
                    Console.Write("Downloading updates... ");
                    manager.DownloadReleases(releases).Wait();

                    Console.Write("Applying updates... ");
                    string version = manager.ApplyReleases(updates).Result;

                    Console.WriteLine($"Successfully updated to version {version}");
                }
                else
                {
                    Console.WriteLine("No updates available");
                }
            }
        }

        private static UpdateManager GetUpdateManager()
        {
            return UpdateManager.GitHubUpdateManager("https://github.com/Wyamio/Wyam", prerelease: true).Result;
        }

        private static void AddPath()
        {
            string currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
            Console.WriteLine($"Current PATH: {currentPath}");
            string path = Path.GetDirectoryName(Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), ".."));
            string newPath = $"{currentPath};{path}";
            Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.Machine);
            Console.WriteLine($"New PATH: {newPath}");
        }

        private static void RemovePath()
        {
            string currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
            Console.WriteLine($"Current PATH: {currentPath}");
            List<string> paths = currentPath.Split(';').ToList();
            string path = Path.GetDirectoryName(Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), ".."));
            int pathIndex = paths.IndexOf(path);
            if (pathIndex < 0)
            {
                Console.WriteLine($"{path} was not found in PATH, no modifications made");
                return;
            }
            paths.RemoveAt(pathIndex);
            string newPath = string.Join(";", paths);
            Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.Machine);
            Console.WriteLine($"New PATH: {newPath}");
        }
    }
}
