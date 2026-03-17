namespace PicoLauncher.Core.Extensions;

public static class ImageExtension
{
    public static List<string> GetImages(this string path, bool AllDir = false, params string[] fileExtensions)
    {

        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));

        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException(path);

        if (fileExtensions == null || fileExtensions.Length == 0)
            return [];

        var collection = fileExtensions.Select(e => e.StartsWith('.') ? e : "." + e);
        var extensions = new HashSet<string>(collection, StringComparer.OrdinalIgnoreCase);

        if (AllDir)
        {
            return [.. Directory
            .EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
            .Where(file => extensions.Contains(Path.GetExtension(file)))];
        }
        else
        {
            return [.. Directory
            .EnumerateFiles(path)
            .Where(file => extensions.Contains(Path.GetExtension(file)))];
        }

    }
}
