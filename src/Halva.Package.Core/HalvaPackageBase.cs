using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Halva.Package.Core
{
    public class HalvaPackageBase
    {
        protected StringBuilder SourceLocation { get; set; } 
        public StringBuilder DestinationLocation { get; set; }
        public List<string> FileList { get; set; } = new List<string>();
        protected ZipArchive ArchiveMemoryStream { get; set; }
        public string WorkingArchive { get; set; }

        public static string GetFolderCharacter()
        {
            return (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ? "\\" : "/";
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
            List<string> foundFiles = new List<string>();
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
    }
}
