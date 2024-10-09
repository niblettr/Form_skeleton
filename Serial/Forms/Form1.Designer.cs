namespace Serial
{
    partial class MainFormSerial
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainFormSerial));
            this.MainForm_SplitC = new System.Windows.Forms.SplitContainer();
            this.MainRealEstate_SplitC = new System.Windows.Forms.SplitContainer();
            this.LeftRight_SplitC = new System.Windows.Forms.SplitContainer();
            this.MacroSplitPannel = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.MainForm_SplitC)).BeginInit();
            this.MainForm_SplitC.Panel2.SuspendLayout();
            this.MainForm_SplitC.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MainRealEstate_SplitC)).BeginInit();
            this.MainRealEstate_SplitC.Panel1.SuspendLayout();
            this.MainRealEstate_SplitC.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LeftRight_SplitC)).BeginInit();
            this.LeftRight_SplitC.Panel1.SuspendLayout();
            this.LeftRight_SplitC.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MacroSplitPannel)).BeginInit();
            this.MacroSplitPannel.Panel2.SuspendLayout();
            this.MacroSplitPannel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainForm_SplitC
            // 
            this.MainForm_SplitC.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.MainForm_SplitC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainForm_SplitC.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.MainForm_SplitC.Location = new System.Drawing.Point(0, 0);
            this.MainForm_SplitC.Name = "MainForm_SplitC";
            this.MainForm_SplitC.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // MainForm_SplitC.Panel1
            // 
            this.MainForm_SplitC.Panel1.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.MainForm_SplitC.Panel1.CausesValidation = false;
            this.MainForm_SplitC.Panel1MinSize = 1;
            // 
            // MainForm_SplitC.Panel2
            // 
            this.MainForm_SplitC.Panel2.Controls.Add(this.MainRealEstate_SplitC);
            this.MainForm_SplitC.Panel2MinSize = 1;
            this.MainForm_SplitC.Size = new System.Drawing.Size(1511, 608);
            this.MainForm_SplitC.SplitterDistance = 115;
            this.MainForm_SplitC.SplitterWidth = 2;
            this.MainForm_SplitC.TabIndex = 0;
            this.MainForm_SplitC.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.TopSplitterMoved);
            this.MainForm_SplitC.DoubleClick += new System.EventHandler(this.MainForm_SplitC_DoubleClick);
            // 
            // MainRealEstate_SplitC
            // 
            this.MainRealEstate_SplitC.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.MainRealEstate_SplitC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainRealEstate_SplitC.Location = new System.Drawing.Point(0, 0);
            this.MainRealEstate_SplitC.Name = "MainRealEstate_SplitC";
            this.MainRealEstate_SplitC.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // MainRealEstate_SplitC.Panel1
            // 
            this.MainRealEstate_SplitC.Panel1.Controls.Add(this.LeftRight_SplitC);
            this.MainRealEstate_SplitC.Size = new System.Drawing.Size(1511, 491);
            this.MainRealEstate_SplitC.SplitterDistance = 392;
            this.MainRealEstate_SplitC.SplitterWidth = 2;
            this.MainRealEstate_SplitC.TabIndex = 0;
            // 
            // LeftRight_SplitC
            // 
            this.LeftRight_SplitC.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LeftRight_SplitC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LeftRight_SplitC.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.LeftRight_SplitC.Location = new System.Drawing.Point(0, 0);
            this.LeftRight_SplitC.Margin = new System.Windows.Forms.Padding(0);
            this.LeftRight_SplitC.Name = "LeftRight_SplitC";
            // 
            // LeftRight_SplitC.Panel1
            // 
            this.LeftRight_SplitC.Panel1.Controls.Add(this.MacroSplitPannel);
            this.LeftRight_SplitC.Panel1MinSize = 0;
            // 
            // LeftRight_SplitC.Panel2
            // 
            this.LeftRight_SplitC.Panel2.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.LeftRight_SplitC.Panel2.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.LeftRight_SplitC.Panel2MinSize = 0;
            this.LeftRight_SplitC.Size = new System.Drawing.Size(1511, 392);
            this.LeftRight_SplitC.SplitterDistance = 1279;
            this.LeftRight_SplitC.SplitterWidth = 3;
            this.LeftRight_SplitC.TabIndex = 0;
            this.LeftRight_SplitC.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.LeftRightSplitterChanged);
            this.LeftRight_SplitC.DoubleClick += new System.EventHandler(this.LeftRightSplitterDoubleClick);
            // 
            // MacroSplitPannel
            // 
            this.MacroSplitPannel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.MacroSplitPannel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MacroSplitPannel.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.MacroSplitPannel.Location = new System.Drawing.Point(0, 0);
            this.MacroSplitPannel.Margin = new System.Windows.Forms.Padding(1);
            this.MacroSplitPannel.Name = "MacroSplitPannel";
            this.MacroSplitPannel.Panel1MinSize = 1;
            // 
            // MacroSplitPannel.Panel2
            // 
            this.MacroSplitPannel.Panel2MinSize = 1;
            this.MacroSplitPannel.Size = new System.Drawing.Size(1279, 392);
            this.MacroSplitPannel.SplitterDistance = 250;
            this.MacroSplitPannel.TabIndex = 0;
            this.MacroSplitPannel.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.MacroSplitterMoved);
            this.MacroSplitPannel.DoubleClick += new System.EventHandler(this.MacroSplitPannel_DoubleClick);
            // 
            // MainFormSerial
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(1511, 608);
            this.Controls.Add(this.MainForm_SplitC);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.Name = "MainFormSerial";
            this.Text = "Nibble Cereal v4.75 R.Niblett 2021-2024";
            this.MainForm_SplitC.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MainForm_SplitC)).EndInit();
            this.MainForm_SplitC.ResumeLayout(false);
            this.MainRealEstate_SplitC.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MainRealEstate_SplitC)).EndInit();
            this.MainRealEstate_SplitC.ResumeLayout(false);
            this.LeftRight_SplitC.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.LeftRight_SplitC)).EndInit();
            this.LeftRight_SplitC.ResumeLayout(false);
            this.MacroSplitPannel.Panel2.ResumeLayout(false);
            this.MacroSplitPannel.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MacroSplitPannel)).EndInit();
            this.MacroSplitPannel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer MainForm_SplitC;
        private System.Windows.Forms.SplitContainer MainRealEstate_SplitC;
        private System.Windows.Forms.SplitContainer LeftRight_SplitC;
        private System.Windows.Forms.SplitContainer MacroSplitPannel;
    }
}

