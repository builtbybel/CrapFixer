using Crapfixer;
using MSCFixer.Views;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Views
{
    public partial class AboutView : UserControl

    {

        public AboutView()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Update version label
            this.lblVersionInfo.Text = $"v{Program.GetAppVersion()} ";
        }

        private void linkGitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/builtbybel/Crapfixer/releases");
        }

        private void btnDonate_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Hi! I'm building Crapfixer solo. Want to support me and help launch Version 1.0?",
                "Support Crapfixer ❤️",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
            {
                var donationChoice = MessageBox.Show(
                    "Would you like to donate via PayPal? (Click No for Ko-fi)",
                    "Choose Your Support Method",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (donationChoice == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://www.paypal.com/donate/?hosted_button_id=M9DW4VNKH9ECQ",
                        UseShellExecute = true
                    });
                }
                else if (donationChoice == DialogResult.No)
                {
                    System.Diagnostics.Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://ko-fi.com/builtbybel",
                        UseShellExecute = true
                    });
                }
            }
        }

        private void AboutView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}