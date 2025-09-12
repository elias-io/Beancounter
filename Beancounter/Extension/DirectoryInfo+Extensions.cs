namespace Beancounter.Extension;

public static class DirectoryInfo_Extensions {
    public static DirectoryInfo Add(this DirectoryInfo directoryInfo, string path) {
        return directoryInfo.CreateSubdirectory(path);
    }

    /// <summary>
    /// Retrieves a <see cref="FileInfo"/> object representing the file with the specified name in the given directory.
    /// </summary>
    /// <param name="directoryInfo">The directory to search within.</param>
    /// <param name="fileName">The name of the file to get.</param>
    /// <returns>
    /// A <see cref="FileInfo"/> object for the specified file.
    /// </returns>
    /// <exception cref="Exception">Thrown if the file does not exist.</exception>
    public static FileInfo GetFile(this DirectoryInfo directoryInfo, string fileName) {
        var filePath = Path.Combine(directoryInfo.FullName, fileName);
        var fileInfo = new FileInfo(filePath);
        if (!fileInfo.Exists)
            throw new Exception($"File does not exist. {fileInfo.FullName}");
        return fileInfo;
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