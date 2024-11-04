using Xunit;
using Halva.Package.Core.Managers;

namespace Halva.Package.Core.Tests;

public class InMemoryEncryptedHalvaTest
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
        PackageBuilder package = new(destinationArchive, true, testPassword);
        package.Commit();
        PackageUtilities.ExportFromArchive(destinationArchive, destinationFolder, true, testPassword);
    }

    [Fact]
    public async Task ArchiveBuilderAyncTest()
    {
        Cleanup();
        PackageBuilder package = new(destinationArchive, true, testPassword);
        await package.CommitAsync();
        await PackageUtilities.ExportFromArchiveAsync(destinationArchive, destinationFolder, true, testPassword);
    }

    [Fact]
    public void ArchiveBuilderTest2()
    {
        Cleanup();
        PackageBuilder package = new(destinationArchive, true, testPassword, ivKey);
        package.Commit();
        PackageUtilities.ExportFromArchive(destinationArchive, destinationFolder, true, testPassword, ivKey);
    }

    [Fact]
    public async Task ArchiveBuilderTestAsync2()
    {
        Cleanup();
        PackageBuilder package = new(destinationArchive, true, testPassword, ivKey);
        await package.CommitAsync();
        await PackageUtilities.ExportFromArchiveAsync(destinationArchive, destinationFolder, true, testPassword, ivKey);
    }

    [Fact]
    public void CanArchiveBuilderExtract()
    {
        PackageReader package = new(destinationArchive, true, testPassword);
        package.ExtractFile("TestImage.webp", Path.Combine(destinationFolder, "TestImage.webp"));
    }

    [Fact]
    public async Task CanArchiveBuilderExtractAsync()
    {
        PackageReader package = new(destinationArchive, true, testPassword);
        await package.ExtractFileAsync("TestImage.webp", Path.Combine(destinationFolder, "TestImage.webp"));
    }

    [Fact]
    public void CanArchiveBuilderExtractWithIV()
    {
        PackageReader package = new(destinationArchive, true, testPassword, ivKey);
        package.ExtractFile("TestImage.webp", Path.Combine(destinationFolder, "TestImage.webp"));
    }

    [Fact]
    public async Task CanArchiveBuilderExtractAsyncWithIV()
    {
        PackageReader package = new(destinationArchive, true, testPassword, ivKey);
        await package.ExtractFileAsync("TestImage.webp", Path.Combine(destinationFolder, "TestImage.webp"));
    }

    [Fact]
    public void CanLibraryCheckForDifferencesInEncryptedArchives()
    {
        PackageReader package = new(destinationArchive, true, testPassword);
        package.UpdateFromArchive(destinationFolder);
    }

    [Fact]
    public async Task CanLibraryCheckForDifferencesInEncryptedArchivesAsync()
    {
        PackageReader package = new(destinationArchive, true, testPassword);
        await package.UpdateFromArchiveAsync(destinationFolder);
    }

    [Fact]
    public void CanLibraryCheckForDifferencesInEncryptedArchivesWithIV()
    {
        PackageReader package = new(destinationArchive, true, testPassword, ivKey);
        package.UpdateFromArchive(destinationFolder);
    }

    [Fact]
    public async Task CanLibraryCheckForDifferencesInEncryptedArchivesAsyncWithIV()
    {
        PackageReader package = new(destinationArchive, true, testPassword, ivKey);
        await package.UpdateFromArchiveAsync(destinationFolder);
    }

}
