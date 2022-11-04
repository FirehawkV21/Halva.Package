using System.IO.Compression;
using System.Formats.Tar;

namespace Halva.Package.Core.Utilities;

/// <summary>
/// A set of utilities for simple workloads.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0063:Use simple 'using' statement", Justification = "For safety reasons, using simplified using should be avoided (unless noted otherwise).")]
public static class PackageUtilities
{
    /// <summary>
    /// The location of a temporary archive.
    /// </summary>
    public static readonly string TempArchive = Path.Combine(Path.GetTempPath(), "TempArchive_");

    /// <summary>
    /// Creates a Halva package from a folder.
    /// </summary>
    /// <param name="input">The folder that will be used as source.</param>
    /// <param name="archiveLocation">The location of the package.</param>
    public static void CreateArchiveFromFolder(in string input, in string archiveLocation)
    {
        Random random = new();
        string archive = TempArchive + random.Next(9999) + ".tmp";
        if (File.Exists(archive)) File.Delete(archive);
        TarFile.CreateFromDirectory(input, archive, false);
        //ZipFile.CreateFromDirectory(input, archive, CompressionLevel.NoCompression, false);
        CompressArchive(TempArchive, archiveLocation);
        File.Delete(TempArchive);
    }

    /// <summary>
    /// Compresses the archive.
    /// </summary>
    /// <param name="inputArchive">The input archive.</param>
    /// <param name="outputArchive">The output archive.</param>
    public static void CompressArchive(in string inputArchive, in string outputArchive) => CompressArchive(inputArchive, outputArchive, CompressionLevel.Optimal);


    /// <summary>
    /// Compresses the archive, with a selected compression level.
    /// </summary>
    /// <param name="inputArchive"></param>
    /// <param name="outputArchive"></param>
    /// <param name="Compression"></param>
    public static void CompressArchive(in string inputArchive, in string outputArchive, CompressionLevel Compression)
    {
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(outputArchive))
        using (BrotliStream compressorStream = new(outputStream, Compression))
        {
            inputStream.CopyTo(compressorStream);
        }
    }

    /// <summary>
    /// Exports all files from a Halva package.
    /// </summary>
    /// <param name="inputArchive">The Halva package for input.</param>
    /// <param name="destination">The location for extracting the files.</param>
    public static void ExportFromArchive(in string inputArchive, in string destination)
    {
        Random random = new();
        string archive = TempArchive + random.Next(9999) + ".tmp";
        if (File.Exists(archive)) File.Delete(archive);
        DecompressArchive(inputArchive, archive);
        TarFile.ExtractToDirectory(archive, destination, true);
        File.Delete(archive);

    }

    /// <summary>
    /// Decompresses the archive.
    /// </summary>
    /// <param name="inputArchive">The input archive.</param>
    /// <param name="workerArchive">The location for the temp file (that will hold the decompressed archive).</param>
    public static void DecompressArchive(string inputArchive, string workerArchive)
    {
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(workerArchive))
        using (BrotliStream decompressorStream = new(inputStream, CompressionMode.Decompress))
        {
            decompressorStream.CopyTo(outputStream);
        }
    }
}
