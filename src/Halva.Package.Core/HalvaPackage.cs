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
            archiveMemoryStream.Dispose();
            PackageUtilities.CompressArchive(PackageUtilities.TempArchive, destinationLocation.ToString());
        }

        /// <summary>
        /// Reloads the archive for editing.
        /// </summary>
        public void ReloadArchive()
        {
            PackageUtilities.DecompressArchive(destinationLocation.ToString());
            archiveMemoryStream = ZipFile.Open(PackageUtilities.TempArchive, ZipArchiveMode.Update);
        }
    }
}
