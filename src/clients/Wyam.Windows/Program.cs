using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using IWshRuntimeLibrary;
using Squirrel;
using File = System.IO.File;

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
        }

        private static void SquirrelUninstall(string version)
        {
            File.WriteAllText("E:\\temp\\uninstall.txt", "TEST");
            RemoveShortcuts();
            RemovePath();
        }

        private static void SquirrelObsolete(string version)
        {
        }

        private static void SquirrelFirstRun()
        {
        }

        private static void CreateCmdFiles(string version)
        {
            string installDirectory = GetInstallDirectory();
            foreach (string exeFile in _exeFiles)
            {
                string cmdContent = $@"@echo off{Environment.NewLine}{installDirectory}\app-{version}\{exeFile} %*";
                string cmdPath = Path.Combine(installDirectory, Path.ChangeExtension(exeFile, ".cmd"));
                File.WriteAllText(cmdPath, cmdContent);
            }

            string promptContent = $@"@echo off{Environment.NewLine}set PATH={installDirectory};%PATH%{Environment.NewLine}@exit /B 0";
            string promptPath = Path.Combine(installDirectory, "wyam.prompt.cmd");
            File.WriteAllText(promptPath, promptContent);
        }

        private static void CreateShortcuts()
        {
            // From http://stackoverflow.com/questions/25024785/how-to-create-start-menu-shortcut
            string startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            string startMenuFolderPath = Path.Combine(startMenuPath, "Wyam.Windows");
            if (!Directory.Exists(startMenuFolderPath))
            {
                Directory.CreateDirectory(startMenuFolderPath);
            }
            string shortcutLocation = Path.Combine(startMenuFolderPath, "Wyam Command Prompt.lnk");
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);
            shortcut.TargetPath = "cmd.exe";
            shortcut.Arguments = $"/k {Path.Combine(GetInstallDirectory(), "wyam.prompt.cmd")}";
            shortcut.Save();
        }

        private static void RemoveShortcuts()
        {
            string startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            string startMenuFolderPath = Path.Combine(startMenuPath, "Wyam.Windows");
            if (Directory.Exists(startMenuFolderPath))
            {
                Directory.Delete(startMenuFolderPath, true);
            }
        }

        private static string GetInstallDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Wyam");
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
            Console.WriteLine("Current PATH:");
            Console.WriteLine(currentPath);
            string installDirectory = GetInstallDirectory();
            string newPath = $"{currentPath};{installDirectory}";
            try
            {
                Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.Machine);
                Console.WriteLine("New PATH:");
                Console.WriteLine(newPath);
            }
            catch (SecurityException)
            {
                Console.WriteLine("Failed to set new PATH due to security, you must run as administrator to change system environment variables.");
            }
        }

        private static void RemovePath()
        {
            string currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
            Console.WriteLine("Current PATH:");
            Console.WriteLine(currentPath);
            List<string> paths = currentPath.Split(';').ToList();
            string installDirectory = GetInstallDirectory();
            int pathIndex = paths.IndexOf(installDirectory);
            if (pathIndex < 0)
            {
                Console.WriteLine($"{installDirectory} was not found in PATH, no modifications made");
                return;
            }
            paths.RemoveAt(pathIndex);
            string newPath = string.Join(";", paths);
            try
            {
                Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.Machine);
                Console.WriteLine("New PATH:");
                Console.WriteLine(newPath);
            }
            catch (SecurityException)
            {
                Console.WriteLine("Failed to set new PATH due to security, you must run as administrator to change system environment variables.");
            }
        }
    }
}
