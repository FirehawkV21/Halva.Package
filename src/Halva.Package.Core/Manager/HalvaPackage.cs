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
        /// <param name="source">The source folder.</param>
        /// <param name="destination">The location of the archive.</param>
        public HalvaPackage(string source, string destination)
        {
            SourceLocation = new StringBuilder(source);
            WorkingArchive = ReserveRandomArchive();
            DestinationLocation = new StringBuilder(destination);
            List<string> foundFilesList = PullFilesFromFolder(source);
            ArchiveMemoryStream = ZipFile.Open(WorkingArchive, ZipArchiveMode.Create);
            foreach (string file in foundFilesList)
            {
                AddFileToList(file);
            }
        }

        /// <summary>
        /// Creates an empty Halva package 
        /// </summary>
        public HalvaPackage()
        {
            WorkingArchive = ReserveRandomArchive();
            ArchiveMemoryStream = ZipFile.Open(WorkingArchive, ZipArchiveMode.Create);
        }

        /// <summary>
        /// Creates a Halva package in memory. This is used to update an archive or export some files.
        /// </summary>
        /// <param name="source">The source archive.</param>
        public HalvaPackage(string source)
        {
            WorkingArchive = ReserveRandomArchive();
            SourceLocation = new StringBuilder(Path.GetDirectoryName(source));
            DestinationLocation = new StringBuilder(source);
            PackageUtilities.DecompressArchive(DestinationLocation.ToString(), WorkingArchive);
            ArchiveMemoryStream = ZipFile.Open(WorkingArchive, ZipArchiveMode.Update);
            FileList = PullFiles(ArchiveMemoryStream);
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
            PackageUtilities.CompressArchive(WorkingArchive, DestinationLocation.ToString(), UseAggresiveCompression);
#else
            PackageUtilities.CompressArchive(WorkingArchive, DestinationLocation.ToString());
#endif
        }

        /// <summary>
        /// Reloads the archive for editing.
        /// </summary>
        public void ReloadArchive()
        {
            PackageUtilities.DecompressArchive(DestinationLocation.ToString(), WorkingArchive);
            ArchiveMemoryStream = ZipFile.Open(WorkingArchive, ZipArchiveMode.Update);
        }
    }
}
