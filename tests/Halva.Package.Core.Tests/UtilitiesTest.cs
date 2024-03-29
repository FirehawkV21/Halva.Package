using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;
using Halva.Package.Core.Utilities;

namespace Halva.Package.Core.Tests;

public class UtilitiesTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public UtilitiesTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void CompressionCheck()
    {
        bool finished = true;
        if (File.Exists("SampleFiles.halva")) File.Delete("SampleFiles.halva");
        try
        {
           PackageUtilities.CreateArchiveFromFolder("SampleFiles", "SampleFiles.halva");
        }
        catch (Exception e)
        {
            _testOutputHelper.WriteLine(e.ToString());
            finished = false;
        }
        Assert.True(finished);
    }

    [Fact]
    public void DecompressionCheck()
    {
        bool finished = true;
        if (Directory.Exists("SampleFiles1")) Directory.Delete("SampleFiles1", true);
        try
        {
            PackageUtilities.ExportFromArchive("SampleFiles.halva", "SampleFiles1");
        }
        catch (Exception e)
        {
            _testOutputHelper.WriteLine(e.ToString());
            finished = false;
        }
        Assert.True(finished);
    }

    [Fact]
    public void EncryptedCompressionCheck()
    {
        bool finished = true;
        if (File.Exists("EncryptedSampleFiles.halva")) File.Delete("EncryptedSampleFiles.halva");
        try
        {
            EncryptedPackageUtilities.CreateArchiveFromFolder("SampleFiles", "EncryptedSampleFiles.halva", "1234567890abcdefghijklm");
        }
        catch (Exception e)
        {
            _testOutputHelper.WriteLine(e.ToString());
            finished = false;
        }
        Assert.True(finished);
    }

    [Fact] 
    public void EncryptedDecompressionCheck()
    {
        bool finished = true;
        if (Directory.Exists("SampleFiles2")) Directory.Delete("SampleFiles2", true);
        try
        {
            EncryptedPackageUtilities.ExportFromArchive("EncryptedSampleFiles.halva", "SampleFiles2", "1234567890abcdefghijklm");
        }
        catch (Exception e)
        {
            _testOutputHelper.WriteLine(e.ToString());
            finished = false;
        }
        Assert.True(finished);
    }
}
