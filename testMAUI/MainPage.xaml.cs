



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

public partial class MainPage : ContentPage
{
    //create file saver
    IFileSaver fileSaver;
    private Player player;
    private AudioPlaylist playlist;
    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    private Timer trackTimer;
    public MainPage(IFileSaver fileSaver)
    {
        InitializeComponent();
        player = new Player();
        playlist = new AudioPlaylist();

        playlistView.ItemsSource = playlist.Tracks;

        this.fileSaver = fileSaver;

        VolumeSlider.Value = 100;


    }





    private async void filesBtn_Clicked(object sender, EventArgs e)
    {

        var files = await FilePicker.PickMultipleAsync();
        foreach (var file in files)
        {
            AudioFile audioFile = new AudioFile(file.FullPath);
            playlist.AddTrack(audioFile);

        }
        // zrobic jedna funkcje
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

        AudioFile audioFile = playlist.GetCurrentTrack();
        if (audioFile != null)
        {
            player.Load(audioFile.GetFilePath());
            player.Play();

        }
    }

    private void stopBtn_Clicked(object sender, EventArgs e)
    {
        player.Pause();
    }
    private void playBtn_Clicked(object sender, EventArgs e)
    {

        playAudio();
    }

    private void prevBtn_Clicked(object sender, EventArgs e)
    {
        playlist.Previous();
        AudioFile audioFile = playlist.GetCurrentTrack();

        if (audioFile != null)
        {
            player.Load(audioFile.GetFilePath());
            player.Play();

        }
    }

    private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem == null)
            return;

        var list = new List<object>();
        playAudio();
        if (playlistView.ItemsSource is IEnumerable<object> enumerable)
        {
            list = enumerable.ToList();
        }

        int selectedIndex = list.IndexOf(e.SelectedItem);

        playlist.SetCurrentTrack(selectedIndex);
        TagLib.IPicture currentTrackCover = ((dynamic)playlistView.SelectedItem).Cover;
        if (currentTrackCover != null && currentTrackCover.Data != null)
        {
            using (MemoryStream memoryStream = new MemoryStream(currentTrackCover.Data.Data))
            {
                var imageSource = ImageSource.FromStream(() => memoryStream);
                CurrentAlbumImageControl.Source = imageSource;
            }
        }
        else
        {
            CurrentAlbumImageControl.Source = null;
        }
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
    private async void saveListBtn_Clicked(object sender, TappedEventArgs e)
    {
        using var stream = new MemoryStream(Encoding.Default.GetBytes(playlist.SaveToM3U()));
        var path = await fileSaver.SaveAsync(".M3U", stream, cancellationTokenSource.Token);



    }


    private void playAudio()
    {
        if (playlistView.SelectedItem != null)
        {
            string path = ((dynamic)playlistView.SelectedItem).Path;

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
        player.SkipBackward();
    }

    private void forwardBtn_Clicked(object sender, TappedEventArgs e)
    {
        player.SkipForward();
    }

    private async void StartTimer()
    {
      //  await Task.Run(()=> )
    }

    
}

