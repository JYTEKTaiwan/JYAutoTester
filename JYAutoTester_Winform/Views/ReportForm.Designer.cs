namespace JYAutoTester_Winform.Views
{
    partial class ReportForm
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.dToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.localPath_toolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.otherPath_toolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cSVToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.filaName_StripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tableLayoutPanel1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.menuStrip1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.dataGridView1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1333, 865);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // menuStrip1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.menuStrip1, 2);
            this.menuStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dToolStripMenuItem,
            this.wToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1333, 70);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // dToolStripMenuItem
            // 
            this.dToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.dToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.localPath_toolStripMenuItem,
            this.otherPath_toolStripMenuItem});
            this.dToolStripMenuItem.Name = "dToolStripMenuItem";
            this.dToolStripMenuItem.Size = new System.Drawing.Size(79, 66);
            this.dToolStripMenuItem.Text = "Load...";
            this.dToolStripMenuItem.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.dToolStripMenuItem.Click += new System.EventHandler(this.dToolStripMenuItem_Click);
            // 
            // localPath_toolStripMenuItem
            // 
            this.localPath_toolStripMenuItem.Name = "localPath_toolStripMenuItem";
            this.localPath_toolStripMenuItem.Size = new System.Drawing.Size(270, 34);
            this.localPath_toolStripMenuItem.Text = ".\\report.txt";
            this.localPath_toolStripMenuItem.Click += new System.EventHandler(this.localPath_toolStripMenuItem_Click);
            // 
            // otherPath_toolStripMenuItem
            // 
            this.otherPath_toolStripMenuItem.Name = "otherPath_toolStripMenuItem";
            this.otherPath_toolStripMenuItem.Size = new System.Drawing.Size(270, 34);
            this.otherPath_toolStripMenuItem.Text = "other...";
            // 
            // wToolStripMenuItem
            // 
            this.wToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cSVToolStripMenuItem});
            this.wToolStripMenuItem.Name = "wToolStripMenuItem";
            this.wToolStripMenuItem.Size = new System.Drawing.Size(102, 66);
            this.wToolStripMenuItem.Text = "Save As...";
            this.wToolStripMenuItem.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // cSVToolStripMenuItem
            // 
            this.cSVToolStripMenuItem.Name = "cSVToolStripMenuItem";
            this.cSVToolStripMenuItem.Size = new System.Drawing.Size(270, 34);
            this.cSVToolStripMenuItem.Text = "CSV";
            this.cSVToolStripMenuItem.Click += new System.EventHandler(this.cSVToolStripMenuItem_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tableLayoutPanel1.SetColumnSpan(this.dataGridView1, 2);
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(3, 73);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 62;
            this.dataGridView1.RowTemplate.Height = 33;
            this.dataGridView1.Size = new System.Drawing.Size(1327, 789);
            this.dataGridView1.TabIndex = 1;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.filaName_StripStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 837);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1333, 28);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // filaName_StripStatusLabel
            // 
            this.filaName_StripStatusLabel.Name = "filaName_StripStatusLabel";
            this.filaName_StripStatusLabel.Size = new System.Drawing.Size(0, 21);
            // 
            // ReportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1333, 865);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.Name = "ReportForm";
            this.Text = "Report View Panel";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ReportViewer_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ReportForm_FormClosed);
            this.Shown += new System.EventHandler(this.ReportForm_Shown);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem dToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wToolStripMenuItem;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.ToolStripMenuItem localPath_toolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem otherPath_toolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cSVToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel filaName_StripStatusLabel;
    }
}