using PicoLauncher.Core;
using PicoLauncher.Core.Models;

namespace PicoLauncher
{
    public partial class MainPicoLauncher : Form
    {
        private bool isUpdatingConfig = false;
        private MConfig config = null;

        private MConfig CreateConfig()
        {
            return new MConfig()
            {
                SourcePath = txtSrc.Text,
                DestinationPath = txtDest.Text,
                RomsPath = txtRoms.Text,
                CleanDestination = chkClean.Checked,
                OverwriteExisting = chkOverwrite.Checked,
                OpenDestinationAfter = chkOpenDir.Checked
            };
        }

        public MainPicoLauncher()
        {
            InitializeComponent();
            UpdateUI();
        }


        private void UpdateUI()
        {
            bool c = chkClean.Checked;
            txtRoms.Enabled = btnBrowseRoms.Enabled = c;
            lblRomsTitle.ForeColor = c ? Color.Silver : Color.FromArgb(60, 60, 65);
            chkClean.ForeColor = chkClean.Checked ? neonBlue : Color.White;
            chkOpenDir.ForeColor = chkOpenDir.Checked ? neonBlue : Color.White;
            chkOverwrite.ForeColor = chkOverwrite.Checked ? neonBlue : Color.White;
        }

        private void Textbox_TextChanged(object sender, EventArgs e)
        {
            if (isUpdatingConfig) return;
            config = CreateConfig();
            CoreLauncher.SaveConfig(Resources.Resources.ConfigurationFilename, config);
        }

        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var cb = sender as CheckBox;
            UpdateUI();
            if (isUpdatingConfig) return;

            CoreLauncher.SaveConfig(Resources.Resources.ConfigurationFilename, config);
            CoreLauncher.PlayCustomSound(cb.Checked ? Resources.Resources.Select : Resources.Resources.Remove);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            config = CoreLauncher.LoadConfig(Resources.Resources.ConfigurationFilename);
            if (config != null)
            {
                isUpdatingConfig = true;
                txtSrc.Text = config.SourcePath;
                txtDest.Text = config.DestinationPath;
                txtRoms.Text = config.RomsPath;
                chkClean.Checked = config.CleanDestination;
                chkOverwrite.Checked = config.OverwriteExisting;
                chkOpenDir.Checked = config.OpenDestinationAfter;
                isUpdatingConfig = false;
            }
        }

        private void btnBrowseSrc_Click(object sender, EventArgs e)
        {
            config = CreateConfig();

            if (config == null) return;

            CoreLauncher.OpenDialog(txtSrc, Resources.Resources.ConfigurationFilename, config);
        }

        private void btnBrowseDest_Click(object sender, EventArgs e)
        {
            config = CreateConfig();

            if (config == null) return;

            CoreLauncher.OpenDialog(txtDest, Resources.Resources.ConfigurationFilename, config);
        }

        private void btnBrowseRoms_Click(object sender, EventArgs e)
        {
            config = CreateConfig();

            if (config == null) return;

            CoreLauncher.OpenDialog(txtRoms, Resources.Resources.ConfigurationFilename, config);
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            config = CreateConfig();
            try
            {
                btnStart.Enabled = false;
                btnStart.BackColor = Color.DimGray;
                btnStart.ForeColor = Color.White;
                var result = await CoreLauncher.Convert(btnStart, lblStatus, progressBar, pbPreview, config);
                switch (result)
                {
                    case CoreLauncher.ConversionStatus.Success:
                        {
                            btnStart.Text = "START PROCESS";
                            lblStatus.Text = "SUCCESSFULLY COMPLETED";
                            lblStatus.ForeColor = Color.LawnGreen;
                            CoreLauncher.ResetPreview(pbPreview);
                            CoreLauncher.PlayCustomSound(Resources.Resources.Completed);

                            if (config.OpenDestinationAfter)
                            {
                                CoreLauncher.OpenDestination(config);
                            }
                            break;
                        }
                    case CoreLauncher.ConversionStatus.NoFilesToProcess:
                        lblStatus.Text = "No files to process.";
                        break;
                    case CoreLauncher.ConversionStatus.InvalidDirectories:
                        lblStatus.Text = "Invalid directories. Please check your paths.";
                        break;
                    case CoreLauncher.ConversionStatus.Error:
                        lblStatus.Text = "An error occurred during conversion.";
                        break;
                }
                await Task.Delay(2000);
                lblStatus.Text = Resources.Resources.System_Ready;
                btnStart.BackColor = Color.DeepSkyBlue;
                btnStart.ForeColor = Color.Black;
                btnStart.Enabled = true;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "An unexpected error occurred.";
                MessageBox.Show($"An unexpected error occurred:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnMin_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
