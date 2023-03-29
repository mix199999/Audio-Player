using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using TagLib.Image;


namespace testMAUI
{
    internal class AudioFile
    {

       
        private string _title;
        private string _artist;
        private string _album;
        private TimeSpan _duration;
        private string _filePath;
        private TagLib.IPicture _cover;
      


        public TagLib.IPicture GetCover()
        {
            return _cover;
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

        /// <summary>
        /// Konstruktor klasy AudioFile, który wykorzystuje bibliotekę TagLib 
        /// do pozyskania informacji dotyczących danego utworu
        /// </summary>
        /// <param name="filePath">ścieżka do pliku audio </param>
        public AudioFile(string filePath)
        {
            _filePath = filePath;

            try 
            {
                TagLib.File tagFile = TagLib.File.Create(filePath);

                _title = tagFile.Tag.Title;
                _artist = tagFile.Tag.Performers.Length > 0 ? tagFile.Tag.Performers[0] : "unknown";
                _album = tagFile.Tag.Album.Length > 0 ? tagFile.Tag.Album : "unknown";
                _duration = tagFile.Properties.Duration;

                //nie działa
                if (tagFile.Tag.Pictures.Length > 0)
                {
                    foreach (TagLib.Picture pic in tagFile.Tag.Pictures)
                    {
                        if (pic.Type == TagLib.PictureType.FrontCover)
                        {
                            _cover = pic;
                            break;
                        }
                    }
                }
            }

            catch(Exception e) 
            {
                // coś tu dać 
                e.ToString();
               
            }
            
            
        }


    }
}
