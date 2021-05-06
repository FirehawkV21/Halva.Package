using System.IO;
using System.Linq;
using Xunit;
using Halva.Package.Core.Utilities;
using Halva.Package.Core.Manager;

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
        }

        [Fact]
        public void ArchiveBuilderTest()
        {
            Cleanup();
            HalvaPackage package = new HalvaPackage(sourceFolder, destinationArchive);
            package.CloseArchive();
            PackageUtilities.ExportFromArchive(destinationArchive, destinationFolder);
            package.Dispose();
        }

        [Fact]
        public void CanArchiveBuilderExtract()
        {
            HalvaPackage package = new HalvaPackage(PackageUtilities.TempArchive, destinationArchive);
            package.ExtractFile("TestImage.webp", Path.Combine(destinationFolder, "TestImage.webp"));
            package.Dispose();
        }

        [Fact]
        public void CanArchiveRemoveEntry()
        {
            HalvaPackage package = new HalvaPackage(PackageUtilities.TempArchive, destinationArchive);
            package.RemoveFileFromList("TestImage.webp");
            package.CloseArchive();
            if(Directory.Exists(destinationFolder)) Directory.Delete(destinationFolder, true);
            PackageUtilities.ExportFromArchive(destinationArchive, destinationFolder);
            Assert.Equal(2, Directory.EnumerateFiles(destinationFolder).Count());
            package.Dispose();
        }

        [Fact]
        public void CanArchiveAddEntry()
        {
            HalvaPackage package = new HalvaPackage(PackageUtilities.TempArchive, destinationArchive);
            package.AddFileToList(Path.Combine(sourceFolder, "TestImage.webp"));
            package.CloseArchive();
            if (Directory.Exists(destinationFolder)) Directory.Delete(destinationFolder, true);
            PackageUtilities.ExportFromArchive(destinationArchive, destinationFolder);
            Assert.Equal(3, Directory.EnumerateFiles(destinationFolder).Count());
            package.Dispose();
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

        [Fact]
        public void CanLibraryCheckForDifferences()
        {
            HalvaPackage package = new HalvaPackage(destinationArchive);
            package.UpdateFromArchive(destinationFolder);
            package.Dispose();
        }

        [Fact]
        public void CanLibraryUpdateArhive()
        {
            HalvaPackage package = new HalvaPackage(destinationArchive);
            package.UpdateArchive(sourceFolder);
            package.Dispose();
        }

    }
}
