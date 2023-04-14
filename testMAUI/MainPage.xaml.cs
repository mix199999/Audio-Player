
using Microsoft.Maui.Controls.PlatformConfiguration;

namespace testMAUI;
using CommunityToolkit.Maui.Views;
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
using Microsoft.Extensions.Configuration;

using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Microsoft.Extensions.Configuration.Json;
using System.Text.Json;
using System.Xml;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;



public partial class MainPage : ContentPage
{
    //create file saver
    IFileSaver fileSaver;
    private Player player;
    TimeSpan currentTrackTime = TimeSpan.Zero;
    private AudioPlaylist playlist;
    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    CancellationToken cancellationToken = new CancellationToken();
    private System.Timers.Timer trackTimer = new System.Timers.Timer();
    private double currentTrackProgress;
    private bool ValueChangedEnabled = true;
    private IConfiguration _configuration;
    private List<AudioPlaylist> _playlist;
    private List<string> foldersList = new List<string>();

    public MainPage(IFileSaver fileSaver, IConfiguration configuration)
    {
        _configuration = configuration;
        _configuration.GetSection("FolderList").Bind(foldersList);


        InitializeComponent();
        player = new Player();
        player._status = playerStatus.IsNotPlaying;
        playlist = new AudioPlaylist();

        playlistView.ItemsSource = playlist.Tracks;

        this.fileSaver = fileSaver;

        VolumeSlider.Value = 100;
        trackTimer.Interval = 1000;
        trackTimer.Elapsed += TimerTick;
        TrackProgressBarSlider.Value = 0;


        foreach (var Folder in foldersList)
        {
            loadToListView(Folder);
        }
        if (foldersList.Count == 0) foldersList.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
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


    private void nextTrack()
    {
        //if()
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

    private void nextBtn_Clicked(object sender, EventArgs e) => nextTrack();


    private void stopBtn_Clicked(object sender, EventArgs e)
    {
        trackTimer.Stop();
        player.Pause();
    }
    private void playBtn_Clicked(object sender, EventArgs e) => playAudio();


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

            // player.status = playerStatus.IsPlaying;
            player._totalTime = audioFile.GetDuration();
            TrackProgressBarSlider.Maximum = audioFile.GetDuration().TotalSeconds;
        }


    }




    private async void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        await Task.Run(() => player.SetVolume(e.NewValue));
    }

    private void backwardBtn_Clicked(object sender, TappedEventArgs e)
    {
        currentTrackTime -= TimeSpan.FromSeconds(15);
        player.SkipBackward();
        currentTrackProgress -= 15;
    }

    private void forwardBtn_Clicked(object sender, TappedEventArgs e)
    {
        currentTrackTime += TimeSpan.FromSeconds(15);
        player.SkipForward();

        currentTrackProgress += 15;
    }


    private async void TimerTick(object sender, ElapsedEventArgs e)
    {
        await Dispatcher.DispatchAsync(() =>
        {
            currentTrackTime += TimeSpan.FromSeconds(1);
            TimeSpan durationTime = playlist.GetCurrentTrack().GetDuration();
            currentTrackProgress = durationTime.TotalSeconds - currentTrackTime.TotalSeconds;

            if (currentTrackProgress <= 0) { nextTrack(); }
            else
            {
                CurrentTimeLabel.Text = currentTrackTime.ToString("mm\\:ss");
                //testCurrentTimeLabel.Text = currentTrackProgress.ToString();
                ValueChangedEnabled = false;
                TrackProgressBarSlider.Value = currentTrackTime.TotalSeconds;
                ValueChangedEnabled = true;
            }
        });
    }


    private async Task setCurrentTrackInfo()
    {
        await Dispatcher.DispatchAsync(() =>
        {
            CurrentTrackAlbum.Text = ((dynamic)playlist.GetCurrentTrack().GetAlbum());
            CurrentTrackArtist.Text = ((dynamic)playlist.GetCurrentTrack().GetArtist());
            CurrentTrackTitle.Text = ((dynamic)playlist.GetCurrentTrack().GetTitle());
            CurrentTrackCover.Source = (dynamic)playlist.GetCurrentTrack().GetCoverUrl();
            showToastInfo();
        });
    }

    private void TrackProgressBarSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        if (ValueChangedEnabled && player._status == playerStatus.IsPlaying)
        {
            currentTrackTime = TimeSpan.FromSeconds(TrackProgressBarSlider.Value);
            player.SetTime(TrackProgressBarSlider.Value);

        }
    }

    private bool ReplayPlaylist()
    {
        return true;

    }

    private void PlayRandom()
    {

    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        PickFolder(cancellationToken);
    }

    private void loadToListView(string Path)
    {
        string musicPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        playlist.LoadFromDirectory(Path);
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

    async Task PickFolder(CancellationToken cancellationToken)
    {
        var result = await FolderPicker.Default.PickAsync(cancellationToken);
        if (result.IsSuccessful)
        {
            if (foldersList.Contains(result.Folder.Path))
            {
                await Toast.Make($"The selected folder {result.Folder.Path} is already added.").Show(cancellationToken);
            }
            else
            {
                foldersList.Add(result.Folder.Path);
                loadToListView(result.Folder.Path);
                SaveFoldersList();
            }
        }
        else
        {
            await Toast.Make($"The folder was not picked with error: {result.Exception.Message}").Show(cancellationToken);
        }
    }
    private void SaveFoldersList()
    {
        var appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appSettings.json");

        var foldersSettings = new Configuration { FolderList = foldersList };
        var json = JsonConvert.SerializeObject(foldersSettings, Newtonsoft.Json.Formatting.Indented);

        System.IO.File.WriteAllText(appSettingsPath, json);
    }

    private async Task ShowPopupInfo()
    {
        var popup = new Popup();
        popup.Size = new Size(300, 300);

        var stackLayout = new VerticalStackLayout();
        var image = new Image { Source = playlist.GetCurrentTrack().GetCoverUrl() };
        var label = new Label
        {
            Text = $"\n\r{((dynamic)playlist.GetCurrentTrack().GetTitle())}\n\r" +
            $" Artist - {((dynamic)playlist.GetCurrentTrack().GetArtist())}\n\r" +
            $" Album - {((dynamic)playlist.GetCurrentTrack().GetAlbum())}\n\r",
            VerticalTextAlignment = TextAlignment.Center
        };

        stackLayout.Children.Add(image);
        stackLayout.Children.Add(label);
        popup.Content = stackLayout;
        await this.ShowPopupAsync(popup);

    }

    private async void showToastInfo()
    {
        await Toast.Make($"\n\r{((dynamic)playlist.GetCurrentTrack().GetTitle())}\n\r" +
            $" Artist - {((dynamic)playlist.GetCurrentTrack().GetArtist())}\n\r" +
            $" Album - {((dynamic)playlist.GetCurrentTrack().GetAlbum())}\n\r",
            ToastDuration.Short).Show(cancellationToken);

    }
}

