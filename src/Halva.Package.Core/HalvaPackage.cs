using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Halva.Package.Core
{
    public class HalvaPackage
    {
        private StringBuilder sourceLocation;
        private StringBuilder destinationLocation;
        private List<string> fileList = new List<string>();
        private ZipArchive archiveMemoryStream;

        /// <summary>
        /// Creates a Halva package using the source folder as the input. It will automatically put the files in the input folder to a temporary archive.
        /// </summary>
        /// <param name="source">The source folder.</param>
        /// <param name="destination">The location of the archive.</param>
        public HalvaPackage(string source, string destination)
        {
            sourceLocation = new StringBuilder(source);
            destinationLocation = new StringBuilder(destination);
            List<string> foundFilesList = PullFiles(source);
            archiveMemoryStream = ZipFile.Open(PackageUtilities.TempArchive, ZipArchiveMode.Create);
            foreach (string file in foundFilesList)
            {
                AddFileToList(file);
            }
        }

        /// <summary>
        /// Creates a Halva package in memory. This is used to update an archive or export some files.
        /// </summary>
        /// <param name="source">The source archive.</param>
        public HalvaPackage(string source)
        {
            sourceLocation = new StringBuilder(Path.GetDirectoryName(source));
            destinationLocation = new StringBuilder(source);
            PackageUtilities.DecompressArchive(destinationLocation.ToString());
            archiveMemoryStream = ZipFile.Open(PackageUtilities.TempArchive, ZipArchiveMode.Update);
            PullFiles(archiveMemoryStream);
        }

        /// <summary>
        /// Adds a specified file to the list. This method only works if the file is in the source folder.
        /// </summary>
        /// <param name="fileLocation">The location of the file.</param>
        public void AddFileToList(string fileLocation)
        {
            fileList.Add(fileLocation.Replace(sourceLocation.ToString() + PackageUtilities.GetFolderCharacter(), ""));
            archiveMemoryStream.CreateEntryFromFile(fileLocation,fileLocation.Replace(sourceLocation + PackageUtilities.GetFolderCharacter(), ""),CompressionLevel.NoCompression);
        }

        public void RemoveFileFromList(string fileLocation)
        {
            var entry = archiveMemoryStream.GetEntry(fileLocation);
            if (entry == null) return;
            entry.Delete();
            fileList.Remove(fileLocation);
        }

        /// <summary>
        /// Finds all of the files in a folder.
        /// </summary>
        /// <param name="source">The folder to scan for files.</param>
        /// <returns>A list of files in the specified folder.</returns>
        public List<string> PullFiles(string source)
        {
            IEnumerable<string> foundFiles = Directory.EnumerateFiles(source, "*",SearchOption.AllDirectories);
            return foundFiles.ToList();
        }

        /// <summary>
        /// Pulls a list of files from the archive.
        /// </summary>
        /// <param name="inputStream">The input archive.</param>
        /// <returns>A list of files in the archive.</returns>
        public List<string> PullFiles(ZipArchive inputStream)
        {
           List<string> foundFiles = new List<string>();
           foreach (ZipArchiveEntry entry in inputStream.Entries)
           {
               foundFiles.Add(entry.FullName);
           }

           return foundFiles;
        }

        public void CloseArchive()
        {
            archiveMemoryStream.Dispose();
            PackageUtilities.CompressArchive(PackageUtilities.TempArchive, destinationLocation.ToString());
        }

        public void ExtractFile(string entry, string exportLocation)
        {
            var candidateFile = archiveMemoryStream.GetEntry(entry);
            candidateFile.ExtractToFile(exportLocation, true);

        }
    }
}
