using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Halva.Package.Bootstrapper;

internal class Program
{
    [RequiresUnreferencedCode("Calls Halva.Package.Bootstrapper.GamePackageManager.GamePackageManager()")]
    private static void Main()
    {
        Process GameProcess = new();
        // Edit this part to point over to the game's executable.
        ProcessStartInfo GameInfo = new(Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "Binaries", RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Game.exe" : "Game"));

        Console.WriteLine(Properties.Resources.SplitterText);
        Console.WriteLine(Properties.Resources.ProgramTitle);
        Console.WriteLine(Properties.Resources.ProgramVersion, Assembly.GetExecutingAssembly().GetName().Version);
        Console.WriteLine(Properties.Resources.AuthorSignature);
        Console.WriteLine(Properties.Resources.LicenseText);
        Console.WriteLine(Properties.Resources.SplitterText);
        Console.WriteLine();

        GamePackageManager packageManager = new();
        if (GamePackageManager.IsPackageMetadataPresent()) { 
             int updatedPackages = 0;
            if (!packageManager.IsInstalledPackageLatest("assetsVersion")) {
                Console.WriteLine(Properties.Resources.UpdatingAssetsText);
                packageManager.UpdateDataFromArchive("AssetsPackage.halva", "assetsVersion");
                updatedPackages += 1;
                    }
            if (!packageManager.IsInstalledPackageLatest("audioVersion"))
            {
                Console.WriteLine(Properties.Resources.UpdatingAssetsText);
                packageManager.UpdateDataFromArchive("AudioPackage.halva", "audioVersion");
                updatedPackages += 1;
            }
            if (!packageManager.IsInstalledPackageLatest("databaseVersion"))
            {
                Console.WriteLine(Properties.Resources.UpdatingDatabaseText);
                packageManager.UpdateDataFromArchive("DatabasePackage.halva", "databaseVersion");
                updatedPackages += 1;
            }
            if (!packageManager.IsInstalledPackageLatest("engineVersion")) {
                Console.WriteLine(Properties.Resources.UpdatingEngineText);
                packageManager.UpdateDataFromArchive("EnginePackage.halva", "engineVersion");
                updatedPackages += 1;
            }
            if (updatedPackages > 0) packageManager.SavePackageMetadata();
        }
        else
        {
            Console.WriteLine(Properties.Resources.DecompressingDataText);
            packageManager.ExtractPackage("AssetsPackage.halva", "assetsVersion");
            packageManager.ExtractPackage("AudioPackage.halva", "audioVersion");
            packageManager.ExtractPackage("DatabasePackage.halva", "databaseVersion");
            packageManager.ExtractPackage("EnginePackage.halva", "engineVersion");
            packageManager.SavePackageMetadata();
        }

        GameInfo.Arguments += "--nwapp=\"" + Path.Combine(GamePackageManager.ExctractLocation, "GameData") + "\"";
        GameProcess.StartInfo = GameInfo;
        GameProcess.Start();
    }        
}
