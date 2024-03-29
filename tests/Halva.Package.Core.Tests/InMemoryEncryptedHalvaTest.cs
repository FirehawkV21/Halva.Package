﻿using System.IO;
using System.Linq;
using Xunit;
using Halva.Package.Core.Utilities;
using Halva.Package.Core.Manager;

namespace Halva.Package.Core.Tests;

public class InMemoryEncryptedHalvaTest
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
        HalvaPackage package = new(PassKey: testPassword, sourceFolder, destinationArchive, true);
        package.Finish();
        EncryptedPackageUtilities.ExportFromArchive(destinationArchive, destinationFolder, testPassword);
    }

    [Fact]
    public void ArchiveBuilderTest2()
    {
        Cleanup();
        HalvaPackage package = new(PassKey: testPassword, IV: ivKey, sourceFolder, destinationArchive, true);
        package.Finish();
        EncryptedPackageUtilities.ExportFromArchive(destinationArchive, destinationFolder, testPassword, ivKey);
    }

    [Fact]
    public void CanArchiveBuilderExtract()
    {
        HalvaPackage package = new(PassKey: testPassword, destinationArchive, true);
        package.ExtractFile("TestImage.webp", Path.Combine(destinationFolder, "TestImage.webp"));
    }

    [Fact]
    public void CanArchiveBuilderExtractWithIV()
    {
        HalvaPackage package = new(PassKey: testPassword, IV: ivKey, destinationArchive, true);
        package.ExtractFile("TestImage.webp", Path.Combine(destinationFolder, "TestImage.webp"));
    }

    [Fact]
    public void CanArchiveRemoveEntry()
    {
        HalvaPackage package = new(PassKey: testPassword, destinationArchive, true);
        package.RemoveFileFromList("TestImage.webp");
        package.Finish();
        if (Directory.Exists(destinationFolder)) Directory.Delete(destinationFolder, true);
        EncryptedPackageUtilities.ExportFromArchive(destinationArchive, destinationFolder, testPassword);
        Assert.Equal(2, Directory.EnumerateFiles(destinationFolder).Count());
    }

    [Fact]
    public void CanArchiveRemoveEntryWithIV()
    {
        HalvaPackage package = new(PassKey: testPassword, IV: ivKey, destinationArchive, true);
        package.RemoveFileFromList("TestImage.webp");
        package.Finish();
        if (Directory.Exists(destinationFolder)) Directory.Delete(destinationFolder, true);
        EncryptedPackageUtilities.ExportFromArchive(destinationArchive, destinationFolder, testPassword, ivKey);
        Assert.Equal(2, Directory.EnumerateFiles(destinationFolder).Count());
    }

    [Fact]
    public void CanArchiveAddEntry()
    {
        HalvaPackage package = new(PassKey: testPassword, destinationArchive, true);
        package.AddFileToList(Path.Combine(sourceFolder, "TestImage.webp"));
        package.Finish();
        if (Directory.Exists(destinationFolder)) Directory.Delete(destinationFolder, true);
        EncryptedPackageUtilities.ExportFromArchive(destinationArchive, destinationFolder, testPassword);
        Assert.Equal(3, Directory.EnumerateFiles(destinationFolder).Count());
    }

    [Fact]
    public void CanArchiveAddEntryWithIV()
    {
        HalvaPackage package = new(PassKey: testPassword, IV: ivKey, destinationArchive, true);
        package.AddFileToList(Path.Combine(sourceFolder, "TestImage.webp"));
        package.Finish();
        if (Directory.Exists(destinationFolder)) Directory.Delete(destinationFolder, true);
        EncryptedPackageUtilities.ExportFromArchive(destinationArchive, destinationFolder, testPassword, ivKey);
        Assert.Equal(3, Directory.EnumerateFiles(destinationFolder).Count());
    }

    [Fact]
    public void CanLibrarySaveChanges()
    {
        HalvaPackage package = new(PassKey: testPassword, destinationArchive, true);
        package.RemoveFileFromList("TestImage.webp");
        package.Save();
        package.AddFileToList(Path.Combine(sourceFolder, "TestImage.webp"));
        package.Save();
    }

    [Fact]
    public void CanLibrarySaveChangesWithIV()
    {
        HalvaPackage package = new(PassKey: testPassword, IV: ivKey, destinationArchive, true);
        package.RemoveFileFromList("TestImage.webp");
        package.Save();
        package.AddFileToList(Path.Combine(sourceFolder, "TestImage.webp"));
        package.Save();
    }

    [Fact]
    public void CanLibraryCheckForDifferencesInEncryptedArchives()
    {
        HalvaPackage package = new(PassKey: testPassword, destinationArchive, true);
        package.UpdateFromArchive(destinationFolder);
    }

    [Fact]
    public void CanLibraryCheckForDifferencesInEncryptedArchivesWithIV()
    {
        HalvaPackage package = new(PassKey: testPassword, IV: ivKey, destinationArchive, true);
        package.UpdateFromArchive(destinationFolder);
    }

}
