using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Halva.Package.Core
{
    public static class EncryptedPackageUtilities
    {
        public static readonly string TempArchive = Path.Combine(Path.GetTempPath(), "TempArchive.tmp");

        /// <summary>
        /// Compresses the encrypted archive.
        /// </summary>
        /// <param name="inputArchive">The input archive.</param>
        /// <param name="outputArchive">The output archive.</param>
        /// <param name="password">The archive's password.</param>
        public static void CompressArchive(in string inputArchive, in string outputArchive, in string password)
        {

            AesManaged encryptionKit = new AesManaged
            {
                KeySize = 256, Padding = PaddingMode.PKCS7
            };
            byte[] hashCode;
            using (HashAlgorithm hash = new SHA512Managed())
            {
                hashCode = hash.ComputeHash(Encoding.UTF8.GetBytes(password));
            }

            var key = new Rfc2898DeriveBytes(password, hashCode, 50000, HashAlgorithmName.SHA512);
            encryptionKit.Key = key.GetBytes(encryptionKit.KeySize / 8);
            encryptionKit.IV = key.GetBytes(encryptionKit.BlockSize / 8);
            key.Dispose();
            using (FileStream inputStream = File.OpenRead(inputArchive))
            using (FileStream outputStream = File.Create(outputArchive))
            using (CryptoStream cryptStream = new CryptoStream(outputStream, encryptionKit.CreateEncryptor(), CryptoStreamMode.Write))
            using (BrotliStream compressorStream = new BrotliStream(cryptStream, CompressionLevel.Optimal))
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
        public static void DecompressArchive(in string inputArchive, in string password)
        {
            AesManaged encryptionKit = new AesManaged
            {
                KeySize = 256,
                Padding = PaddingMode.PKCS7
            };
            byte[] hashCode;
            using (HashAlgorithm hash = new SHA512Managed())
            {
                hashCode = hash.ComputeHash(Encoding.UTF8.GetBytes(password));
            }

            var key = new Rfc2898DeriveBytes(password, hashCode, 50000, HashAlgorithmName.SHA512);
            encryptionKit.Key = key.GetBytes(encryptionKit.KeySize / 8);
            encryptionKit.IV = key.GetBytes(encryptionKit.BlockSize / 8);
            key.Dispose();
            using (FileStream inputStream = File.OpenRead(inputArchive))
            using (FileStream outputStream = File.Create(TempArchive))
            using (CryptoStream cryptStream = new CryptoStream(inputStream, encryptionKit.CreateDecryptor(), CryptoStreamMode.Read))
            using (BrotliStream decompressorStream = new BrotliStream(cryptStream, CompressionMode.Decompress))
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
            if (File.Exists(TempArchive)) File.Delete(TempArchive);
            ZipFile.CreateFromDirectory(input, TempArchive, CompressionLevel.NoCompression, false);
            CompressArchive(TempArchive, archiveLocation, password);
            File.Delete(TempArchive);
        }

        /// <summary>
        /// Exports all files from an encrypted Halva package.
        /// </summary>
        /// <param name="inputArchive">The archive you want to decompress.</param>
        /// <param name="exportDestination">The location where the contents of the archive will be exported.</param>
        /// <param name="password">The archive's password.</param>
        public static void ExportFromArchive(in string inputArchive, in string exportDestination, in string password)
        {
            if (File.Exists(TempArchive)) File.Delete(TempArchive);
            DecompressArchive(inputArchive, password);
            ZipFile.ExtractToDirectory(TempArchive, exportDestination, true);
            File.Delete(TempArchive);

        }
    }
}
