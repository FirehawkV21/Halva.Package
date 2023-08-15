global using System;
global using System.IO;
using Halva.Package.Core.Manager;
using Halva.Package.Packer.ProjectMetadata;
using System.IO.Compression;
using System.Reflection;
using System.Text;

namespace Halva.Package.Packer;

internal sealed class Program
{
    private static async Task Main(string[] args)
    {
        string projectLocation = null;
        bool mustEncrypt = false;
        bool useMemoryStream = true;
        string archiveDestination = null;
        string password = null;
        string ivKey = null;
        bool settingsSet = false;
        int assetCompress = 0;
        int binCompress = 0;
        string stringBuffer;
        HalvaPackage audioPackage;
        HalvaPackage assetsPackage;
        HalvaPackage databasePackage;
        HalvaPackage enginePackage;
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
                    case "--InitVector":
                        if (argnum <= args.Length - 1 && !args[argnum + 1].Contains("--"))
                        {
                            ivKey = args[argnum + 1].Replace("\"", "");
                            mustEncrypt = true;
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine(Properties.Resources.InitVectorSetText);
                            Console.ResetColor();

                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine(Properties.Resources.NoInitVectorSetText);
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
                    case "--AssetsCompression":
                        if (argnum <= args.Length - 1 && !args[argnum + 1].Contains("--"))
                        {

                            stringBuffer = args[argnum + 1];
                            if (int.TryParse(stringBuffer, out assetCompress) && assetCompress <= 3)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine(Properties.Resources.AssetsCompressionLevelSet);
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                Console.WriteLine(Properties.Resources.AssetsCompressionLevelNotSet);
                                Console.ResetColor();
                            }
                        }
                        break;
                    case "--BinCompression":
                        if (argnum <= args.Length - 1 && !args[argnum + 1].Contains("--"))
                        {

                            stringBuffer = args[argnum + 1];
                            if (int.TryParse(stringBuffer, out assetCompress) && assetCompress <= 3)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine(Properties.Resources.BinCompressionLevelSet);
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                Console.WriteLine(Properties.Resources.BinCompressionLevelNotSet);
                                Console.ResetColor();
                            }
                        }
                        break;
                    case "--RequireFS":
                        useMemoryStream = false;
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
                ConsoleKeyInfo tempChar = Console.ReadKey();
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

