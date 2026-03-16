namespace PicoLauncher.Core.Models;

public class MConfig
{
    public string SourcePath { get; set; } = string.Empty;
    public string DestinationPath { get; set; } = string.Empty;
    public string RomsPath { get; set; } = string.Empty;
    public bool CleanDestination { get; set; } = false;
    public bool OverwriteExisting { get; set; } = false;
    public bool OpenDestinationAfter { get; set; } = false;

    public string[] ToLines()
    {
        return
        [
            SourcePath,
            DestinationPath,
            RomsPath,
            CleanDestination.ToString(),
            OverwriteExisting.ToString(),
            OpenDestinationAfter.ToString()
        ];
    }
}
