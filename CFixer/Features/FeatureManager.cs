using Features;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CFixer.Helpers;

namespace CrapFixer
{
    /// <summary>
    /// Provides operations to load, analyze, fix, restore, and show help for FeatureNodes.
    /// </summary>
    public static class FeatureNodeManager
    {
        private static int totalChecked;
        private static int issuesFound;

        // Public properties to access the analysis results
        public static int TotalChecked => totalChecked;

        public static int IssuesFound => issuesFound;

        public static void ResetAnalysis()
        {
            totalChecked = 0;
            issuesFound = 0;
            Logger.Clear();
        }

        /// <summary>
        /// Loads all features into the TreeView.
        /// </summary>
        public static void LoadFeatures(TreeView tree)
        {
            // Hide the TreeView to avoid flickering and visible scroll jump
            tree.Visible = false;

            var features = FeatureLoader.Load();
            tree.Nodes.Clear();

            foreach (var feature in features)
                AddNode(tree.Nodes, feature);

            // root nodes (categories)
            foreach (TreeNode root in tree.Nodes)
            {
                root.NodeFont = new Font(tree.Font, FontStyle.Bold);
                root.ForeColor = Color.RoyalBlue; // category color
            }

            tree.ExpandAll(); // expand all nodes

            // Set scroll to top and make TreeView visible
            tree.BeginInvoke(new Action(() =>
            {
                // Ensure the first node is shown at the top (prevents auto-scroll to bottom)
                if (tree.Nodes.Count > 0) tree.TopNode = tree.Nodes[0];
                tree.Visible = true;
            }));
        }

        /// <summary>
        /// Recursively adds a FeatureNode and its children into the TreeView.
        /// </summary>
        private static void AddNode(TreeNodeCollection treeNodes, FeatureNode featureNode)
        {
            string text = featureNode.IsCategory
                ? "  " + featureNode.Name + "  " // add extra space to avoid clipping
                : featureNode.Name;

            TreeNode node = new TreeNode(text)
            {
                Tag = featureNode,
                Checked = featureNode.DefaultChecked,
            };
            treeNodes.Add(node);

            foreach (var child in featureNode.Children)
                AddNode(node.Nodes, child);
        }

        /// <summary>
        /// Analyzes all checked features recursively and logs only issues.
        /// </summary>
        public static async Task AnalyzeAll(TreeNodeCollection nodes)
        {
            ResetAnalysis();

            // Iterate through all nodes and analyze each one recursively
            foreach (TreeNode node in nodes)
            {
                // Recursively analyze each node and ensure async tasks are awaited
                await AnalyzeCheckedRecursive(node);
            }

            Logger.Log("🔎 ANALYSIS COMPLETE", LogLevel.Info);
            Logger.Log(new string('=', 50), LogLevel.Info);

            int ok = totalChecked - issuesFound;
            Logger.Log($"요약: 선택한 설정 {totalChecked}개 중 {ok}개는 정상이며, {issuesFound}개는 점검이 필요합니다.",
                issuesFound > 0 ? LogLevel.Warning : LogLevel.Info);
        }

        /// <summary>
        /// Recursively checks all features and logs misconfigurations.
        /// </summary>
        private static async Task AnalyzeCheckedRecursive(TreeNode node)
        {
            if (node.Tag is FeatureNode fn)
            {
                // If the node is not a category, is checked, and has a feature to check
                if (!fn.IsCategory && node.Checked && fn.Feature != null)
                {
                    totalChecked++;
                    bool isOk = await fn.Feature.CheckFeature();  // Await the async operation

                    if (!isOk)
                    {
                        issuesFound++;
                        node.ForeColor = Color.Red; // Mark as misconfigured
                        string category = Localization.TreeText(node.Parent?.Text ?? "General");
                        Logger.Log($"❌ [{category}] {fn.Name} - Not configured as recommended.");
                        Logger.Log($"   ➤ {Localization.Detail(fn.Feature.GetFeatureDetails())}");
                        // Log a separator when an issue was found
                        Logger.Log(new string('-', 50), LogLevel.Info);
                    }
                    else
                    {
                        node.ForeColor = Color.Gray; // Mark as properly configured
                    }
                }

                // Recursively process child nodes and ensure awaiting the tasks
                foreach (TreeNode child in node.Nodes)
                {
                    await AnalyzeCheckedRecursive(child);  // Recursively call and await the result
                }
            }
        }

        /// <summary>
        /// Fixes all checked features recursively.
        /// </summary>
        public static async Task FixChecked(TreeNode node)
        {
            if (node.Tag is FeatureNode fn)
            {
                if (!fn.IsCategory && node.Checked && fn.Feature != null)
                {
                    bool result = await fn.Feature.DoFeature();
                    Logger.Log(result
                        ? $"🔧 {fn.Name} - Fixed"
                        : $"❌ {fn.Name} - ⚠️ Fix failed (This feature may require admin privileges)",
                        result ? LogLevel.Info : LogLevel.Error);
                }

                foreach (TreeNode child in node.Nodes)
                    await FixChecked(child);
            }
        }

