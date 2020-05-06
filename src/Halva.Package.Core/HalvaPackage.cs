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

        public HalvaPackage(string source, string destination)
        {
            sourceLocation = new StringBuilder(source);
            destinationLocation = new StringBuilder(destination);
            List<string> foundFilesList = PullFiles(source);
            archiveMemoryStream = ZipFile.Open(PackageUtilities.TempArchive + "2", ZipArchiveMode.Create);
            foreach (string file in foundFilesList)
            {
                AddFileToList(file);
            }
        }

        public HalvaPackage(string source)
        {
            sourceLocation = new StringBuilder(source);
            destinationLocation = new StringBuilder(source);
            OpenArchiveToMemory();
        }

        public void AddFileToList(string fileLocation)
        {
            fileList.Add(fileLocation);
            archiveMemoryStream.CreateEntryFromFile(fileLocation,fileLocation.Replace(sourceLocation.ToString(), ""),CompressionLevel.NoCompression);
        }

        public void RemoveFileFromList(string fileLocation)
        {
            var entry = archiveMemoryStream.GetEntry(fileLocation);
            if (entry == null) return;
            entry.Delete();
            fileList.Remove(fileLocation);
        }

        public void OpenArchiveToMemory()
        {
            using (MemoryStream tempMemoryStream = new MemoryStream())
            using (FileStream inputFileStream = File.OpenRead(sourceLocation.ToString()))
            using (BrotliStream decompresorStream = new BrotliStream(inputFileStream, CompressionMode.Decompress))
            {
                decompresorStream.CopyTo(tempMemoryStream);
                archiveMemoryStream = new ZipArchive(tempMemoryStream);
                fileList = PullFiles(archiveMemoryStream);
            }
        }

        public List<string> PullFiles(string source)
        {
            IEnumerable<string> foundFiles = Directory.EnumerateFiles(source);
            return foundFiles.ToList();
        }

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
            PackageUtilities.CompressArchive(PackageUtilities.TempArchive + "2", destinationLocation.ToString());
        }
    }
}
