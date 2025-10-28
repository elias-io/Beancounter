namespace Beancounter.Extension;

public static class FileInfo_Extension {
    public static string GetNameWithoutExtension(this FileInfo fileInfo) {
        return fileInfo.Name[0..^fileInfo.Extension.Length];
    }
}