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
    private AudioPlaylist mainPlaylist;
    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    CancellationToken cancellationToken = new CancellationToken();
    private System.Timers.Timer trackTimer = new System.Timers.Timer();
    private double currentTrackProgress;
    private bool ValueChangedEnabled = true;
    private IConfiguration _configuration;
    private List<AudioPlaylist> _playlists;
    private List<string> _foldersList = new List<string>();
    private bool _visibility = true;
    private List<String> _favImgTheme;
    private AudioPlaylist _favouriteSongsPlaylist;
    private int _previousIndex = -1;
    private DateTime _previousClickTime = DateTime.MinValue;

    List<PlaylistViewModel> trackViewModels = new List<PlaylistViewModel>();





    public MainPage(IFileSaver fileSaver, IConfiguration configuration)

    {
        _configuration = configuration;
        _playlists = new List<AudioPlaylist>();
        _configuration.GetSection("FolderList").Bind(_foldersList);
        _playlists = _configuration.GetSection("AudioPlaylists").Get<List<AudioPlaylist>>();
        //1 sza jest lista z ulubionymi
       


        InitializeComponent();

        

        player = new Player();
        player._status = playerStatus.IsNotPlaying;
        mainPlaylist = new AudioPlaylist();


       

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
        if (_playlists == null)
        {
            var emptyPlaylist = new AudioPlaylist()
            {
                Name = "",
                Path = ""
            };
            _playlists.Add(emptyPlaylist);
        }

        foreach (var Folder in _foldersList)
        {
            mainPlaylist.LoadFromDirectory(Folder);

        }
        foreach (var playlist in _playlists)
        {
            playlist.LoadFromM3U(playlist.Path);
        }


        _favouriteSongsPlaylist = _playlists[0];


        playlistView.ItemTapped += PlaylistListView_ItemTapped;
        

        playlistListView.ItemsSource = _playlists;
        MarkFavoriteSongsInMainPlaylist();
        LoadToListView();
      


        MessagingCenter.Subscribe<SettingsPage, List<string>>(this, "FoldersList", (sender, foldersList) =>
        {
            this.mainPlaylist = new AudioPlaylist();
            if (foldersList != null)
            {
                this._foldersList = foldersList;
                foreach (var Folder in _foldersList) { mainPlaylist.LoadFromDirectory(Folder); }
                LoadToListView();
                SaveToJson();
            }

        });


    }

    private async void PlaylistListView_ItemTapped(object sender, ItemTappedEventArgs e)
    {
       
        var selectedTrack = (PlaylistViewModel)e.Item;
        var selectedIndex = e.ItemIndex;
        if (selectedTrack != null)
        {
            if ( _previousIndex == selectedIndex)
            {
                //nie ulubiony fav[0]
                if (selectedTrack.Favourite == _favImgTheme[0])
                {
                    _favouriteSongsPlaylist.AddTrack(mainPlaylist.Tracks[selectedIndex]);                                           
                    _playlists[0] = _favouriteSongsPlaylist;
                    AudioPlaylist.AppendTrackToFavoritelistFile(mainPlaylist.Tracks[selectedIndex]);

                }
                else if (selectedTrack.Favourite == _favImgTheme[1])
                {
                    
                    _favouriteSongsPlaylist.RemoveTrack(mainPlaylist.Tracks[selectedIndex]);
                    _playlists[0] = null;
                    _playlists[0] = _favouriteSongsPlaylist;
                   // _playlists[0] = _favouriteSongsPlaylist;
                    
                    AudioPlaylist.RemoveTrackFromM3U(mainPlaylist.Tracks[selectedIndex]);
                }

               // await Task.Delay(100);
                LoadToListView();
                // ((ListView)sender).SelectedItem = null;
                // ((ListView)sender).SelectedItem = selectedTrack;


                
            }

            _previousIndex = selectedIndex;


        }

    }

    public void MarkFavoriteSongsInMainPlaylist()
    {
        for (int i = 0; i < mainPlaylist.Tracks.Count; i++)
        {
            mainPlaylist.Tracks[i].SetFavourite(false);
            for (int j = 0; j < _favouriteSongsPlaylist.Tracks.Count; j++)
            {
                if (_favouriteSongsPlaylist.Tracks[j].GetTitle() == mainPlaylist.Tracks[i].GetTitle() 
                    && _favouriteSongsPlaylist.Tracks[j].GetArtist() == mainPlaylist.Tracks[i].GetArtist()
                    && _favouriteSongsPlaylist.Tracks[j].GetDuration() == mainPlaylist.Tracks[i].GetDuration())
                    mainPlaylist.Tracks[i].SetFavourite(true);
                

            }
        }



        
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
            mainPlaylist.AddTrack(audioFile);

        }

        playlistView.ItemsSource = mainPlaylist.Tracks.Select(track => new
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
        
        mainPlaylist.Next();
        currentTrackTime = TimeSpan.Zero;
        AudioFile audioFile = mainPlaylist.GetCurrentTrack();
        if (audioFile != null)
        {
            player.Load(audioFile.GetFilePath());
            player.Play();
            setCurrentTrackInfo();

        }
    }

    private  void favImg_Clicked(object sender, TappedEventArgs e)
    {

        //    bool fav = mainPlaylist.Tracks[mainPlaylist.GetCurrentTrackIndex()].GetFavourite();
        //    mainPlaylist.Tracks[mainPlaylist.GetCurrentTrackIndex()].SetFavourite(!fav);
        //if (sender is Image image)
        //{
        //    if (fav)
        //    {

        //        image.Source = _favImgTheme[1];
        //        _favouriteSongsPlaylist.RemoveTrack(mainPlaylist.Tracks[mainPlaylist.GetCurrentTrackIndex()]);
        //        _playlists[0] = _favouriteSongsPlaylist;
        //        // kłopoty trzeba jakoś usunąć z playlisty
        //        AudioPlaylist.RemoveTrackFromM3U(mainPlaylist.Tracks[mainPlaylist.GetCurrentTrackIndex()]);
        //    }
        //    else
        //    {
        //        image.Source = _favImgTheme[0];
        //        //dodawanie do playlisty ulublionych
        //        _favouriteSongsPlaylist.AddTrack(mainPlaylist.Tracks[mainPlaylist.GetCurrentTrackIndex()]);
        //        //update ulubionej playlisty
        //        _playlists[0] = _favouriteSongsPlaylist;
        //        // dopisanie
        //        AudioPlaylist.AppendTrackToFavoritelistFile(mainPlaylist.Tracks[mainPlaylist.GetCurrentTrackIndex()]);

        //    }
        //}

        
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
        if(mainPlaylist.Tracks.Count == 0) { return; }
        AudioPlayingImageControl.Opacity = 1;
    }

   


    private void prevBtn_Clicked(object sender, EventArgs e)
    {
        mainPlaylist.Previous();
        currentTrackTime = TimeSpan.Zero;
        AudioFile audioFile = mainPlaylist.GetCurrentTrack();

        if (audioFile != null)
        {
            player.Load(audioFile.GetFilePath());
            player.Play();
            setCurrentTrackInfo();
        }
    }

    //testy
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

        mainPlaylist.SetCurrentTrack(selectedIndex);

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

            mainPlaylist.clearList();
            mainPlaylist.LoadFromM3U(playlistFile.FullPath);
            playlistView.ItemsSource = null;
            playlistView.ItemsSource = mainPlaylist.Tracks.Select(track => new
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
            _playlists.Add(newPlaylist);
            SaveToJson();
        }

    }

    private async void saveListBtn_Clicked(object sender, TappedEventArgs e)
    {
        using var stream = new MemoryStream(Encoding.Default.GetBytes(mainPlaylist.SaveToM3U()));
        var path = await fileSaver.SaveAsync(".M3U", stream, cancellationTokenSource.Token);

        var newPlaylist = new AudioPlaylist()
        {
            Name = Path.GetFileNameWithoutExtension(path.FilePath),
            Path = path.FilePath
        };
        _playlists.Add(newPlaylist);
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
            TimeSpan durationTime = mainPlaylist.GetCurrentTrack().GetDuration();
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
            CurrentTrackAlbum.Text = ((dynamic)mainPlaylist.GetCurrentTrack().GetAlbum());
            CurrentTrackArtist.Text = ((dynamic)mainPlaylist.GetCurrentTrack().GetArtist());
            CurrentTrackTitle.Text = ((dynamic)mainPlaylist.GetCurrentTrack().GetTitle());
            CurrentTrackCover.Source = (dynamic)mainPlaylist.GetCurrentTrack().GetCoverUrl();
           
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
        await PickFolder(cancellationToken);
    }

    private void loadToListViewFromDirectory(string Path)
    {
       
        mainPlaylist.LoadFromDirectory(Path);
        playlistView.ItemsSource = null;
        playlistView.ItemsSource = mainPlaylist.Tracks.Select(track => new
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
            AudioPlaylists = _playlists
        };

        var json = JsonConvert.SerializeObject(foldersSettings, Newtonsoft.Json.Formatting.Indented);

        System.IO.File.WriteAllText(appSettingsPath, json);
        playlistListView.ItemsSource = null;
        playlistListView.ItemsSource = _playlists;
    }

    private async Task ShowPopupInfo()
    {
        var popup = new Popup();
        popup.Size = new Size(300, 300);

        var stackLayout = new VerticalStackLayout();
        var image = new Image { Source = mainPlaylist.GetCurrentTrack().GetCoverUrl() };
        var label = new Label
        {
            Text = $"\n\r{((dynamic)mainPlaylist.GetCurrentTrack().GetTitle())}\n\r" +
            $" Artist - {((dynamic)mainPlaylist.GetCurrentTrack().GetArtist())}\n\r" +
            $" Album - {((dynamic)mainPlaylist.GetCurrentTrack().GetAlbum())}\n\r",
            VerticalTextAlignment = TextAlignment.Center
        };

        stackLayout.Children.Add(image);
        stackLayout.Children.Add(label);
        popup.Content = stackLayout;
        await this.ShowPopupAsync(popup);

    }

    private async void showToastInfo()
    {
        var toast = Toast.Make($"\n\r{((dynamic)mainPlaylist.GetCurrentTrack().GetTitle())}\n\r" +
           $" Artist - {((dynamic)mainPlaylist.GetCurrentTrack().GetArtist())}\n\r" +
           $" Album - {((dynamic)mainPlaylist.GetCurrentTrack().GetAlbum())}\n\r",
           ToastDuration.Short );          
            await toast.Show(cancellationToken);
       

    }



    private async void settingsButtonClicked(object sender, EventArgs e)
    {
       await Navigation.PushAsync(new SettingsPage(_foldersList));
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

            mainPlaylist.Tracks.Clear();
            mainPlaylist.LoadFromM3U(selectedPlaylist.Path);            
            playlistView.ItemsSource = null;
            LoadToListView();

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
        using var stream = new MemoryStream(Encoding.Default.GetBytes(mainPlaylist.SaveToM3U()));
        await using var fileStream = System.IO.File.Create(fullPath);
        await stream.CopyToAsync(fileStream);

        var newPlaylist = new AudioPlaylist()
        {
            Name = playlistName,
            Path = fullPath
        };
        _playlists.Add(newPlaylist);
        SaveToJson();
    }


    private void LoadToListView()
    {
        playlistView.ItemsSource = null;
        MarkFavoriteSongsInMainPlaylist();

        trackViewModels = new List<PlaylistViewModel>();
        foreach (var track in mainPlaylist.Tracks)
        {
            var trackViewModel = new PlaylistViewModel
            {
                Title = track.GetTitle(),
                Duration = track.GetDuration().ToString("mm\\:ss"),
                Album = track.GetAlbum(),
                Artist = track.GetArtist(),
                Path = track.GetFilePath(),           
                Favourite = track.GetFavourite() ? _favImgTheme[1] : _favImgTheme[0]
            };

            trackViewModels.Add(trackViewModel);

        }
        
        playlistView.ItemsSource = trackViewModels;


    }

}

public class PlaylistViewModel
{
    public string Title { get; set; }
    public string Duration { get; set; }
    public string Album { get; set; }
    public string Artist { get; set; }
    public string Path { get; set; }
    public string Cover { get; set; }
    public string Favourite { get; set; }
}




