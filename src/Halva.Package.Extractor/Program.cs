using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Halva.Package.Bootstrapper
{
    class Program
    {

        static void Main(string[] args)
        {
            Process GameProcess = new Process();
            ProcessStartInfo GameInfo = new ProcessStartInfo(Directory.GetParent(Assembly.GetExecutingAssembly().Location) + "\\Binaries\\Game.exe");

            Console.WriteLine("===========================================");
            Console.WriteLine("= Halva Bootstrapper");
            Console.WriteLine("= Version D1.00 ({0})", Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine("= Developed by Studio ACE");
            Console.WriteLine("= Licesned under the MIT license.");
            Console.WriteLine("===========================================");
            Console.WriteLine();

            GamePackageManager packageManager = new GamePackageManager();
            if (packageManager.IsPackageMetadataPresent()) { 
                 int updatedPackages = 0;
                if (!packageManager.IsInstalledPackageLatest("assetsVersion")) {
                    Console.WriteLine("Updating assets...");
                    packageManager.ExtractPackage("AssetsPackage.halva", "assetsVersion");
                    updatedPackages += 1;
                        }
                if (!packageManager.IsInstalledPackageLatest("databaseVersion"))
                {
                    Console.WriteLine("Updating database...");
                    packageManager.ExtractPackage("DatabasePackage.halva", "databaseVersion");
                    updatedPackages += 1;
                }
                if (!packageManager.IsInstalledPackageLatest("engineVersion")) {
                    Console.WriteLine("Updating engine...");
                    packageManager.ExtractPackage("EnginePackage.halva", "engineVersion");
                    updatedPackages += 1;
                }
                if (updatedPackages > 0) packageManager.SavePackageMetadata();
            }
            else
            {
                Console.WriteLine("Decompressing the game's assets. This will take a while.");
                packageManager.ExtractPackage("AssetsPackage.halva", "assetsVersion");
                packageManager.ExtractPackage("DatabasePackage.halva", "databaseVersion");
                packageManager.ExtractPackage("EnginePackage.halva", "engineVersion");
                packageManager.SavePackageMetadata();
            }

            GameInfo.Arguments += "--nwapp=\"" + Path.Combine(packageManager.ExctractLocation, "GameData") + "\"";
            GameProcess.StartInfo = GameInfo;
            GameProcess.Start();
        }        
    }
}