        /// <summary>
        /// Restores all checked features recursively.
        /// </summary>
        public static void RestoreChecked(TreeNode node)
        {
            if (node.Tag is FeatureNode fn)
            {
                if (!fn.IsCategory && node.Checked && fn.Feature != null)
                {
                    bool ok = fn.Feature.UndoFeature();
                    string category = Localization.TreeText(node.Parent?.Text ?? "General");
                    Logger.Log(ok
                        ? $"↩️ [{category}] {fn.Name} - Restored"
                        : $"❌ [{category}] {fn.Name} - Restore failed",
                        ok ? LogLevel.Info : LogLevel.Error);
                }

                foreach (TreeNode child in node.Nodes)
                    RestoreChecked(child);
            }
        }
        /// <summary>
        /// Analyzes a selected feature or, if it's a category, analyzes only checked child features.
        /// </summary>
        public static async void AnalyzeFeature(TreeNode node)
        {
            // Analyze this node if it's a leaf node (not a category)
            if (node.Tag is FeatureNode fn && !fn.IsCategory && fn.Feature != null)
            {
                bool isOk = await fn.Feature.CheckFeature();
                node.ForeColor = isOk ? Color.Gray : Color.Red;

                if (isOk)
                {
                    Logger.Log($"✅ Feature: {fn.Name} is properly configured.", LogLevel.Info);
                }
                else
                {
                    Logger.Log($"❌ Feature: {fn.Name} requires attention.", LogLevel.Warning);
                    Logger.Log($"   ➤ {Localization.Detail(fn.Feature.GetFeatureDetails())}");
                    Logger.Log(new string('-', 50), LogLevel.Info);
                }
            }
            else
            {
                // If it's a category node, analyze only checked child nodes
                foreach (TreeNode child in node.Nodes)
                {
                    if (child.Checked)
                        AnalyzeFeature(child);
                }
            }
        }


        /// <summary>
        /// Attempts to fix the selected feature or, if it is a category, fixes only checked child features.
        /// </summary>
        public static async Task FixFeature(TreeNode node)
        {
            // Try to fix this node if it is NOT a category (i.e., a leaf node)
            if (node.Tag is FeatureNode fn && !fn.IsCategory && fn.Feature != null)
            {
                // Always fix the selected leaf node, regardless of Checked
                bool result = await fn.Feature.DoFeature();
                Logger.Log(result
                    ? $"🔧 {fn.Name} - Fixed"
                    : $"❌ {fn.Name} - ⚠️ Fix failed (This feature may require admin privileges)",
                    result ? LogLevel.Info : LogLevel.Error);
            }
            else
            {
                // If it's a category node, fix only checked child nodes (recursively)
                foreach (TreeNode child in node.Nodes)
                {
                    if (child.Checked)
                        await FixFeature(child);
                }
            }
        }


        /// <summary>
        /// Restores a selected feature (always) or, if it's a category, only restores checked child features.
        /// Logs success or failure.
        /// </summary>
        public static void RestoreFeature(TreeNode node)
        {
            // Restore feature node regardless of Checked state
            if (node.Tag is FeatureNode fn && !fn.IsCategory && fn.Feature != null)
            {
                bool ok = fn.Feature.UndoFeature();
                Logger.Log(ok
                    ? $"↩️ {fn.Name} - Restored"
                    : $"❌ {fn.Name} - Restore failed",
                    ok ? LogLevel.Info : LogLevel.Error);
            }
            else
            {
                // For category nodes, only restore checked children
                foreach (TreeNode child in node.Nodes)
                {
                    if (child.Checked)
                        RestoreFeature(child);
                }
            }
        }


        /// <summary>
        /// Displays help information for the selected feature or plugin.
        /// If a feature is selected, also offers to search online.
        /// </summary>
        public static void ShowHelp(TreeNode node)
        {
            // Show help for features
            if (node?.Tag is FeatureNode fn && fn.Feature != null)
            {
                string info = fn.Feature.Info();
                MessageBox.Show(
                    !string.IsNullOrEmpty(info) ? Localization.T(info) : Localization.T("No additional information available."),
                    $"{Localization.T("Help")}: {fn.Name}",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Optional online help
                var result = MessageBox.Show(
                    Localization.T("Would you like to search online for more information about this feature?"),
                    Localization.T("Online Help"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    string searchQuery = Uri.EscapeDataString(fn.Feature.GetFeatureDetails());
                    string webUrl = $"https://www.google.com/search?q={searchQuery}";
                    System.Diagnostics.Process.Start(new ProcessStartInfo
                    {
                        FileName = webUrl,
                        UseShellExecute = true
                    });
                }

                return;
            }

            // Show help for plugins
            if (!PluginManager.ShowHelp(node))
            {
                MessageBox.Show(Localization.T("⚠️ No feature or plugin selected, or help info unavailable."),
                    Localization.T("Help"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }
}
