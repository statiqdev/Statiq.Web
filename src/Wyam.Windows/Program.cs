using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Squirrel;

namespace Wyam.Windows
{
    // TODO: Create add-path and remove-path commands
    public class Program
    {
        private static void Main(string[] args)
        {
            Program program = new Program();
            program.Run(args);
        }

        private void Run(string[] args)
        {
            // Configure Squirrel events
            SquirrelAwareApp.HandleEvents(
                onInitialInstall: OnInitialInstall,
                onAppUpdate: OnAppUpdate,
                onAppUninstall: OnAppUninstall,
                onFirstRun: OnFirstRun);

            // Parse the command arguments
            Settings settings = new Settings();
            bool hasErrors;
            if (!settings.ParseArgs(args, out hasErrors))
            {
                return;
            }

            // Process args
            if (settings.Command == Settings.CommandEnum.Update)
            {
                Update();
            }
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

        private static void OnInitialInstall(Version version)
        {
            CreateCmdFiles();
            CreateShortcuts();
        }

        private static void OnAppUpdate(Version version)
        {
            CreateCmdFiles();
            CreateShortcuts();
        }

        private static void OnAppUninstall(Version version)
        {
            RemoveShortcuts();
        }

        private static void OnFirstRun()
        {
        }

        private static void CreateCmdFiles()
        {
            // TODO: Create .cmd files in the root (get assembly, go one level up)
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

        private static UpdateManager GetUpdateManager()
        {
            return UpdateManager.GitHubUpdateManager("https://github.com/Wyamio/Wyam", prerelease: true).Result;
        }
    }
}
