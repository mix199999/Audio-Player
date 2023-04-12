



using Microsoft.Maui.Controls.PlatformConfiguration;

namespace testMAUI;

using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Storage;
using System.Collections;
using TagLib.Mpeg;
using Microsoft.Maui.Controls;
using System.Threading;
using System.Text;
using CommunityToolkit.Maui.Storage;
using System.Timers;
using Microsoft.Maui.Animations;

public partial class MainPage : ContentPage
{
    //create file saver
    IFileSaver fileSaver;
    private Player player;
    TimeSpan currentTrackTime = TimeSpan.Zero;
    private AudioPlaylist playlist;
    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    private System.Timers.Timer trackTimer = new System.Timers.Timer();


    public MainPage(IFileSaver fileSaver)
    {
        InitializeComponent();
        player = new Player();
        playlist = new AudioPlaylist();

        playlistView.ItemsSource = playlist.Tracks;

        this.fileSaver = fileSaver;

        VolumeSlider.Value = 100;
        trackTimer.Interval = 1000;
        trackTimer.Elapsed += TimerTick;
    }



    private async void filesBtn_Clicked(object sender, EventArgs e)
    {
       // var allowedExtensions = new[] { ".mp3", ".mp4", ".wave", ".flac" };
       // var files = await FilePicker.PickMultipleAsync();

        var m3uPicker = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            {DevicePlatform.WinUI, new[]{ ".mp3", ".mp4", ".wave", ".flac" } }
        });

        var files = await FilePicker.PickMultipleAsync(new PickOptions
        {
            FileTypes = m3uPicker
        });

        trackTimer.Stop();
        player.Pause();

        foreach (var file in files)
        {
           
                AudioFile audioFile = new AudioFile(file.FullPath);
                playlist.AddTrack(audioFile);
            
        }

        playlistView.ItemsSource = playlist.Tracks.Select(track => new
        {
            Title = track.GetTitle(),
            Duration = track.GetDuration().ToString("mm\\:ss"),
            Album = track.GetAlbum(),
            Artist = track.GetArtist(),
            Path = track.GetFilePath(),
            Cover = track.GetCover()
        });
    }


    private void nextBtn_Clicked(object sender, EventArgs e)
    {
        playlist.Next();
        currentTrackTime = TimeSpan.Zero;
        AudioFile audioFile = playlist.GetCurrentTrack();
        if (audioFile != null)
        {
            player.Load(audioFile.GetFilePath());
            player.Play();
            setCurrentTrackInfo();


        }
    }

    private void stopBtn_Clicked(object sender, EventArgs e)
    {
        trackTimer.Stop();
        player.Pause();
    }
    private void playBtn_Clicked(object sender, EventArgs e)
    {

        playAudio();
    }

    private void prevBtn_Clicked(object sender, EventArgs e)
    {
        playlist.Previous();
        currentTrackTime = TimeSpan.Zero;
        AudioFile audioFile = playlist.GetCurrentTrack();

        if (audioFile != null)
        {
            player.Load(audioFile.GetFilePath());
            player.Play();
            setCurrentTrackInfo();
        }
    }

    private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
    {

        if (e.SelectedItem == null)
            return;

        var list = new List<object>();
        currentTrackTime = TimeSpan.Zero;
        playAudio();

        if (playlistView.ItemsSource is IEnumerable<object> enumerable)
        {
            list = enumerable.ToList();
        }

        int selectedIndex = list.IndexOf(e.SelectedItem);

        playlist.SetCurrentTrack(selectedIndex);
            setCurrentTrackInfo();


    }

    private async void loadListBtn_Clicked(object sender, TappedEventArgs e)
    {


        var m3uPicker = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            {DevicePlatform.WinUI, new[]{".m3u" } }
        });

        var playlistFile = await FilePicker.PickAsync(new PickOptions
        {
            FileTypes = m3uPicker
        });



        if (playlistFile != null)
        {
            trackTimer.Stop();
            player.Pause();

            playlist.clearList();
            playlist.LoadFromM3U(playlistFile.FullPath);
            playlistView.ItemsSource = null;
            playlistView.ItemsSource = playlist.Tracks.Select(track => new
            {
                Title = track.GetTitle(),
                Duration = track.GetDuration().ToString("hh\\:mm\\:ss"),
                Album = track.GetAlbum(),
                Artist = track.GetArtist(),
                Path = track.GetFilePath(),
                Cover = track.GetCover()
            });

        }

    }
    private async void saveListBtn_Clicked(object sender, TappedEventArgs e)
    {
        using var stream = new MemoryStream(Encoding.Default.GetBytes(playlist.SaveToM3U()));
        var path = await fileSaver.SaveAsync(".M3U", stream, cancellationTokenSource.Token);
    }


    private void playAudio()
    {
        if (playlistView.SelectedItem != null)
        {
            trackTimer.Start();
            string path = ((dynamic)playlistView.SelectedItem).Path;
            setCurrentTrackInfo();
            AudioFile audioFile = new AudioFile(path);
            player.Load(audioFile.GetFilePath());
            player.Play();
            player.status = playerStatus.IsPlaying;
            player._totalTime = audioFile.GetDuration();
        }
    }

    private async void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        await Task.Run(() => player.SetVolume(e.NewValue));
    }

    private void backwardBtn_Clicked(object sender, TappedEventArgs e)
    {
        currentTrackTime += TimeSpan.FromSeconds(-15);
        player.SkipBackward();
    }

    private void forwardBtn_Clicked(object sender, TappedEventArgs e)
    {
        currentTrackTime += TimeSpan.FromSeconds(15);
        player.SkipForward();
       // CurrentTrackTitle.Text = "test";
    }


    private async void TimerTick(object sender, ElapsedEventArgs e)
    {
        await Device.InvokeOnMainThreadAsync(() =>
        {
            currentTrackTime += TimeSpan.FromSeconds(1);

            string durationString = ((dynamic)playlistView.SelectedItem).Duration;
            CurrentTimeLabel.Text = currentTrackTime.ToString("mm\\:ss");
            TimeSpan duration = TimeSpan.Parse(durationString);
            double progress = currentTrackTime.TotalSeconds / duration.TotalSeconds;
            TrackProgressBar.Progress = progress;
        });
    }


    private async void setCurrentTrackInfo()
    {  
        await Device.InvokeOnMainThreadAsync(() =>
        {
           CurrentTrackAlbum.Text = ((dynamic)playlist.GetCurrentTrack().GetAlbum());
           CurrentTrackArtist.Text = ((dynamic)playlist.GetCurrentTrack().GetArtist());
           CurrentTrackTitle.Text = ((dynamic)playlist.GetCurrentTrack().GetTitle());
            CurrentTrackCover.Source = (dynamic)playlist.GetCurrentTrack().GetCoverUrl();
        });
    }





}

