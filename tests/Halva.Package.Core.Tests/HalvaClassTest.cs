﻿using System.IO;
using System.Linq;
using Xunit;
using Halva.Package.Core.Managers;

namespace Halva.Package.Core.Tests;

public class HalvaClassTest
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
        PackageBuilder package = new(destinationArchive);
        package.AddFilesFromAFolder(AppDomain.CurrentDomain.BaseDirectory, "SampleFiles");
        package.Commit();
        PackageUtilities.ExportFromArchive(destinationArchive, destinationFolder);
        package.Dispose();
    }

    [Fact]
    public async Task ArchiveBuilderAsyncTest()
    {
        Cleanup();
        PackageBuilder package = new(destinationArchive);
        package.AddFilesFromAFolder(AppDomain.CurrentDomain.BaseDirectory, "SampleFiles");
        await package.CommitAsync();
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
    public async Task CanArchiveBuilderExtractAsync()
    {
        PackageReader package = new(destinationArchive);
        await package.ExtractFileAsync("TestImage.webp", Path.Combine(destinationFolder, "TestImage.webp"));
        package.Dispose();
    }

    [Fact]
    public void CanLibraryCheckForDifferences()
    {
        PackageReader package = new(destinationArchive, false);
        package.UpdateFromArchive(destinationFolder);
        package.Dispose();
    }

    [Fact]
    public async Task CanLibraryCheckForDifferencesAsync()
    {
        PackageReader package = new(destinationArchive, false);
        await package.UpdateFromArchiveAsync(destinationFolder);
        package.Dispose();
    }
}
