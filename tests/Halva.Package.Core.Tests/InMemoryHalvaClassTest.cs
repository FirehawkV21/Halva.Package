﻿using Xunit;
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
        PackageBuilder package = new(destinationArchive, true);
        package.Commit();
        PackageUtilities.ExportFromArchive(destinationArchive, destinationFolder, true);
        package.Dispose();
    }

    [Fact]
    public async Task ArchiveBuilderAsyncTest()
    {
        Cleanup();
        PackageBuilder package = new(destinationArchive, true);
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
