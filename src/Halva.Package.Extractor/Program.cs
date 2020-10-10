using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Halva.Package.Bootstrapper
{
    class Program
    {
        static async void Main(string[] args)
        {
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
                    await Task.Run(() => packageManager.ExtractPackage("AssetsPackage.halva", "assetsVersion")).ConfigureAwait(true);
                    updatedPackages += 1;
                        }
                if (!packageManager.IsInstalledPackageLatest("databaseVersion"))
                {
                    await Task.Run(() => packageManager.ExtractPackage("DatabasePackage.halva", "databaseVersion")).ConfigureAwait(true);
                    updatedPackages += 1;
                }
                if (!packageManager.IsInstalledPackageLatest("engineVersion")) {
                    await Task.Run(() => packageManager.ExtractPackage("EnginePackage.halva", "engineVersion")).ConfigureAwait(true);
                    updatedPackages += 1;
                }
                if (updatedPackages > 0) packageManager.SavePackageMetadata();
            }
            else
            {
                await Task.Run(() => packageManager.ExtractPackage("AssetsPackage.halva", "assetsVersion")).ConfigureAwait(true);
                await Task.Run(() => packageManager.ExtractPackage("DatabasePackage.halva", "databaseVersion")).ConfigureAwait(true);
                await Task.Run(() => packageManager.ExtractPackage("EnginePackage.halva", "engineVersion")).ConfigureAwait(true);
                packageManager.SavePackageMetadata();
            }
        }

        
    }
}
