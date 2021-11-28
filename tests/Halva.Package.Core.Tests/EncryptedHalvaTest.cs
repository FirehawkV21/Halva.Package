using System.IO;
using System.Linq;
using Xunit;
using Halva.Package.Core.Utilities;
using Halva.Package.Core.Manager;

namespace Halva.Package.Core.Tests
{
    public class EncryptedHalvaTest
    {
        private string sourceFolder = "SampleFiles";
        private string destinationArchive = "SampleFiles4.halva";
        private string destinationFolder = "SampleFiles4";
        private string testPassword = "1234567890abcdefghijklm";

        private void Cleanup()
        {
            if (File.Exists(destinationArchive)) File.Delete(destinationArchive);
            if (Directory.Exists(destinationFolder)) Directory.Delete(destinationFolder, true);
            if (File.Exists(PackageUtilities.TempArchive)) File.Delete(PackageUtilities.TempArchive);
        }

        [Fact]
        public void ArchiveBuilderTest()
        {
            Cleanup();
            EncryptedHalvaPackage package = new(sourceFolder, destinationArchive, testPassword);
            package.CloseArchive();
            EncryptedPackageUtilities.ExportFromArchive(destinationArchive, destinationFolder, testPassword);
        }

        [Fact]
        public void CanArchiveBuilderExtract()
        {
            EncryptedHalvaPackage package = new(destinationArchive, testPassword);
            package.ExtractFile("TestImage.webp", Path.Combine(destinationFolder, "TestImage.webp"));
        }

        [Fact]
        public void CanArchiveRemoveEntry()
        {
            EncryptedHalvaPackage package = new(destinationArchive, testPassword);
            package.RemoveFileFromList("TestImage.webp");
            package.CloseArchive();
            if (Directory.Exists(destinationFolder)) Directory.Delete(destinationFolder, true);
            EncryptedPackageUtilities.ExportFromArchive(destinationArchive, destinationFolder, testPassword);
            Assert.Equal(2, Directory.EnumerateFiles(destinationFolder).Count());
        }

        [Fact]
        public void CanArchiveAddEntry()
        {
            EncryptedHalvaPackage package = new(destinationArchive, testPassword);
            package.AddFileToList(Path.Combine(sourceFolder, "TestImage.webp"));
            package.CloseArchive();
            if (Directory.Exists(destinationFolder)) Directory.Delete(destinationFolder, true);
            EncryptedPackageUtilities.ExportFromArchive(destinationArchive, destinationFolder, testPassword);
            Assert.Equal(3, Directory.EnumerateFiles(destinationFolder).Count());
        }

        [Fact]
        public void CanLibrarySaveChanges()
        {
            EncryptedHalvaPackage package = new(destinationArchive, testPassword);
            package.RemoveFileFromList("TestImage.webp");
            package.Save();
            package.AddFileToList(Path.Combine(sourceFolder, "TestImage.webp"));
            package.Save();
        }

        [Fact]
        public void CanLibraryCheckForDifferencesInEncryptedArchives()
        {
            EncryptedHalvaPackage package = new(destinationArchive, testPassword);
            package.UpdateFromArchive(destinationFolder);
        }

    }
}
