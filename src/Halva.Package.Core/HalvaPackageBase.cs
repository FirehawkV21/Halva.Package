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
        protected StringBuilder DestinationLocation { get; set; }
        List<string> FileList { get; set; } = new List<string>();
        protected ZipArchive ArchiveMemoryStream { get; set; }

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
        public static List<string> PullFiles(string source)
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
