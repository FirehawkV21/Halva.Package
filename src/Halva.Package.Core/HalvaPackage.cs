using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Halva.Package.Core
{
    class HalvaPackage
    {
        private StringBuilder sourceLocation;
        private StringBuilder destinationLocation;
        private List<string> fileList;

        public HalvaPackage(string source, string destination)
        {
            sourceLocation = new StringBuilder(source);
            destinationLocation = new StringBuilder(destination);
            fileList = PullFiles(sourceLocation.ToString());
        }

        public void AddFileToList(string fileLocation)
        {
            fileList.Add(fileLocation);
        }

        public void RemoveFileFromList(string fileLocation)
        {
            fileList.Remove(fileLocation);
        }

        public void OpenArchive()
        {

        }

        public List<string> PullFiles(string source)
        {
            IEnumerable<string> foundFiles = Directory.EnumerateFiles(source);
            return foundFiles.ToList();
        }

        public void CloseArchive()
        {
            if (File.Exists(PackageUtilities.TempArchive)) File.Delete(PackageUtilities.TempArchive);
            using (ZipArchive finalArchive = ZipFile.Open(PackageUtilities.TempArchive, ZipArchiveMode.Create))
            {
                foreach (string file in fileList)
                {
                    finalArchive.CreateEntryFromFile(file, file.Replace(sourceLocation.ToString(), ""), CompressionLevel.NoCompression);
                }
            }
            PackageUtilities.CompressArchive(PackageUtilities.TempArchive, destinationLocation.ToString());
        }
    }
}
