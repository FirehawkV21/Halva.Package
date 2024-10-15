using System.Formats.Tar;
using System.IO.Compression;

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


    public static void CreateArchiveFromFolder(in string input, in string archiveLocation)
    {
        CreateArchive(input, archiveLocation, true);
    }

    /// <summary>
    /// Exports all files from a Halva package.
    /// </summary>
    /// <param name="inputArchive">The Halva package for input.</param>
    /// <param name="destination">The location for extracting the files.</param>
    public static void ExportFromArchive(in string inputArchive, in string destination)
    {
        ExportFiles(inputArchive, destination, true);
    }

    /// <summary>
    /// Compresses the archive.
    /// </summary>
    /// <param name="inputArchive">The input archive.</param>
    /// <param name="outputArchive">The output archive.</param>
    public static void CompressArchive(in MemoryStream inputArchive, in string outputArchive) => CompressArchive(inputArchive, outputArchive, CompressionLevel.Optimal);


    /// <summary>
    /// Compresses the archive, with a selected compression level.
    /// </summary>
    /// <param name="inputArchive"></param>
    /// <param name="outputArchive"></param>
    /// <param name="Compression"></param>
    public static void CompressArchive(in MemoryStream inputArchive, in string outputArchive, CompressionLevel Compression)
    {
        inputArchive.Position = 0;
        using (FileStream outputStream = File.Create(outputArchive))
        using (BrotliStream compressorStream = new(outputStream, Compression))
        {
            inputArchive.CopyTo(compressorStream);
        }
    }

    /// <summary>
    /// Decompresses the archive.
    /// </summary>
    /// <param name="inputStream">The input archive in a stream.</param>
    /// <param name="uncompressedStream">The stream that will accept the uncompressed Stream.</param>
    public static void DecompressArchive(in Stream inputStream, out MemoryStream uncompressedStream)
    {
        inputStream.Position = 0;
        uncompressedStream = new MemoryStream();
        using (BrotliStream decompressorStream = new(inputStream, CompressionMode.Decompress))
        {
            decompressorStream.CopyTo(uncompressedStream);
        }
    }

#if NET8_0_OR_GREATER

    /// <summary>
    /// Creates a Halva package from a folder.
    /// </summary>
    /// <param name="input">The folder that will be used as source.</param>
    /// <param name="archiveLocation">The location of the package.</param>
    /// <param name="useStreams">Use MemoryStream for temp storage. Suitable for smaller archives (less than 4GB).</param>
    public static void CreateArchive(in string input, in string archiveLocation, bool useStreams)
    {
        if(useStreams)
        {
            MemoryStream fileWrite = new();
            TarFile.CreateFromDirectory(input, fileWrite,  false);
            CompressArchive(fileWrite, archiveLocation);
        }
        else
        {
            Random random = new();
            string archive = TempArchive + random.Next(9999) + ".tmp";
            if (File.Exists(archive)) File.Delete(archive);
            TarFile.CreateFromDirectory(input, archive, false);
            CompressArchive(archive, archiveLocation);
            File.Delete(archive);
        }
    }

    /// <summary>
    /// Exports all files from a Halva package.
    /// </summary>
    /// <param name="inputArchive">The Halva package for input.</param>
    /// <param name="destination">The location for extracting the files.</param>
    public static void ExportFiles(in string inputArchive, in string destination, bool useMemoryStreams)
    {
        if (useMemoryStreams)
        {
            MemoryStream stream = new();
            DecompressArchive(File.OpenRead(inputArchive), out stream);
            if (!Directory.Exists(destination)) Directory.CreateDirectory(destination);
            TarFile.ExtractToDirectory(stream, destination, true);
            stream.Close();
        }
        else
        {
            Random random = new();
            string archive = TempArchive + random.Next(9999) + ".tmp";
            if (File.Exists(archive)) File.Delete(archive);
            DecompressArchive(inputArchive, archive);
            if (!Directory.Exists(destination))
            Directory.CreateDirectory(destination);
            TarFile.ExtractToDirectory(archive, destination, true);
            File.Delete(archive);
        }
    }

#endif

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
