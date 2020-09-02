using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Halva.Package.Core
{
    public class HalvaPackage: HalvaPackageBase
    {
        /// <summary>
        /// Creates a Halva package using the source folder as the input. It will automatically put the files in the input folder to a temporary archive.
        /// </summary>
        /// <param name="source">The source folder.</param>
        /// <param name="destination">The location of the archive.</param>
        public HalvaPackage(string source, string destination)
        {
            SourceLocation = new StringBuilder(source);
            DestinationLocation = new StringBuilder(destination);
            List<string> foundFilesList = PullFiles(source);
            ArchiveMemoryStream = ZipFile.Open(PackageUtilities.TempArchive, ZipArchiveMode.Create);
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
            SourceLocation = new StringBuilder(Path.GetDirectoryName(source));
            DestinationLocation = new StringBuilder(source);
            PackageUtilities.DecompressArchive(DestinationLocation.ToString());
            ArchiveMemoryStream = ZipFile.Open(PackageUtilities.TempArchive, ZipArchiveMode.Update);
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
            PackageUtilities.CompressArchive(PackageUtilities.TempArchive, DestinationLocation.ToString());
        }

        /// <summary>
        /// Reloads the archive for editing.
        /// </summary>
        public void ReloadArchive()
        {
            PackageUtilities.DecompressArchive(DestinationLocation.ToString());
            ArchiveMemoryStream = ZipFile.Open(PackageUtilities.TempArchive, ZipArchiveMode.Update);
        }
    }
}
