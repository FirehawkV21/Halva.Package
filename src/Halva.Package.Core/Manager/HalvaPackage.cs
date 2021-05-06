using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Halva.Package.Core.Utilities;

namespace Halva.Package.Core.Manager
{
    /// <summary>
    /// This is the class that handles unencrypted Halva packages.
    /// </summary>
    public class HalvaPackage: HalvaPackageBase
    {
        /// <summary>
        /// Creates a Halva package using the source folder as the input. It will automatically put the files in the input folder to a temporary archive.
        /// </summary>
        /// <param name="workingarchive">The temporary archive location used to temporaily save data.</param>
        /// <param name="source">The source folder.</param>
        /// <param name="destination">The location of the archive.</param>
        public HalvaPackage(string workingarchive, string source, string destination)
        {
            SourceLocation = new StringBuilder(source);
            WorkingArchive = workingarchive;
            DestinationLocation = new StringBuilder(destination);
            List<string> foundFilesList = PullFiles(source);
            ArchiveMemoryStream = ZipFile.Open(WorkingArchive, ZipArchiveMode.Create);
            foreach (string file in foundFilesList)
            {
                AddFileToList(file);
            }
        }

        /// <summary>
        /// Creates a Halva package 
        /// </summary>
        /// <param name="workingarchive">The temporary archive location used to temporaily save data.</param>
        public HalvaPackage(string workingarchive)
        {
            WorkingArchive = workingarchive;
            ArchiveMemoryStream = ZipFile.Open(WorkingArchive, ZipArchiveMode.Create);
        }

        /// <summary>
        /// Creates a Halva package in memory. This is used to update an archive or export some files.
        /// </summary>
        /// <param name="source">The source archive.</param>
        public HalvaPackage(string workingarchive, string source)
        {
            SourceLocation = new StringBuilder(Path.GetDirectoryName(source));
            DestinationLocation = new StringBuilder(source);
            PackageUtilities.DecompressArchive(DestinationLocation.ToString());
            ArchiveMemoryStream = ZipFile.Open(WorkingArchive, ZipArchiveMode.Update);
            PullFiles(ArchiveMemoryStream);
        }

        /// <summary>
        /// Saves current changes to the destination archive.
        /// </summary>
        public void Save()
        {
            CloseArchive();
            ReloadArchive();
        }

        /// <summary>
        /// Closes the archive.
        /// </summary>
        public void CloseArchive()
        {
            ArchiveMemoryStream.Dispose();
            PackageUtilities.CompressArchive(WorkingArchive, DestinationLocation.ToString());
        }

        /// <summary>
        /// Reloads the archive for editing.
        /// </summary>
        public void ReloadArchive()
        {
            PackageUtilities.DecompressArchive(DestinationLocation.ToString());
            ArchiveMemoryStream = ZipFile.Open(WorkingArchive, ZipArchiveMode.Update);
        }
    }
}
