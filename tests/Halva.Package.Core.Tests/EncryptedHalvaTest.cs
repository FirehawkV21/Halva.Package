using System.IO;
using System.Linq;
using Xunit;
using Halva.Package.Core.Managers;

namespace Halva.Package.Core.Tests;

public class EncryptedHalvaTest
{
    private readonly string sourceFolder = "SampleFiles";
    private readonly string destinationArchive = "SampleFiles4.halva";
    private readonly string destinationFolder = "SampleFiles4";
    private readonly string testPassword = "1234567890abcdefghijklm";
    private readonly string ivKey = "mlkjihgfedcba0987654321";

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
        PackageBuilder package = new(sourceFolder, false, testPassword);
        package.AddFilesFromAFolder(sourceFolder, Path.GetPathRoot(sourceFolder));
        package.Finish();
        PackageUtilities.ExportFromArchive(destinationArchive, destinationFolder, false,testPassword);
    }

    [Fact]
    public void ArchiveBuilderTest2()
    {
        Cleanup();
        PackageBuilder package = new(destinationArchive, false, testPassword, ivKey);
        package.AddFilesFromAFolder(sourceFolder, Path.GetPathRoot(sourceFolder));
        package.Finish();
        PackageUtilities.ExportFromArchive(destinationArchive, destinationFolder, false, testPassword, ivKey);
    }

    [Fact]
    public void CanArchiveBuilderExtract()
    {
        PackageReader package = new(destinationArchive, false, testPassword);
        package.ExtractFile("TestImage.webp", Path.Combine(destinationFolder, "TestImage.webp"));
    }

    [Fact]
    public void CanArchiveBuilderExtractWithIV()
    {
        PackageReader package = new(destinationArchive, false, testPassword, ivKey);
        package.ExtractFile("TestImage.webp", Path.Combine(destinationFolder, "TestImage.webp"));
    }

    [Fact]
    public void CanLibraryCheckForDifferencesInEncryptedArchives()
    {
        PackageReader package = new(destinationArchive, false, testPassword, ivKey);
        package.UpdateFromArchive(destinationFolder);
    }

    [Fact]
    public void CanLibraryCheckForDifferencesInEncryptedArchivesWithIV()
    {
        PackageReader package = new(destinationArchive, false, testPassword, ivKey);
        package.UpdateFromArchive(destinationFolder);
    }

}
