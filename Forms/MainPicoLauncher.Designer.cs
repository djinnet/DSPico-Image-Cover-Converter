using PicoLauncher.Core.Controls;

namespace PicoLauncher
{
    partial class MainPicoLauncher
    {
        private Color neonBlue = Color.DeepSkyBlue;
        private Color darkBg = Color.FromArgb(25, 25, 28);
        private Color darkInput = Color.FromArgb(40, 40, 45);
        private Color btnGray = Color.FromArgb(55, 55, 60);

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainPicoLauncher));
            btnMin = new Button();
            btnClose = new Button();
            lblOrigin = new Label();
            txtSrc = new TextBox();
            btnBrowseSrc = new Button();
            lbldest = new Label();
            chkClean = new CheckBox();
            lblRomsTitle = new Label();
            txtRoms = new TextBox();
            btnBrowseRoms = new Button();
            chkOpenDir = new CheckBox();
            chkOverwrite = new CheckBox();
            pbPreview = new PictureBox();
            lblStatus = new Label();
            btnStart = new Button();
            txtDest = new TextBox();
            btnBrowseDest = new Button();
            progressBar = new AnimatedProgressBar();
            lblTitles = new AnimatedLabel();
            ((System.ComponentModel.ISupportInitialize)pbPreview).BeginInit();
            SuspendLayout();
            // 
            // btnMin
            // 
            btnMin.BackColor = Color.FromArgb(55, 55, 60);
            btnMin.Cursor = Cursors.Hand;
            btnMin.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(btnMin, "btnMin");
            btnMin.ForeColor = Color.White;
            btnMin.Name = "btnMin";
            btnMin.UseVisualStyleBackColor = false;
            btnMin.Click += BtnMin_Click;
            // 
            // btnClose
            // 
            btnClose.BackColor = Color.FromArgb(100, 30, 30);
            btnClose.Cursor = Cursors.Hand;
            btnClose.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(btnClose, "btnClose");
            btnClose.ForeColor = Color.White;
            btnClose.Name = "btnClose";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += BtnClose_Click;
            // 
            // lblOrigin
            // 
            resources.ApplyResources(lblOrigin, "lblOrigin");
            lblOrigin.ForeColor = Color.Gray;
            lblOrigin.Name = "lblOrigin";
            // 
            // txtSrc
            // 
            txtSrc.BackColor = Color.FromArgb(40, 40, 45);
            txtSrc.BorderStyle = BorderStyle.FixedSingle;
            resources.ApplyResources(txtSrc, "txtSrc");
            txtSrc.ForeColor = Color.White;
            txtSrc.Name = "txtSrc";
            txtSrc.ReadOnly = true;
            txtSrc.TextChanged += Textbox_TextChanged;
            // 
            // btnBrowseSrc
            // 
            btnBrowseSrc.BackColor = Color.FromArgb(55, 55, 60);
            btnBrowseSrc.Cursor = Cursors.Hand;
            btnBrowseSrc.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(btnBrowseSrc, "btnBrowseSrc");
            btnBrowseSrc.ForeColor = Color.White;
            btnBrowseSrc.Name = "btnBrowseSrc";
            btnBrowseSrc.Tag = "src";
            btnBrowseSrc.UseVisualStyleBackColor = false;
            btnBrowseSrc.Click += BtnBrowse_Click;
            // 
            // lbldest
            // 
            resources.ApplyResources(lbldest, "lbldest");
            lbldest.ForeColor = Color.Gray;
            lbldest.Name = "lbldest";
            // 
            // chkClean
            // 
            chkClean.Cursor = Cursors.Hand;
            resources.ApplyResources(chkClean, "chkClean");
            chkClean.Name = "chkClean";
            chkClean.CheckedChanged += CheckBox_CheckedChanged;
            // 
            // lblRomsTitle
            // 
            resources.ApplyResources(lblRomsTitle, "lblRomsTitle");
            lblRomsTitle.ForeColor = Color.Gray;
            lblRomsTitle.Name = "lblRomsTitle";
            // 
            // txtRoms
            // 
            txtRoms.BackColor = Color.FromArgb(40, 40, 45);
            txtRoms.BorderStyle = BorderStyle.FixedSingle;
            resources.ApplyResources(txtRoms, "txtRoms");
            txtRoms.ForeColor = Color.White;
            txtRoms.Name = "txtRoms";
            txtRoms.ReadOnly = true;
            txtRoms.TextChanged += Textbox_TextChanged;
            // 
            // btnBrowseRoms
            // 
            btnBrowseRoms.BackColor = Color.FromArgb(55, 55, 60);
            btnBrowseRoms.Cursor = Cursors.Hand;
            btnBrowseRoms.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(btnBrowseRoms, "btnBrowseRoms");
            btnBrowseRoms.ForeColor = Color.White;
            btnBrowseRoms.Name = "btnBrowseRoms";
            btnBrowseRoms.Tag = "roms";
            btnBrowseRoms.UseVisualStyleBackColor = false;
            btnBrowseRoms.Click += BtnBrowse_Click;
            // 
            // chkOpenDir
            // 
            chkOpenDir.Cursor = Cursors.Hand;
            resources.ApplyResources(chkOpenDir, "chkOpenDir");
            chkOpenDir.Name = "chkOpenDir";
            chkOpenDir.CheckedChanged += CheckBox_CheckedChanged;
            // 
            // chkOverwrite
            // 
            chkOverwrite.Cursor = Cursors.Hand;
            resources.ApplyResources(chkOverwrite, "chkOverwrite");
            chkOverwrite.Name = "chkOverwrite";
            chkOverwrite.CheckedChanged += CheckBox_CheckedChanged;
            // 
            // pbPreview
            // 
            pbPreview.BackColor = Color.Transparent;
            pbPreview.Image = Resources.Resources.dspico_Image_logo;
            resources.ApplyResources(pbPreview, "pbPreview");
            pbPreview.Name = "pbPreview";
            pbPreview.TabStop = false;
            // 
            // lblStatus
            // 
            resources.ApplyResources(lblStatus, "lblStatus");
            lblStatus.ForeColor = Color.Silver;
            lblStatus.Name = "lblStatus";
            // 
            // btnStart
            // 
            btnStart.BackColor = Color.DeepSkyBlue;
            btnStart.Cursor = Cursors.Hand;
            btnStart.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(btnStart, "btnStart");
            btnStart.ForeColor = Color.Black;
            btnStart.Name = "btnStart";
            btnStart.UseVisualStyleBackColor = false;
            btnStart.Click += BtnStart_Click;
            // 
            // txtDest
            // 
            txtDest.BackColor = Color.FromArgb(40, 40, 45);
            txtDest.BorderStyle = BorderStyle.FixedSingle;
            resources.ApplyResources(txtDest, "txtDest");
            txtDest.ForeColor = Color.White;
            txtDest.Name = "txtDest";
            txtDest.ReadOnly = true;
            txtDest.TextChanged += Textbox_TextChanged;
            // 
            // btnBrowseDest
            // 
            btnBrowseDest.BackColor = Color.FromArgb(55, 55, 60);
            btnBrowseDest.Cursor = Cursors.Hand;
            btnBrowseDest.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(btnBrowseDest, "btnBrowseDest");
            btnBrowseDest.ForeColor = Color.White;
            btnBrowseDest.Name = "btnBrowseDest";
            btnBrowseDest.Tag = "dest";
            btnBrowseDest.UseVisualStyleBackColor = false;
            btnBrowseDest.Click += BtnBrowse_Click;
            // 
            // progressBar
            // 
            progressBar.AnimationSpeed = 5;
            progressBar.AnimationStyle = AnimationStyle.Smooth;
            progressBar.BackgroundBarColor = Color.FromArgb(55, 55, 60);
            progressBar.BorderColor = Color.FromArgb(55, 55, 60);
            progressBar.CustomText = "";
            resources.ApplyResources(progressBar, "progressBar");
            progressBar.Name = "progressBar";
            progressBar.ProgressColor = Color.DeepSkyBlue;
            progressBar.ShowText = false;
            progressBar.TextMode = ProgressTextMode.Percentage;
            // 
            // lblTitles
            // 
            resources.ApplyResources(lblTitles, "lblTitles");
            lblTitles.ForeColor = Color.DeepSkyBlue;
            lblTitles.Name = "lblTitles";
            lblTitles.Titles.Add("DSPico Converter v4.2");
            lblTitles.Titles.Add("Rikisoft 2026");
            // 
            // MainPicoLauncher
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(25, 25, 28);
            Controls.Add(lblTitles);
            Controls.Add(btnMin);
            Controls.Add(btnClose);
            Controls.Add(lblOrigin);
            Controls.Add(txtSrc);
            Controls.Add(lbldest);
            Controls.Add(txtDest);
            Controls.Add(btnBrowseSrc);
            Controls.Add(btnBrowseDest);
            Controls.Add(chkClean);
            Controls.Add(lblRomsTitle);
            Controls.Add(txtRoms);
            Controls.Add(btnBrowseRoms);
            Controls.Add(chkOpenDir);
            Controls.Add(chkOverwrite);
            Controls.Add(pbPreview);
            Controls.Add(progressBar);
            Controls.Add(lblStatus);
            Controls.Add(btnStart);
            ForeColor = Color.White;
            FormBorderStyle = FormBorderStyle.None;
            Icon = Resources.Resources.dspico_logo;
            Name = "MainPicoLauncher";
            ((System.ComponentModel.ISupportInitialize)pbPreview).EndInit();
            ResumeLayout(false);
            PerformLayout();


        }


        private TextBox txtSrc, txtRoms;
        private Button btnStart, btnBrowseSrc, btnBrowseDst, btnBrowseRoms, btnClose, btnMin;
        private CheckBox chkClean, chkOverwrite, chkOpenDir;
        private Label lblStatus, lblRomsTitle;
        private MarqueeLabel lblTitle;
        private PictureBox pbPreview;
        private Label lblOrigin;
        private Label lbldest;
        private TextBox txtDest;
        private Button btnBrowseDest;
        #endregion


        private AnimatedProgressBar progressBar;
        private AnimatedLabel lblTitles;
    }
}