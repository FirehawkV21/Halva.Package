using System.IO;
using System.Linq;
using Xunit;
using Halva.Package.Core.Managers;

namespace Halva.Package.Core.Tests;

public class EncryptedHalvaTest
{
    private readonly string sourceFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SampleFiles");
    private readonly string destinationArchive = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SampleFiles4.halva2");
    private readonly string destinationFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SampleFiles4");
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
        PackageBuilder package = new(destinationArchive, false, testPassword);
        package.AddFilesFromAFolder(sourceFolder);
        package.Commit();
        PackageUtilities.ExportFromArchive(destinationArchive, destinationFolder, false,testPassword);
    }

    [Fact]
    public void ArchiveBuilderTest2()
    {
        Cleanup();
        PackageBuilder package = new(destinationArchive, false, testPassword, ivKey);
        package.AddFilesFromAFolder(sourceFolder);
        package.Commit();
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
        PackageReader package = new(destinationArchive, false, testPassword);
        package.UpdateFromArchive(destinationFolder);
    }

    [Fact]
    public void CanLibraryCheckForDifferencesInEncryptedArchivesWithIV()
    {
        PackageReader package = new(destinationArchive, false, testPassword, ivKey);
        package.UpdateFromArchive(destinationFolder);
    }

}
