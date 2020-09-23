namespace TauCode.Working.TestDemo.Gui.Server.Forms
{
    partial class AllJobsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.listViewJobs = new System.Windows.Forms.ListView();
            this.tabControlJobInfo = new System.Windows.Forms.TabControl();
            this.tabPageGeneral = new System.Windows.Forms.TabPage();
            this.tabPageRuns = new System.Windows.Forms.TabPage();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControlJobInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.listViewJobs);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControlJobInfo);
            this.splitContainer1.Size = new System.Drawing.Size(769, 527);
            this.splitContainer1.SplitterDistance = 286;
            this.splitContainer1.TabIndex = 0;
            this.splitContainer1.Text = "splitContainer1";
            // 
            // listViewJobs
            // 
            this.listViewJobs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewJobs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.listViewJobs.FullRowSelect = true;
            this.listViewJobs.GridLines = true;
            this.listViewJobs.HideSelection = false;
            this.listViewJobs.Location = new System.Drawing.Point(0, 0);
            this.listViewJobs.Name = "listViewJobs";
            this.listViewJobs.Size = new System.Drawing.Size(283, 527);
            this.listViewJobs.TabIndex = 0;
            this.listViewJobs.UseCompatibleStateImageBehavior = false;
            this.listViewJobs.View = System.Windows.Forms.View.Details;
            // 
            // tabControlJobInfo
            // 
            this.tabControlJobInfo.Controls.Add(this.tabPageGeneral);
            this.tabControlJobInfo.Controls.Add(this.tabPageRuns);
            this.tabControlJobInfo.Location = new System.Drawing.Point(3, 3);
            this.tabControlJobInfo.Name = "tabControlJobInfo";
            this.tabControlJobInfo.SelectedIndex = 0;
            this.tabControlJobInfo.Size = new System.Drawing.Size(476, 524);
            this.tabControlJobInfo.TabIndex = 0;
            // 
            // tabPageGeneral
            // 
            this.tabPageGeneral.Location = new System.Drawing.Point(4, 24);
            this.tabPageGeneral.Name = "tabPageGeneral";
            this.tabPageGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageGeneral.Size = new System.Drawing.Size(468, 496);
            this.tabPageGeneral.TabIndex = 0;
            this.tabPageGeneral.Text = "General";
            this.tabPageGeneral.UseVisualStyleBackColor = true;
            // 
            // tabPageRuns
            // 
            this.tabPageRuns.Location = new System.Drawing.Point(4, 24);
            this.tabPageRuns.Name = "tabPageRuns";
            this.tabPageRuns.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageRuns.Size = new System.Drawing.Size(468, 496);
            this.tabPageRuns.TabIndex = 1;
            this.tabPageRuns.Text = "Runs";
            this.tabPageRuns.UseVisualStyleBackColor = true;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Job name";
            this.columnHeader1.Width = 200;
            // 
            // AllJobsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(769, 527);
            this.Controls.Add(this.splitContainer1);
            this.Name = "AllJobsForm";
            this.Text = "AllJobsForm";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControlJobInfo.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView listViewJobs;
        private System.Windows.Forms.TabControl tabControlJobInfo;
        private System.Windows.Forms.TabPage tabPageGeneral;
        private System.Windows.Forms.TabPage tabPageRuns;
        private System.Windows.Forms.ColumnHeader columnHeader1;
    }
}