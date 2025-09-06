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
                    using (CryptoStream cryptoStream = new(fs, GetEncryptionKey(password, ivKey).CreateEncryptor(), CryptoStreamMode.Write))
                    using (BrotliStream CompressionStream = new(cryptoStream, compression))
                    {
                        TarFile.CreateFromDirectory(sourceFolder, CompressionStream, false);
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
                    using (CryptoStream cryptoStream = new(fs, GetEncryptionKey(password, ivKey).CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (BrotliStream CompressionStream = new(cryptoStream, compression))
                        {
                            await TarFile.CreateFromDirectoryAsync(sourceFolder, CompressionStream, false, abortToken);
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
                    using (CryptoStream cryptoStream = new(fs, GetEncryptionKey(password, ivKey).CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (BrotliStream decompressionStream = new(cryptoStream, CompressionMode.Decompress))
                        {
                            TarFile.ExtractToDirectory(decompressionStream, targetFolder, true);
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
                    using (CryptoStream cryptoStream = new(fs, GetEncryptionKey(password, ivKey).CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (BrotliStream decompressionStream = new(cryptoStream, CompressionMode.Decompress))
                        {
                            await TarFile.ExtractToDirectoryAsync(decompressionStream, targetFolder, true, abortToken);
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

    #region Encryption Key Handling

    static internal Aes GetEncryptionKey(in string password, in string iv)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            CreateKey(out AesCng encryptionKey, password, iv);
            return encryptionKey;
        }
        else
        {
            CreateKey(out Aes encryptionKey, password, iv);
            return encryptionKey;
        }
    }
    /// <summary>
    /// Sets up the Aes Encryptor/Decryptor with IV used in code.
    /// </summary>
    /// <param name="encryptor">The Aes Encryptor/Decryptor to initialize.</param>
    /// <param name="password">The password of the archive.</param>
    /// <param name="ivKey">The IV for the archive.</param>
    static internal void CreateKey(out Aes encryptor, in string password, in string ivKey = "")
    {
        encryptor = Aes.Create();
        encryptor.KeySize = 256;
        encryptor.Padding = PaddingMode.PKCS7;
#if NET10_0_OR_GREATER
        byte[] hashCode = SHA512.HashData(Encoding.UTF8.GetBytes(password));
        byte[] hashIV = !string.IsNullOrEmpty(ivKey) && !string.IsNullOrWhiteSpace(ivKey) ? SHA512.HashData(Encoding.UTF8.GetBytes(ivKey)) : SHA512.HashData(hashCode);

        encryptor.Key = Rfc2898DeriveBytes.Pbkdf2(
            password: password,
            salt: hashCode,
            iterations: 50000,
            hashAlgorithm: HashAlgorithmName.SHA512,
            outputLength: encryptor.KeySize / 8
        );

        encryptor.IV = Rfc2898DeriveBytes.Pbkdf2(
            password: ivKey,
            salt: hashIV,
            iterations: 50000,
            hashAlgorithm: HashAlgorithmName.SHA512,
            outputLength: encryptor.BlockSize / 8
        );
#else
        byte[] hashCode = SHA512.HashData(Encoding.UTF8.GetBytes(password));
        byte[] hashIV = !string.IsNullOrEmpty(ivKey) && !string.IsNullOrWhiteSpace(ivKey) ? SHA512.HashData(Encoding.UTF8.GetBytes(ivKey)) : SHA512.HashData(hashCode);
        Rfc2898DeriveBytes key = new(password, hashCode, 50000, HashAlgorithmName.SHA512);
        Rfc2898DeriveBytes vectorKey = new(ivKey, hashIV, 50000, HashAlgorithmName.SHA512);

        encryptor.Key = key.GetBytes(encryptor.KeySize / 8);
        encryptor.IV = vectorKey.GetBytes(encryptor.BlockSize / 8);
        key.Dispose();
        vectorKey.Dispose();
#endif
    }

    [SupportedOSPlatform("windows")]
    /// <summary>
    /// Sets up the Aes Encryptor/Decryptor with IV used in code (Using Windows' Cryptography Next Generation).
    /// </summary>
    /// <param name="encryptor">The Aes Encryptor/Decryptor to initialize.</param>
    /// <param name="password">The password of the archive.</param>
    /// <param name="ivKey">The IV for the archive.</param>
    static internal void CreateKey(out AesCng encryptor, in string password, in string ivKey = "")
    {
        encryptor = new AesCng
        {
            KeySize = 256,
            Padding = PaddingMode.PKCS7
        };
#if NET10_0_OR_GREATER
        byte[] hashCode = SHA512.HashData(Encoding.UTF8.GetBytes(password));
        byte[] hashIV = !string.IsNullOrEmpty(ivKey) && !string.IsNullOrWhiteSpace(ivKey) ? SHA512.HashData(Encoding.UTF8.GetBytes(ivKey)) : SHA512.HashData(hashCode);

        encryptor.Key = Rfc2898DeriveBytes.Pbkdf2(
            password: password,
            salt: hashCode,
            iterations: 50000,
            hashAlgorithm: HashAlgorithmName.SHA512,
            outputLength: encryptor.KeySize / 8
        );

        encryptor.IV = Rfc2898DeriveBytes.Pbkdf2(
            password: ivKey,
            salt: hashIV,
            iterations: 50000,
            hashAlgorithm: HashAlgorithmName.SHA512,
            outputLength: encryptor.BlockSize / 8
        );
#else
        byte[] hashCode = SHA512.HashData(Encoding.UTF8.GetBytes(password));
        byte[] hashIV = !string.IsNullOrEmpty(ivKey) && !string.IsNullOrWhiteSpace(ivKey) ? SHA512.HashData(Encoding.UTF8.GetBytes(ivKey)) : SHA512.HashData(hashCode);
        Rfc2898DeriveBytes key = new(password, hashCode, 50000, HashAlgorithmName.SHA512);
        Rfc2898DeriveBytes vectorKey = new(ivKey, hashIV, 50000, HashAlgorithmName.SHA512);

        encryptor.Key = key.GetBytes(encryptor.KeySize / 8);
        encryptor.IV = vectorKey.GetBytes(encryptor.BlockSize / 8);
        key.Dispose();
        vectorKey.Dispose();
#endif
    }
    #endregion
}
