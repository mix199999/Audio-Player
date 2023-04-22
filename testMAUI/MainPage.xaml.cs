﻿
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
using CommunityToolkit.Mvvm.Messaging;



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
    private List<string> _foldersList = new List<string>();
    private bool _visibility = true;
    private List<String> _favImgTheme;



    public MainPage(IFileSaver fileSaver, IConfiguration configuration)

    {
        _configuration = configuration;
        _playlist = new List<AudioPlaylist>();
        _configuration.GetSection("FolderList").Bind(_foldersList);
        _playlist = _configuration.GetSection("AudioPlaylists").Get<List<AudioPlaylist>>();


        InitializeComponent();
        player = new Player();
        player._status = playerStatus.IsNotPlaying;
        playlist = new AudioPlaylist();
        

        playlistView.ItemsSource = playlist.Tracks;
        playlistListView.ItemsSource = null;
        playlistListView.ItemsSource = _playlist;

        this.fileSaver = fileSaver;

        VolumeSlider.Value = 0;
        player.SetVolume(0);

        AudioPlayingImageControl.Opacity = 0;
        trackTimer.Interval = 1000;
        trackTimer.Elapsed += TimerTick;
        TrackProgressBarSlider.Value = 0;

        this.Unfocused += hidePopup;
        this.Focused += callPopup;
         var test = AppDomain.CurrentDomain.BaseDirectory;

        //Ładowanie ikon w zależności od motywu aplikacji
        _favImgTheme = new List<string>();
        if ((AppTheme)Application.Current.RequestedTheme == AppTheme.Light)
        {
            _favImgTheme.Add("favorite0solid.png");
            _favImgTheme.Add("favorite1solid.png");
        }
        else
        {
            _favImgTheme.Add("favorite0whitesolid.png");
            _favImgTheme.Add("favorite1whitesolid.png");
        }

        if (_foldersList.Count == 0) _foldersList.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
        if (_playlist == null)
        {
            var emptyPlaylist = new AudioPlaylist()
            {
                Name = "",
                Path = ""
            };
            _playlist.Add(emptyPlaylist);
        }
        
        foreach (var Folder in _foldersList)
        {
            playlist.LoadFromDirectory(Folder);

        }
        LoadToListView();

     
        foreach (var playlist in _playlist)
        {
            playlist.LoadFromM3U(playlist.Path);
        }

       



        MessagingCenter.Subscribe<SettingsPage, List<string>>(this, "FoldersList", (sender, foldersList) =>
        {
            this.playlist = new AudioPlaylist();
            if (foldersList != null)
            {
                this._foldersList = foldersList;
                foreach (var Folder in _foldersList) { playlist.LoadFromDirectory(Folder); }
                LoadToListView();
                SaveToJson();
            }

        });

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
            Cover = track.GetCover(),
            Favourite = track.GetFavourite() ? _favImgTheme[1] : _favImgTheme[0]
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

    private void favImg_Clicked(object sender, TappedEventArgs e)
    {
        bool fav = playlist.Tracks[playlist.GetCurrentTrackIndex()].GetFavourite();
        playlist.Tracks[playlist.GetCurrentTrackIndex()].SetFavourite(!fav);
        if (sender is Image image)
        {
            if (fav)
            {
                image.Source = _favImgTheme[0];
            } else { image.Source = _favImgTheme[1]; }
        }
    }

    private void nextBtn_Clicked(object sender, EventArgs e) => nextTrack();


    private void stopBtn_Clicked(object sender, EventArgs e)
    {
        trackTimer.Stop();
        CurrentTimeLabel.Opacity = 0.7;
        player.Pause();
        AudioPlayingImageControl.Opacity = 0;
    }
    private void playBtn_Clicked(object sender, EventArgs e)
    {
        playAudio();
        CurrentTimeLabel.Opacity = 1;
        if(playlist.Tracks.Count == 0) { return; }
        AudioPlayingImageControl.Opacity = 1;
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
        AudioPlayingImageControl.Opacity = 1;

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
                Cover = track.GetCover(),
                Favourite = track.GetFavourite() ? _favImgTheme[1] : _favImgTheme[0]
            });

            var newPlaylist = new AudioPlaylist()
            {
                Name = Path.GetFileNameWithoutExtension(playlistFile.FileName),
                Path = playlistFile.FullPath
            };
            _playlist.Add(newPlaylist);
            SaveToJson();
        }

    }

    private async void saveListBtn_Clicked(object sender, TappedEventArgs e)
    {
        using var stream = new MemoryStream(Encoding.Default.GetBytes(playlist.SaveToM3U()));
        var path = await fileSaver.SaveAsync(".M3U", stream, cancellationTokenSource.Token);

        var newPlaylist = new AudioPlaylist()
        {
            Name = Path.GetFileNameWithoutExtension(path.FilePath),
            Path = path.FilePath
        };
        _playlist.Add(newPlaylist);
        SaveToJson();
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

    private void replayBtn_Clicked(object sender, TappedEventArgs e) => ReplayPlaylist(sender);

    private void shuffleBtn_Clicked(Object sender, TappedEventArgs e) => PlayRandom(sender);

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
           
        });
        if (!_visibility) showToastInfo();
    }

    private void TrackProgressBarSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        if (ValueChangedEnabled && player._status == playerStatus.IsPlaying)
        {
            currentTrackTime = TimeSpan.FromSeconds(TrackProgressBarSlider.Value);
            player.SetTime(TrackProgressBarSlider.Value);

        }
    }

    private bool ReplayPlaylist(object s)
    {

        if(s is Image)
        {
            Image img = (Image)s;
            if(img.Opacity == 0.75) { img.Opacity = 0.4; } else { img.Opacity = 0.75; }
        }
        return true; 


    }

    private void PlayRandom(object s)
    {
        if (s is Image)
        {
            Image img = (Image)s;
            if (img.Opacity == 0.75) { img.Opacity = 0.4; } else { img.Opacity = 0.75; }
        }
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        PickFolder(cancellationToken);
    }

    private void loadToListViewFromDirectory(string Path)
    {
       
        playlist.LoadFromDirectory(Path);
        playlistView.ItemsSource = null;
        playlistView.ItemsSource = playlist.Tracks.Select(track => new
        {
            Title = track.GetTitle(),
            Duration = track.GetDuration().ToString("hh\\:mm\\:ss"),
            Album = track.GetAlbum(),
            Artist = track.GetArtist(),
            Path = track.GetFilePath(),
            Cover = track.GetCover(),
            Favourite = track.GetFavourite() ? _favImgTheme[1] : _favImgTheme[0]
        });
    }

    async Task PickFolder(CancellationToken cancellationToken)
    {
        var result = await FolderPicker.Default.PickAsync(cancellationToken);
        if (result.IsSuccessful)
        {
            if (_foldersList.Contains(result.Folder.Path))
            {
                await Toast.Make($"The selected folder {result.Folder.Path} is already added.").Show(cancellationToken);
            }
            else
            {
                _foldersList.Add(result.Folder.Path);
                loadToListViewFromDirectory(result.Folder.Path);
                SaveToJson();
            }
        }
        else
        {
            await Toast.Make($"The folder was not picked with error: {result.Exception.Message}").Show(cancellationToken);
        }
    }
    private void SaveToJson()
    {
        var appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appSettings.json");


        var foldersSettings = new Configuration
        {
            FolderList = _foldersList,
            AudioPlaylists = _playlist
        };

        var json = JsonConvert.SerializeObject(foldersSettings, Newtonsoft.Json.Formatting.Indented);

        System.IO.File.WriteAllText(appSettingsPath, json);
        playlistListView.ItemsSource = null;
        playlistListView.ItemsSource = _playlist;
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
        var toast = Toast.Make($"\n\r{((dynamic)playlist.GetCurrentTrack().GetTitle())}\n\r" +
           $" Artist - {((dynamic)playlist.GetCurrentTrack().GetArtist())}\n\r" +
           $" Album - {((dynamic)playlist.GetCurrentTrack().GetAlbum())}\n\r",
           ToastDuration.Short );          
            await toast.Show(cancellationToken);
       

    }



    private void settingsButtonClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new SettingsPage(_foldersList));
        //ustawienia.IsVisible = true;
        //glowny.IsVisible = false;
    }

    private void OnPlaylistSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem == null)
        {
            return;
        }

        var selectedPlaylist = (AudioPlaylist)e.SelectedItem;

        if (selectedPlaylist.Path != null)
        {
            trackTimer.Stop();
            player.Pause();

            playlist.clearList();
            playlist.LoadFromM3U(selectedPlaylist.Path);
            playlistView.ItemsSource = null;
            playlistView.ItemsSource = playlist.Tracks.Select(track => new
            {
                Title = track.GetTitle(),
                Duration = track.GetDuration().ToString("hh\\:mm\\:ss"),
                Album = track.GetAlbum(),
                Artist = track.GetArtist(),
                Path = track.GetFilePath(),
                Cover = track.GetCover(),
                Favourite = track.GetFavourite() ? _favImgTheme[1] : _favImgTheme[0]
            });

        }

    }

    private void callPopup(object sender, FocusEventArgs e)=>
        _visibility = true;
    

    private void hidePopup(object sender, FocusEventArgs e)=>
        _visibility = false;



    private async void SaveListBtn_Clicked(object sender, EventArgs e)
    {
        var popup = new PopupTrackInfo();
        popup.PlaylistSaved += OnPlaylistSaved;
        this.ShowPopup(popup);
    }

    private void PlaylistReturnBtn_Clicked(object sender, TappedEventArgs e)
    {
        
    }

    private async void OnPlaylistSaved(object? sender, string playlistName)
    {
        string Name = playlistName;
        string Path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

        var fullPath = Path + "\\" + playlistName + ".M3U";
        using var stream = new MemoryStream(Encoding.Default.GetBytes(playlist.SaveToM3U()));
        await using var fileStream = System.IO.File.Create(fullPath);
        await stream.CopyToAsync(fileStream);

        var newPlaylist = new AudioPlaylist()
        {
            Name = playlistName,
            Path = fullPath
        };
        _playlist.Add(newPlaylist);
        SaveToJson();
    }


    private void LoadToListView()
    {
        playlistView.ItemsSource = playlist.Tracks.Select(track => new
        {
            Title = track.GetTitle(),
            Duration = track.GetDuration().ToString("mm\\:ss"),
            Album = track.GetAlbum(),
            Artist = track.GetArtist(),
            Path = track.GetFilePath(),
            Cover = track.GetCover(),
            Favourite = track.GetFavourite() ? _favImgTheme[1] : _favImgTheme[0]
        });
    }

}



