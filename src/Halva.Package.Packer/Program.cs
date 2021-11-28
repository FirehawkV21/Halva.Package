global using System;
global using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Halva.Package.Core.Manager;

namespace Halva.Package.Packer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string projectLocation = null;
            bool mustEncrypt = false;
            string archiveDestination = null;
            string password = null;
            bool settingsSet = false;
            string stringBuffer;
            Console.WriteLine(Properties.Resources.SplitterText);
            Console.WriteLine(Properties.Resources.ProgramTitle);
            Console.WriteLine(Properties.Resources.ProgramVersion, Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine(Properties.Resources.AuthorSignature);
            Console.WriteLine(Properties.Resources.LicenseText);
            Console.WriteLine(Properties.Resources.SplitterText);
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
                                    Console.WriteLine(Properties.Resources.ProjectFoundText);
                                    Console.ResetColor();
                                    settingsSet = true;
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkRed;
                                    Console.WriteLine(Properties.Resources.NoProjectFoundText);
                                    Console.ResetColor();
                                }
                            }
                            break;
                        case "--Password":
                            if (argnum <= args.Length - 1 && !args[argnum + 1].Contains("--"))
                            {
                                password = args[argnum + 1].Replace("\"", "");
                                mustEncrypt = true;
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine(Properties.Resources.PasswordSetText);
                                Console.ResetColor();

                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                Console.WriteLine(Properties.Resources.NoPassowrdSetText);
                                Console.ResetColor();
                            }
                            break;
                        case "--OutputLocation":
                            if (argnum <= args.Length - 1 && !args[argnum + 1].Contains("--"))
                            {
                                stringBuffer = args[argnum + 1];
                                archiveDestination = stringBuffer.Replace("\"", "");

                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine(Properties.Resources.OutputSetText);
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                Console.WriteLine(Properties.Resources.NoOutputSetText);
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
                    Console.WriteLine(Properties.Resources.ProjectLocationQuestion);
                    projectLocation = Console.ReadLine();
                    if (!Directory.Exists(projectLocation))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(Properties.Resources.LocationDoesNotExistText);
                        Console.ResetColor();
                    }
                    else if (!File.Exists(Path.Combine(projectLocation, "package.json")))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(Properties.Resources.ProjectFileNotPresentText);
                        Console.ResetColor();
                        projectLocation = null;
                    }
                } while (projectLocation == null && !Directory.Exists(projectLocation));

                if (string.IsNullOrEmpty(password))
                {
                    Console.WriteLine(Properties.Resources.EncryptionQuestion);
                    var tempChar = Console.ReadKey();
                    mustEncrypt = tempChar.KeyChar switch
                    {
                        'Y' or 'y' or 'Ν' or 'ν' => true,
                        _ => false,
                    };
                    Console.WriteLine();
                    if (mustEncrypt)
                    {
                        Console.WriteLine(Properties.Resources.PasswordQuestion);
                        password = Console.ReadLine();
                    }
                }

                if (string.IsNullOrEmpty(archiveDestination))
                {
                    Console.WriteLine(Properties.Resources.OutputLocationQuestion);
                    archiveDestination = Console.ReadLine();
                }
            }

            var gameFolder = JsonReader.FindGameFolder(Path.Combine(projectLocation, "package.json"));
            if (gameFolder == "Unknown" || gameFolder == "Null")
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(Properties.Resources.ProjectFilesLocationNotFound);
            }
            else
            {
                if (!Directory.Exists(archiveDestination) && !string.IsNullOrEmpty(archiveDestination)) Directory.CreateDirectory(archiveDestination);
                string destinationPath = !string.IsNullOrEmpty(archiveDestination) ? archiveDestination : gameFolder;
                try
                {
                    if (mustEncrypt && password != null)
                    {
                        var encryptedAssetsPackage =
                            new EncryptedHalvaPackage();
                        var encryptedAudioAssetsPackage = new EncryptedHalvaPackage();
                        var encryptedDatabasePackage =
                            new EncryptedHalvaPackage();
                        var encryptedEnginePackage =
                            new EncryptedHalvaPackage();
                        encryptedAssetsPackage.Password = password;
                        encryptedAudioAssetsPackage.Password = password;
                        encryptedDatabasePackage.Password = password;
                        encryptedEnginePackage.Password = password;
                        encryptedAssetsPackage.DestinationLocation =
                            new StringBuilder(Path.Combine(destinationPath, "AssetsPackage.halva"));
                        encryptedAudioAssetsPackage.DestinationLocation = new StringBuilder(Path.Combine(destinationPath, "AudioPackage.halva"));
                        encryptedDatabasePackage.DestinationLocation =
                            new StringBuilder(Path.Combine(destinationPath, "DatabasePackage.halva"));
                        encryptedEnginePackage.DestinationLocation =
                            new StringBuilder(Path.Combine(destinationPath, "EnginePackage.halva"));
                        Task buildEncryptedAssets = Task.Run(() =>
                        {
                            Console.WriteLine(Properties.Resources.CompressingAssetsText);
                            encryptedAssetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "img");
                            if (Directory.Exists(Path.Combine(gameFolder, "fonts"))) encryptedAssetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "fonts");
                            if (Directory.Exists(Path.Combine(gameFolder, "css"))) encryptedAssetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "css");
                            if (Directory.Exists(Path.Combine(gameFolder, "effects"))) encryptedAssetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "effects");
                            if (Directory.Exists(Path.Combine(gameFolder, "movies"))) encryptedAssetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "movies");
                            if (Directory.Exists(Path.Combine(gameFolder, "icon"))) encryptedAssetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "icon");
                            encryptedAssetsPackage.CloseArchive();
                            encryptedAssetsPackage.Dispose();
                            Console.WriteLine(Properties.Resources.AssetsCompressedText);
                        });
                        Task buildEncryptedAudio = Task.Run(() =>
                        {
                            Console.WriteLine(Properties.Resources.CompressingAudioFilesText);
                            encryptedAudioAssetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "audio");
                            encryptedAudioAssetsPackage.CloseArchive();
                            encryptedAudioAssetsPackage.Dispose();
                            Console.WriteLine(Properties.Resources.AudioCompressedText);
                        });
                        Task buildEncryptedDatabase = Task.Run(() =>
                        {
                            Console.WriteLine(Properties.Resources.CompressingDatabaseText);
                            encryptedDatabasePackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "data");
                            encryptedDatabasePackage.CloseArchive();
                            encryptedDatabasePackage.Dispose();
                            Console.WriteLine(Properties.Resources.DatabaseCompressedText);
                        });
                        Task buildEncryptedEngine = Task.Run(() =>
                        {
                            Console.WriteLine(Properties.Resources.CompressingEngineFilesText);
                            encryptedEnginePackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "js");
                            if (gameFolder == projectLocation) encryptedEnginePackage.AddFileToList(gameFolder, "index.html");
                            else
                            {
                                var relativeLocation = gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "");
                                encryptedEnginePackage.AddFileToList(projectLocation, Path.Combine(relativeLocation, "index.html"));
                            }
                            encryptedEnginePackage.AddFileToList(projectLocation, "package.json");
                            encryptedEnginePackage.CloseArchive();
                            encryptedEnginePackage.Dispose();
                            Console.WriteLine(Properties.Resources.EngineCompressedText);
                        });
                        await Task.WhenAll(buildEncryptedAssets, buildEncryptedAudio, buildEncryptedDatabase, buildEncryptedEngine);
                    }
                    else
                    {
                        var assetsPackage = new HalvaPackage();
                        var databasePackage = new HalvaPackage();
                        var enginePackage = new HalvaPackage();
                        var audioAssetsPackage = new HalvaPackage();
                        assetsPackage.DestinationLocation =
                            new StringBuilder(Path.Combine(destinationPath, "AssetsPackage.halva"));
                        audioAssetsPackage.DestinationLocation = new StringBuilder(Path.Combine(destinationPath, "audioPackage.halva"));
                        databasePackage.DestinationLocation =
                            new StringBuilder(Path.Combine(destinationPath, "DatabasePackage.halva"));
                        enginePackage.DestinationLocation =
                            new StringBuilder(Path.Combine(destinationPath, "EnginePackage.halva"));
                        Task buildAssets = Task.Run(() =>
                        {
                            Console.WriteLine(Properties.Resources.CompressingAssetsText);
                            assetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "img");
                            assetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "icon");
                            if (Directory.Exists(Path.Combine(gameFolder, "fonts"))) assetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "fonts");
                            if (Directory.Exists(Path.Combine(gameFolder, "css"))) assetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "css");
                            if (Directory.Exists(Path.Combine(gameFolder, "effects"))) assetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "effects");
                            if (Directory.Exists(Path.Combine(gameFolder, "movies"))) assetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "movies");
                            if (Directory.Exists(projectLocation + Path.Combine(gameFolder, "icon"))) assetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "icon");
                            assetsPackage.CloseArchive();
                            assetsPackage.Dispose();
                            Console.WriteLine(Properties.Resources.AssetsCompressedText);
                        });
                        Task buildAudio = Task.Run(() =>
                        {
                            Console.WriteLine(Properties.Resources.CompressingAudioFilesText);
                            audioAssetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "audio");
                            audioAssetsPackage.CloseArchive();
                            audioAssetsPackage.Dispose();
                            Console.WriteLine(Properties.Resources.AudioCompressedText);
                        });
                        Task buildDatabase = Task.Run(() =>
                        {
                            Console.WriteLine(Properties.Resources.CompressingDatabaseText);
                            databasePackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "data");
                            databasePackage.CloseArchive();
                            databasePackage.Dispose();
                            Console.WriteLine(Properties.Resources.DatabaseCompressedText);
                        });
                        Task buildEngine = Task.Run(() =>
                        {
                            Console.WriteLine(Properties.Resources.CompressingEngineFilesText);
                            enginePackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "") + HalvaPackageBase.GetFolderCharacter() + "js");
                            if (gameFolder == projectLocation) enginePackage.AddFileToList(gameFolder, "index.html");
                            else
                            {
                                var relativeLocation = gameFolder.Replace(projectLocation + HalvaPackageBase.GetFolderCharacter(), "");
                                enginePackage.AddFileToList(projectLocation, Path.Combine(relativeLocation, "index.html"));
                            }
                            enginePackage.AddFileToList(projectLocation, "package.json");
                            enginePackage.CloseArchive();
                            enginePackage.Dispose();
                            Console.WriteLine(Properties.Resources.EngineCompressedText);
                        });
                        await Task.WhenAll(buildAssets, buildAudio, buildDatabase, buildEngine);
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
