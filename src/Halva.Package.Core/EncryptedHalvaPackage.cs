using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Halva.Package.Core
{
    public class EncryptedHalvaPackage : HalvaPackage
    {
        private string password { get; set; }
        public EncryptedHalvaPackage(string source, string destination) : base(source, destination)
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

        public EncryptedHalvaPackage(string source) : base(source)
        {
            sourceLocation = new StringBuilder(Path.GetDirectoryName(source));
            destinationLocation = new StringBuilder(source);
            EncryptedPackageUtilities.DecompressArchive(destinationLocation.ToString(), password);
            archiveMemoryStream = ZipFile.Open(PackageUtilities.TempArchive, ZipArchiveMode.Update);
            PullFiles(archiveMemoryStream);
        }


    }
}
