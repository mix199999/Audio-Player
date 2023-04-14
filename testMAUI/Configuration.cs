using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testMAUI
{
internal class Configuration
{
    private List<string> folderList;
    private List<AudioAlbum> audioPlaylists;

    public List<string> FolderList { get => folderList; set => folderList = value; }
    public List<AudioAlbum> AudioPlaylists { get => audioPlaylists; set => audioPlaylists = value; }
}

internal class AudioAlbum
{
    public string Name { get; set; }
    public string Path { get; set; }
}
}
