using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib.Mpeg;
using System.IO;

using System.Text.Json.Serialization;
using TagLib.Matroska;


namespace testMAUI
{
    internal class AudioPlaylist
    {
        [JsonIgnore]
        private List<AudioFile> _tracks;
        [JsonIgnore]
        private int _currentIndex;
        [JsonIgnore]
        public List<AudioFile> Tracks => _tracks;
        [JsonIgnore]
        private static string _favoriteSongsListPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "favoritesongs.M3U");

        public string Name { get; set; }
        public string Path { get; set; }





        public AudioPlaylist()
        {
            _tracks = new List<AudioFile>();
            _currentIndex = -1;
        }

        public void AddTrack(AudioFile track)
        {
            _tracks.Add(track);
        }
        public void RemoveTrack(AudioFile track)
        {
            var trackToRemove = _tracks.FirstOrDefault(t => t.GetFilePath() == track.GetFilePath());
            if (trackToRemove != null)
            {
                _tracks.Remove(trackToRemove);
            }
        }


        public int GetCurrentTrackIndex()
        { return _currentIndex; }

        public void SetTracks(List<AudioFile> tracks)
        { _tracks = tracks; }

       

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
        /// Metoda służąca do tworzenia mainPlaylist w formacie M3U
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

        public static void AppendTrackToFavoritelistFile(AudioFile track)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"#EXTINF:{track.GetDuration()},{track.GetTitle()}");
            sb.AppendLine(track.GetFilePath());
            sb.AppendLine();
            System.IO.File.AppendAllText(_favoriteSongsListPath, sb.ToString());
        }
        public static void RemoveTrackFromM3U(AudioFile track)
        {
            var lines = System.IO.File.ReadAllLines(_favoriteSongsListPath);
            var lineIndex = Array.FindIndex(lines, line => line.Contains(track.GetFilePath()));

            if (lineIndex >= 0)
            {
               
                var linesList = lines.ToList();
                linesList.RemoveAt(lineIndex);

             
                if (lineIndex > 0)
                {
                    linesList.RemoveAt(lineIndex - 1);
                }

                lines = linesList.ToArray();
                System.IO.File.WriteAllLines(_favoriteSongsListPath, lines);
            }
        }



        /// <summary>
        /// Funkcja służąca do sparsowania danych dotyczących playlisty zapisanych w pliku .M3U
        /// </summary>
        /// <param name="filePath">ścieżka do pliku z playlistą w formacie M3U</param>
        /// <returns>zwraca liczbę odczytanych utworów</returns>
        public int LoadFromM3U(string filePath)
        {
            if (System.IO.File.Exists(filePath))
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
                            var audioFilePath = entries[i + 1];

                            if (System.IO.File.Exists(audioFilePath))
                            {
                                var track = new AudioFile(audioFilePath);
                                _tracks.Add(track);
                                added++;
                            }
                            else
                            {
                                Console.WriteLine($"File not found: {audioFilePath}");
                            }
                        }
                    }
                }
                return added;

            }
            else return 0;
        }


        public void LoadFromDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                string[] files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);

                List<string> musicFiles = new List<string>();

                foreach (string file in files)
                {
                    string extension = System.IO.Path.GetExtension(file).ToLower();
                    if (extension == ".mp3" || extension == ".mp4" || extension == ".wave" || extension == ".wav" || extension == ".flac")
                    {
                        musicFiles.Add(file);
                    }
                }

                foreach (string file in musicFiles)
                {
                    if (System.IO.File.Exists(file))
                    {
                        var track = new AudioFile(file);
                        _tracks.Add(track);                       
                    }
                }
            }
        }


    }




}
