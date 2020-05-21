using System.IO;
using System.Linq;
using Xunit;

namespace Halva.Package.Core.Tests

{
    public class HalvaClassTest
    {

        private string sourceFolder = "SampleFiles";
        private string destinationArchive = "SampleFiles2.halva";
        private string destinationFolder = "SampleFiles2";

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
            HalvaPackage package = new HalvaPackage(sourceFolder, destinationArchive);
            package.CloseArchive();
            PackageUtilities.ExportFromArchive(destinationArchive, destinationFolder);
        }

        [Fact]
        public void CanArchiveBuilderExtract()
        {
            HalvaPackage package = new HalvaPackage(destinationArchive);
            package.ExtractFile("TestImage.webp", Path.Combine(destinationFolder, "TestImage.webp"));
        }

        [Fact]
        public void CanArchiveRemoveEntry()
        {
            HalvaPackage package = new HalvaPackage(destinationArchive);
            package.RemoveFileFromList("TestImage.webp");
            package.CloseArchive();
            if(Directory.Exists(destinationFolder)) Directory.Delete(destinationFolder, true);
            PackageUtilities.ExportFromArchive(destinationArchive, destinationFolder);
            Assert.Equal(2, Directory.EnumerateFiles(destinationFolder).Count());
        }

        [Fact]
        public void CanArchiveAddEntry()
        {
            HalvaPackage package = new HalvaPackage(destinationArchive);
            package.AddFileToList(Path.Combine(sourceFolder, "TestImage.webp"));
            package.CloseArchive();
            if (Directory.Exists(destinationFolder)) Directory.Delete(destinationFolder, true);
            PackageUtilities.ExportFromArchive(destinationArchive, destinationFolder);
            Assert.Equal(3, Directory.EnumerateFiles(destinationFolder).Count());
        }

        [Fact]
        public void CanLibrarySaveChanges()
        {
            HalvaPackage package = new HalvaPackage(destinationArchive);
            package.RemoveFileFromList("TestImage.webp");
            package.Save();
            package.AddFileToList(Path.Combine(sourceFolder, "TestImage.webp"));
            package.Save();
        }

    }
}
