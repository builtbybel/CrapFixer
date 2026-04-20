using System;
using System.Windows.Forms;
using Views;
using CFixer.Helpers;

namespace CFixer.Views
{
    public partial class OptionsView : UserControl
    {
        private NavigationManager subNavigation;

        public OptionsView()
        {
            InitializeComponent();
            ApplyLocalization();
            subNavigation = new NavigationManager(panelSubContent);
            subNavigation.SwitchView(new AboutView()); // Startsite
        }

        private void ApplyLocalization()
        {
            btnSettingsMenu.Text = Localization.T("Settings");
            btnPluginsMenu.Text = Localization.T("Plugins");
            btnViveMenu.Text = Localization.T("Features ");
            btnAboutMenu.Text = Localization.T("About");
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
