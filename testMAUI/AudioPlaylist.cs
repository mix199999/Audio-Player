using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib.Mpeg;
using System.IO;
namespace testMAUI
{
    internal class AudioPlaylist
    {
        private List<AudioFile> _tracks;
        private int _currentIndex;

        public List<AudioFile> Tracks => _tracks;



        public AudioPlaylist()
        {
            _tracks = new List<AudioFile>();
            _currentIndex = -1;
        }

        public void AddTrack(AudioFile track)
        {
            _tracks.Add(track);
        }


       

        public AudioFile GetCurrentTrack()
        {
            if (_currentIndex >= 0 && _currentIndex < _tracks.Count)
            {
                return _tracks[_currentIndex];
            }
            else
            {
                return null;
            }
        }

        public void clearList()
        {
            this._tracks.Clear();
        }

        public void SetCurrentTrack(int index)
        {
            if (index >= 0 && index < _tracks.Count)
            {
                _currentIndex = index;
            }
        }

        public void Next()
        {
            if (_currentIndex < _tracks.Count - 1)
            {
                _currentIndex++;
            }
        }

        public void Previous()
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
            }
        }

        /// <summary>
        /// Metoda służąca do tworzenia playlist w formacie M3U
        /// Dodatkowe informacje o formacie M3U: https://en.wikipedia.org/wiki/M3U
        /// </summary>
        /// <returns>zwraca playliste zapisana w formie stringa</returns>
        public string SaveToM3U()
        {
            var sb = new StringBuilder();
            sb.AppendLine("#EXTM3U");

            foreach (var track in _tracks)
            {
                sb.AppendLine($"#EXTINF:{track.GetDuration()},{track.GetTitle()}");
                sb.AppendLine(track.GetFilePath());
                sb.AppendLine();
            }

           return sb.ToString();
        }

        /// <summary>
        /// Funkcja służąca do sparsowania danych dotyczących playlisty zapisanych w pliku .M3U
        /// </summary>
        /// <param name="filePath">ścieżka do pliku z playlistą w formacie M3U</param>
        /// <returns>zwraca liczbę odczytanych utworów</returns>
        public int LoadFromM3U(string filePath)
        {
            var content = System.IO.File.ReadAllText(filePath);
            var entries = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var added = 0;

            for (var i = 0; i < entries.Length; i++)
            {
                if (entries[i].StartsWith("#EXTINF"))
                {
                    var durationTitle = entries[i].Substring(8);                                   
                  

                    if (i + 1 < entries.Length && !entries[i + 1].StartsWith("#EXT"))
                    {
                        var track = new AudioFile(entries[i + 1]);
                        
                       // track.SetTitle(title);
                        _tracks.Add(track);
                        added++;
                    }
                }
            }

            return added;
        }






    }
}
