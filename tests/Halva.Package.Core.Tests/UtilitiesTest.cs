using Xunit;

namespace Halva.Package.Core.Tests;

public class UtilitiesTest(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    [Fact]
    public void CompressionCheck()
    {
        bool finished = true;
        if (File.Exists("SampleFiles.halva")) File.Delete("SampleFiles.halva");
        try
        {
           PackageUtilities.CreatePackageFromFolder("SampleFiles", "SampleFiles.halva");
        }
        catch (Exception e)
        {
            _testOutputHelper.WriteLine(e.ToString());
            finished = false;
        }
        Assert.True(finished);
    }

    [Fact]
    public async Task AsyncCompressionCheck()
    {
        bool finished = true;
        if (File.Exists("SampleFiles.halva")) File.Delete("SampleFiles.halva");
        try
        {
            await PackageUtilities.CreatePackageFromFolderAsync("SampleFiles", "SampleFiles.halva", abortToken: TestContext.Current.CancellationToken);
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
        Directory.CreateDirectory("SampleFiles1");
        try
        {
            PackageUtilities.DecompressPackageToFolder("SampleFiles.halva", "SampleFiles1");
        }
        catch (Exception e)
        {
            _testOutputHelper.WriteLine(e.ToString());
            finished = false;
        }
        Assert.True(finished);
    }

    [Fact]
    public async Task AsyncDecompressionCheck()
    {
        bool finished = true;
        if (Directory.Exists("SampleFiles1")) Directory.Delete("SampleFiles1", true);
        try
        {
            await PackageUtilities.DecompressPackageToFolderAsync("SampleFiles.halva", "SampleFiles1", abortToken: TestContext.Current.CancellationToken);
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
            PackageUtilities.CreatePackageFromFolder("SampleFiles", "EncryptedSampleFiles.halva",  "1234567890abcdefghijklm");
        }
        catch (Exception e)
        {
            _testOutputHelper.WriteLine(e.ToString());
            finished = false;
        }
        Assert.True(finished);
    }

    [Fact]
    public async Task EncryptedCompressionAsyncCheck()
    {
        bool finished = true;
        if (File.Exists("EncryptedSampleFiles.halva")) File.Delete("EncryptedSampleFiles.halva");
        try
        {
            await PackageUtilities.CreatePackageFromFolderAsync("SampleFiles", "EncryptedSampleFiles.halva", "1234567890abcdefghijklm", abortToken: TestContext.Current.CancellationToken);
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
            PackageUtilities.DecompressPackageToFolder("EncryptedSampleFiles.halva", "SampleFiles2", "1234567890abcdefghijklm");
        }
        catch (Exception e)
        {
            _testOutputHelper.WriteLine(e.ToString());
            finished = false;
        }
        Assert.True(finished);
    }

    [Fact]
    public async Task EncryptedDecompressionAsyncCheck()
    {
        bool finished = true;
        if (Directory.Exists("SampleFiles2")) Directory.Delete("SampleFiles2", true);
        try
        {
            await PackageUtilities.DecompressPackageToFolderAsync("EncryptedSampleFiles.halva", "SampleFiles2", "1234567890abcdefghijklm", abortToken: TestContext.Current.CancellationToken);
        }
        catch (Exception e)
        {
            _testOutputHelper.WriteLine(e.ToString());
            finished = false;
        }
        Assert.True(finished);
    }
}
