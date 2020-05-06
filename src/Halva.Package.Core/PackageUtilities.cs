using System.IO;
using System.IO.Compression;

namespace Halva.Package.Core
{
    public static class PackageUtilities
    {
        public static readonly string TempArchive = Path.Combine(Path.GetTempPath(), "TempArchive.tmp");
        public static void CreateArchiveFromFolder(in string input, in string archiveLocation)
        {
            if (File.Exists(TempArchive)) File.Delete(TempArchive);
            ZipFile.CreateFromDirectory(input, TempArchive, CompressionLevel.NoCompression, false);
            CompressArchive(TempArchive, archiveLocation);
            File.Delete(TempArchive);
        }
        public static void CompressArchive(in string inputArchive, in string outputArchive)
        {
            using (FileStream inputStream = File.OpenRead(inputArchive))
            using (FileStream outputStream = File.Create(outputArchive))
            using (BrotliStream compressorStream = new BrotliStream(outputStream, CompressionLevel.Optimal))
            {
                inputStream.CopyTo(compressorStream);
            }
        }

        public static void ExportFromArchive(in string inputArchive, in string destination)
        {
            if (File.Exists(TempArchive)) File.Delete(TempArchive);
            DecompressArchive(inputArchive);
            ZipFile.ExtractToDirectory(TempArchive, destination, true);
            File.Delete(TempArchive);

        }

        public static void ExportFromArchive(in MemoryStream inputMemoryStream)
        {

        }

        public static void DecompressArchive(in string inputArchive)
        {
            using (FileStream inputStream = File.OpenRead(inputArchive))
            using (FileStream outputStream = File.Create(TempArchive))
            using (BrotliStream decompressorStream = new BrotliStream(inputStream, CompressionMode.Decompress))
            {
                decompressorStream.CopyTo(outputStream);
            }
        }
    }
}
