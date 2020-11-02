using System;
using System.IO;
using System.Reflection;
using System.Text;
using Halva.Package.Core;

namespace Halva.Package.Packer
{
    class Program
    {
        static void Main(string[] args)
        {
            string projectLocation = null;
            bool mustEncrypt = false;
            string archiveDestination = null;
            string password = null;
            bool settingsSet = false;
            string stringBuffer;
            Console.WriteLine("===========================================");
            Console.WriteLine("= Halva Packer Tool");
            Console.WriteLine("= Version R1.00 ({0})", Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine("= Developed by Studio ACE");
            Console.WriteLine("= Licesned under the MIT license.");
            Console.WriteLine("===========================================");
            Console.WriteLine();

            if (args.Length >= 1)
            {
                for (int argnum = 0; argnum < args.Length; argnum++)
                {
                    switch (args[argnum])
                    {
                        case "--SourceLocation":
                            if (argnum <= args.Length - 1 && !args[argnum + 1].Contains("--"))
                            {
                                stringBuffer = args[argnum + 1];
                                projectLocation = stringBuffer.Replace("\"", "");
                                if (Directory.Exists(projectLocation) && File.Exists(Path.Combine(projectLocation, "package.json")))
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    Console.WriteLine("The project is found.");
                                    Console.ResetColor();
                                    settingsSet = true;
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkRed;
                                    Console.WriteLine("No project was found.");
                                    Console.ResetColor();
                                }
                            }
                            break;
                        case "--Password":
                            if(argnum <= args.Length - 1 && !args[argnum + 1].Contains("--"))
                            {
                                password = args[argnum + 1].Replace("\"", "");
                                mustEncrypt = true;
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine("Password set. The archives will now be encrypted.");
                                Console.ResetColor();

                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                Console.WriteLine("No password added. The archives will not be encrypted.");
                                Console.ResetColor();
                            }
                            break;
                        case "--OutputLocation":
                            if (argnum <= args.Length - 1 && !args[argnum + 1].Contains("--"))
                            {
                                stringBuffer = args[argnum + 1];
                                archiveDestination = stringBuffer.Replace("\"", "");

                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    Console.WriteLine("The output is now set.");
                                    Console.ResetColor();
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                Console.WriteLine("No output folder set. The archives will be placed to the project's folder.");
                                Console.ResetColor();

                            }
                            break;

                    }
                }
            }

            if (!settingsSet)
            {
                do
                {
                    Console.WriteLine("Where's the location of the project?");
                    projectLocation = Console.ReadLine();
                    if (!Directory.Exists(projectLocation))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("The location is not present.");
                        Console.ResetColor();
                    }
                    else if (!File.Exists(Path.Combine(projectLocation, "package.json")))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("The file package.json isn't present.");
                        Console.ResetColor();
                        projectLocation = null;
                    }
                } while (projectLocation == null && !Directory.Exists(projectLocation));

                if (string.IsNullOrEmpty(password))
                {
                    Console.WriteLine("Do you need to encrypt the archives? (Y/N, default is No)");
                    var tempChar = Console.ReadKey();
                    switch (tempChar.KeyChar)
                    {
                        case 'Y':
                        case 'y':
                        case 'Ν':
                        case 'ν':
                            mustEncrypt = true;
                            break;
                        case 'N':
                        case 'n':
                        case 'Ο':
                        case 'ο':
                        default:
                            mustEncrypt = false;
                            break;
                    }

                    if (mustEncrypt)
                    {
                        Console.WriteLine("Please type in the password:");
                        password = Console.ReadLine();
                    }
                }

                if (string.IsNullOrEmpty(archiveDestination))
                {
                    Console.WriteLine("Please put the location for the archives (by default it will output the archives to the same location as the game files):");
                    archiveDestination = Console.ReadLine();
                }
            }

