using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CFixer.Helpers;
using Windows.Foundation;
using Windows.Management.Deployment;

namespace CrapFixer
{
    public class AppAnalysisResult
    {
        public string AppName { get; set; }
        public string FullName { get; set; }
    }

    public class AppManagerService
    {
        private Dictionary<string, string> _appDirectory = new Dictionary<string, string>();

        // Loads all apps and stores them in the directory (name -> fullName)
        private async Task LoadAppsAsync()
        {
            _appDirectory.Clear();

            var pm = new PackageManager();
            var packages = await Task.Run(() =>
                pm.FindPackagesForUserWithPackageTypes(string.Empty, PackageTypes.Main));

            foreach (var p in packages)
            {
                string name = p.Id.Name;
                string fullName = p.Id.FullName;

                if (!_appDirectory.ContainsKey(name))
                {
                    _appDirectory[name] = fullName;
                }
            }
            Logger.Log($"(Checked against {_appDirectory.Count} apps from the system)");
        }

        /// <summary>
        /// Analyzes the installed apps against provided bloatware patterns and whitelist,
        /// logs the results, and returns the matches.
        /// </summary>
        /// <param name="bloatwarePatterns">List of bloatware keywords to check against</param>
        /// <param name="whitelistPatterns">List of app names to ignore</param>
        /// <param name="scanAll">If true, scans all apps regardless of bloatware patterns</param>
        public async Task<List<AppAnalysisResult>> AnalyzeAndLogAppsAsync(
            string[] bloatwarePatterns,
            string[] whitelistPatterns,
            bool scanAll)
        {
            var apps = await AnalyzeAppsAsync(bloatwarePatterns, whitelistPatterns, scanAll);

            if (apps.Count > 0)
            {
                Logger.Log(Localization.T("Bloatware apps detected:"), LogLevel.Info);
                foreach (var app in apps)
                {
                    Logger.Log($"❌ [ Bloatware ] {app.AppName} ({app.FullName})", LogLevel.Warning);
                }
            }
            else
            {
                Logger.Log(Localization.T("✅ No Microsoft Store bloatware apps found."), LogLevel.Info);
            }

            Logger.Log(""); // Add a blank line for spacing

            return apps;
        }

        /// <summary>
        /// Analyzes the apps based on predefined apps (from resources) and returns matching apps.
        /// </summary>
        /// <param name="predefinedApps"></param>
        /// <returns></returns>
        public async Task<List<AppAnalysisResult>> AnalyzeAppsAsync(string[] bloatwarePatterns, string[] whitelistPatterns, bool scanAll = false)
        {
            Logger.Log("\n🧩 APPS ANALYSIS", LogLevel.Info);
            Logger.Log(new string('=', 50), LogLevel.Info);

            await LoadAppsAsync(); // Load all installed apps

            var result = new List<AppAnalysisResult>();

            foreach (var app in _appDirectory)
            {
                string appName = app.Key.ToLower();

                // Always skip whitelisted apps
                if (whitelistPatterns.Any(w => appName.Contains(w)))
                    continue;

                if (scanAll)
                {
                    // If wildcard is set, include everything not whitelisted
                    result.Add(new AppAnalysisResult
                    {
                        AppName = app.Key,
                        FullName = app.Value
                    });
                }
                else
                {
                    // Only match against provided patterns
                    foreach (var pattern in bloatwarePatterns)
                    {
                        if (appName.Contains(pattern))
                        {
                            result.Add(new AppAnalysisResult
                            {
                                AppName = app.Key,
                                FullName = app.Value
                            });
                            break;
                        }
                    }
                }
            }

            return result;
        }

