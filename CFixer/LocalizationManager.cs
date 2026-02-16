using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;

namespace CFixer
{
    internal static class LocalizationManager
    {
        public static event EventHandler LanguageChanged;

        public const string DefaultLanguage = "en-US";
        public const string ChineseLanguage = "zh-CN";

        private static readonly Dictionary<string, Dictionary<string, string>> LocalizedTexts =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, string> LanguageDisplayNames =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, string> FeatureTypeKeys =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["BasicCleanup"] = "issues.disk_cleanup_basic",
                ["WingetUpgradeAll"] = "issues.upgrade_all_apps_winget",

                ["BSODDetails"] = "system.show_bsod_details",
                ["VerboseStatus"] = "system.enable_logon_verbose_status",
                ["SpeedUpShutdown"] = "system.speed_up_shutdown",
                ["NetworkThrottling"] = "system.disable_network_throttling",
                ["SystemResponsiveness"] = "system.optimize_responsiveness",
                ["MenuShowDelay"] = "system.reduce_menu_show_delay",
                ["TaskbarEndTask"] = "system.enable_taskbar_end_task",

                ["BrowserSignin"] = "edge.disable_browser_signin_sync",
                ["DefaultTopSites"] = "edge.hide_new_tab_sponsored_links",
                ["DefautBrowserSetting"] = "edge.disable_default_browser_setting",
                ["EdgeCollections"] = "edge.disable_collections_access",
                ["EdgeShoppingAssistant"] = "edge.disable_shopping_assistant",
                ["FirstRunExperience"] = "edge.hide_first_run_experience",
                ["GamerMode"] = "edge.disable_gamer_mode",
                ["HubsSidebar"] = "edge.disable_copilot_symbol",
                ["ImportOnEachLaunch"] = "edge.disable_cross_browser_import_on_launch",
                ["StartupBoost"] = "edge.disable_startup_boost",
                ["TabPageQuickLinks"] = "edge.hide_new_tab_quick_links",
                ["UserFeedback"] = "edge.disable_user_feedback_submit",

                ["FullContextMenus"] = "ui.show_windows11_full_context_menu",
                ["LockScreen"] = "ui.disable_personalized_lock_screen",
                ["SearchboxTaskbarMode"] = "ui.hide_taskbar_search_box",
                ["ShowOrHideMostUsedApps"] = "ui.hide_start_menu_most_used_apps",
                ["ShowTaskViewButton"] = "ui.hide_taskbar_task_view_button",
                ["DisableSearchBoxSuggestions"] = "ui.disable_search_box_suggestions",
                ["DisableBingSearch"] = "ui.disable_bing_search",
                ["StartLayout"] = "ui.enable_start_menu_more_pins",
                ["TaskbarAlignment"] = "ui.align_taskbar_start_left",
                ["Transparency"] = "ui.disable_transparency_effects",
                ["AppDarkMode"] = "ui.enable_dark_mode_apps",
                ["SystemDarkMode"] = "ui.enable_dark_mode_system",
                ["DisableSnapAssistFlyout"] = "ui.disable_snap_assist_flyout",

                ["GameDVR"] = "gaming.disable_game_dvr",
                ["PowerThrottling"] = "gaming.disable_power_throttling",
                ["VisualFX"] = "gaming.disable_visual_effects",

                ["ActivityHistory"] = "privacy.disable_activity_history",
                ["LocationTracking"] = "privacy.disable_location_tracking",
                ["PrivacyExperience"] = "privacy.disable_signin_privacy_experience",
                ["Telemetry"] = "privacy.disable_telemetry",

                ["FileExplorerAds"] = "ads.disable_file_explorer",
                ["FinishSetupAds"] = "ads.disable_finish_setup",
                ["LockScreenAds"] = "ads.disable_lock_screen_tips",
                ["PersonalizedAds"] = "ads.disable_personalized",
                ["SettingsAds"] = "ads.disable_settings",
                ["StartmenuAds"] = "ads.disable_start_menu",
                ["TailoredExperiences"] = "ads.disable_tailored_experiences",
                ["TipsAndSuggestions"] = "ads.disable_tips_suggestions",
                ["WelcomeExperienceAds"] = "ads.disable_welcome_experience",

                ["AskCopilot"] = "ai.remove_ask_copilot_context_menu",
                ["ClickToDo"] = "ai.disable_click_to_do",
                ["CopilotTaskbar"] = "ai.hide_taskbar_copilot",
                ["Recall"] = "ai.disable_recall",
            };

