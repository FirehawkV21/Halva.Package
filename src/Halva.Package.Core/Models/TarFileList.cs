using System.Diagnostics;

namespace Halva.Package.Core.Models;
[DebuggerDisplay("File Location = {FileLocation} | Entry in Tar file = {FileEntry}")]
internal record TarFileList
{
    public string FileLocation { get; set; }
    public string FileEntry { get; set; }

    internal TarFileList(string file, string entry)
    {
        FileLocation = file;
        FileEntry = entry;
    }
}
