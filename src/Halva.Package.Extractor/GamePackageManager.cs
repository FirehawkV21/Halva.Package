using System.Reflection;
using WinSystem = Windows;
using Halva.Package.Core.Utilities;
using Halva.Package.Core.Manager;
using System.Text.Json;

namespace Halva.Package.Bootstrapper
{
    public class GamePackageManager
    {
        private string PackageLocation = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString(), "GamePackages");
        private static string LocalFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "RMDev", "Game");
        public static string ExctractLocation
        {
            get
            {
                if (OperatingSystem.IsWindowsVersionAtLeast(10,0,10240,0))
                {
                    if (IsRunningInCentennial())
                    {
                        var LocalStorageFolderUWP = WinSystem.Storage.ApplicationData.Current.LocalFolder;
                        return LocalStorageFolderUWP.Path;
                    }
                    else return LocalFolder;
                }
                //Change this to set a different folder.
                else return LocalFolder;
            }
        }
        //If you have a password for the archives, place it here or read from a file.
        private string PackagePassword = "";
        private PackageMetadata TargetPackageVersion = new();
        private PackageMetadata CurrentPackageVersion = new();

        /// <summary>
        /// Creates a package manager.
        /// </summary>
        public GamePackageManager()
        {
            TargetPackageVersion.PackageList.AssetsVersion = 20211016;
            TargetPackageVersion.PackageList.AudioVersion = 20211204;
            TargetPackageVersion.PackageList.DatabaseVersion = 20211204;
            TargetPackageVersion.PackageList.EngineVersion = 20211204;
            if (!Directory.Exists(Path.Combine(ExctractLocation, "GameData"))) Directory.CreateDirectory(Path.Combine(ExctractLocation, "GameData"));
            if (File.Exists(Path.Combine(ExctractLocation, "PackageData.json")))
            {
                var inputFile = File.ReadAllText(Path.Combine(ExctractLocation, "PackageData.json"));
                CurrentPackageVersion = JsonSerializer.Deserialize<PackageMetadata>(inputFile);
            }
        }

        public static bool IsRunningInCentennial()
        {
            DesktopBridge.Helpers checker = new();
            return checker.IsRunningAsUwp();
        }

        private void ExtractPackage(string PackageName)
        {
            if (PackagePassword != "") EncryptedPackageUtilities.ExportFromArchive(Path.Combine(PackageLocation, PackageName), Path.Combine(ExctractLocation, "GameData"), PackagePassword);
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
            if (PackagePassword != "")
            {
                var package = new EncryptedHalvaPackage(Path.Combine(PackageLocation, PackageName), PackagePassword);
                package.UpdateFromArchive(Path.Combine(ExctractLocation, "GameData"));
                package.Dispose();
            }
            else
            {
                var package = new HalvaPackage(Path.Combine(PackageLocation, PackageName));
                package.UpdateFromArchive(Path.Combine(ExctractLocation, "GameData"));
                package.Dispose();
            }
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

        public bool IsInstalledPackageLatest(string PackageVersionKey)
        {
            return PackageVersionKey switch
            {
                "assetsVersion" => CurrentPackageVersion.PackageList.AssetsVersion == TargetPackageVersion.PackageList.AssetsVersion,
                "audioVersion" => CurrentPackageVersion.PackageList.AudioVersion == TargetPackageVersion.PackageList.AudioVersion,
                "databaseVersion" => CurrentPackageVersion.PackageList.DatabaseVersion == TargetPackageVersion.PackageList.DatabaseVersion,
                "engineVersion" => CurrentPackageVersion.PackageList.EngineVersion == TargetPackageVersion.PackageList.EngineVersion,
                _ => true,
            };
        }

        public static bool IsPackageMetadataPresent()
        {
            return File.Exists(Path.Combine(ExctractLocation, "PackageData.json"));
        }

        public void SavePackageMetadata()
        {
            string output = JsonSerializer.Serialize<PackageMetadata>(CurrentPackageVersion);
            File.WriteAllText(Path.Combine(ExctractLocation, "PackageData.json"), output);
        }
    }
}
