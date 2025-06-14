using System.Formats.Tar;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace Halva.Package.Core;
public static class PackageUtilities
{
    #region Creating Packages
    /// <summary>
    /// Creates a Halva package from a folder.
    /// </summary>
    /// <param name="sourceFolder">The folder with the files you want in the package.</param>
    /// <param name="targetPackagePath">The location of the package to save.</param>
    /// <param name="password">The password for the package.</param>
    /// <param name="ivKey">The IV key for the package.</param>
    /// <param name="compression">The level of compression for the package.</param>
    public static void CreatePackageFromFolder(string sourceFolder, string targetPackagePath, string password = "", string ivKey = "", CompressionLevel compression = CompressionLevel.Optimal)
    {
        using (FileStream fs = new(targetPackagePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.SequentialScan))
        {
            if (!string.IsNullOrEmpty(password) && !string.IsNullOrWhiteSpace(password))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    CreateKey(out AesCng cngEncryptionKit, password, ivKey);
                    using (CryptoStream cryptoStream = new(fs, cngEncryptionKit.CreateEncryptor(), CryptoStreamMode.Write))
                    using (BrotliStream CompressionStream = new(cryptoStream, compression))
                    {
                        TarFile.CreateFromDirectory(sourceFolder, CompressionStream, false);
                    }
                }
                else
                {
                    CreateKey(out Aes cngEncryptionKit, password, ivKey);
                    using (CryptoStream cryptoStream = new(fs, cngEncryptionKit.CreateEncryptor(), CryptoStreamMode.Write))
                    using (BrotliStream CompressionStream = new(cryptoStream, compression))
                    {
                        TarFile.CreateFromDirectory(sourceFolder, CompressionStream, false);
                    }
                }
            }
            else
            {
                using (BrotliStream CompressionStream = new(fs, compression))
                {
                    TarFile.CreateFromDirectory(sourceFolder, CompressionStream, false);
                }
            }
        }
    }

    /// <summary>
    /// Creates a Halva package from a folder.
    /// </summary>
    /// <param name="sourceFolder">The folder with the files you want in the package.</param>
    /// <param name="targetPackagePath">The location of the package to save.</param>
    /// <param name="password">The password for the package.</param>
    /// <param name="ivKey">The IV key for the package.</param>
    /// <param name="compression">The level of compression for the package.</param>
    /// <param name="abortToken">The cancellation token to abort the operation.</param>
    public static async Task CreatePackageFromFolderAsync(string sourceFolder, string targetPackagePath, string password ="", string ivKey = "", CompressionLevel compression = CompressionLevel.Optimal, CancellationToken abortToken = default)
    {
        using (FileStream fs = new(targetPackagePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
        {
            if (!string.IsNullOrEmpty(password) && !string.IsNullOrWhiteSpace(password))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    CreateKey(out AesCng cngEncryptionKit, password, ivKey);
                    using (CryptoStream cryptoStream = new(fs, cngEncryptionKit.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (BrotliStream CompressionStream = new(cryptoStream, compression))
                        {
                            await TarFile.CreateFromDirectoryAsync(sourceFolder, CompressionStream, false, abortToken);
                        }
                    }
                }
                else
                {
                    CreateKey(out Aes cngEncryptionKit, password, ivKey);
                    using (CryptoStream cryptoStream = new(fs, cngEncryptionKit.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (BrotliStream CompressionStream = new(cryptoStream, compression))
                        {
                            await TarFile.CreateFromDirectoryAsync(sourceFolder, CompressionStream, false, abortToken);
                        }
                    }
                }
            }
            else
            {
                using (BrotliStream CompressionStream = new(fs, compression))
                {
                    await TarFile.CreateFromDirectoryAsync(sourceFolder, CompressionStream, false, abortToken);
                }
            }
        }
    }
    #endregion

    #region Decompression Packages
    /// <summary>
    /// Decompresses a Halva package to a folder.
    /// </summary>
    /// <param name="packagePath">The source package file.</param>
    /// <param name="targetFolder">The folder where the files of the package will be decompressed to.</param>
    /// <param name="password">The password for the package.</param>
    /// <param name="ivKey">The IV key for the package.</param>
    public static void DecompressPackageToFolder(string packagePath, string targetFolder, string password = "", string ivKey = "")
    {
        if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);
        using (FileStream fs = new(packagePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan))
        {
            if (!string.IsNullOrEmpty(password) && !string.IsNullOrWhiteSpace(password))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    CreateKey(out AesCng cngEncryptionKit, password, ivKey);
                    using (CryptoStream cryptoStream = new(fs, cngEncryptionKit.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (BrotliStream decompressionStream = new(cryptoStream, CompressionMode.Decompress))
                        {
                            TarFile.ExtractToDirectory(decompressionStream, targetFolder, true);
                        }
                    }
                }
                else
                {
                    CreateKey(out Aes cngEncryptionKit, password, ivKey);
                    using (CryptoStream cryptoStream = new(fs, cngEncryptionKit.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (BrotliStream decompressionStream = new(cryptoStream, CompressionMode.Decompress))
                        {
                            TarFile.ExtractToDirectory(decompressionStream, targetFolder, true);
                        }
                    }
                }
            }
            else
            {
                using (BrotliStream decompressionStream = new(fs, CompressionMode.Decompress))
                {
                    TarFile.ExtractToDirectory(decompressionStream, targetFolder, true);
                }
            }
        }
    }

    /// <summary>
    /// Decompresses a Halva package to a folder.
    /// </summary>
    /// <param name="packagePath">The source package file.</param>
    /// <param name="targetFolder">The folder where the files of the package will be decompressed to.</param>
    /// <param name="password">The password for the package.</param>
    /// <param name="ivKey">The IV key for the package.</param>
    /// <param name="abortToken">The cancellation token to abort the operation.</param>
    public static async Task DecompressPackageToFolderAsync(string packagePath, string targetFolder, string password = "", string ivKey = "", CancellationToken abortToken = default)
    {
        if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);
        using (FileStream fs = new(packagePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
        {
            if (!string.IsNullOrEmpty(password) && !string.IsNullOrWhiteSpace(password))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    CreateKey(out AesCng cngEncryptionKit, password, ivKey);
                    using (CryptoStream cryptoStream = new(fs, cngEncryptionKit.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (BrotliStream decompressionStream = new(cryptoStream, CompressionMode.Decompress))
                        {
                            await TarFile.ExtractToDirectoryAsync(decompressionStream, targetFolder, true, abortToken);
                        }
                    }
                }
                else
                {
                    CreateKey(out Aes cngEncryptionKit, password, ivKey);
                    using (CryptoStream cryptoStream = new(fs, cngEncryptionKit.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (BrotliStream decompressionStream = new(cryptoStream, CompressionMode.Decompress))
                        {
                            await TarFile.ExtractToDirectoryAsync(decompressionStream, targetFolder, true, abortToken);
                        }
                    }
                }
            }
            else
            {
                using (BrotliStream decompressionStream = new(fs, CompressionMode.Decompress))
                {
                    await TarFile.ExtractToDirectoryAsync(decompressionStream, targetFolder, true, abortToken);
                }
            }
        }
    }
    #endregion

    /// <summary>
    /// Normalizes a file path to use the correct directory separator character for the current platform.
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <returns>The given path, normalized to the OS'.</returns>
    static internal string NormalizePath(string path)
    {
        string temp = path.TrimStart('/', '\\');
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return temp.Replace('/', Path.DirectorySeparatorChar);
        else return temp.Replace('\\', Path.DirectorySeparatorChar);
    }

    #region Encryption Key Handling
    /// <summary>
    /// Sets up the Aes Encryptor/Decryptor with IV used in code.
    /// </summary>
    /// <param name="encryptor">The Aes Encryptor/Decryptor to initialize.</param>
    /// <param name="password">The password of the archive.</param>
    /// <param name="ivKey">The IV for the archive.</param>
    internal static void CreateKey(out Aes encryptor, in string password, in string ivKey = "")
    {
        encryptor = Aes.Create();
        encryptor.KeySize = 256;
        encryptor.Padding = PaddingMode.PKCS7;
        byte[] hashCode = SHA512.HashData(Encoding.UTF8.GetBytes(password));
        byte[] hashIV = !string.IsNullOrEmpty(ivKey) && !string.IsNullOrWhiteSpace(ivKey) ? SHA512.HashData(Encoding.UTF8.GetBytes(ivKey)) : SHA512.HashData(hashCode);
        Rfc2898DeriveBytes key = new(password, hashCode, 50000, HashAlgorithmName.SHA512);
        Rfc2898DeriveBytes vectorKey = new(ivKey, hashIV, 50000, HashAlgorithmName.SHA512);
        encryptor.Key = key.GetBytes(encryptor.KeySize / 8);
        encryptor.IV = vectorKey.GetBytes(encryptor.BlockSize / 8);
        key.Dispose();
        vectorKey.Dispose();
    }

    [SupportedOSPlatform("windows")]
    /// <summary>
    /// Sets up the Aes Encryptor/Decryptor with IV used in code (Using Windows' Cryptography Next Generation).
    /// </summary>
    /// <param name="encryptor">The Aes Encryptor/Decryptor to initialize.</param>
    /// <param name="password">The password of the archive.</param>
    /// <param name="ivKey">The IV for the archive.</param>
    internal static void CreateKey(out AesCng encryptor, in string password, in string ivKey = "")
    {
        encryptor = new AesCng
        {
            KeySize = 256,
            Padding = PaddingMode.PKCS7
        };
        byte[] hashCode = SHA512.HashData(Encoding.UTF8.GetBytes(password));
        byte[] hashIV = !string.IsNullOrEmpty(ivKey) && !string.IsNullOrWhiteSpace(ivKey) ? SHA512.HashData(Encoding.UTF8.GetBytes(ivKey)) : SHA512.HashData(hashCode);
        Rfc2898DeriveBytes key = new(password, hashCode, 50000, HashAlgorithmName.SHA512);
        Rfc2898DeriveBytes vectorKey = new(ivKey, hashIV, 50000, HashAlgorithmName.SHA512);
        encryptor.Key = key.GetBytes(encryptor.KeySize / 8);
        encryptor.IV = vectorKey.GetBytes(encryptor.BlockSize / 8);
        key.Dispose();
        vectorKey.Dispose();
    }
    #endregion
}
