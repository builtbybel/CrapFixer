using CFixer;
using CFixer.Properties;
using CFixer.Views;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrapFixer
{
    public partial class MainForm : Form, ILocalizedControl
    {
        private NavigationManager _navigationManager;
        private NavigationHandler _navigationHandler;
        private LogActions _logActions;
        private LogActionsController _logActionsController;
        private readonly AppManagerService _appManager = new AppManagerService();

        public MainForm()
        {
            InitializeComponent();
            IniStateManager.ApplyWindowState(this);
            ApplyLocalization();
            LocalizationManager.LanguageChanged += LocalizationManager_LanguageChanged;

            // Set up the main navigation manager and logger
            _navigationManager = new NavigationManager(panelContainer);
            Logger.OutputBox = rtbLogger;

            // Set up log actions controller
            _logActions = new LogActions(rtbLogger);
        }

        private async void MainForm_Shown(object sender, EventArgs e)
        {
            await InitializeUI(); // _ = InitializeUI();
            InitializeAppState();
        }

        private void InitializeAppState()
        {
            // Load features and plugins into the tree view
            FeatureNodeManager.LoadFeatures(treeFeatures);
            PluginManager.LoadPlugins(treeFeatures);

            // Load settings from INI file if enabled
            IniStateManager.LoadFeaturesIfEnabled(treeFeatures);
        }

        private async Task InitializeUI()
        {
            // Initialize the navigation handler with buttons
            _navigationHandler = new NavigationHandler(btnFixer, btnRestore, btnTools, btnGitHub);

            // Load navigation icons
            await _navigationHandler.LoadNavigationIcons();

            // Register navigation handler
            _navigationHandler.NavigationButtonClicked += NavigationHandler_NavigationButtonClicked;

            // Register click handlers for GitHub links
            pictureHeader.Click += PictureHeader_Click;
            lblHeader.Click += PictureHeader_Click;

            // Re-initialize log actions controller (optional if not changed)
            _logActionsController = new LogActionsController(comboLogActions, _logActions);

            // Set version and OS info
            lblVersionInfo.Text = $"v{Program.GetAppVersion()} ";
            lblOSInfo.Text = await OSHelper.OSHelper.GetWindowsVersion();
        }

        private void ApplyLocalization()
        {
            btnAnalyze.Text = LocalizationManager.T("main.btnAnalyze");
            Windows.Text = LocalizationManager.T("main.tabWindows");
            Apps.Text = LocalizationManager.T("main.tabApplications");
            analyzeMarkedFeatureToolStripMenuItem.Text = LocalizationManager.T("main.ctxAnalyze");
            fixMarkedFeatureToolStripMenuItem.Text = LocalizationManager.T("main.ctxFix");
            restoreMarkedFeatureToolStripMenuItem.Text = LocalizationManager.T("main.ctxRestore");
            helpMarkedFeatureToolStripMenuItem.Text = LocalizationManager.T("main.ctxHelp");
            btnFix.Text = LocalizationManager.T("main.btnFix");
            btnRestore.Text = LocalizationManager.T("main.btnRestore");
            linkUpdateCheck.Text = LocalizationManager.T("main.linkUpdate");
            btnTools.Text = LocalizationManager.T("main.btnTools");
            btnFixer.Text = LocalizationManager.T("main.btnFixer");
            linkSelection.Text = LocalizationManager.T("main.linkSelection");

            var appsPlaceholder = LocalizationManager.T("main.appsPlaceholder");
            if (checkedListBoxApps.Items.Count == 1 &&
                checkedListBoxApps.Items[0] is string placeholder &&
                (placeholder == "No analysis yet" || placeholder == appsPlaceholder))
            {
                checkedListBoxApps.Items[0] = appsPlaceholder;
            }

            toolTip.SetToolTip(btnGitHub, LocalizationManager.T("main.tooltipSupport"));
            toolTip.SetToolTip(lblHeader, LocalizationManager.T("main.tooltipGithub"));
            toolTip.SetToolTip(pictureHeader, LocalizationManager.T("main.tooltipGithub"));
        }

        public void RefreshLocalization()
        {
            ApplyLocalization();
            RefreshFeatureTreeLocalization();
            if (panelContainer.Controls.Count > 0 && panelContainer.Controls[0] is ILocalizedControl localizedControl)
            {
                localizedControl.RefreshLocalization();
            }
        }

        private void RefreshFeatureTreeLocalization()
        {
            var checkedMap = CaptureNodeCheckedState(treeFeatures.Nodes);
            FeatureNodeManager.LoadFeatures(treeFeatures);
            PluginManager.LoadPlugins(treeFeatures);
            RestoreNodeCheckedState(treeFeatures.Nodes, checkedMap);
        }

        private static System.Collections.Generic.Dictionary<string, bool> CaptureNodeCheckedState(TreeNodeCollection nodes)
        {
            var map = new System.Collections.Generic.Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            CaptureNodeCheckedStateRecursive(nodes, map);
            return map;
        }

        private static void CaptureNodeCheckedStateRecursive(TreeNodeCollection nodes, System.Collections.Generic.Dictionary<string, bool> map)
        {
            foreach (TreeNode node in nodes)
            {
                string key = string.IsNullOrWhiteSpace(node.Name) ? node.Text.Trim() : node.Name.Trim();
                map[key] = node.Checked;

                if (node.Nodes.Count > 0)
                {
                    CaptureNodeCheckedStateRecursive(node.Nodes, map);
                }
            }
        }

        private static void RestoreNodeCheckedState(TreeNodeCollection nodes, System.Collections.Generic.Dictionary<string, bool> map)
        {
            foreach (TreeNode node in nodes)
            {
                string key = string.IsNullOrWhiteSpace(node.Name) ? node.Text.Trim() : node.Name.Trim();
                if (map.TryGetValue(key, out bool checkedState))
                {
                    node.Checked = checkedState;
                }

                if (node.Nodes.Count > 0)
                {
                    RestoreNodeCheckedState(node.Nodes, map);
                }
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

        // Handles navigation button clicks and switches views accordingly
        private void NavigationHandler_NavigationButtonClicked(Button button)
        {
            if (button == btnFixer)
            {
                _navigationManager.GoToMain();
            }
            else if (button == btnTools)
            {
                _navigationManager.SwitchView(new OptionsView());
            }
        }

        private async void btnAnalyze_Click(object sender, EventArgs e)
        {
            // Analyze features
            await FeatureNodeManager.AnalyzeAll(treeFeatures.Nodes);

            // Analyze plugins
            await PluginManager.AnalyzeAllPlugins(treeFeatures.Nodes);

            // Analyze apps
            await AnalyzeApps();

            // Show log actions combo box
            comboLogActions.Visible = true;
        }

        /// <summary>
        /// Analyzes the apps and logs the results.
        /// </summary>
        private async Task AnalyzeApps()
        {
            checkedListBoxApps.Items.Clear();

            // Try loading patterns from CFEnhancer.txt (located in Plugins folder)
            var (bloatwarePatterns, whitelistPatterns, scanAll) = _appManager.LoadExternalBloatwarePatterns();

            if (bloatwarePatterns.Length == 0 && !scanAll)
            {
                // Fallback to internal resource if external file not found or empty and scanAll is not enabled
                bloatwarePatterns = Resources.PredefinedApps?
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim().ToLower()).ToArray() ?? Array.Empty<string>();

                whitelistPatterns = Array.Empty<string>();
                Logger.Log("Using built-in bloatware list.", LogLevel.Info);
            }
            else
            {
                Logger.Log("🔎 Plugin ready: CFEnhancer (external bloatware list)", LogLevel.Info);
            }

            // Analyze installed apps based on patterns and whitelist, and optionally scan all
            var apps = await _appManager.AnalyzeAndLogAppsAsync(bloatwarePatterns, whitelistPatterns, scanAll);

            foreach (var app in apps)
            {
                checkedListBoxApps.Items.Add(app.FullName);
            }
        }

        private async void btnFix_Click(object sender, EventArgs e)
        {
            rtbLogger.Clear();

            // Fix all features
            foreach (TreeNode node in treeFeatures.Nodes)
                await FeatureNodeManager.FixChecked(node);

            // Fix all plugins
            foreach (TreeNode node in treeFeatures.Nodes)
                await PluginManager.FixChecked(node);

            // Fix selected Store apps
            var selectedApps = checkedListBoxApps.CheckedItems.Cast<string>().ToList();
            if (selectedApps.Count == 0)
                return;

            var appService = new AppManagerService();
            var removedApps = await appService.UninstallSelectedAppsAsync(selectedApps);

            // Update UI after uninstall
            foreach (var app in removedApps)
            {
                checkedListBoxApps.Items.Remove(app);
            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
           LocalizationManager.T("main.restoreMessage"),
           LocalizationManager.T("main.restoreTitle"),
           MessageBoxButtons.YesNo,
           MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                rtbLogger.Clear();
                foreach (TreeNode node in treeFeatures.Nodes)
                    FeatureNodeManager.RestoreChecked(node);

                Logger.Log(LocalizationManager.T("main.restoreLog"), LogLevel.Info);
            }
        }

        /// <summary>
        /// Analyzes all plugins and features starting from the selected node from the context menu.
        /// </summary>
        private async void analyzeMarkedFeatureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeFeatures.SelectedNode is TreeNode selectedNode)
            {
                Logger.Log($"🔎 Analyzing Feature: {selectedNode.Text}", LogLevel.Info);

                // If a single node is selected (leaf node with no children),
                // always analyze this node regardless of its Checked state.
                if (selectedNode.Nodes.Count == 0)
                {
                    await PluginManager.AnalyzePlugin(selectedNode);
                }
                else
                {
                    // If a parent node is selected (has children),
                    // recursively analyze only the checked plugin nodes.
                    await PluginManager.AnalyzeAll(selectedNode);
                }

                // Perform feature-specific analysis (non-plugin)
                FeatureNodeManager.AnalyzeFeature(selectedNode);
            }
        }

        /// <summary>
        /// Fixes all checked plugin and feature nodes starting from the selected node from the context menu.
        /// </summary>
        private async void fixMarkedFeatureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeFeatures.SelectedNode is TreeNode selectedNode)
            {
                Logger.Log($"🔧 Fixing Feature: {selectedNode.Text}", LogLevel.Info);

                // Recursively fix all checked feature nodes (non-plugin)
                await FeatureNodeManager.FixFeature(selectedNode);

                // Recursively fix all checked plugin nodes starting from the selected node
                await PluginManager.FixPlugin(selectedNode);
            }
        }

        /// <summary>
        /// Restores the selected plugin or feature to its previous state from the context menu.
        /// </summary>
        private async void restoreMarkedFeatureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeFeatures.SelectedNode is TreeNode selectedNode)
            {
                if (PluginManager.IsPluginNode(selectedNode))
                    // Restore the plugin using its Undo command if available!
                    await PluginManager.RestorePlugin(selectedNode);
                else
                    Logger.Log($"↩️ Restoring Feature: {selectedNode.Text}", LogLevel.Info);

                // Perform feature-specific restore (non-plugin)
                FeatureNodeManager.RestoreFeature(selectedNode);
            }
        }

        private void helpMarkedFeatureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeFeatures.SelectedNode is TreeNode selectedNode)
            {
                FeatureNodeManager.ShowHelp(selectedNode);
            }
        }

        /// <summary>
        /// Checks or unchecks all child nodes when a parent node is checked/unchecked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeFeatures_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Action != TreeViewAction.Unknown)
            {
                foreach (TreeNode child in e.Node.Nodes)
                    child.Checked = e.Node.Checked;
            }
        }

        private void treeFeatures_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // Get the node under the mouse cursor
                TreeNode nodeUnderMouse = treeFeatures.GetNodeAt(e.X, e.Y);

                if (nodeUnderMouse != null)
                {
                    treeFeatures.SelectedNode = nodeUnderMouse;

                    // Show the context menu at the mouse position
                    contextMenuStrip.Show(treeFeatures, e.Location);
                }
            }
        }

        /// <summary>
        /// Handles the link click event for selecting or deselecting all items in the list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private bool treeChecked = false;

        private void linkSelection_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (tabControl.SelectedTab == Windows)
            {
                foreach (TreeNode node in treeFeatures.Nodes)
                {
                    node.Checked = treeChecked;
                    foreach (TreeNode child in node.Nodes)
                    {
                        child.Checked = treeChecked;
                        foreach (TreeNode grandChild in child.Nodes)
                            grandChild.Checked = treeChecked;
                    }
                }

                treeChecked = !treeChecked;
            }
            else if (tabControl.SelectedTab == Apps)
            {
                bool shouldCheck = checkedListBoxApps.Items.Cast<object>()
                    .Any(item => checkedListBoxApps.GetItemChecked(checkedListBoxApps.Items.IndexOf(item)) == false);

                for (int i = 0; i < checkedListBoxApps.Items.Count; i++)
                {
                    checkedListBoxApps.SetItemChecked(i, shouldCheck);
                }
            }
        }

        private void panelHeader_Paint(object sender, PaintEventArgs e)
        {
            var panel = sender as Panel;
            var g = e.Graphics;

            // Solid background: #4D4D4D
            g.Clear(Color.FromArgb(77, 77, 77));

            // Inset line effect (3D-like): light line + shadow line
            Color baseColor = Color.FromArgb(80, 80, 80); // inset base

            using (var topLine = new Pen(ControlPaint.Light(baseColor, 0.0f)))
            using (var bottomLine = new Pen(ControlPaint.Dark(baseColor, 0.2f)))
            {
                g.SmoothingMode = SmoothingMode.None;
                g.DrawLine(topLine, 0, panel.Height - 2, panel.Width, panel.Height - 2);
                g.DrawLine(bottomLine, 0, panel.Height - 1, panel.Width, panel.Height - 1);
            }
        }

        // Handles click on the header image to open the GitHub page
        private void PictureHeader_Click(object sender, EventArgs e)
        {
            Utils.OpenGitHubPage(sender, e);
        }

        // Handles link click to check for updates
        private void linkUpdateCheck_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var updateUrl = $"https://builtbybel.github.io/CrapFixer/update-check.html?version={Program.GetAppVersion()}";

            var psi = new ProcessStartInfo
            {
                FileName = updateUrl,
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            LocalizationManager.LanguageChanged -= LocalizationManager_LanguageChanged;

            if (IniStateManager.IsViewSettingEnabled("SETTINGS", "checkSaveToINI"))
            {
                IniStateManager.Save(treeFeatures, this);
            }

            Logger.OutputBox = null; // Remove reference
        }

        private void btnGitHub_Click(object sender, EventArgs e)
        {
            _navigationManager.SwitchView(new OptionsView());
        }
    }
}