            var gameFolder = JsonReader.FindGameFolder(Path.Combine(projectLocation, "package.json"));
            if (gameFolder == "Unknown" || gameFolder == "Null")
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("The location of the project's files couldn't be found. Please make sure that the \"main\" variable in the JSON file is set and points to the correct location.");
            }
            else
            {
                if (!Directory.Exists(archiveDestination) && !string.IsNullOrEmpty(archiveDestination)) Directory.CreateDirectory(archiveDestination);
                string destinationPath = !string.IsNullOrEmpty(archiveDestination) ? archiveDestination : gameFolder;
                if(File.Exists(Path.Combine(Path.GetTempPath(), "assetsPack.tmp"))) File.Delete(Path.Combine(Path.GetTempPath(), "assetsPack.tmp"));
                if(File.Exists(Path.Combine(Path.GetTempPath(), "dbPack.tmp"))) File.Delete(Path.Combine(Path.GetTempPath(), "dbPack.tmp"));
                if(File.Exists(Path.Combine(Path.GetTempPath(), "enginePack.tmp"))) File.Delete(Path.Combine(Path.GetTempPath(), "enginePack.tmp"));
                try
                {
                    if (mustEncrypt && password != null)
                    {
                        var encryptedAssetsPackage =
                            new EncryptedHalvaPackage(Path.Combine(Path.GetTempPath(), "assetsPack.tmp"));
                        var encryptedDatabasePackage =
                            new EncryptedHalvaPackage(Path.Combine(Path.GetTempPath(), "dbPack.tmp"));
                        var encryptedEnginePackage =
                            new EncryptedHalvaPackage(Path.Combine(Path.GetTempPath(), "enginePack.tmp"));
                        encryptedAssetsPackage.Password = password;
                        encryptedDatabasePackage.Password = password;
                        encryptedEnginePackage.Password = password;
                        encryptedAssetsPackage.DestinationLocation =
                            new StringBuilder(Path.Combine(destinationPath, "AssetsPackage.halva"));
                        encryptedDatabasePackage.DestinationLocation =
                            new StringBuilder(Path.Combine(destinationPath, "DatabasePackage.halva"));
                        encryptedEnginePackage.DestinationLocation =
                            new StringBuilder(Path.Combine(destinationPath, "EnginePackage.halva"));
                        Console.WriteLine("Compressing assets...");
                        encryptedAssetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "audio");
                        encryptedAssetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "img");
                        encryptedAssetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "icon");
                        encryptedAssetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "fonts");
                        encryptedAssetsPackage.CloseArchive();
                        File.Delete(Path.Combine(Path.GetTempPath(), "assetsPack.tmp"));
                        Console.WriteLine("Compressing database...");
                        encryptedDatabasePackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "data");
                        encryptedDatabasePackage.CloseArchive();
                        File.Delete(Path.Combine(Path.GetTempPath(), "dbPack.tmp"));
                        Console.WriteLine("Compressing engine files...");
                        encryptedEnginePackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "js");
                        encryptedEnginePackage.AddFileToList(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "index.html");
                        encryptedEnginePackage.AddFileToList(projectLocation, "package.json");
                        encryptedEnginePackage.CloseArchive();
                        File.Delete(Path.Combine(Path.GetTempPath(), "enginePack.tmp"));
                    }
                    else
                    {
                        var assetsPackage = new HalvaPackage(Path.Combine(Path.GetTempPath(), "assetsPack.tmp"));
                        var databasePackage = new HalvaPackage(Path.Combine(Path.Combine(Path.GetTempPath(), "dbPack.tmp")));
                        var enginePackage = new HalvaPackage(Path.Combine(Path.Combine(Path.GetTempPath(), "enginePack.tmp")));
                        assetsPackage.DestinationLocation =
                            new StringBuilder(Path.Combine(destinationPath, "AssetsPackage.halva"));
                        databasePackage.DestinationLocation =
                            new StringBuilder(Path.Combine(destinationPath, "DatabasePackage.halva"));
                        enginePackage.DestinationLocation =
                            new StringBuilder(Path.Combine(destinationPath, "EnginePackage.halva"));
                        Console.WriteLine("Compressing assets...");
                        assetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "audio");
                        assetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "img");
                        assetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "icon");
                        if(Directory.Exists(Path.Combine(gameFolder,"fonts"))) assetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "fonts");
                        if(Directory.Exists(Path.Combine(gameFolder, "css"))) assetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "css");
                        if (Directory.Exists(Path.Combine(gameFolder, "effects"))) assetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "effects");
                        if (Directory.Exists(Path.Combine(gameFolder, "movies"))) assetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "movies");
                        assetsPackage.CloseArchive();
                        File.Delete(Path.Combine(Path.GetTempPath(), "assetsPack.tmp"));
                        Console.WriteLine("Compressing database...");
                        databasePackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "data");
                        databasePackage.CloseArchive();
                        File.Delete(Path.Combine(Path.GetTempPath(), "dbPack.tmp"));
                        Console.WriteLine("Compressing engine files...");
                        enginePackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "js");
                        enginePackage.AddFileToList(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "index.html");
                        enginePackage.AddFileToList(projectLocation, "package.json");
                        enginePackage.CloseArchive();
                        File.Delete(Path.Combine(Path.GetTempPath(), "enginePack.tmp"));
                    }
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(e);
                }

                
            }

        }
    }
}
