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
        private List<AudioPlaylist> audioPlaylists;
        private bool firstTimeRun;

        public List<string> FolderList { get => folderList; set => folderList = value; }
        public List<AudioPlaylist> AudioPlaylists { get => audioPlaylists; set => audioPlaylists = value; }
        public bool FirstTimeRun { get => firstTimeRun; set => firstTimeRun = value; }
    }


}
