using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using TagLib.Mpeg;

namespace testMAUI
{
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

        public playerStatus _status { get; set; }
        public TimeSpan _totalTime { get; set; }
        public TimeSpan currentTime { get; set; }

        public event EventHandler StartedPlaying;
        public event EventHandler StoppedPlaying;
        public event EventHandler Paused;
       

    

        public Player()
        {
            _waveOut = new WaveOutEvent();
        }

        public void Load(string filePath)
        {
            if (_audioFile != null)
            {
                _waveOut.Stop();
                _waveOut.Dispose();
                _audioFile.Dispose();
            }

            _audioFile = new AudioFileReader(filePath);
            _waveOut.Init(_audioFile);
        }

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

        public void Stop()
        {
            _waveOut.Stop();
            _audioFile.Position = 0;
            OnStoppedPlaying(EventArgs.Empty);
            _status = playerStatus.IsNotPlaying;

        }

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

        //od 0.0 do 1.0
        public void SetVolume(double volume)
        {
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