        // Uninstall an app by its full name
        public async Task<bool> UninstallApp(string fullName)
        {
            try
            {
                var pm = new PackageManager();
                var operation = pm.RemovePackageAsync(fullName);

                var resetEvent = new ManualResetEvent(false);
                operation.Completed = (o, s) => resetEvent.Set();
                await Task.Run(() => resetEvent.WaitOne());

                if (operation.Status == AsyncStatus.Completed)
                {
                    Logger.Log($"Successfully uninstalled app: {fullName}");
                    return true;
                }
                else
                {
                    Logger.Log($"Failed to uninstall appe: {fullName}", LogLevel.Warning);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error during uninstalling app {fullName}: {ex.Message}", LogLevel.Warning);
                return false;
            }
        }

        /// <summary>
        /// Uninstalls the selected apps and logs the results.
        /// </summary>
        /// <param name="selectedApps"></param>
        /// <returns></returns>
        public async Task<List<string>> UninstallSelectedAppsAsync(List<string> selectedApps)
        {
            List<string> removedApps = new List<string>();

            foreach (var fullName in selectedApps)
            {
                Logger.Log($"🗑️ Removing app: {fullName}...");

                // Uninstall the app using its full name
                var success = await UninstallApp(fullName);
                if (success)
                {
                    removedApps.Add(fullName);
                }
            }

            // Log results for apps that were successfully removed
            foreach (var app in removedApps)
            {
                Logger.Log($"🗑️ Removed Store App: {app}");
            }

            // Log failed attempts
            var failedApps = selectedApps.Except(removedApps).ToList();
            foreach (var app in failedApps)
            {
                Logger.Log($"⚠️ Failed to remove Store App: {app}", LogLevel.Warning);
            }

            Logger.Log(Localization.T("App cleanup complete."));

            return removedApps; // Return removed apps to update the UI
        }


        /// <summary>
        /// Loads external bloatware and whitelist patterns from a text file (e.g., CFEnhancer.txt).
        /// Also checks if wildcard (*) is set to scan all apps.
        /// </summary>
        /// <param name="fileName">Name of the file to load from (must be in Plugins folder)</param>
        /// <returns>
        /// A tuple containing:
        /// - bloatwarePatterns: List of apps to flag as bloatware
        /// - whitelistPatterns: List of apps to ignore/exclude from detection
        /// - scanAll: Whether all apps should be shown regardless of matching patterns
        /// </returns>
        public (string[] bloatwarePatterns, string[] whitelistPatterns, bool scanAll) LoadExternalBloatwarePatterns(string fileName = "CFEnhancer.txt")
        {
            try
            {
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string fullPath = Path.Combine(exeDir, "Plugins", fileName);

                if (!File.Exists(fullPath))
                {
                    Logger.Log($"⚠️ The bloatware radar stays basic for now 🧠. Get the enhanced detection list from Options > Plugins > CFEnhancer plugin", LogLevel.Warning);
                    return (Array.Empty<string>(), Array.Empty<string>(), false);
                }

                var lines = File.ReadAllLines(fullPath);
                var bloatware = new List<string>();  // Apps to detect as bloatware
                var whitelist = new List<string>();  // Apps to ignore completely
                bool scanAll = false;               // Set to true if wildcard (*) is present

                foreach (var line in lines)
                {
                    // Strip comments after "#" and trim whitespace
                    var entry = line.Split('#')[0].Trim();

                    // Skip empty lines or lines with only comments
                    if (string.IsNullOrWhiteSpace(entry))
                        continue;

                    // Wildcard entry means: show all installed apps
                    if (entry == "*" || entry == "*.*")
                    {
                        scanAll = true;
                        continue;
                    }

                    // Entries starting with "!" go to the whitelist (excluded apps)
                    if (entry.StartsWith("!"))
                        whitelist.Add(entry.Substring(1).Trim().ToLower());
                    else
                        bloatware.Add(entry.ToLower());  // All other entries are bloatware patterns
                }

                return (bloatware.ToArray(), whitelist.ToArray(), scanAll);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error reading external bloatware file: {ex.Message}", LogLevel.Warning);
                return (Array.Empty<string>(), Array.Empty<string>(), false);
            }
        }


        /// <summary>
        /// OPTIONALLY!Returns all installed apps in the system.
        /// </summary>
        /// <returns></returns>
        public async Task<List<AppAnalysisResult>> GetAllInstalledAppsAsync()
        {
            await LoadAppsAsync();

            return _appDirectory.Select(kvp => new AppAnalysisResult
            {
                AppName = kvp.Key,
                FullName = kvp.Value
            }).ToList();
        }
    }
}