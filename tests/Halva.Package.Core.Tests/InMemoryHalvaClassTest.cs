using System.IO;
using System.Linq;
using Xunit;
using Halva.Package.Core.Managers;

namespace Halva.Package.Core.Tests;

public class InMemoryHalvaClassTest
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
        HalvaPackage package = new(sourceFolder, destinationArchive, true);
        package.Save();
        PackageUtilities.ExportFromArchive(destinationArchive, destinationFolder);
        package.Dispose();
    }

    [Fact]
    public void CanArchiveBuilderExtract()
    {
        HalvaPackage package = new(PackageUtilities.TempArchive, destinationArchive, true);
        package.ExtractFile("TestImage.webp", Path.Combine(destinationFolder, "TestImage.webp"));
        package.Dispose();
    }

    [Fact]
    public void CanArchiveRemoveEntry()
    {
        HalvaPackage package = new(PackageUtilities.TempArchive,  destinationArchive, true);
        package.RemoveFileFromList("TestImage.webp");
        package.Finish();
        if(Directory.Exists(destinationFolder)) Directory.Delete(destinationFolder, true);
        PackageUtilities.ExportFromArchive(destinationArchive, destinationFolder);
        Assert.Equal(2, Directory.EnumerateFiles(destinationFolder).Count());
        package.Dispose();
    }

    [Fact]
    public void CanArchiveAddEntry()
    {
        HalvaPackage package = new(PackageUtilities.TempArchive, destinationArchive, true);
        package.AddFileToList(Path.Combine(sourceFolder, "TestImage.webp"));
        package.Finish();
        if (Directory.Exists(destinationFolder)) Directory.Delete(destinationFolder, true);
        PackageUtilities.ExportFromArchive(destinationArchive, destinationFolder);
        Assert.Equal(3, Directory.EnumerateFiles(destinationFolder).Count());
        package.Dispose();
    }

    [Fact]
    public void CanLibrarySaveChanges()
    {
        HalvaPackage package = new(destinationArchive, true);
        package.RemoveFileFromList("TestImage.webp");
        package.Save();
        package.AddFileToList(Path.Combine(sourceFolder, "TestImage.webp"));
        package.Save();
    }

    [Fact]
    public void CanLibraryCheckForDifferences()
    {
        HalvaPackage package = new(destinationArchive, true);
        package.UpdateFromArchive(destinationFolder);
        package.Dispose();
    }

    [Fact]
    public void CanLibraryUpdateArhive()
    {
        HalvaPackage package = new(destinationArchive, true);
        package.UpdateArchive(sourceFolder);
        package.Dispose();
    }

}
