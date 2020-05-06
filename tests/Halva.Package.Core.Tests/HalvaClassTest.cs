using System.IO;
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
            if (Directory.Exists(destinationFolder)) Directory.Delete(destinationFolder);
            if (File.Exists(PackageUtilities.TempArchive + "2")) File.Delete(PackageUtilities.TempArchive + "2");
        }

        [Fact]
        public void ArchiveBuilderTest()
        {
            Cleanup();
            HalvaPackage package = new HalvaPackage(sourceFolder, destinationArchive);
            package.CloseArchive();
            PackageUtilities.ExportFromArchive(destinationArchive, destinationFolder);
        }


    }
}
