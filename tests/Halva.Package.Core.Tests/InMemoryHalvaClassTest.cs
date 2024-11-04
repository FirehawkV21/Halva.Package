using Xunit;
using Halva.Package.Core.Managers;

namespace Halva.Package.Core.Tests;

public class InMemoryHalvaClassTest
{

    private readonly string sourceFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SampleFiles");
    private readonly string destinationArchive = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SampleFiles2.halva2");
    private readonly string destinationFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SampleFiles2");

    private void Cleanup()
    {
        if (File.Exists(destinationArchive)) File.Delete(destinationArchive);
        if (Directory.Exists(destinationFolder)) Directory.Delete(destinationFolder, true);
    }

    [Fact]
    public void ArchiveBuilderTest()
    {
        Cleanup();
        PackageBuilder package = new(destinationArchive, true);
        package.AddFilesFromAFolder(AppContext.BaseDirectory, "\\SampleFiles");
        package.Commit();
        PackageUtilities.ExportFromArchive(destinationArchive, destinationFolder, true);
        package.Dispose();
    }

    [Fact]
    public async Task ArchiveBuilderAsyncTest()
    {
        Cleanup();
        PackageBuilder package = new(destinationArchive, true);
        List<string> fileList = Directory.EnumerateFiles(sourceFolder).ToList();
        foreach (string file in fileList)
        {
            package.AddFileToList(Path.TrimEndingDirectorySeparator(sourceFolder), file.Replace(Path.TrimEndingDirectorySeparator(sourceFolder), ""));
        }
        await package.CommitAsync();
        await PackageUtilities.ExportFromArchiveAsync(destinationArchive, destinationFolder, true);
        package.Dispose();
    }

    [Fact]
    public void CanArchiveBuilderExtract()
    {
        PackageReader package = new(destinationArchive, true);
        package.ExtractFile("TestImage.webp", Path.Combine(destinationFolder, "TestImage.webp"));
        package.Dispose();
    }

    [Fact]
    public async Task CanArchiveBuilderExtractAsync()
    {
        PackageReader package = new(destinationArchive, true);
        await package.ExtractFileAsync("TestImage.webp", Path.Combine(destinationFolder, "TestImage.webp"));
        package.Dispose();
    }

    [Fact]
    public void CanLibraryCheckForDifferences()
    {
        PackageReader package = new(destinationArchive, true);
        package.UpdateFromArchive(destinationFolder);
        package.Dispose();
    }

    [Fact]
    public async Task CanLibraryCheckForDifferencesAsync()
    {
        PackageReader package = new(destinationArchive, true);
        await package.UpdateFromArchiveAsync(destinationFolder);
        package.Dispose();
    }

}
