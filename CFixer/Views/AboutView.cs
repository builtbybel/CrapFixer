using CrapFixer;
using CFixer;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Views
{
    public partial class AboutView : UserControl, ILocalizedControl

    {
        public AboutView()
        {
            InitializeComponent();
            InitializeUI();
            ApplyLocalization();
            LocalizationManager.LanguageChanged += LocalizationManager_LanguageChanged;
            Disposed += AboutView_Disposed;
        }

        private void InitializeUI()
        {
            // Update version label
            this.lblVersionInfo.Text = $"v{Program.GetAppVersion()} ";

            // Populate amount choices
            comboBoxAmount.Items.AddRange(new object[] { "3.50", "5", "10",
                                                          "12", "15", "16",
                                                          "17", "18","20",
                                                          "25", "30", "35",
                                                          "40", "50", "60",
                                                          "70", "80", "100"});
            comboBoxAmount.SelectedIndex = 2;

            // Populate currency options
            comboBoxCurrency.Items.AddRange(new object[] { "EUR", "USD", "GBP", "CAD", "AUD", "CHF" });
            comboBoxCurrency.SelectedIndex = 0;
        }

        private void ApplyLocalization()
        {
            label1.Text = LocalizationManager.T("about.description");
            btnDonate.Text = LocalizationManager.T("about.donate");
            lblCopyright.Text = LocalizationManager.T("about.copyright");
        }

        public void RefreshLocalization()
        {
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

        private void AboutView_Disposed(object sender, EventArgs e)
        {
            LocalizationManager.LanguageChanged -= LocalizationManager_LanguageChanged;
            Disposed -= AboutView_Disposed;
        }

        private void linkGitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/builtbybel/CrapFixer/releases");
        }

        private void btnDonate_Click(object sender, EventArgs e)
        {
            string amount = comboBoxAmount.SelectedItem?.ToString();
            string currency = comboBoxCurrency.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(amount) || string.IsNullOrEmpty(currency))
            {
                MessageBox.Show(LocalizationManager.T("about.donationValidation"));
                return;
            }

            string email = "belim@builtbybel.com";
            string purpose = Uri.EscapeDataString(LocalizationManager.T("about.donationPurpose"));

            string returnUrl = Uri.EscapeDataString("https://github.com/Belim/support");
            string cancelUrl = Uri.EscapeDataString("https://github.com/builtbybel/CrapFixer");

            string url = $"https://www.paypal.com/cgi-bin/webscr?cmd=_donations" +
                         $"&business={Uri.EscapeDataString(email)}" +
                         $"&amount={amount}" +
                         $"&currency_code={currency}" +
                         $"&item_name={purpose}" +
                         $"&return={returnUrl}" +
                         $"&cancel_return={cancelUrl}";

            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
    }
}