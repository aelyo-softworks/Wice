
namespace Wice.Utilities
{
    partial class VisualsTree
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.splitterMain = new System.Windows.Forms.Splitter();
            this.contextMenuStripTree = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.expandAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.collapseAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.closeWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.highlightSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeApplicationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.trackVisualUnderMouseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            this.treeViewVisuals = new System.Windows.Forms.TreeView();
            this.propertyGridVisual = new System.Windows.Forms.PropertyGrid();
            this.contextMenuStripTree.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitterMain
            // 
            this.splitterMain.Location = new System.Drawing.Point(0, 0);
            this.splitterMain.Name = "splitterMain";
            this.splitterMain.Size = new System.Drawing.Size(3, 692);
            this.splitterMain.TabIndex = 0;
            this.splitterMain.TabStop = false;
            // 
            // contextMenuStripTree
            // 
            this.contextMenuStripTree.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.expandAllToolStripMenuItem,
            this.collapseAllToolStripMenuItem,
            this.toolStripSeparator2,
            this.closeWindowToolStripMenuItem});
            this.contextMenuStripTree.Name = "contextMenuStripTree";
            this.contextMenuStripTree.Size = new System.Drawing.Size(151, 76);
            // 
            // expandAllToolStripMenuItem
            // 
            this.expandAllToolStripMenuItem.Name = "expandAllToolStripMenuItem";
            this.expandAllToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.expandAllToolStripMenuItem.Text = "Expand All";
            this.expandAllToolStripMenuItem.Click += new System.EventHandler(this.ExpandAllToolStripMenuItem_Click);
            // 
            // collapseAllToolStripMenuItem
            // 
            this.collapseAllToolStripMenuItem.Name = "collapseAllToolStripMenuItem";
            this.collapseAllToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.collapseAllToolStripMenuItem.Text = "Collapse All";
            this.collapseAllToolStripMenuItem.Click += new System.EventHandler(this.CollapseAllToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(147, 6);
            // 
            // closeWindowToolStripMenuItem
            // 
            this.closeWindowToolStripMenuItem.Name = "closeWindowToolStripMenuItem";
            this.closeWindowToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.closeWindowToolStripMenuItem.Text = "Close Window";
            this.closeWindowToolStripMenuItem.Click += new System.EventHandler(this.CloseWindowToolStripMenuItem_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewToolStripMenuItem,
            this.closeApplicationToolStripMenuItem,
            this.trackVisualUnderMouseToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(3, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(849, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.highlightSelectionToolStripMenuItem,
            this.toolStripSeparator1,
            this.refreshToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "&View";
            // 
            // highlightSelectionToolStripMenuItem
            // 
            this.highlightSelectionToolStripMenuItem.Checked = true;
            this.highlightSelectionToolStripMenuItem.CheckOnClick = true;
            this.highlightSelectionToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.highlightSelectionToolStripMenuItem.Name = "highlightSelectionToolStripMenuItem";
            this.highlightSelectionToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.highlightSelectionToolStripMenuItem.Text = "Highlight Selection";
            this.highlightSelectionToolStripMenuItem.Click += new System.EventHandler(this.HighlightSelectionToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(172, 6);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.refreshToolStripMenuItem.Text = "Refresh";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.RefreshToolStripMenuItem_Click);
            // 
            // closeApplicationToolStripMenuItem
            // 
            this.closeApplicationToolStripMenuItem.Name = "closeApplicationToolStripMenuItem";
            this.closeApplicationToolStripMenuItem.Size = new System.Drawing.Size(112, 20);
            this.closeApplicationToolStripMenuItem.Text = "Close All Applications";
            this.closeApplicationToolStripMenuItem.Click += new System.EventHandler(this.CloseApplicationToolStripMenuItem_Click);
            // 
            // trackVisualUnderMouseToolStripMenuItem
            // 
            this.trackVisualUnderMouseToolStripMenuItem.Name = "trackVisualUnderMouseToolStripMenuItem";
            this.trackVisualUnderMouseToolStripMenuItem.Size = new System.Drawing.Size(154, 20);
            this.trackVisualUnderMouseToolStripMenuItem.Text = "Track Visual Under Mouse";
            this.trackVisualUnderMouseToolStripMenuItem.Click += new System.EventHandler(this.TrackVisualUnderMouseToolStripMenuItem_Click);
            // 
            // splitContainerMain
            // 
            this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMain.Location = new System.Drawing.Point(3, 24);
            this.splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.treeViewVisuals);
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.propertyGridVisual);
            this.splitContainerMain.Size = new System.Drawing.Size(849, 668);
            this.splitContainerMain.SplitterDistance = 254;
            this.splitContainerMain.TabIndex = 3;
            // 
            // treeViewVisuals
            // 
            this.treeViewVisuals.ContextMenuStrip = this.contextMenuStripTree;
            this.treeViewVisuals.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewVisuals.FullRowSelect = true;
            this.treeViewVisuals.HideSelection = false;
            this.treeViewVisuals.Location = new System.Drawing.Point(0, 0);
            this.treeViewVisuals.Name = "treeViewVisuals";
            this.treeViewVisuals.Size = new System.Drawing.Size(254, 668);
            this.treeViewVisuals.TabIndex = 0;
            // 
            // propertyGridVisual
            // 
            this.propertyGridVisual.CommandsVisibleIfAvailable = false;
            this.propertyGridVisual.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGridVisual.HelpVisible = false;
            this.propertyGridVisual.Location = new System.Drawing.Point(0, 0);
            this.propertyGridVisual.Name = "propertyGridVisual";
            this.propertyGridVisual.Size = new System.Drawing.Size(591, 668);
            this.propertyGridVisual.TabIndex = 0;
            // 
            // VisualsTree
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(852, 692);
            this.Controls.Add(this.splitContainerMain);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.splitterMain);
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "VisualsTree";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Visuals Tree";
            this.contextMenuStripTree.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Splitter splitterMain;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripTree;
        private System.Windows.Forms.ToolStripMenuItem expandAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem collapseAllToolStripMenuItem;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.TreeView treeViewVisuals;
        private System.Windows.Forms.PropertyGrid propertyGridVisual;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem highlightSelectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeApplicationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem trackVisualUnderMouseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem closeWindowToolStripMenuItem;
    }
}