using System.IO.Compression;
using System.Text;
using Halva.Package.Core.Utilities;

namespace Halva.Package.Core.Manager
{
    /// <summary>
    /// This is the class that handles the encrypted Halva packages.
    /// </summary>
    public class EncryptedHalvaPackage : HalvaPackageBase
    {
        
        /// <summary>
        /// The password for the archive.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Creates an encrypted Halva package in memory. This is used to update an archive or export some files.
        /// </summary>
        /// <param name="source">The archive you want to open.</param>
        /// <param name="pwsd">The password of the archive (if it's encrypted).</param>
        public EncryptedHalvaPackage(string source, string pwsd)
        {
            WorkingArchive = ReserveRandomArchive();
            Password = pwsd;
            SourceLocation = new StringBuilder(Path.GetDirectoryName(source));
            DestinationLocation = new StringBuilder(source);
            EncryptedPackageUtilities.DecompressArchive(DestinationLocation.ToString(), Password, WorkingArchive);
            ArchiveMemoryStream = ZipFile.Open(WorkingArchive, ZipArchiveMode.Update);
            PullFiles(ArchiveMemoryStream);
        }
        
        /// <summary>
        /// Creates an empty Encrypted Halva Package.
        /// </summary>
        public EncryptedHalvaPackage()
        {
            WorkingArchive = ReserveRandomArchive();
            ArchiveMemoryStream = ZipFile.Open(WorkingArchive, ZipArchiveMode.Create);
        }

        /// <summary>
        /// Creates an encrypted Halva package using the source folder as the input. It will automatically put the files in the input folder to a temporary archive.
        /// </summary>
        /// <param name="source">The folders that will work as source.</param>
        /// <param name="destination">The location of the final archive.</param>
        /// <param name="pwsd">The password for the encrypted archive.</param>
        public EncryptedHalvaPackage(string source, string destination, string pwsd)
        {
            WorkingArchive = ReserveRandomArchive();
            SourceLocation = new StringBuilder(source);
            DestinationLocation = new StringBuilder(destination);
            Password = pwsd;
            List<string> foundFilesList = HalvaPackageBase.PullFilesFromFolder(source);
            ArchiveMemoryStream = ZipFile.Open(WorkingArchive, ZipArchiveMode.Create);
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
            ArchiveMemoryStream.Dispose();
#if NET6_0_OR_GREATER
            EncryptedPackageUtilities.CompressArchive(WorkingArchive, DestinationLocation.ToString(), Password, UseAggresiveCompression);
#else
            EncryptedPackageUtilities.CompressArchive(WorkingArchive, DestinationLocation.ToString(), Password);
#endif
        }

        /// <summary>
        /// Reloads the archive for editing.
        /// </summary>
        public void ReloadArchive()
        {
            EncryptedPackageUtilities.DecompressArchive(DestinationLocation.ToString(), Password, WorkingArchive);
            ArchiveMemoryStream = ZipFile.Open(PackageUtilities.TempArchive, ZipArchiveMode.Update);
        }


    }
}
