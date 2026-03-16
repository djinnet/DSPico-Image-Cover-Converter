using PicoLauncher.Core.Extensions;
using PicoLauncher.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ImageMagick;

namespace PicoLauncher.Core;

public class CoreLauncher
{
    [DllImport("user32.dll", EntryPoint = "ReleaseCapture")]
    private extern static void ReleaseCapture();
    [DllImport("user32.dll", EntryPoint = "SendMessage")]
    private extern static void SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

    public static void DragWindow(IntPtr handle)
    {
        ReleaseCapture();
        SendMessage(handle, 0x112, 0xf012, 0);
    }

    public static MConfig LoadConfig(string configurationFilePath)
    {
        try
        {
            MConfig config = new();
            if (!File.Exists(configurationFilePath))
            {
                return config;
            }

            string[] lines = File.ReadAllLines(configurationFilePath);
            if (lines.Length >= 6)
            {
                config.SourcePath = lines[0];
                config.DestinationPath = lines[1];
                config.RomsPath = lines[2];
                config.CleanDestination = lines[3] == "True";
                config.OverwriteExisting = lines[4] == "True";
                config.OpenDestinationAfter = lines[5] == "True";
            }
            return config;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading configuration: {ex.Message}");
            return null;
        }
    }

    public enum ConversionStatus
    {
        Success,
        NoFilesToProcess,
        InvalidDirectories,
        Error
    }

    public static async Task<ConversionStatus> Convert(Button button, Label status,  ProgressBar bar, PictureBox preview,  MConfig config)
    {
        if (!Directory.Exists(config.SourcePath) || !Directory.Exists(config.DestinationPath)) 
        { 
            return ConversionStatus.NoFilesToProcess;
        }

        try
        {
            List<string> toProcess = GetFilesRequiringConversion(config);

            if (toProcess.Count == 0 && !config.CleanDestination)
            {
                return ConversionStatus.NoFilesToProcess;
            }
            
            bar.Value = 0;
            bar.Maximum = toProcess.Count;
            PlayCustomSound(Resources.Resources.Home);
            await Task.Run(() =>
            {
                for (int i = 0; i < toProcess.Count; i++)
                {
                    string file = toProcess[i];
                    using var image = Image.FromFile(file);
                    using var bitmap = new Bitmap(image);
                    string destinationFile = Path.Combine(config.DestinationPath, Path.GetFileNameWithoutExtension(file) + ".bmp");

                    bool resultpreview = AssignImageToPreview(file, preview);

                    // if the preview failed to load, we still want to inform the user about failed preview but continue with the conversion
                    if (resultpreview)
                    {
                        //informed the user about the current file being processed
                        button.Invoke((Action)(() => button.Text = $"PROCESSING: {Path.GetFileName(file)}"));
                        status.Invoke((Action)(() => status.Text = $"CONVERTING: {Path.GetFileName(file)}"));
                    }
                    else
                    {
                        button.Invoke((Action)(() => button.Text = $"PROCESSING: {Path.GetFileName(file)} (Preview Failed)"));
                        status.Invoke((Action)(() => status.Text = $"CONVERTING: {Path.GetFileName(file)} (Preview Failed)"));
                    }

                    bitmap.Save(destinationFile, System.Drawing.Imaging.ImageFormat.Bmp);
                    bar.Invoke((Action)(() => bar.Value++));

                    bool magickresult = UsingMagick(file, destinationFile);

                    if (magickresult)
                    {
                        // if magick succeeded, we inform the user about the successful conversion
                        button.Invoke((Action)(() => button.Text = $"MAGICK HAS PROCESSED: {Path.GetFileName(file)}"));
                        status.Invoke((Action)(() => status.Text = $"MAGICK HAS CONVERTED: {Path.GetFileName(file)}"));
                    }
                    else
                    {
                        // if magick failed, we inform the user about the failed conversion but continue with the next files
                        button.Invoke((Action)(() => button.Text = $"FAILED TO PROCESS: {Path.GetFileName(file)}"));
                        status.Invoke((Action)(() => status.Text = $"FAILED TO CONVERT: {Path.GetFileName(file)}"));
                    }
                }

                if (config.CleanDestination && Directory.Exists(config.RomsPath))
                {
                    //informed the user about the current file being processed
                    button.Invoke((Action)(() => button.Text = $"CLEANING"));
                    status.Invoke((Action)(() => status.Text = $"CLEANING FILES..."));
                    RemoveUnusedBmpFiles(config);
                }
            });
            return ConversionStatus.Success;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error during conversion process: " + ex.Message);
            return ConversionStatus.Error;
        }
    }

