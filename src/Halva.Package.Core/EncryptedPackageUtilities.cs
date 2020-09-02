using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Halva.Package.Core
{
    public class EncryptedPackageUtilities
    {
        public static string GetFolderCharacter()
        {
            return (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ? "\\" : "/";
        }
        public static readonly string TempArchive = Path.Combine(Path.GetTempPath(), "TempArchive.tmp");

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

        public static void CreateArchiveFromFolder(in string input, in string archiveLocation, in string password)
        {
            if (File.Exists(TempArchive)) File.Delete(TempArchive);
            ZipFile.CreateFromDirectory(input, TempArchive, CompressionLevel.NoCompression, false);
            CompressArchive(TempArchive, archiveLocation, password);
            File.Delete(TempArchive);
        }

        public static void ExportFromArchive(in string inputArchive, in string destination, in string password)
        {
            if (File.Exists(TempArchive)) File.Delete(TempArchive);
            DecompressArchive(inputArchive, password);
            ZipFile.ExtractToDirectory(TempArchive, destination, true);
            File.Delete(TempArchive);

        }
    }
}
