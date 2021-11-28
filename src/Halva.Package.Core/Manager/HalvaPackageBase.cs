using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Halva.Package.Core.Manager
{
    /// <summary>
    /// This is the base class for the Halva Package. You should use either HalvaPackage or EncryptedHalvaPackage.
    /// </summary>
    public class HalvaPackageBase : IDisposable
    {
        private bool disposedValue;

        protected StringBuilder SourceLocation { get; set; } 
        public StringBuilder DestinationLocation { get; set; }
        public List<string> FileList { get; set; } = new List<string>();
        protected ZipArchive ArchiveMemoryStream { get; set; }
        public string WorkingArchive { get; set; }

        /// <summary>
        /// Gets the character used for path designation.
        /// </summary>
        /// <returns>Either "\\" (in Windows) or "/" (Unix systems).</returns>
        public static string GetFolderCharacter()
        {
            return (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ? "\\" : "/";
        }

        public static string ReserveRandomArchive()
        {
            string tempString = "TempArchive_";
            Random _random = new();
            int check;
            do
            {
                check = _random.Next(99999);
            }
            while (File.Exists(Path.Combine(Path.GetTempPath(), tempString + check + ".tmp")));
            return Path.Combine(Path.GetTempPath(), tempString + check + ".tmp");
        }

        /// <summary>
        /// Adds a specified file to the list. This method only works if the file is in the source folder.
        /// </summary>
        /// <param name="fileLocation">The location of the file.</param>
        public void AddFileToList(string fileLocation)
        {
            FileList.Add(fileLocation.Replace(SourceLocation + GetFolderCharacter(), ""));
            ArchiveMemoryStream.CreateEntryFromFile(fileLocation, fileLocation.Replace(SourceLocation + GetFolderCharacter(), ""), CompressionLevel.NoCompression);
        }

        /// <summary>
        /// Adds a specified file to the list. This method is better suited for preserving the folder structure.
        /// </summary>
        /// <param name="source">The base folder that holds the file.</param>
        /// <param name="fileRelativeLocation">The relative location of the file.</param>
        public void AddFileToList(string source, string fileRelativeLocation)
        {
                FileList.Add(fileRelativeLocation);
                ArchiveMemoryStream.CreateEntryFromFile(Path.Combine(source, fileRelativeLocation), fileRelativeLocation, CompressionLevel.NoCompression);
        }

        /// <summary>
        /// Adds files from a specific folder.
        /// </summary>
        /// <param name="source">The location of the source folder.</param>
        public void AddFilesFromAFolder(string source)
        {
            var tempList = PullFiles(source);

            foreach(string fileEntry in tempList)
            {
                ArchiveMemoryStream.CreateEntryFromFile(fileEntry,
                    fileEntry.Replace(source + GetFolderCharacter(), ""), CompressionLevel.NoCompression);
                FileList.Add(fileEntry.Replace(source + GetFolderCharacter(), ""));
            }
            
        }

        /// <summary>
        /// Adds files from a specific folder. The folder relative location is used to avoid messing up the folder structure.
        /// </summary>
        /// <param name="sourceLocation">The location of the source folder</param>
        /// <param name="SourceFolderRelativeLocation">The relatice location of the source folder.</param>
        public void AddFilesFromAFolder(string sourceLocation, string SourceFolderRelativeLocation)
        {
            var tempList = PullFiles(Path.Combine(sourceLocation, SourceFolderRelativeLocation));

            foreach (string fileEntry in tempList)
            {
                ArchiveMemoryStream.CreateEntryFromFile(fileEntry,
                    fileEntry.Replace(sourceLocation + GetFolderCharacter(), ""), CompressionLevel.NoCompression);
                FileList.Add(fileEntry.Replace(sourceLocation + GetFolderCharacter(), ""));
            }

        }

        /// <summary>
        /// Removes a specified file from the archive.
        /// </summary>
        /// <param name="fileLocation">The file you want to remove. This must be in a relative path. ("folder1/file.extension")</param>
        public void RemoveFileFromList(string fileLocation)
        {
            var entry = ArchiveMemoryStream.GetEntry(fileLocation);
            if (entry == null) return;
            entry.Delete();
            FileList.Remove(fileLocation);
        }

        /// <summary>
        /// Finds all of the files in a folder.
        /// </summary>
        /// <param name="source">The folder to scan for files.</param>
        /// <returns>A list of files in the specified folder.</returns>
        public List<string> PullFiles(string source)
        {
            IEnumerable<string> foundFiles = Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories);
            return foundFiles.ToList();
        }

        /// <summary>
        /// Pulls a list of files from the archive.
        /// </summary>
        /// <param name="inputStream">The input archive.</param>
        /// <returns>A list of files in the archive.</returns>
        public static List<string> PullFiles(ZipArchive inputStream)
        {
            List<string> foundFiles = new();
            foreach (ZipArchiveEntry entry in inputStream.Entries)
            {
                foundFiles.Add(entry.FullName);
            }

            return foundFiles;
        }

        /// <summary>
        /// Extracts a specified file to a location specified.
        /// </summary>
        /// <param name="entry">The file you want to extract. This must be in a relative path. ("folder1\\file.extension")</param>
        /// <param name="exportLocation">The location where the file will be extracted to.</param>
        public void ExtractFile(string entry, string exportLocation)
        {
            var candidateFile = ArchiveMemoryStream.GetEntry(entry);
            
            candidateFile.ExtractToFile(exportLocation, true);

        }

        /// <summary>
        /// Exports the files that are different between the archive and the target folder.
        /// </summary>
        /// <param name="TargetFolder">The folder where the files to update are.</param>
        public void UpdateFromArchive(string TargetFolder)
        {
            foreach (ZipArchiveEntry entry in ArchiveMemoryStream.Entries)
            {
                if (File.Exists(Path.Combine(TargetFolder, entry.FullName)))
                {
                    string originalFileHash;
                    string targetFileHash;
                    using (var algo = SHA256.Create())
                    {
                        var archivedFile = entry.Open();
                        var targetFile = File.OpenRead(Path.Combine(TargetFolder, entry.FullName));
                        var originalFileSignature = algo.ComputeHash(archivedFile);
                        originalFileHash = BitConverter.ToString(originalFileSignature);
                        originalFileSignature = algo.ComputeHash(targetFile);
                        targetFileHash = BitConverter.ToString(originalFileSignature);
                        archivedFile.Dispose();
                        targetFile.Close();                        
                    }
                    if (originalFileHash != targetFileHash) 
                    {
                        ExtractFile(entry.FullName, Path.Combine(TargetFolder, entry.FullName));
                    }
                }
                else
                {
                    ExtractFile(entry.FullName, Path.Combine(TargetFolder, entry.FullName));
                }
            }
        }



        /// <summary>
        /// Updates the files in the archive with the files found in specified folder.
        /// </summary>
        /// <param name="SourceFolder">The folder specified.</param>
        public void UpdateArchive (string SourceFolder)
        {
            List<String> SourceFiles = new();
            SourceFiles = PullFiles(SourceFolder);
            foreach (string file in SourceFiles)
            {
                var entry = ArchiveMemoryStream.GetEntry(file.Replace(SourceFolder + GetFolderCharacter(), ""));
                if (entry != null)
                {
                    string originalFileHash;
                    string targetFileHash;
                    using (var algo = SHA256.Create())
                    {
                        var archivedFile = entry.Open();
                        var targetFile = File.OpenRead(file);
                        var originalFileSignature = algo.ComputeHash(archivedFile);
                        originalFileHash = BitConverter.ToString(originalFileSignature);
                        originalFileSignature = algo.ComputeHash(targetFile);
                        targetFileHash = BitConverter.ToString(originalFileSignature);
                        archivedFile.Dispose();
                        targetFile.Close();
                    }
                    if (originalFileHash != targetFileHash)
                    {
                        RemoveFileFromList(entry.FullName);
                        AddFileToList(SourceFolder, file.Replace(SourceFolder + GetFolderCharacter(), ""));
                    }
                }
                else
                {
                    AddFileToList(SourceFolder, file.Replace(SourceFolder + GetFolderCharacter(), ""));
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (DestinationLocation != null) DestinationLocation.Clear();
                    if (SourceLocation != null) SourceLocation.Clear();
                    FileList.Clear();
                    FileList = null;
                    ArchiveMemoryStream.Dispose();
                    if (File.Exists(WorkingArchive)) File.Delete(WorkingArchive);
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);

        }
    }
}
