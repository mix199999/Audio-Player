using NAudio.Extras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testMAUI
{
    /// <summary>
    /// Klasa reprezentująca sparsowany plik konfiguracyjny JSON'a appSettings.json
    /// </summary>
    internal class Configuration
    {
        private List<string> folderList;
        private List<AudioPlaylist> audioPlaylists;
        private bool firstTimeRun;
        private Theme theme;
        public List<EqualizerBand[]> EqualizerSettings { get; set; }


        public List<string> FolderList { get => folderList; set => folderList = value; }
        public List<AudioPlaylist> AudioPlaylists { get => audioPlaylists; set => audioPlaylists = value; }
        public bool FirstTimeRun { get => firstTimeRun; set => firstTimeRun = value; }
        public Theme Theme { get => theme; set => theme = value; }
    }


}
