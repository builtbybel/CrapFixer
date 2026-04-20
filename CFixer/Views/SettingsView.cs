using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using CFixer.Helpers;

namespace CFixer.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            ApplyLocalization();
            LoadSettings();
            CheckIfIconsInstalled();
        }

        private void ApplyLocalization()
        {
            checkSaveToINI.Text = Localization.T("Save all settings to INI file");
            checkBox2.Text = Localization.T("Activate Plugins for PowerShell Tooling (Super Plugins)");
            button1.Text = Localization.T("Basic settings");
            checkInstallIcons.Text = Localization.T("Download optional icons to enhance navigation UI");
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
        }

        /// <summary>
        /// Loads checkbox settings from the INI file and applies them to the view.
        /// </summary>
        public void LoadSettings()
        {
            var settings = IniStateManager.LoadViewSettings("SETTINGS");
            checkSaveToINI.Checked = settings.GetValueOrDefault(nameof(checkSaveToINI), false);
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
            Localization.T("By default, buttons have no icons to reduce app size. Enable this to download and display navigation icons.\nWould you like to install it now?"),
                                    Localization.T("Icons Pack Detected"),
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
                        Localization.T("All icons have been successfully installed in the 'icons' folder!\n\n💖 Love CrapFixer? Consider supporting me with a small donation to keep this tool alive and improving!"),
                        Localization.T("Icons Installed"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    // Restart the application to apply changes
                    Application.Restart();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Localization.T("❌ An error occurred while downloading the icons:\n") + ex.Message,
                        Localization.T("Download Failed"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }
    }
}
