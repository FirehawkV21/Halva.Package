using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Halva.Package.Core
{
    public class EncryptedHalvaPackage : HalvaPackageBase
    {
        private string password { get; set; }

        public EncryptedHalvaPackage(string source, string pwsd)
        {
            password = pwsd;
            sourceLocation = new StringBuilder(Path.GetDirectoryName(source));
            destinationLocation = new StringBuilder(source);
            EncryptedPackageUtilities.DecompressArchive(destinationLocation.ToString(), password);
            archiveMemoryStream = ZipFile.Open(PackageUtilities.TempArchive, ZipArchiveMode.Update);
            PullFiles(archiveMemoryStream);
        }

        public EncryptedHalvaPackage(string source, string destination, string pwsd)
        {
            sourceLocation = new StringBuilder(source);
            destinationLocation = new StringBuilder(destination);
            password = pwsd;
            List<string> foundFilesList = PullFiles(source);
            archiveMemoryStream = ZipFile.Open(PackageUtilities.TempArchive, ZipArchiveMode.Create);
            foreach (string file in foundFilesList)
            {
                AddFileToList(file);
            }
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
            EncryptedPackageUtilities.CompressArchive(PackageUtilities.TempArchive, destinationLocation.ToString(), password);
        }

        /// <summary>
        /// Reloads the archive for editing.
        /// </summary>
        public void ReloadArchive()
        {
            EncryptedPackageUtilities.DecompressArchive(destinationLocation.ToString(), password);
            archiveMemoryStream = ZipFile.Open(PackageUtilities.TempArchive, ZipArchiveMode.Update);
        }


    }
}
