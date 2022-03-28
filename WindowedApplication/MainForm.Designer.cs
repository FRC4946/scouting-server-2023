
namespace WindowedApplication
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.status = new System.Windows.Forms.StatusStrip();
            this.managerStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.loggerStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.strip = new System.Windows.Forms.MenuStrip();
            this.run = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshTimer = new System.Windows.Forms.Timer(this.components);
            this.errors = new System.Windows.Forms.ListView();
            this.errorColumn = new System.Windows.Forms.ColumnHeader();
            this.statusTree = new System.Windows.Forms.TreeView();
            this.status.SuspendLayout();
            this.strip.SuspendLayout();
            this.SuspendLayout();
            // 
            // status
            // 
            this.status.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.status.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.managerStatus,
            this.loggerStatus});
            this.status.Location = new System.Drawing.Point(0, 424);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(800, 26);
            this.status.TabIndex = 0;
            // 
            // managerStatus
            // 
            this.managerStatus.Name = "managerStatus";
            this.managerStatus.Size = new System.Drawing.Size(140, 20);
            this.managerStatus.Text = "Server: Not Running";
            // 
            // loggerStatus
            // 
            this.loggerStatus.Name = "loggerStatus";
            this.loggerStatus.Size = new System.Drawing.Size(146, 20);
            this.loggerStatus.Text = "Logger: Not Running";
            // 
            // strip
            // 
            this.strip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.strip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.run});
            this.strip.Location = new System.Drawing.Point(0, 0);
            this.strip.Name = "strip";
            this.strip.Size = new System.Drawing.Size(800, 28);
            this.strip.TabIndex = 2;
            this.strip.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.strip_ItemClicked);
            // 
            // run
            // 
            this.run.Name = "run";
            this.run.Size = new System.Drawing.Size(53, 24);
            this.run.Text = "RUN";
            // 
            // refreshTimer
            // 
            this.refreshTimer.Interval = 2500;
            this.refreshTimer.Tick += new System.EventHandler(this.refreshTimer_Tick);
            // 
            // errors
            // 
            this.errors.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.errorColumn});
            this.errors.Dock = System.Windows.Forms.DockStyle.Left;
            this.errors.FullRowSelect = true;
            this.errors.GridLines = true;
            this.errors.HideSelection = false;
            this.errors.Location = new System.Drawing.Point(0, 28);
            this.errors.Name = "errors";
            this.errors.Size = new System.Drawing.Size(355, 396);
            this.errors.TabIndex = 3;
            this.errors.UseCompatibleStateImageBehavior = false;
            this.errors.View = System.Windows.Forms.View.Details;
            // 
            // errorColumn
            // 
            this.errorColumn.Text = "Error";
            this.errorColumn.Width = 350;
            // 
            // statusTree
            // 
            this.statusTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statusTree.Location = new System.Drawing.Point(355, 28);
            this.statusTree.Name = "statusTree";
            this.statusTree.Size = new System.Drawing.Size(445, 396);
            this.statusTree.TabIndex = 4;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.statusTree);
            this.Controls.Add(this.errors);
            this.Controls.Add(this.status);
            this.Controls.Add(this.strip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Scouting Server";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.status.ResumeLayout(false);
            this.status.PerformLayout();
            this.strip.ResumeLayout(false);
            this.strip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip status;
        private System.Windows.Forms.MenuStrip strip;
        private System.Windows.Forms.ToolStripMenuItem run;
        private System.Windows.Forms.ToolStripStatusLabel managerStatus;
        private System.Windows.Forms.Timer refreshTimer;
        private System.Windows.Forms.ListView errors;
        private System.Windows.Forms.ColumnHeader errorColumn;
        private System.Windows.Forms.ToolStripStatusLabel loggerStatus;
        private System.Windows.Forms.TreeView statusTree;
    }
}

