using Halva.Package.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using WinSystem = Windows;

namespace Halva.Package.Bootstrapper
{
    public class GamePackageManager
    {
        private string PackageLocation = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString(), "GamePackages");
        public string ExctractLocation { get
            {
                if (IsRunningInCentennial())
                {
                    var LocalStorageFolderUWP = WinSystem.Storage.ApplicationData.Current.LocalFolder;
                    return LocalStorageFolderUWP.Path;
                }
                //Change this to set a different folder.
                else return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "RMDev", "Game");
            }
        }
        private string PackagePassword = "";
        private readonly Dictionary<string, int> TargetPackageVersion = new Dictionary<string, int>();
        private Dictionary<string, int> CurrentPackageVersion = new Dictionary<string, int>();

        /// <summary>
        /// Creates a package manager.
        /// </summary>
        public GamePackageManager()
        {
            // Edit these to show the correct package version (or offload it to a JSON file).
            TargetPackageVersion.Add("assetsVersion", 20201003);
            TargetPackageVersion.Add("databaseVersion", 20201003);
            TargetPackageVersion.Add("engineVersion", 202001003);
            if (!Directory.Exists(Path.Combine(ExctractLocation, "GameData"))) Directory.CreateDirectory(Path.Combine(ExctractLocation, "GameData"));
            if (File.Exists(Path.Combine(ExctractLocation, "PackageData.json")))
            {
                int assetsVersion;
                int databaseVersion;
                int engineVersion;
                char[] JsonIn;
                using (StreamReader settingsLoader = new StreamReader(Path.Combine(ExctractLocation, "PackageData.json")))
                {
                    JsonIn = new Char[(int)settingsLoader.BaseStream.Length];
                    settingsLoader.Read(JsonIn, 0, (int)settingsLoader.BaseStream.Length);
                }
                string JsonString = new string(JsonIn);
                var packageMetadata = JObject.Parse(JsonString);
                assetsVersion = (int)packageMetadata["packages"]["assets"];
                databaseVersion = (int)packageMetadata["packages"]["database"];
                engineVersion = (int)packageMetadata["packages"]["engine"];
                CurrentPackageVersion.Add("assetsVersion", assetsVersion);
                CurrentPackageVersion.Add("databaseVersion", databaseVersion);
                CurrentPackageVersion.Add("engineVersion", engineVersion);
            }
        }

        public static bool IsRunningInCentennial()
        {
            DesktopBridge.Helpers checker = new DesktopBridge.Helpers();
            return checker.IsRunningAsUwp();
        }

        private void ExtractPackage(string PackageName)
        {
            if (PackagePassword != null) EncryptedPackageUtilities.ExportFromArchive(Path.Combine(PackageLocation, PackageName), Path.Combine(ExctractLocation, "GameData"), PackagePassword);
            else PackageUtilities.ExportFromArchive(Path.Combine(PackageLocation, PackageName), Path.Combine(ExctractLocation, "GameData"));
        }

        public void ExtractPackage(string PackageName, string PackageVersionKey)
        {
            ExtractPackage(PackageName);
            int packageVersion;
            if (CurrentPackageVersion.TryGetValue(PackageVersionKey, out _))
            {
                CurrentPackageVersion.Remove(PackageVersionKey);
                TargetPackageVersion.TryGetValue(PackageVersionKey, out packageVersion);
                CurrentPackageVersion.Add(PackageVersionKey, packageVersion);
            }
            else
            {
                TargetPackageVersion.TryGetValue(PackageVersionKey, out packageVersion);
                CurrentPackageVersion.Add(PackageVersionKey, packageVersion);
            }
        }

        public bool IsInstalledPackageLatest(string PackageVersionKey)
        {
            bool sameVersion;
            sameVersion = CurrentPackageVersion.TryGetValue(PackageVersionKey, out int packageVersion)
                && TargetPackageVersion.TryGetValue(PackageVersionKey, out int targetVersion)
                && targetVersion == packageVersion;
            return sameVersion;
        }

        public bool IsPackageMetadataPresent()
        {
            return File.Exists(Path.Combine(ExctractLocation, "PackageData.json"));
        }

        public void SavePackageMetadata()
        {
            CurrentPackageVersion.TryGetValue("assetsVersion", out int assetsVersion);
            CurrentPackageVersion.TryGetValue("databaseVersion", out int databaseVersion);
            CurrentPackageVersion.TryGetValue("engineVersion", out int engineVersion);
            JObject gameMetadata = new JObject(
                new JProperty("packages",
                    new JObject(
                        new JProperty("assets", assetsVersion),
                        new JProperty("database", databaseVersion),
                        new JProperty("engine", engineVersion))));
            using StreamWriter packageDataFile = new StreamWriter(Path.Combine(ExctractLocation, "PackageData.json"));
            packageDataFile.Write(gameMetadata);
        }
    }
}