    private static void RemoveUnusedBmpFiles(MConfig config)
    {
        List<string> bmps = config.DestinationPath.GetImages(false, ".bmp");

        foreach (var file in bmps)
        {
            var matchingRoms = Directory.GetFiles(config.RomsPath, Path.GetFileNameWithoutExtension(file) + ".*");
            if (matchingRoms.Length == 0)
            {
                try { File.Delete(file); } catch { }
            }
        }
    }

    public static void OpenDestination(MConfig config)
    {
        try
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = config.DestinationPath,
                UseShellExecute = true,
                Verb = "open"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error opening destination folder: {ex.Message}");
        }
    }

    private static bool AssignImageToPreview(string file, PictureBox preview)
    {
        try
        {
            if(preview.Image != null)
            {
                preview.Invoke((Action)(() => preview.Image.Dispose()));
            }

            using var image = Image.FromFile(file);
            preview.Invoke((Action)(() =>
            {
                preview.Image = new Bitmap(image);
            }));
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading image for preview: {ex.Message}");
            return false;
        }
    }

    public static bool ResetPreview(PictureBox preview)
    {
        try
        {
            preview.Invoke((Action)(() =>
            {
                preview.Image = Resources.Resources.dspico_Image_logo;
            }));
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error resetting preview: {ex.Message}");
            return false;
        }
    }

    static readonly QuantizeSettings Quantize256 = new() { Colors = 256 };
    private static bool UsingMagick(string file, string destinationFile)
    {
        try
        {
            using var image = new MagickImage(file);

            // Resize (the ! forces exact size)
            image.Resize(new MagickGeometry(106, 96)
            {
                IgnoreAspectRatio = true
            });

            // Prepare background for extent
            image.BackgroundColor = MagickColors.Black;

            // Extend canvas
            image.Extent(128, 96, Gravity.Northwest);

            // Palette / indexed color
            image.ColorType = ColorType.Palette;

            // Bit depth
            image.Depth = 8;

            // Limit colors
            image.Quantize(Quantize256);

            // No compression
            image.Settings.Compression = CompressionMethod.NoCompression;

            // Force BMP3 format
            image.Format = MagickFormat.Bmp3;

            image.Write(destinationFile);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing file {file} with Magick.NET: {ex.Message}");
            return false;
        }
    }

    private static List<string> GetFilesRequiringConversion(MConfig config)
    {
        try
        {
            // Get all image files in the source directory
            List<string> originFiles = config.SourcePath.GetImages(false, ".png", ".jpg", ".jpeg");

            List<string> toProcess = [];
            foreach (string f in originFiles)
            {
                // Check if the corresponding BMP file already exists in the destination directory
                bool bmpExists = File.Exists(Path.Combine(config.DestinationPath, Path.GetFileNameWithoutExtension(f) + ".bmp"));
                if (config.OverwriteExisting || !bmpExists)
                {
                    toProcess.Add(f);
                }
            }
            return toProcess;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting files to process: {ex.Message}");
            return [];
        }
    }

    public static void OpenDialog(Control ctrl, string path, MConfig config)
    {
        using FolderBrowserDialog f = new();
        if (f.ShowDialog() == DialogResult.OK)
        {
            ctrl.Text = f.SelectedPath;
            SaveConfig(path, config);
        }
    }

    public static async void AnimateTitle(Control ctrl, string targetText)
    {
        ctrl.Text = "";
        int mid = targetText.Length / 2;
        for (int i = 0; i <= mid; i++)
        {
            int start = Math.Max(0, mid - i);
            int end = Math.Min(targetText.Length - 1, mid + i);
            string currentView = targetText.Substring(start, end - start + 1);
            int padding = (targetText.Length - currentView.Length) / 2;
            ctrl.Text = new string(' ', padding) + currentView;
            await Task.Delay(50);
        }
        ctrl.Text = targetText;
    }

    

    public static bool SaveConfig(string configFile, MConfig config) { 
        try { 
            File.WriteAllLines(configFile, config.ToLines());
            return true;
        } catch(Exception ex) 
        {
            Console.WriteLine("Error saving configuration: " + ex.Message);
            return false;
        } 
    }


    public static void PlayCustomSound(Stream s)
    {
        try
        {
            if (s == null)
            {
                return;
            }
            SoundPlayer player = new()
            {
                Stream = s
            };
            player.Play();
        }
        catch (Exception ex) 
        {
            Console.WriteLine("Error playing sound: " + ex.Message);
        }
    }




}
