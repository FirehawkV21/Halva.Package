using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Halva.Package.Core.Utilities
{
    /// <summary>
    /// A set of utilities for simple workloads (when working with encrypted archives).
    /// </summary>
    public static class EncryptedPackageUtilities
    {
        /// <summary>
        /// The location of the temporary archive.
        /// </summary>
        public static readonly string TempArchive = Path.Combine(Path.GetTempPath(), "TempArchive_");

        /// <summary>
        /// Compresses the encrypted archive.
        /// </summary>
        /// <param name="inputArchive">The input archive.</param>
        /// <param name="outputArchive">The output archive.</param>
        /// <param name="password">The archive's password.</param>
        public static void CompressArchive(in string inputArchive, in string outputArchive, in string password)
        {

            Aes encryptionKit = Aes.Create();
            encryptionKit.KeySize = 256;
            encryptionKit.Padding = PaddingMode.PKCS7;
            byte[] hashCode = SHA512.HashData(Encoding.UTF8.GetBytes(password));
            var key = new Rfc2898DeriveBytes(password, hashCode, 50000, HashAlgorithmName.SHA512);
            encryptionKit.Key = key.GetBytes(encryptionKit.KeySize / 8);
            encryptionKit.IV = key.GetBytes(encryptionKit.BlockSize / 8);
            key.Dispose();
            using (FileStream inputStream = File.OpenRead(inputArchive))
            using (FileStream outputStream = File.Create(outputArchive))
            using (CryptoStream cryptStream = new(outputStream, encryptionKit.CreateEncryptor(), CryptoStreamMode.Write))
            using (BrotliStream compressorStream = new(cryptStream, CompressionLevel.Optimal))
            {
                inputStream.CopyTo(compressorStream);
            }
            encryptionKit.Dispose();
        }

        /// <summary>
        /// Decompresses the archive.
        /// </summary>
        /// <param name="inputArchive">The input archive.</param>
        /// <param name="password">The password of the archive.</param>
        /// <param name="workerArchive">The location for the temp file (that will hold the decompressed archive).</param>
        public static void DecompressArchive(in string inputArchive, in string password, in string workerArchive)
        {
            Aes encryptionKit = Aes.Create();
            encryptionKit.KeySize = 256;
            encryptionKit.Padding = PaddingMode.PKCS7;
            byte[] hashCode = SHA512.HashData(Encoding.UTF8.GetBytes(password));
            var key = new Rfc2898DeriveBytes(password, hashCode, 50000, HashAlgorithmName.SHA512);
            encryptionKit.Key = key.GetBytes(encryptionKit.KeySize / 8);
            encryptionKit.IV = key.GetBytes(encryptionKit.BlockSize / 8);
            key.Dispose();
            using (FileStream inputStream = File.OpenRead(inputArchive))
            using (FileStream outputStream = File.Create(workerArchive))
            using (CryptoStream cryptStream = new(inputStream, encryptionKit.CreateDecryptor(), CryptoStreamMode.Read))
            using (BrotliStream decompressorStream = new(cryptStream, CompressionMode.Decompress))
            {
                decompressorStream.CopyTo(outputStream);
            }
            encryptionKit.Dispose();
        }

        /// <summary>
        /// Creates a Halva package from a folder.
        /// </summary>
        /// <param name="input">The source folder that the archive will be created from.</param>
        /// <param name="archiveLocation">The location of the final archive.</param>
        /// <param name="password">The archive's password.</param>
        public static void CreateArchiveFromFolder(in string input, in string archiveLocation, in string password)
        {
            Random random = new();
            string archive = TempArchive + random.Next(9999) + ".tmp";
            if (File.Exists(archive)) File.Delete(archive);
            ZipFile.CreateFromDirectory(input, archive, CompressionLevel.NoCompression, false);
            CompressArchive(archive, archiveLocation, password);
            File.Delete(archive);
        }

        /// <summary>
        /// Exports all files from an encrypted Halva package.
        /// </summary>
        /// <param name="inputArchive">The archive you want to decompress.</param>
        /// <param name="exportDestination">The location where the contents of the archive will be exported.</param>
        /// <param name="password">The archive's password.</param>
        public static void ExportFromArchive(in string inputArchive, in string exportDestination, in string password)
        {
            Random random = new();
            string archive = TempArchive + random.Next(9999) + ".tmp";
            if (File.Exists(archive)) File.Delete(archive);
            DecompressArchive(inputArchive, password, archive);
            ZipFile.ExtractToDirectory(archive, exportDestination, true);
            File.Delete(archive);

        }
    }
}
