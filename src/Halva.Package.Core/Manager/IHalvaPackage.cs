using System.IO.Compression;
using System.Text;

namespace Halva.Package.Core.Manager;

public interface IHalvaPackage
{
    CompressionLevel CompressionOption { get; set; }
    StringBuilder DestinationLocation { get; set; }
    List<string> FileList { get; set; }
    string Password { get; set; }

    void AddFilesFromAFolder(string source);
    void AddFilesFromAFolder(string sourceLocation, string SourceFolderRelativeLocation);
    void AddFileToList(string fileLocation);
    void AddFileToList(string source, string fileRelativeLocation);
    void ExtractFile(string entry, string exportLocation);
    void RemoveFileFromList(string fileLocation);
    void UpdateArchive(string SourceFolder);
    void UpdateFromArchive(string TargetFolder);
    void Save();
    void Finalize();
}
