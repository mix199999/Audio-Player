using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using TagLib.Mpeg;
using NAudio.Wave;
using NAudio.Extras;
namespace testMAUI
{
    using GNOM;
    using NAudio.Wave;

    /// <summary>
    /// enum zawierający stany odtwarzacza
    /// </summary>
    enum playerStatus
    {
        IsPlaying,
        IsNotPlaying
    };

    /// <summary>
    /// Klasa zawierająca ...
    /// </summary>
    internal class Player
    {
        private WaveOutEvent _waveOut;
        private AudioFileReader _audioFile;
        private Equalizer _equalizer;
        private EqualizerBand[] _bands;

        public playerStatus _status { get; set; }
        public TimeSpan _totalTime { get; set; }
        public TimeSpan currentTime { get; set; }
        public EqualizerBand[] Bands { get => _bands; set => _bands = value; }

        public event EventHandler StartedPlaying;
        public event EventHandler StoppedPlaying;
        public event EventHandler Paused;

        public Player()
        {
            _waveOut = new WaveOutEvent();
            _bands = new EqualizerBand[]
            {
                new EqualizerBand{Frequency=32, Bandwidth= 0.8f, Gain = 0},
                new EqualizerBand { Frequency = 64, Bandwidth = 0.8f, Gain = 0 },
                new EqualizerBand { Frequency = 125, Bandwidth = 0.8f, Gain = 0},
                new EqualizerBand { Frequency = 250, Bandwidth = 0.8f, Gain = 0 },
                new EqualizerBand { Frequency = 500, Bandwidth = 0.8f, Gain = 0 },
                new EqualizerBand { Frequency = 1000, Bandwidth = 0.8f, Gain = 0 },
                new EqualizerBand { Frequency = 2000, Bandwidth = 0.8f, Gain = 0 },
                new EqualizerBand { Frequency = 4000, Bandwidth = 0.8f, Gain = 0 },
                new EqualizerBand { Frequency = 8000, Bandwidth = 0.8f, Gain = 0 },
                new EqualizerBand { Frequency = 16000, Bandwidth = 0.8f, Gain = 0 }

            };
           
        }


        /// <summary>
        /// Metoda ładująca piosenkę daną ścieżką
        /// </summary>
        /// <param name="filePath">Ścieżka do aplikacji</param>

        public void Load(string filePath)
        {
            if (_audioFile != null)
            {
                _waveOut.Stop();
                _waveOut.Dispose();
                _audioFile.Dispose();
                
                
            }
            
           
            _audioFile = new AudioFileReader(filePath);
            _equalizer = new Equalizer(_audioFile, _bands);
            _waveOut.Init(_equalizer);
           
        }

        public void ApplyEqualizerSettings(EqualizerBand[] settings)
        {
            if (settings.Length != _bands.Length || _equalizer == null)
            {
                _bands = settings;
                return;
            }

            for (int i = 0; i < settings.Length; i++)
            {
                _bands[i].Gain = settings[i].Gain;
            }

            _equalizer.Update();
        }


        public void ApplySingleBand(KeyValuePair<int, float> band)
        {
            if (_equalizer == null)   
            {    
                return;
            }

            _bands[band.Key].Gain = band.Value;
            _equalizer.Update();
        }

        /// <summary>
        /// Metoda odtwarzająca aktualną piosenkę
        /// </summary>
        public void Play()
        {
            _waveOut.Play();
            OnStartedPlaying(EventArgs.Empty);
            _status = playerStatus.IsPlaying;
            // asynchronicznie co sekunde aktualizuje se czas odtwarzania 
            Task.Run(() =>
            {
                while (_waveOut.PlaybackState == PlaybackState.Playing)
                {
                    currentTime = _audioFile.CurrentTime;
                    Task.Delay(1000).Wait();
                   
                }
            });
        }

        /// <summary>
        /// Metoda zatrzymująca odtwarzanie piosenki
        /// </summary>
        public void Stop()
        {
            _waveOut.Stop();
            _audioFile.Position = 0;
            OnStoppedPlaying(EventArgs.Empty);
            _status = playerStatus.IsNotPlaying;
        }

        /// <summary>
        /// Metoda pauzująca odtwarzanie piosenki
        /// </summary>
        public void Pause()
        {
            _waveOut.Pause();
            OnPaused(EventArgs.Empty);
            _status = playerStatus.IsNotPlaying;
        }


        protected virtual void OnStartedPlaying(EventArgs e)
        {
            StartedPlaying?.Invoke(this, e);
        }

        protected virtual void OnStoppedPlaying(EventArgs e)
        {
            StoppedPlaying?.Invoke(this, e);
        }

        protected virtual void OnPaused(EventArgs e)
        {
            Paused?.Invoke(this, e);
        }

        public void SetVolume(double volume)
        {
            //od 0.0 do 1.0
            _waveOut.Volume = (float)volume/100;
        }


        public void SkipForward()
        {
            TimeSpan newTime = currentTime + TimeSpan.FromSeconds(15);
            if (newTime <= _totalTime)
            {
                _audioFile.CurrentTime = newTime;
            }
        }

        public void SkipBackward()
        {
            TimeSpan newTime = currentTime - TimeSpan.FromSeconds(15);
            if (newTime >= TimeSpan.Zero)
            {
                _audioFile.CurrentTime = newTime;
            }
        }

        public void SetTime(double time)
        {
            _audioFile.CurrentTime = TimeSpan.FromSeconds(time);
        }


        public string getCurrentTime(){return _audioFile.CurrentTime.ToString("\"hh\\:mm\\:ss");}


    }
}