        string gameFolder = JsonReader.FindGameFolder(Path.Combine(projectLocation, "package.json"));
        if (gameFolder == "Unknown" || gameFolder == "Null")
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(Properties.Resources.ProjectFilesLocationNotFound);
        }
        else
        {
            if (!Directory.Exists(archiveDestination) && !string.IsNullOrEmpty(archiveDestination)) Directory.CreateDirectory(archiveDestination);
            string destinationPath = !string.IsNullOrEmpty(archiveDestination) ? archiveDestination : gameFolder;
            audioPackage = new HalvaPackage(useMemoryStream);
            assetsPackage = new HalvaPackage(useMemoryStream);
            databasePackage = new HalvaPackage(useMemoryStream);
            enginePackage = new HalvaPackage(useMemoryStream);
            if (mustEncrypt && password != null)
            {
                audioPackage.Password = password;
                assetsPackage.Password = password;
                databasePackage.Password = password;
                enginePackage.Password = password;
                if (!string.IsNullOrEmpty(ivKey) && !string.IsNullOrWhiteSpace(ivKey))
                {
                    audioPackage.IVKey = ivKey;
                    assetsPackage.IVKey = ivKey;
                    databasePackage.IVKey = ivKey;
                    enginePackage.IVKey = ivKey;
                }
            }
            assetsPackage.DestinationLocation = new StringBuilder(Path.Combine(destinationPath, "AssetsPackage.halva"));
            assetsPackage.CompressionOption = CheckLevel(assetCompress);
            audioPackage.DestinationLocation = new StringBuilder(Path.Combine(destinationPath, "AudioPackage.halva"));
            audioPackage.CompressionOption = CheckLevel(assetCompress);
            databasePackage.DestinationLocation = new StringBuilder(Path.Combine(destinationPath, "DatabasePackage.halva"));
            databasePackage.CompressionOption = CheckLevel(assetCompress);
            enginePackage.DestinationLocation = new StringBuilder(Path.Combine(destinationPath, "EnginePackage.halva"));
            enginePackage.CompressionOption = CheckLevel(assetCompress);
            try
            {
                Task buildAssets = Task.Run(() =>
                {
                    Console.WriteLine(Properties.Resources.CompressingAssetsText);
                    assetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackage.GetFolderCharacter(), "") + HalvaPackage.GetFolderCharacter() + "img");
                    if (Directory.Exists(Path.Combine(gameFolder, "fonts"))) assetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackage.GetFolderCharacter(), "") + HalvaPackage.GetFolderCharacter() + "fonts");
                    if (Directory.Exists(Path.Combine(gameFolder, "css"))) assetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackage.GetFolderCharacter(), "") + HalvaPackage.GetFolderCharacter() + "css");
                    if (Directory.Exists(Path.Combine(gameFolder, "effects"))) assetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackage.GetFolderCharacter(), "") + HalvaPackage.GetFolderCharacter() + "effects");
                    if (Directory.Exists(Path.Combine(gameFolder, "movies"))) assetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackage.GetFolderCharacter(), "") + HalvaPackage.GetFolderCharacter() + "movies");
                    if (Directory.Exists(Path.Combine(gameFolder, "icon"))) assetsPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackage.GetFolderCharacter(), "") + HalvaPackage.GetFolderCharacter() + "icon");
                    assetsPackage.Finish();
                    assetsPackage.Dispose();
                    Console.WriteLine(Properties.Resources.AssetsCompressedText);
                });
                Task buildAudio = Task.Run(() =>
                {
                    Console.WriteLine(Properties.Resources.CompressingAudioFilesText);
                    audioPackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackage.GetFolderCharacter(), "") + HalvaPackage.GetFolderCharacter() + "audio");
                    audioPackage.Finish();
                    audioPackage.Dispose();
                    Console.WriteLine(Properties.Resources.AudioCompressedText);
                });
                Task buildDatabase = Task.Run(() =>
                {
                    Console.WriteLine(Properties.Resources.CompressingDatabaseText);
                    databasePackage.CompressionOption = CheckLevel(binCompress);
                    databasePackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackage.GetFolderCharacter(), "") + HalvaPackage.GetFolderCharacter() + "data");
                    databasePackage.Finish();
                    databasePackage.Dispose();
                    Console.WriteLine(Properties.Resources.DatabaseCompressedText);
                });
                Task buildEngine = Task.Run(() =>
                {
                    Console.WriteLine(Properties.Resources.CompressingEngineFilesText);
                    enginePackage.CompressionOption = CheckLevel(binCompress);
                    enginePackage.AddFilesFromAFolder(projectLocation, gameFolder.Replace(projectLocation + HalvaPackage.GetFolderCharacter(), "") + HalvaPackage.GetFolderCharacter() + "js");
                    if (gameFolder == projectLocation) enginePackage.AddFileToList(gameFolder, "index.html");
                    else
                    {
                        string relativeLocation = gameFolder.Replace(projectLocation + HalvaPackage.GetFolderCharacter(), "");
                        enginePackage.AddFileToList(projectLocation, Path.Combine(relativeLocation, "index.html"));
                    }
                    enginePackage.AddFileToList(projectLocation, "package.json");
                    enginePackage.Finish();
                    enginePackage.Dispose();
                    Console.WriteLine(Properties.Resources.EngineCompressedText);
                });
                await Task.WhenAll(buildAssets, buildAudio, buildDatabase, buildEngine);

            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(e);
            }
        }
    }

    private static CompressionLevel CheckLevel(int level) => level switch
    {
        3 => CompressionLevel.NoCompression,
        2 => CompressionLevel.Fastest,
        1 => CompressionLevel.SmallestSize,
        _ => CompressionLevel.Optimal,
    };
}
