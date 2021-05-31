using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using WinSystem = Windows;
using Halva.Package.Core.Utilities;
using Halva.Package.Core.Manager;

namespace Halva.Package.Bootstrapper
{
    public class GamePackageManager
    {
        private string PackageLocation = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString(), "GamePackages");
        public static string ExctractLocation
        {
            get
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
        //If you have a password for the archives, place it here or read from a file.
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
            TargetPackageVersion.Add("audioVersion", 20201003);
            TargetPackageVersion.Add("databaseVersion", 20201003);
            TargetPackageVersion.Add("engineVersion", 202001003);
            if (!Directory.Exists(Path.Combine(ExctractLocation, "GameData"))) Directory.CreateDirectory(Path.Combine(ExctractLocation, "GameData"));
            if (File.Exists(Path.Combine(ExctractLocation, "PackageData.json")))
            {
                int assetsVersion;
                int audioVersion;
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
                if (packageMetadata["packages"]["audio"] != null)
                    audioVersion = (int)packageMetadata["packages"]["audio"];
                else audioVersion = 0;
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

        public static bool IsPackageMetadataPresent()
        {
            return File.Exists(Path.Combine(ExctractLocation, "PackageData.json"));
        }

        public void SavePackageMetadata()
        {
            CurrentPackageVersion.TryGetValue("assetsVersion", out int assetsVersion);
            CurrentPackageVersion.TryGetValue("audioVersion", out int audioVersion);
            CurrentPackageVersion.TryGetValue("databaseVersion", out int databaseVersion);
            CurrentPackageVersion.TryGetValue("engineVersion", out int engineVersion);
            JObject gameMetadata = new JObject(
                new JProperty("packages",
                    new JObject(
                        new JProperty("assets", assetsVersion),
                        new JProperty("audio", audioVersion),
                        new JProperty("database", databaseVersion),
                        new JProperty("engine", engineVersion))));
            using StreamWriter packageDataFile = new StreamWriter(Path.Combine(ExctractLocation, "PackageData.json"));
            packageDataFile.Write(gameMetadata);
        }
    }
}
