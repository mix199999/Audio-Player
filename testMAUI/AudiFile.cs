using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using TagLib.Image;
using System.Net;
using System.Xml.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

using NAudio.CoreAudioApi;


namespace testMAUI
{   /// <summary>
    /// Klasa która reprezentuje obiekt pliku dźwiękowego.
    /// Zawiera właściwości i metody do pobierania i ustawiania metadanych pliku dźwiękowego,
    /// takich jak tytuł, artysta, album, czas trwania i ścieżka pliku. 
    /// Zawiera także metody do pobierania okładek albumów z interfejsu Deezer API.
    /// </summary>
    internal class AudioFile
    {

        private string _title;
        private string _artist;
        private string _album;
        private TimeSpan _duration;
        private string _filePath;       
        private string _coverUrl;
        private bool _favourite;
        
        public string GetCoverUrl()
        { return _coverUrl; }
        public async Task SetCoverUrl()
        { await SetAlbumArtFromDeezerApiAsync();  }


        

        public AudioFile()
        {

        }
       

        public string GetDurationString()
        {
            string temp = _duration.ToString("hh\\:mm\\:ss");
            return temp;
        }

        public string GetTitle()
        {
            return _title;
        }

        public void SetTitle(string title)
        {
            _title = title;
        }

        public string GetArtist()
        {
            return _artist;
        }

        public void SetArtist(string artist)
        {
            _artist = artist;
        }

        public string GetAlbum()
        {
            return _album;
        }

        public void SetAlbum(string album)
        {
            _album = album;
        }

        public TimeSpan GetDuration()
        {
            return _duration;
        }

        public void SetDuration(TimeSpan duration)
        {
            _duration = duration;
        }
        public string GetFilePath()
        {
            return _filePath;
        }

        public void SetFilePatch(string filePath)
        {
            _filePath = filePath.Trim();
        }

        public bool GetFavourite()
        {
            return _favourite;
        }

        public bool SetFavourite(bool favourite) => _favourite = favourite;

        /// <summary>
        /// Konstruktor klasy AudioFile, który wykorzystuje bibliotekę TagLib 
        /// do pozyskania informacji dotyczących danego utworu
        /// </summary>
        /// <param name="filePath">ścieżka do pliku audio </param>
        public  AudioFile(string filePath)
        {
            _filePath = filePath;

            try
            {
                var tagFile = TagLib.File.Create(filePath);

                _title = tagFile.Tag.Title ?? "unknown";
                _artist = tagFile.Tag.Performers.Length > 0 ? tagFile.Tag.Performers[0] : "unknown";
                _album = tagFile.Tag.Album ?? "unknown";
                _album = Regex.Replace(_album, @"\s*\[.*?\]\s*", "");
                _duration = tagFile.Properties.Duration.TotalSeconds <= 0 ? TimeSpan.Zero : tagFile.Properties.Duration;
                _favourite = false;
                
                if(_title ==  "unknown" && _album == "unknown" && _artist == "unknown") 
                {
                    _coverUrl = "note_icon.png";
                }
              

            }

            catch (FileNotFoundException)
            {
                Console.WriteLine($"File not found: {filePath}");
            }
            catch (UnsupportedFormatException)
            {
                Console.WriteLine($"Unsupported file format: {filePath}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while reading file: {filePath}\n{e.Message}");
            }

        }
        /// <summary>
        /// Asynchroniczna metoda, która służy do pobierania okładki albumu z API Deezer i ustawiania pola _coverUrl w obiekcie AudioFile na odpowiednią wartość.
        ///  Do tego celu wykorzystuje bibliotekę HttpClient do wykonywania żądań HTTP do API Deezer.
        ///  Na początku metoda składa zapytanie o okładkę albumu, używając artysty i tytułu albumu z pola _artist i _album w celu znalezienia najbardziej odpowiedniego wyniku.
        ///  Następnie metoda pobiera szczegóły wybranego albumu i ustawia pole _coverUrl na adres URL okładki albumu.
        /// Jeśli wystąpi błąd podczas pobierania okładki albumu, metoda przechwytuje wyjątek HttpRequestException i ustawia pole _coverUrl na wartość "note_icon.png".
        /// </summary>
        /// <returns></returns>
        public async Task SetAlbumArtFromDeezerApiAsync()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var query = $"{_artist} {_album}".Replace(' ', '+');
                    var requestUrl = $"https://api.deezer.com/search?q={query}&limit=1";
                    var response = await httpClient.GetAsync(requestUrl);
                    response.EnsureSuccessStatusCode();
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var jsonResponse = System.Text.Json.JsonDocument.Parse(responseBody);
                    var albumId = jsonResponse.RootElement.GetProperty("data")[0].GetProperty("album").GetProperty("id").GetInt32();
                    requestUrl = $"https://api.deezer.com/album/{albumId}";
                    response = await httpClient.GetAsync(requestUrl);
                    response.EnsureSuccessStatusCode();
                    responseBody = await response.Content.ReadAsStringAsync();
                    jsonResponse = System.Text.Json.JsonDocument.Parse(responseBody);
                    _coverUrl = jsonResponse.RootElement.GetProperty("cover_xl").GetString();
                    
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error while retrieving album art: {e.Message}");
                _coverUrl = "note_icon.png";
            }
        }



    }




}
