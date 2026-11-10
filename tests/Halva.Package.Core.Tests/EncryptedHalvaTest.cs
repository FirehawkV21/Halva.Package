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
    }

    [Fact]
    public void ArchiveBuilderTest()
    {
        Cleanup();
        PackageBuilder package = new(destinationArchive, testPassword);
        package.AddFilesFromAFolder(sourceFolder);
        package.Commit();
        PackageUtilities.DecompressPackageToFolder(destinationArchive, destinationFolder, testPassword);
    }

    [Fact]
    public async Task ArchiveBuilderTestAsync()
    {
        Cleanup();
        PackageBuilder package = new(destinationArchive, testPassword);
        package.AddFilesFromAFolder(sourceFolder);
        await package.CommitAsync(TestContext.Current.CancellationToken);
        await PackageUtilities.DecompressPackageToFolderAsync(destinationArchive, destinationFolder, testPassword, abortToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public void ArchiveBuilderTest2()
    {
        Cleanup();
        PackageBuilder package = new(destinationArchive, testPassword, ivKey);
        package.AddFilesFromAFolder(sourceFolder);
        package.Commit();
        PackageUtilities.DecompressPackageToFolder(destinationArchive, destinationFolder, testPassword, ivKey);
    }

    [Fact]
    public async Task ArchiveBuilderTest2Async()
    {
        Cleanup();
        PackageBuilder package = new(destinationArchive, testPassword, ivKey);
        package.AddFilesFromAFolder(sourceFolder);
        await package.CommitAsync(TestContext.Current.CancellationToken);
        await PackageUtilities.DecompressPackageToFolderAsync(destinationArchive, destinationFolder, testPassword, ivKey, TestContext.Current.CancellationToken);
    }

    [Fact]
    public void CanArchiveBuilderExtract()
    {
        PackageReader package = new(destinationArchive, testPassword);
        package.ExtractFile("TestImage.webp", Path.Combine(destinationFolder, "TestImage.webp"));
    }

    [Fact]
    public async Task CanArchiveBuilderExtractAsync()
    {
        PackageReader package = new(destinationArchive, testPassword);
        await package.ExtractFileAsync("TestImage.webp", Path.Combine(destinationFolder, "TestImage.webp"), TestContext.Current.CancellationToken);
    }

    [Fact]
    public void CanArchiveBuilderExtractWithIV()
    {
        PackageReader package = new(destinationArchive, testPassword, ivKey);
        package.ExtractFile("TestImage.webp", Path.Combine(destinationFolder, "TestImage.webp"));
    }

    [Fact]
    public async Task CanArchiveBuilderExtractAsyncWithIV()
    {
        PackageReader package = new(destinationArchive, testPassword, ivKey);
        await package.ExtractFileAsync("TestImage.webp", Path.Combine(destinationFolder, "TestImage.webp"), TestContext.Current.CancellationToken);
    }

    [Fact]
    public void CanLibraryCheckForDifferencesInEncryptedArchives()
    {
        PackageReader package = new(destinationArchive, testPassword);
        package.UpdateFromArchive(destinationFolder);
    }

    [Fact]
    public async Task CanLibraryCheckForDifferencesInEncryptedArchivesAsync()
    {
        PackageReader package = new(destinationArchive, testPassword);
        await package.UpdateFromArchiveAsync(destinationFolder, TestContext.Current.CancellationToken);
    }

    [Fact]
    public void CanLibraryCheckForDifferencesInEncryptedArchivesWithIV()
    {
        PackageReader package = new(destinationArchive, testPassword, ivKey);
        package.UpdateFromArchive(destinationFolder);
    }

    [Fact]
    public async Task CanLibraryCheckForDifferencesInEncryptedArchivesAsyncWithIVAsync()
    {
        PackageReader package = new(destinationArchive, testPassword, ivKey);
        await package.UpdateFromArchiveAsync(destinationFolder, TestContext.Current.CancellationToken);
    }

}
