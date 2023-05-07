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
        private List<string> folderList; //!< Lista ścieżek do folderów z których piosenki mają być czytane
        private List<AudioPlaylist> audioPlaylists; //!< Lista playlist zapisanych w programie
        private bool firstTimeRun; //!< Flaga czy aplikacja jest odpalana po raz pierwszy
        private Theme theme; //!< Motyw aplikacji

        public List<string> FolderList { get => folderList; set => folderList = value; }
        public List<AudioPlaylist> AudioPlaylists { get => audioPlaylists; set => audioPlaylists = value; }
        public bool FirstTimeRun { get => firstTimeRun; set => firstTimeRun = value; }
        public Theme Theme { get => theme; set => theme = value; }
    }


}
