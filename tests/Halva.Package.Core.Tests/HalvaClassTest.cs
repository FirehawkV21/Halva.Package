using System.IO;
using System.Linq;
using Xunit;
using Halva.Package.Core.Managers;

namespace Halva.Package.Core.Tests;

public class HalvaClassTest
{

    private readonly string sourceFolder = "SampleFiles";
    private readonly string destinationArchive = "SampleFiles2.halva";
    private readonly string destinationFolder = "SampleFiles2";

    private void Cleanup()
    {
        if (File.Exists(destinationArchive)) File.Delete(destinationArchive);
        if (Directory.Exists(destinationFolder)) Directory.Delete(destinationFolder, true);
    }

    [Fact]
    public void ArchiveBuilderTest()
    {
        Cleanup();
        PackageBuilder package = new(destinationArchive);
        package.Finish();
        PackageUtilities.ExportFromArchive(destinationArchive, destinationFolder);
        package.Dispose();
    }

    [Fact]
    public void CanArchiveBuilderExtract()
    {
        PackageReader package = new(destinationArchive);
        package.ExtractFile("TestImage.webp", Path.Combine(destinationFolder, "TestImage.webp"));
        package.Dispose();
    }

    [Fact]
    public void CanLibraryCheckForDifferences()
    {
        PackageReader package = new(destinationArchive, false);
        package.UpdateFromArchive(destinationFolder);
        package.Dispose();
    }
}
