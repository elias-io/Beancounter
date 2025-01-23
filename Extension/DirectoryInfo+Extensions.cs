namespace Beancounter.Extension;

public static class DirectoryInfo_Extensions {
    public static DirectoryInfo Add(this DirectoryInfo directoryInfo, string path) {
        return directoryInfo.CreateSubdirectory(path);
    }

    /// <summary>
    /// Deletes the specified directory and any subdirectories and files in the directory.
    /// </summary>
    /// <exception cref="T:System.IO.IOException">A file with the same name and location specified by <paramref name="path" /> exists.
    ///
    /// -or-
    ///
    /// The directory specified by <paramref name="path" /> is read-only, or <paramref name="recursive" /> is <see langword="false" /> and <paramref name="path" /> is not an empty directory.
    ///
    /// -or-
    ///
    /// The directory is the application's current working directory.
    ///
    /// -or-
    ///
    /// The directory contains a read-only file.
    ///
    /// -or-
    ///
    /// The directory is being used by another process.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.ArgumentException">.NET Framework and .NET Core versions older than 2.1: <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters by using the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">
    ///        <paramref name="path" /> does not exist or could not be found.
    ///
    /// -or-
    ///
    /// The specified path is invalid (for example, it is on an unmapped drive).</exception>
    public static void Purge(this DirectoryInfo directoryInfo) {
        Directory.Delete(directoryInfo.FullName, true);
    }

}