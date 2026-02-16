using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace CFixer.Views
{
    public partial class SettingsView : UserControl, ILocalizedControl
    {
        private bool _isInitializingLanguage;

        public SettingsView()
        {
            InitializeComponent();
            PopulateLanguageOptions(LocalizationManager.CurrentLanguageCode);
            LoadSettings();
            ApplyLocalization();
            CheckIfIconsInstalled();
            LocalizationManager.LanguageChanged += LocalizationManager_LanguageChanged;
            Disposed += SettingsView_Disposed;
        }

        /// <summary>
        /// Collects and saves all relevant checkbox settings to the INI file.
        /// </summary>
        public void SaveSettings()
        {
            var settings = new Dictionary<string, bool>
    {
        { nameof(checkSaveToINI), checkSaveToINI.Checked },
    };

            IniStateManager.SaveViewSettings("SETTINGS", settings);
            IniStateManager.SaveViewStringSetting("SETTINGS", "Language", GetSelectedLanguageCode());
        }

        /// <summary>
        /// Loads checkbox settings from the INI file and applies them to the view.
        /// </summary>
        public void LoadSettings()
        {
            var settings = IniStateManager.LoadViewSettings("SETTINGS");
            checkSaveToINI.Checked = settings.GetValueOrDefault(nameof(checkSaveToINI), false);

            var savedLanguage = IniStateManager.LoadViewStringSetting("SETTINGS", "Language", LocalizationManager.CurrentLanguageCode);
            SetLanguageSelection(savedLanguage);
        }

        private void ApplyLocalization()
        {
            button1.Text = LocalizationManager.T("settings.sectionBasic");
            checkSaveToINI.Text = LocalizationManager.T("settings.saveIni");
            checkBox2.Text = LocalizationManager.T("settings.superPlugins");
            checkInstallIcons.Text = LocalizationManager.T("settings.installIcons");
            labelLanguage.Text = LocalizationManager.T("settings.languageLabel");

            PopulateLanguageOptions(GetSelectedLanguageCode());
        }

        public void RefreshLocalization()
        {
            ApplyLocalization();
        }

        private void PopulateLanguageOptions(string selectedLanguageCode)
        {
            _isInitializingLanguage = true;
            try
            {
                comboLanguage.Items.Clear();

                var languages = LocalizationManager.GetAvailableLanguages();
                var options = languages
                    .OrderBy(l => l.Key.Equals(LocalizationManager.DefaultLanguage, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                    .ThenBy(l => l.Key, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                foreach (var option in options)
                {
                    comboLanguage.Items.Add(option);
                }

                comboLanguage.DisplayMember = "Value";
                comboLanguage.ValueMember = "Key";

                var targetCode = languages.ContainsKey(selectedLanguageCode)
                    ? selectedLanguageCode
                    : LocalizationManager.DefaultLanguage;

                int index = options.FindIndex(o => o.Key == targetCode);
                comboLanguage.SelectedIndex = index >= 0 ? index : (options.Count > 0 ? 0 : -1);
            }
            finally
            {
                _isInitializingLanguage = false;
            }
        }

        private string GetSelectedLanguageCode()
        {
            if (comboLanguage.SelectedItem is KeyValuePair<string, string> selected)
            {
                return selected.Key;
            }

            return LocalizationManager.DefaultLanguage;
        }

        private void SetLanguageSelection(string languageCode)
        {
            var languages = LocalizationManager.GetAvailableLanguages();
            var normalizedCode = languages.ContainsKey(languageCode)
                ? languageCode
                : LocalizationManager.DefaultLanguage;

            for (int i = 0; i < comboLanguage.Items.Count; i++)
            {
                var option = (KeyValuePair<string, string>)comboLanguage.Items[i];
                if (option.Key == normalizedCode)
                {
                    comboLanguage.SelectedIndex = i;
                    return;
                }
            }

            comboLanguage.SelectedIndex = 0;
        }

        private void SettingsView_Leave(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void CheckIfIconsInstalled()
        {
            string iconFolder = Path.Combine(Application.StartupPath, "icons");
            string[] requiredIcons = { "fixer.png", "options.png", "restore.png" };

            bool allIconsExist = requiredIcons.All(icon => File.Exists(Path.Combine(iconFolder, icon)));

            checkInstallIcons.Enabled = !allIconsExist;
        }

        private async void checkInstallIcons_CheckedChanged(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                LocalizationManager.T("settings.iconPrompt"),
                LocalizationManager.T("settings.iconPromptTitle"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information
            );

            if (result == DialogResult.Yes)
            {
                try
                {
                    string iconFolder = Path.Combine(Application.StartupPath, "icons");
                    if (!Directory.Exists(iconFolder))
                        Directory.CreateDirectory(iconFolder);

                    string[] iconFiles = new string[]
                    {
                        "fixer.png",
                        "options.png",
                        "restore.png"
                    };

                    string baseUrl = "https://raw.githubusercontent.com/builtbybel/CrapFixer/main/icons/";

                    using (var wc = new WebClient())
                    {
                        foreach (string fileName in iconFiles)
                        {
                            string url = baseUrl + fileName;
                            string localPath = Path.Combine(iconFolder, fileName);
                            await wc.DownloadFileTaskAsync(new Uri(url), localPath);
                        }
                    }

                    MessageBox.Show(
                        LocalizationManager.T("settings.iconInstalled"),
                        LocalizationManager.T("settings.iconInstalledTitle"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    // Restart the application to apply changes
                    Application.Restart();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        LocalizationManager.T("settings.iconDownloadFailed") + ex.Message,
                        LocalizationManager.T("settings.iconDownloadFailedTitle"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        private void comboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isInitializingLanguage)
            {
                return;
            }

            var selectedLanguage = GetSelectedLanguageCode();
            if (selectedLanguage == LocalizationManager.CurrentLanguageCode)
            {
                return;
            }

            LocalizationManager.SetLanguage(selectedLanguage);
            SaveSettings();
            ApplyLocalization();
        }

        private void LocalizationManager_LanguageChanged(object sender, EventArgs e)
        {
            if (IsDisposed) return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(RefreshLocalization));
            }
            else
            {
                RefreshLocalization();
            }
        }

        private void SettingsView_Disposed(object sender, EventArgs e)
        {
            LocalizationManager.LanguageChanged -= LocalizationManager_LanguageChanged;
            Disposed -= SettingsView_Disposed;
        }
    }
}
