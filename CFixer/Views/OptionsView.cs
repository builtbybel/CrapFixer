using System;
using System.Windows.Forms;
using Views;

namespace CFixer.Views
{
    public partial class OptionsView : UserControl, ILocalizedControl
    {
        private NavigationManager subNavigation;

        public OptionsView()
        {
            InitializeComponent();
            ApplyLocalization();
            LocalizationManager.LanguageChanged += LocalizationManager_LanguageChanged;
            Disposed += OptionsView_Disposed;
            subNavigation = new NavigationManager(panelSubContent);
            subNavigation.SwitchView(new AboutView()); // Startsite
        }

        private void ApplyLocalization()
        {
            btnSettingsMenu.Text = LocalizationManager.T("options.settings");
            btnPluginsMenu.Text = LocalizationManager.T("options.plugins");
            btnViveMenu.Text = LocalizationManager.T("options.features");
            btnAboutMenu.Text = LocalizationManager.T("options.about");
        }

        public void RefreshLocalization()
        {
            ApplyLocalization();
            if (panelSubContent.Controls.Count > 0 && panelSubContent.Controls[0] is ILocalizedControl localizedControl)
            {
                localizedControl.RefreshLocalization();
            }
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

        private void OptionsView_Disposed(object sender, EventArgs e)
        {
            LocalizationManager.LanguageChanged -= LocalizationManager_LanguageChanged;
            Disposed -= OptionsView_Disposed;
        }

        private void btnAboutMenu_Click(object sender, EventArgs e)
        {
            subNavigation.SwitchView(new AboutView());
        }

        private void btnSettingsMenu_Click(object sender, EventArgs e)
        {
            subNavigation.SwitchView(new SettingsView());
        }

        private void btnPluginsMenu_Click(object sender, EventArgs e)
        {
            subNavigation.SwitchView(new PluginsView());
        }

        private void btnViveMenu_Click(object sender, EventArgs e)
        {
            subNavigation.SwitchView(new ViveView());
        }
    }
}