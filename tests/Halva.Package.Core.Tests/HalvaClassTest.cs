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
            if (File.Exists(PackageUtilities.TempArchive + "2")) File.Delete(PackageUtilities.TempArchive + "2");
        }

        [Fact]
        public void ArchiveBuilderTest()
        {
            Cleanup();
            HalvaPackage package = new HalvaPackage(Path.GetFullPath(sourceFolder), destinationArchive);
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

    }
}
