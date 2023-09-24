#if WINDOWS10_0_17763_0_OR_GREATER
using WinSystem = Windows;
#endif
using Halva.Package.Core.Utilities;
using Halva.Package.Core.Manager;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;

namespace Halva.Package.Bootstrapper;

public class GamePackageManager
{
    private readonly string PackageLocation = Path.Combine(AppContext.BaseDirectory, "GamePackages");
    //Change this to set a different folder.
    private static readonly string LocalFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "RMDev", "Game");
    public static string ExctractLocation
    {
        get
        {
#if WINDOWS10_0_17763_0_OR_GREATER
            if (OperatingSystem.IsWindowsVersionAtLeast(10,0,10240,0))
            {
                if (IsRunningInCentennial())
                {
                    WinSystem.Storage.StorageFolder LocalStorageFolderUWP = WinSystem.Storage.ApplicationData.Current.LocalFolder;
                    return LocalStorageFolderUWP.Path;
                }
                else return LocalFolder;
            }
            else return LocalFolder;
#else
            return LocalFolder;
#endif
        }
    }
    //If you have a password for the archives, place it here or read from a file.
    private readonly string PackagePassword = "";
    //If you have set a IV for the archives, place it here or read from a file.
    private readonly string PackageIV = "";
    private readonly bool useMemoryStream = true;
    private readonly PackageMetadata TargetPackageVersion = new();
    private readonly PackageMetadata CurrentPackageVersion = new();

    /// <summary>
    /// Creates a package manager.
    /// </summary>
    [RequiresUnreferencedCode("Uses JSON Source Generator")]
    public GamePackageManager()
    {
        TargetPackageVersion.PackageList.AssetsVersion = 20211016;
        TargetPackageVersion.PackageList.AudioVersion = 20211204;
        TargetPackageVersion.PackageList.DatabaseVersion = 20211204;
        TargetPackageVersion.PackageList.EngineVersion = 20211204;
        if (!Directory.Exists(Path.Combine(ExctractLocation, "GameData"))) Directory.CreateDirectory(Path.Combine(ExctractLocation, "GameData"));
        if (File.Exists(Path.Combine(ExctractLocation, "PackageData.json")))
        {
            string inputFile = File.ReadAllText(Path.Combine(ExctractLocation, "PackageData.json"));
            CurrentPackageVersion = JsonSerializer.Deserialize(inputFile, PackageMetadataSerializer.Default.PackageMetadata);
        }
    }

    public static bool IsRunningInCentennial()
    {
        DesktopBridge.Helpers checker = new();
        return checker.IsRunningAsUwp();
    }

    private void ExtractPackage(string PackageName)
    {
        if (!string.IsNullOrEmpty(PackagePassword) && !string.IsNullOrWhiteSpace(PackagePassword)) {
            if (!string.IsNullOrEmpty(PackageIV) && !string.IsNullOrWhiteSpace(PackageIV))
                EncryptedPackageUtilities.ExportFromArchive(Path.Combine(PackageLocation, PackageName), Path.Combine(ExctractLocation, "GameData"), PackagePassword, PackageIV);
            else  EncryptedPackageUtilities.ExportFromArchive(Path.Combine(PackageLocation, PackageName), Path.Combine(ExctractLocation, "GameData"), PackagePassword);
        }
        else PackageUtilities.ExportFromArchive(Path.Combine(PackageLocation, PackageName), Path.Combine(ExctractLocation, "GameData"));
    }

    public void ExtractPackage(string PackageName, string PackageVersionKey)
    {
        ExtractPackage(PackageName);
        switch (PackageVersionKey)
        {
            case "assetsVersion":
                CurrentPackageVersion.PackageList.AssetsVersion = TargetPackageVersion.PackageList.AssetsVersion;
                break;
            case "audioVersion":
                CurrentPackageVersion.PackageList.AudioVersion = TargetPackageVersion.PackageList.AudioVersion;
                break;
            case "databaseVersion":
                CurrentPackageVersion.PackageList.DatabaseVersion = TargetPackageVersion.PackageList.DatabaseVersion;
                break;
            case "engineVersion":
                CurrentPackageVersion.PackageList.EngineVersion = TargetPackageVersion.PackageList.EngineVersion;
                break;
        }
    }

    public void UpdateDataFromArchive(string PackageName, string PackageVersionKey)
    {
        HalvaPackage package;
        if (!string.IsNullOrEmpty(PackagePassword) && !string.IsNullOrWhiteSpace(PackagePassword)) {
            if (!string.IsNullOrEmpty(PackageIV) && !string.IsNullOrWhiteSpace(PackageIV)) package = new HalvaPackage(Path.Combine(PackageLocation, PackageName), PackagePassword, PackageIV, useMemoryStream);
            else package = new HalvaPackage(Path.Combine(PackageLocation, PackageName), PackagePassword, useMemoryStream);
        }
        else package = new HalvaPackage(Path.Combine(PackageLocation, PackageName), useMemoryStream);
            package.Password = PackagePassword;
            package.UpdateFromArchive(Path.Combine(ExctractLocation, "GameData"));
            package.Dispose();
        switch (PackageVersionKey)
        {
            case "assetsVersion":
                CurrentPackageVersion.PackageList.AssetsVersion = TargetPackageVersion.PackageList.AssetsVersion;
                break;
            case "audioVersion":
                CurrentPackageVersion.PackageList.AudioVersion = TargetPackageVersion.PackageList.AudioVersion;
                break;
            case "databaseVersion":
                CurrentPackageVersion.PackageList.DatabaseVersion = TargetPackageVersion.PackageList.DatabaseVersion;
                break;
            case "engineVersion":
                CurrentPackageVersion.PackageList.EngineVersion = TargetPackageVersion.PackageList.EngineVersion;
                break;
        }
    }

    public bool IsInstalledPackageLatest(string PackageVersionKey) => PackageVersionKey switch
    {
        "assetsVersion" => CurrentPackageVersion.PackageList.AssetsVersion == TargetPackageVersion.PackageList.AssetsVersion,
        "audioVersion" => CurrentPackageVersion.PackageList.AudioVersion == TargetPackageVersion.PackageList.AudioVersion,
        "databaseVersion" => CurrentPackageVersion.PackageList.DatabaseVersion == TargetPackageVersion.PackageList.DatabaseVersion,
        "engineVersion" => CurrentPackageVersion.PackageList.EngineVersion == TargetPackageVersion.PackageList.EngineVersion,
        _ => true,
    };

    public static bool IsPackageMetadataPresent() => File.Exists(Path.Combine(ExctractLocation, "PackageData.json"));

    [RequiresUnreferencedCode("Uses JSON Source Generator")]
    public void SavePackageMetadata()
    {
        string output = JsonSerializer.Serialize(CurrentPackageVersion, PackageMetadataSerializer.Default.PackageMetadata);
        File.WriteAllText(Path.Combine(ExctractLocation, "PackageData.json"), output);
    }
}
