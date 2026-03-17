using PicoLauncher.Core;
using PicoLauncher.Core.Enums;
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
            this.MouseDown += MainPicoLauncher_MouseDown;
            UpdateUI();
        }

        private void MainPicoLauncher_MouseDown(object sender, MouseEventArgs e)
        {
            CoreLauncher.DragWindow(this.Handle);
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

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            string tag = (sender as Button)?.Tag?.ToString();
            if (tag == null) return;
            BtnClick(tag);
        }

        private void BtnClick(string tag)
        {
            config = CreateConfig();

            if (config == null) return;

            TextBox targetTextBox = tag switch
            {
                "src" => txtSrc,
                "dest" => txtDest,
                "roms" => txtRoms,
                _ => null
            };

            CoreLauncher.OpenDialog(targetTextBox, Resources.Resources.ConfigurationFilename, config);
        }

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            config = CreateConfig();
            try
            {
                btnStart.Enabled = false;
                btnStart.BackColor = Color.DimGray;
                btnStart.ForeColor = Color.White;
                ConversionStatus result = await CoreLauncher.Convert(btnStart, lblStatus, progressBar, pbPreview, config);
                switch (result)
                {
                    case ConversionStatus.Success:
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
                    case ConversionStatus.NoFilesToProcess:
                        CoreLauncher.PlayCustomSound(Resources.Resources.Remove);
                        lblStatus.Text = "No files to process.";
                        break;
                    case ConversionStatus.InvalidDirectories:
                        CoreLauncher.PlayCustomSound(Resources.Resources.Remove);
                        lblStatus.Text = "Invalid directories. Please check your paths.";
                        break;
                    case ConversionStatus.Error:
                        CoreLauncher.PlayCustomSound(Resources.Resources.Remove);
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

        private void BtnMin_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