        private static readonly Regex DontWordPattern = new Regex(@"\bdon't\b", RegexOptions.Compiled);
        private static readonly Regex DontNoApostrophePattern = new Regex(@"\bdont\b", RegexOptions.Compiled);
        private static readonly Regex NonAlphaNumericPattern = new Regex(@"[^a-z0-9]+", RegexOptions.Compiled);

        static LocalizationManager()
        {
            LoadLanguageFiles();
        }

        public static string CurrentLanguageCode { get; private set; } = DefaultLanguage;

        public static void SetLanguage(string languageCode)
        {
            string normalizedLanguage = DefaultLanguage;
            if (!string.IsNullOrWhiteSpace(languageCode) && LocalizedTexts.ContainsKey(languageCode))
            {
                normalizedLanguage = languageCode;
            }

            bool changed = !string.Equals(CurrentLanguageCode, normalizedLanguage, StringComparison.OrdinalIgnoreCase);
            CurrentLanguageCode = normalizedLanguage;

            var culture = new CultureInfo(CurrentLanguageCode);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            if (changed)
            {
                LanguageChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static string T(string key)
        {
            if (LocalizedTexts.TryGetValue(CurrentLanguageCode, out var languageValues) && languageValues.TryGetValue(key, out var text))
            {
                return text;
            }

            if (LocalizedTexts.TryGetValue(DefaultLanguage, out var fallbackLanguage) && fallbackLanguage.TryGetValue(key, out var fallback))
            {
                return fallback;
            }

            return key;
        }

        public static string LocalizeFeatureCategory(string categoryName)
        {
            return T("feature.category." + categoryName);
        }

        public static string LocalizeFeatureName(FeatureNode featureNode)
        {
            if (featureNode?.Feature != null)
            {
                var typeName = featureNode.Feature.GetType().Name;
                if (FeatureTypeKeys.TryGetValue(typeName, out var semanticKey))
                {
                    return T("feature.name." + semanticKey);
                }
            }

            return T("feature.name." + ToFeatureKey(featureNode?.Name ?? string.Empty));
        }

        public static IReadOnlyDictionary<string, string> GetAvailableLanguages()
        {
            return new Dictionary<string, string>(LanguageDisplayNames, StringComparer.OrdinalIgnoreCase);
        }

        private static void LoadLanguageFiles()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var langDir = Path.Combine(baseDir, "lang");

            LocalizedTexts.Clear();
            LanguageDisplayNames.Clear();

            if (Directory.Exists(langDir))
            {
                foreach (var file in Directory.GetFiles(langDir, "*.json").OrderBy(f => f))
                {
                    var languageCode = Path.GetFileNameWithoutExtension(file);
                    LoadLanguageFromFile(file, languageCode);
                }
            }

            if (!LocalizedTexts.ContainsKey(DefaultLanguage))
            {
                LocalizedTexts[DefaultLanguage] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                LanguageDisplayNames[DefaultLanguage] = DefaultLanguage;
            }
        }

        private static void LoadLanguageFromFile(string filePath, string languageCode)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            try
            {
                var json = File.ReadAllText(filePath);
                var serializer = new JavaScriptSerializer();
                var dictionary = serializer.Deserialize<Dictionary<string, string>>(json);

                if (dictionary != null)
                {
                    LocalizedTexts[languageCode] = new Dictionary<string, string>(dictionary, StringComparer.OrdinalIgnoreCase);

                    if (dictionary.TryGetValue("__meta.languageName", out var languageName) && !string.IsNullOrWhiteSpace(languageName))
                    {
                        LanguageDisplayNames[languageCode] = languageName;
                    }
                    else
                    {
                        LanguageDisplayNames[languageCode] = languageCode;
                    }
                }
            }
            catch
            {
                // Keep silent and fallback to existing loaded languages.
            }
        }

        private static string ToFeatureKey(string featureName)
        {
            if (string.IsNullOrWhiteSpace(featureName))
            {
                return string.Empty;
            }

            var normalized = featureName.Trim().ToLowerInvariant();
            normalized = DontWordPattern.Replace(normalized, "disable");
            normalized = DontNoApostrophePattern.Replace(normalized, "disable");
            normalized = NonAlphaNumericPattern.Replace(normalized, "_");
            normalized = normalized.Trim('_');
            return normalized;
        }
    }
}
