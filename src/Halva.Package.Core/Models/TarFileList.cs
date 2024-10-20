﻿namespace Halva.Package.Core.Models;
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
