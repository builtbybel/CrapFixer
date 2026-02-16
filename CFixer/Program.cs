using System;
using System.Reflection;
using System.Windows.Forms;
using CFixer;

namespace CrapFixer
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            var language = IniStateManager.LoadViewStringSetting("SETTINGS", "Language", LocalizationManager.DefaultLanguage);
            LocalizationManager.SetLanguage(language);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        /// <summary>
        /// Retrieves the version of the app
        /// </summary>
        /// <returns>The application version in the format "major.minor.build"</returns>
        public static string GetAppVersion()
        {
            // Get the version of the current executing assembly
            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            // Return the version in the format "major.minor.build"
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }
}