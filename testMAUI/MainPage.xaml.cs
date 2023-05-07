
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
using Microsoft.Maui.Dispatching;
using System;
using System.Diagnostics;
using NAudio.Extras;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using GNOM;

public partial class MainPage : ContentPage
{
    
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
    private bool _isRandom = false;
    private bool _isLoop = false;
    private bool _isAnimating = true;
    private int _sbInputLength = 0;
    private bool _firstTimeRun = true;
    private List<EqualizerBand[]> _equalizerList = new List<EqualizerBand[]>();
    internal Theme _theme = new();
    private int _playAudioIndex = -1;


    List<PlaylistViewModel> trackViewModels = new List<PlaylistViewModel>();
    List<PlaylistViewModel> _searchPlaylist = new List<PlaylistViewModel>();


    public MainPage(IFileSaver fileSaver, IConfiguration configuration)

    {
        _configuration = configuration;
        _playlists = new List<AudioPlaylist>();
        _configuration.GetSection("FolderList").Bind(_foldersList);
        _playlists = _configuration.GetSection("AudioPlaylists").Get<List<AudioPlaylist>>();
        _firstTimeRun = _configuration.GetValue<bool>("FirstTimeRun");
        _configuration.GetSection("Theme").Bind(_theme);
        //1 sza jest lista z ulubionymi




        InitializeComponent();

     

        player = new Player();
        player._status = playerStatus.IsNotPlaying;
        mainPlaylist = new AudioPlaylist();

        this.fileSaver = fileSaver;

        VolumeSlider.Value = 50;
        player.SetVolume(50);

        AudioPlayingImageControl.Opacity = 0;
        trackTimer.Interval = 1000;
        trackTimer.Elapsed += TimerTick;
        TrackProgressBarSlider.Value = 0;

        this.Unfocused += hidePopup;
        this.Focused += callPopup;

        if (_foldersList.Count == 0) _foldersList.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
        


        foreach (var playlist in _playlists)
        {
            playlist.LoadFromM3U(playlist.Path);
        }


        _favouriteSongsPlaylist = _playlists[0];


        playlistView.ItemTapped += PlaylistListView_ItemTapped;
        playlistListView.ItemTapped += MultiplePlaylistView_ItemTapped;

        playlistListView.ItemsSource = MultiplePlaylistViewModel.CreatePlaylistViewModel(_playlists,Color.FromArgb(_theme.SecondaryColor));

        LoadFromDirectory();

        WeakReferenceMessenger.Default.Register<StringListMessage>(this, OnStringListMessageReceived);
        WeakReferenceMessenger.Default.Register<AudioListMessage>(this, OnAudioListMessageReceived);

        searchBar.TextChanged += OnSearchTextChanged;
        searchBar.Focused += OnSearchBarFocused;
        searchBar.Unfocused += OnSearchBarUnfocused;
        searchBar.SearchButtonPressed += SearchBar_SearchButtonPressed;

        _searchPlaylist = PlaylistViewModel.CreatePlaylistViewModel(mainPlaylist);
        resultsList.ItemTapped += ResultsList_ItemTapped;

        LoadColors();
        LoadEq();
        LoadThemedButtons();
        BindingContext = this;

        if (_firstTimeRun)
        {
            ShowInstruction();
            _firstTimeRun = false;
            SaveToJson();
        }
    }


    private void LoadEq()
    {
        if(_configuration.GetSection("EqualizerSettings").Get<List<EqualizerBand[]>>() != null) 
        {
        _equalizerList = _configuration.GetSection("EqualizerSettings").Get<List<EqualizerBand[]>>();
            player.Bands = _equalizerList[0];
        }
        else
        {
            _equalizerList.Add(new EqualizerBand[0]);
            _equalizerList[0]=player.Bands;
        }
    }


    /// <summary>
    /// Metoda służąca do odbierania danych z innych stron, które są obiektem klasy StringListMessage
    /// </summary>
    /// <param name="recipient">odbiorca</param>
    /// <param name="message">odebrana wiadomość </param>
    private void OnStringListMessageReceived(object recipient, StringListMessage message)
    {
        List<string> receivedFolders = message.Strings;

        if (receivedFolders != null)
        {
            this._foldersList = receivedFolders;
            SaveToJson();//chyba niepotrzebne
        }
    }

    /// <summary>
    /// Metoda służąca do odbierania danych z innych stron, które są obiektem klasy AudioListMessage
    /// </summary>
 
    private void OnAudioListMessageReceived(object recipient, AudioListMessage message)
    {
        List<AudioPlaylist> receivedPlaylists = message.Playlist;

        if (receivedPlaylists != null)
        {
            this._playlists = receivedPlaylists;
            SaveToJson();//chyba niepotrzebne

        }

         playlistListView.ItemsSource = MultiplePlaylistViewModel.CreatePlaylistViewModel(_playlists, Color.FromArgb(_theme.SecondaryColor));
    }

    private void ShowInstruction()
    {
        try
        {
            var p = new Process();
            string documentsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string path = $@"{documentsDir}\GNOM\instrukcja.pdf";
            p.StartInfo = new ProcessStartInfo(path) { UseShellExecute = true };
            p.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to open the instuction, " + ex.ToString());
        }
    }

    /// <summary>
    /// Ta metoda jest wywoływana, gdy użytkownik naciska przycisk wyszukiwania lub enter.
    /// Metoda ta zatrzymuje timer śledzenia utworu, zatrzymuje odtwarzacz audio,
    /// tworzy nową playlistę audio i dodaje do niej utwory z listy wyników
    /// Następnie uruchamia asynchronicznie metodę "LoadToListView".
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia, zawierają informacje o wybranym elemencie.</param>

    private void SearchBar_SearchButtonPressed(object sender, EventArgs e)
    {
        trackTimer.Stop();
        player.Pause();
        mainPlaylist = new AudioPlaylist();
        foreach (PlaylistViewModel result in resultsList.ItemsSource)
        {
            mainPlaylist.AddTrack(new AudioFile(result.Path));
        }

        Task.Run(async () => {
            await LoadToListView();
        });

    }

    /// <summary>
    /// Obsługuje zdarzenie "ItemTapped" wywoływane po naciśnięciu na element listy.
    /// Tworzy nową playlistę z wybranym utworem oraz zatrzymuje odtwarzanie i licznik czasu.
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia, zawierają informacje o wybranym elemencie.</param>
    private void ResultsList_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        var selectedResult = (PlaylistViewModel)e.Item;
        if (selectedResult != null)
        {
            trackTimer.Stop();
            player.Pause();
            mainPlaylist = new AudioPlaylist();
            mainPlaylist.AddTrack(new AudioFile(selectedResult.Path));
            Task.Run(async () => {
                await LoadToListView();
            });
        }
    }

    private void OnSearchBarFocused(object sender, FocusEventArgs e)
    {
        if (sender is SearchBar searchBar)
        {
            if (searchBar.Text == null)
            {
                searchBar.HeightRequest = 0;
                return;
            }
            else
            {
                resultsList.ItemsSource = null;
                resultsList.IsVisible = true;
                _sbInputLength = searchBar.Text.Length;
                resultsList.HeightRequest = 0;
            }
        }

    }

    private void OnSearchBarUnfocused(object sender, FocusEventArgs e)
    {
        resultsList.IsVisible = false;
        searchBar.Text = "";
    }

    /// <summary>
    /// Metoda wywoływana po zmianie tekstu w SearchBarze, która aktualizuje wyniki wyszukiwania.
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    private void OnSearchTextChanged(object sender, EventArgs e)
    {

        SearchBar searchBar = (SearchBar)sender;

        if (searchBar.Text == null || searchBar.Text == String.Empty)
        {
            Task.Run(() => ShowResultsList(false));
            //resultsList.HeightRequest = 0;
            return;
        }
        if (_sbInputLength == 0)
            Task.Run(() => ShowResultsList(true));
        //resultsList.HeightRequest = 200;

        string searchText = searchBar.Text.ToLower();


        var searchResults = _searchPlaylist.Where(item =>
            item.Title.ToLower().Contains(searchText) ||
            item.Artist.ToLower().Contains(searchText) ||
            item.Album.ToLower().Contains(searchText)).ToList();


        var searchResultsViewModels = searchResults.Select(item => new PlaylistViewModel
        {
            TitleAndArtist = $"{item.Title} - {item.Artist}",
            Path = item.Path,
            SecondaryColor = Color.FromArgb(_theme.SecondaryColor)

        }).ToList();


        resultsList.ItemsSource = searchResultsViewModels;

    }

    private void ShowResultsList(bool showResults)
    {
        if (showResults)
        {
            var animation = new Microsoft.Maui.Controls.Animation(v => resultsList.HeightRequest = v, 0, 150, Easing.CubicInOut);
            animation.Commit(resultsList, "HeightAnimation", 16, 500, Easing.CubicOut);

            //await resultsSV.ScaleYTo(10, 500, Easing.CubicOut);
        }
        else if (!showResults)
        {
            var animation = new Microsoft.Maui.Controls.Animation(v => resultsList.HeightRequest = v, 150, 0, Easing.CubicInOut);
            animation.Commit(resultsList, "HeightAnimation", 16, 500, Easing.CubicOut);

            //await resultsSV.ScaleYTo(0, 500, Easing.CubicOut);
        }
        _sbInputLength = searchBar.Text.Length;
    }



    /// <summary>
    /// Ładuje ścieżki dźwiękowe z folderów na listę odtwarzania i wyświetla je w widoku listy.
    /// </summary>    

    private void LoadFromDirectory()
    {
        mainPlaylist.Tracks.Clear();
        foreach (var Folder in _foldersList)
        {
            mainPlaylist.LoadFromDirectory(Folder);

        }
        Task.Run(async () => { await LoadToListView(); });
    }

    /// <summary>
    /// Służy do dodawania utworów do playlisty ulubione użytkownika
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    private async void PlaylistListView_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        await Dispatcher.DispatchAsync(async () =>
        {
            var selectedTrack = (PlaylistViewModel)e.Item;
            var selectedIndex = e.ItemIndex;
            if (selectedTrack != null)
            {
                if (_previousIndex == selectedIndex)
                {
                   
                    if (selectedTrack.Favourite == _favImgTheme[0])
                    {
                        _favouriteSongsPlaylist.AddTrack(mainPlaylist.Tracks[selectedIndex]);
                        _playlists[0] = _favouriteSongsPlaylist;
                        AudioPlaylist.AppendTrackToFavoritelistFile(mainPlaylist.Tracks[selectedIndex]);
                        selectedTrack.Favourite = _favImgTheme[1];

                    }
                    else if (selectedTrack.Favourite == _favImgTheme[1])
                    {

                        _favouriteSongsPlaylist.RemoveTrack(mainPlaylist.Tracks[selectedIndex]);
                        _playlists[0] = _favouriteSongsPlaylist;
                        AudioPlaylist.RemoveTrackFromM3U(mainPlaylist.Tracks[selectedIndex]);
                        selectedTrack.Favourite = _favImgTheme[0];
                    }
                   

                }

                _previousIndex = selectedIndex;

            }
        });

    }
    /// <summary>
    /// Oznacza ulubione utwory w głównej playliście. Porównuje utwory w głównej playliście z utworami na 
    /// liście ulubionych i ustawia flagę "ulubiony" na true, jeśli są identyczne.
    /// </summary>
    /// <returns>Task</returns>
    public async Task MarkFavoriteSongsInMainPlaylist()
    {
        await Dispatcher.DispatchAsync(() =>
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

        });

    }



    /// <summary>
    /// Metoda do wczytywania plików audio
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    private async void filesBtn_Clicked(object sender, EventArgs e)
    {
      

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

        await Task.Run(async () => { await LoadToListView(); });


    }

    /// <summary>
    /// Przechodzi do następnego utworu na liście odtwarzania.
    /// </summary>
    private async void nextTrack()
    {
        
        await Task.Delay(500);

       
        await Dispatcher.DispatchAsync(async () =>
        {
            AudioFile audioFile;
            currentTrackTime = TimeSpan.Zero;
            switch (_isRandom)
            {
                case false:
                    // Jeśli bieżący utwór nie jest ostatnim na liście odtwarzania, przejdź do kolejnego utworu.
                    if (mainPlaylist.GetCurrentTrackIndex() != mainPlaylist.Tracks.Count - 1)
                    {
                        mainPlaylist.Next();

                        audioFile = mainPlaylist.GetCurrentTrack();
                        if (audioFile != null)
                        {
                            player.Load(audioFile.GetFilePath());
                            player.Play();
                            await setCurrentTrackInfo();
                        }
                    }
                    // Jeśli bieżący utwór jest ostatnim na liście odtwarzania i jest włączone zapętlanie, rozpocznij odtwarzanie listy od początku.
                    else if (mainPlaylist.GetCurrentTrackIndex() == mainPlaylist.Tracks.Count - 1 && _isLoop)
                    {
                        await PlayInLoop();
                    }
                    // Jeśli bieżący utwór jest ostatnim na liście odtwarzania i jest wyłączone zapętlanie, zatrzymaj odtwarzanie.
                    else
                    {
                        player.Stop();
                        trackTimer.Stop();
                    }
                    break;

                case true:
                    // Jeśli włączone jest losowe odtwarzanie, wygeneruj losowy indeks utworu, dopóki nie zostanie uzyskany inny indeks utworu niż bieżący.
                    int index = await GenerateRandomIndex();
                    while (index == mainPlaylist.GetCurrentTrackIndex())
                    {
                        index = await GenerateRandomIndex();
                    }
                    // Ustaw bieżący utwór na losowo wygenerowany utwór.
                    mainPlaylist.SetCurrentTrack(index);

                    audioFile = mainPlaylist.GetCurrentTrack();
                    if (audioFile != null)
                    {
                        player.Load(audioFile.GetFilePath());
                        player.Play();
                        await setCurrentTrackInfo();
                    }
                    break;
            }
        });
    }


    /// <summary>
    /// Obsługuje zdarzenie kliknięcia przycisku następnego utworu
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    private void nextBtn_Clicked(object sender, EventArgs e) => nextTrack();

    /// <summary>
    /// Obsługuje zdarzenie kliknięcia przycisku zatrzymania odtwarzania
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    private void stopBtn_Clicked(object sender, EventArgs e)
    {
        trackTimer.Stop();
        CurrentTimeLabel.Opacity = 0.7;
        player.Pause();
        AudioPlayingImageControl.Opacity = 0;
        Console.WriteLine(_theme.ToString());
    }

    /// <summary>
    /// Obsługuje zdarzenie kliknięcia przycisku startu odtwarzania
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    private void playBtn_Clicked(object sender, EventArgs e)
    {
        playAudio();
        CurrentTimeLabel.Opacity = 1;
        if (mainPlaylist.Tracks.Count == 0) { return; }
        AudioPlayingImageControl.Opacity = 1;
    }



    /// <summary>
    /// Obsługuje zdarzenie kliknięcia przycisku poprzedniego utworu
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    private void prevBtn_Clicked(object sender, EventArgs e)
    {
        mainPlaylist.Previous();
        currentTrackTime = TimeSpan.Zero;
        AudioFile audioFile = mainPlaylist.GetCurrentTrack();

        if (audioFile != null)
        {
            player.Load(audioFile.GetFilePath());
            player.Play();
            Task.Run(async () => { await setCurrentTrackInfo(); });
        }
    }
    /// <summary>
    /// Obsługuje zdarzenie wybrania elementu na liście odtwarzania.
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem == null)
            return;

        var list = new List<object>();
        currentTrackTime = TimeSpan.Zero;
        AudioPlayingImageControl.Opacity = 1;

        if (playlistView.ItemsSource is IEnumerable<object> enumerable)
        {
            list = enumerable.ToList();
        }

        int selectedIndex = list.IndexOf(e.SelectedItem);

        mainPlaylist.SetCurrentTrack(selectedIndex);

        playAudio();
        Task.Run(async () => { await setCurrentTrackInfo(); });
    }



    /// <summary>
    /// Obsługuje zdarzenie kliknięcia przycisku Load List - ładowania playlisty .m3u
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
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


    /// <summary>
    ///  Obsługuje zdarzenie kliknięcia przycisku Save list - zapisania playlisty .m3u
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    private async void saveListBtn_Clicked(object sender, TappedEventArgs e)
    {
        //using var stream = new MemoryStream(Encoding.Default.GetBytes(mainPlaylist.SaveToM3U()));
        //var path = await fileSaver.SaveAsync(".M3U", stream, cancellationTokenSource.Token);

        //var newPlaylist = new AudioPlaylist()
        //{
        //    Name = Path.GetFileNameWithoutExtension(path.FilePath),
        //    Path = path.FilePath
        //};
        //_playlists.Add(newPlaylist);
        //SaveToJson();
       


    }


    /// <summary>
    /// Rozpoczyna odtwarzanie audio
    /// </summary>
    private async void playAudio()
    {
        if (playlistView.SelectedItem != null)
        {
            trackTimer.Start();
            await Dispatcher.DispatchAsync(() => {


                string path = ((dynamic)playlistView.SelectedItem).Path;

                AudioFile audioFile = new AudioFile(path);
                player.Load(audioFile.GetFilePath());
                player.Play();

                player._totalTime = audioFile.GetDuration();
                TrackProgressBarSlider.Maximum = audioFile.GetDuration().TotalSeconds;

            });

            await Task.Delay(500);
            if (_previousIndex != _playAudioIndex)
            {
                await setCurrentTrackInfo();
                _playAudioIndex = _previousIndex;
            }


        }


    }



    /// <summary>
    /// Obsługuje zdarzenie przesunięcia suwaka głośności
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    private async void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        await Task.Run(() => player.SetVolume(e.NewValue));
    }
    /// <summary>
    /// Obsługuje kliknięcie przycisku "Backward" (Cofnij o 15 sekund).
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    private void backwardBtn_Clicked(object sender, TappedEventArgs e)
    {
        currentTrackTime -= TimeSpan.FromSeconds(15);
        player.SkipBackward();
        currentTrackProgress -= 15;
    }
    /// <summary>
    /// Obsługuje kliknięcie przycisku "Forward" (Przewiń do przodu o 15 sekund).
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    private void forwardBtn_Clicked(object sender, TappedEventArgs e)
    {
        currentTrackTime += TimeSpan.FromSeconds(15);
        player.SkipForward();

        currentTrackProgress += 15;
    }

    /// <summary>
    /// Obsługuje kliknięcie przycisku "Replay" (Odtwarzanie w pętli).
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    private void replayBtn_Clicked(object sender, TappedEventArgs e) => ReplayPlaylist(sender);
    /// <summary>
    /// Obsługuje kliknięcie przycisku "Random" (Odtwarzanie losowe).
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    private void shuffleBtn_Clicked(Object sender, TappedEventArgs e) => PlayRandom(sender);

    private void HoverBegan(object sender, PointerEventArgs e)
    {
        if (sender is Button button)
        {
            switch (App.Current.RequestedTheme)
            {
                case AppTheme.Light:
                    button.BackgroundColor = Color.FromRgba(0, 0, 0, 20);
                    break;
                case AppTheme.Dark:
                    button.BackgroundColor = Color.FromRgba(255, 255, 255, 20);
                    break;
            }
        }
    }

    private void HoverEnded(object sender, PointerEventArgs e)
    {
        if (sender is Button button)
        {
            button.BackgroundColor = Color.FromRgba(0, 0, 0, 0);
        }
    }
    /// <summary>
    /// Obsługuje zdarzenie Tick timera trackTimer.
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    private async void TimerTick(object sender, ElapsedEventArgs e)
    {
        await Dispatcher.DispatchAsync(() =>
        {
            if (mainPlaylist.GetCurrentTrack() != null)
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
            }

        });
    }

    /// <summary>
    /// Asynchronicznie ustawia informacje o bieżącym utworze.
    /// </summary>
    /// <returns>Task</returns>
    private async Task setCurrentTrackInfo()
    {
        await Dispatcher.DispatchAsync(async () =>
        {

            if (mainPlaylist.GetCurrentTrack() != null)
            {
                if (!_isAnimating)
                {
                    CurrentTrackAlbum.Opacity = 0;
                    CurrentTrackAlbum.TranslationX = 100;
                    CurrentTrackArtist.Opacity = 0;
                    CurrentTrackArtist.TranslationX = 100;
                    CurrentTrackTitle.Opacity = 0;
                    CurrentTrackTitle.TranslationX = 100;

                    CurrentTrackAlbum.Text = ((dynamic)mainPlaylist.GetCurrentTrack().GetAlbum());
                    CurrentTrackArtist.Text = ((dynamic)mainPlaylist.GetCurrentTrack().GetArtist());
                    CurrentTrackTitle.Text = ((dynamic)mainPlaylist.GetCurrentTrack().GetTitle());

                    _isAnimating = true;
                    await Task.Run(() => CurrentTrackAnimation());
                }
                _isAnimating = false;

                while (mainPlaylist.GetCurrentTrack().GetCoverUrl() == null)
                {
                    await mainPlaylist.Tracks[mainPlaylist.GetCurrentTrackIndex()].SetCoverUrl();
                }
                await Task.Delay(TimeSpan.FromSeconds(1));
                try { 
                 
                CurrentTrackCover.Source = mainPlaylist.GetCurrentTrack().GetCoverUrl();
                }
                catch (System.NullReferenceException) {
                    CurrentTrackCover.Source = "note.png";
                }
                if (!_visibility) showToastInfo();

            }


        });

    }


    private async Task CurrentTrackAnimation()
    {
        //var tf = CurrentTrackTitle.FadeTo(1, 1000, Easing.CubicOut);
        //var tp = CurrentTrackTitle.TranslateTo(0, 0, 1000, Easing.CubicOut);

        var t1 = Task.WhenAll(
            CurrentTrackTitle.FadeTo(1, 500, Easing.CubicOut),
            CurrentTrackTitle.TranslateTo(0, 0, 500, Easing.CubicOut)
        );

        //var arf = CurrentTrackArtist.FadeTo(1, 1000, Easing.CubicOut);
        //var arp = CurrentTrackArtist.TranslateTo(0, 0, 1000, Easing.CubicOut);

        await Task.Delay(250);

        var t2 = Task.WhenAll(
            CurrentTrackArtist.FadeTo(1, 500, Easing.CubicOut),
            CurrentTrackArtist.TranslateTo(0, 0, 500, Easing.CubicOut)
        );

        //var alf = CurrentTrackAlbum.FadeTo(1, 1000, Easing.CubicOut);
        //var alp = CurrentTrackAlbum.TranslateTo(0, 0, 1000, Easing.CubicOut);

        await Task.Delay(250);

        var t3 = Task.WhenAll(
            CurrentTrackAlbum.FadeTo(1, 500, Easing.CubicOut),
            CurrentTrackAlbum.TranslateTo(0, 0, 500, Easing.CubicOut)
        );

        await Task.WhenAll(t1, t2, t3);
    }

    /// <summary>
    /// Obsługuje zdarzenie przesunięcia suwaka czasu odtwarzania utworu
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>

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
        if (_isLoop) _isLoop = false;
        else if (!_isLoop) { _isLoop = true; }

        if (s is Image)
        {
            Image img = (Image)s;
            if (img.Opacity == 0.75) { img.Opacity = 0.4; } else { img.Opacity = 0.75; }
        }
        return true;


    }
    /// <summary>
    /// Odtwarzanie w pętli playlisty
    /// </summary>
    /// <returns>Task</returns>
    private async Task PlayInLoop()
    {
        await Task.Delay(500);

        await Dispatcher.DispatchAsync(async () =>
        {
            AudioFile audioFile;
            currentTrackTime = TimeSpan.Zero;

            if (_isLoop)
            {
                mainPlaylist.SetCurrentTrack(0);

                audioFile = mainPlaylist.GetCurrentTrack();
                if (audioFile != null)
                {
                    player.Load(audioFile.GetFilePath());
                    player.Play();
                    await setCurrentTrackInfo();
                }
            }

        });
    }

    private void PlayRandom(object s)
    {

        if (_isRandom) { _isRandom = false; }
        else if (!_isRandom) { _isRandom = true; }

        if (s is Image)
        {
            Image img = (Image)s;
            if (img.Opacity == 0.75) { img.Opacity = 0.4; } else { img.Opacity = 0.75; }
        }
    }
    /// <summary>
    /// Generuje losową liczbę w zakresie od 0 do długości obecnie załadowanej playlisty
    /// </summary>
    /// <returns></returns>
    private async Task<int> GenerateRandomIndex()
    {
        int index = 0;
        await Dispatcher.DispatchAsync(new Action(() =>
        {
            Random random = new Random();
            index = random.Next(mainPlaylist.Tracks.Count - 1);

        }));

        return index;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await PickFolder(cancellationToken);
    }

    /// <summary>
    /// Ładuje utwory z katalogu do kontrolki ListView.
    /// </summary>
    /// <param name="Path">ścieżka do katalogu</param>
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
            Favourite = track.GetFavourite() ? _favImgTheme[1] : _favImgTheme[0]
        });
    }

    /// <summary>
    /// Asynchronicznie otwiera okno wyboru folderu i dodaje wybrany folder do listy.
    /// </summary>
    /// <param name="cancellationToken">Token anulowania</param>
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
    /// <summary>
    /// Zapisuje ustawienia do pliku konfiguracyjnego JSON
    /// </summary>
    private void SaveToJson()
    {
        // var appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appSettings.json");
        var tmp = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GNOM");
        var appSettingsPath = Path.Combine(tmp, "appSettings.json");


        var foldersSettings = new Configuration
        {
            FolderList = _foldersList,
            AudioPlaylists = _playlists,
            FirstTimeRun = _firstTimeRun,
            Theme = _theme,
            EqualizerSettings =_equalizerList
        };

        var json = JsonConvert.SerializeObject(foldersSettings, Newtonsoft.Json.Formatting.Indented);

        System.IO.File.WriteAllText(appSettingsPath, json);
        playlistListView.ItemsSource = MultiplePlaylistViewModel.CreatePlaylistViewModel(_playlists, Color.FromArgb(_theme.SecondaryColor));
    }


    
    /// <summary>
    /// Wyświetla toast o bieżącym utworze
    /// </summary>
    /// <returns>Task</returns>
    private async void showToastInfo()
    {
        var toast = Toast.Make($"\n\r{((dynamic)mainPlaylist.GetCurrentTrack().GetTitle())}\n\r" +
        $" Artist - {((dynamic)mainPlaylist.GetCurrentTrack().GetArtist())}\n\r" +
        $" Album - {((dynamic)mainPlaylist.GetCurrentTrack().GetAlbum())}\n\r",
        ToastDuration.Short);
        await toast.Show(cancellationToken);
    }


    /// <summary>
    /// Przechwytuje zdarzenie kliknięcia przycisku Settings (Ustawienia)
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    private async void settingsButtonClicked(object sender, EventArgs e)
    {
        // player.Pause();
        // trackTimer.Stop();
        var preChange = _theme;
        SettingsPage sp;
        await Navigation.PushAsync(sp = new SettingsPage(_foldersList, _theme));
        bool isPageClosed = await sp.WaitForPageClosedAsync();
        if (isPageClosed)
        {
            var tmp = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GNOM");
            var appSettingsPath = Path.Combine(tmp, "appSettings.json");
            string json = System.IO.File.ReadAllText(appSettingsPath);
            dynamic jsonObj = JsonConvert.DeserializeObject(json);
            var newTheme = jsonObj["Theme"];

            if (!JsonConvert.Equals(preChange, jsonObj))
            {
                LoadColors();
                LoadThemedButtons();
            }

        }

        // no generalnie to nie dziala bo async (znalezc jakas metode ktoa jest callowana gdy sie wychodzi z settingsow?)


    }

    private void OnPlaylistSelected(object sender, SelectedItemChangedEventArgs e)
    {

    }

    /// <summary>
    /// Przechwytuje zdarzenie kliknięcia na element z listy playlist w pasku nawigacyjnym
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    private void MultiplePlaylistView_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item == null)
        {
            return;
        }

        var selectedPlaylist = (MultiplePlaylistViewModel)e.Item;

        if (selectedPlaylist.Path != null)
        {
            trackTimer.Stop();
            player.Pause();

            mainPlaylist.Tracks.Clear();
            mainPlaylist.LoadFromM3U(selectedPlaylist.Path);

            Task.Run(async () => { await LoadToListView(); });

        }
    }


    private void callPopup(object sender, FocusEventArgs e) =>
        _visibility = true;


    private void hidePopup(object sender, FocusEventArgs e) =>
        _visibility = false;


    /// <summary>
    /// Obsługuje zdarzenie kliknięcia zapisania playlisty
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    private async void SaveListBtn_Clicked(object sender, EventArgs e)
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


    private async void EqBtn_Clicked(object sender, EventArgs e)
    {
        await Dispatcher.DispatchAsync(() => {
            var popup = new EqPopup(_theme, player.Bands);

            popup.EqualizerSettingsSaved += OnEqualizerSettingsSaved;
            popup.SingleBandChanged += Popup_SingleBandChanged;
            this.ShowPopup(popup);

        });

    }

    private void Popup_SingleBandChanged(object sender, KeyValuePair<int, float> e)
    {
        player.ApplySingleBand(e);
    }

    private void OnEqualizerSettingsSaved(object sender, EqualizerBand[] settings)
    {
        player.ApplyEqualizerSettings(settings);
        _equalizerList[0] = settings;
        SaveToJson();
    }

    /// <summary>
    /// Obsługuje wciśnięcie przycisku powrotu
    /// Wczytuje pliki audio z dodanych katalogów do głównej playlisty 
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    private void PlaylistReturnBtn_Clicked(object sender, TappedEventArgs e)
    {
        LoadFromDirectory();

    }
    /// <summary>
    /// Obsługuje zdarzenie zapisania playlisty
    /// Zapisuje ją w folderze systemowym MyMusic
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="playlistName">Argumenty zdarzenia.</param>
    private async void OnPlaylistSaved(object? sender, string playlistName)
    {
        await Dispatcher.DispatchAsync(async () => {

            if (playlistName != null || playlistName == "")
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
            else
                await Toast.Make("Playlist name cannot be blank", ToastDuration.Short, 14).Show();
        });
    }

    /// <summary>
    /// Asynchronicznie wczytuje utwory z głównej listy odtwarzania do kontrolki playlistView.
    /// </summary>
    /// <returns>Task</returns>
    private async Task LoadToListView()
    {
        await Task.Delay(500);
        await Dispatcher.DispatchAsync(async () => {

            playlistView.ItemsSource = null;
            await MarkFavoriteSongsInMainPlaylist();

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
                    Favourite = track.GetFavourite() ? _favImgTheme[1] : _favImgTheme[0],
                    SecondaryColor = Color.FromArgb(_theme.SecondaryColor)
                };

                trackViewModels.Add(trackViewModel);

            }

            playlistView.ItemsSource = trackViewModels;

            playlistListView.ItemsSource = MultiplePlaylistViewModel.CreatePlaylistViewModel(_playlists, Color.FromArgb(_theme.SecondaryColor));
        });


    }

    private void LoadColors()
    {
        if (_theme.Gradient) { PrimaryColor = _theme.GetGradient(); } else { PrimaryColor = new SolidColorBrush(Color.FromArgb(_theme.PrimaryColor)); }
        SecondaryColor = Color.FromArgb(_theme.SecondaryColor);
    }

    private async void LoadThemedButtons()
    {
        List<string> buttons = _theme.GetButtons();
        BackwardSolid = buttons[0];
        Backward = buttons[1];
        PlaySolid = buttons[2];
        PauseSolid = buttons[3];
        Forward = buttons[4];
        ForwardSolid = buttons[5];
        PlusSolid = buttons[6];
        ListSolid = buttons[7];
        DownloadSolid = buttons[8];
        ReplaySolid = buttons[9];
        ShuffleSolid = buttons[10];
        HomeSolid = buttons[11];
        PlaylistReturnSolid = buttons[12];
        EqSolid = buttons[13];

        _favImgTheme = new();
        if (_theme.DarkButtons)
        {
            _favImgTheme.Add("favorite0solid.png");
            _favImgTheme.Add("favorite1solid.png");
        }
        else
        {
            _favImgTheme.Add("favorite0whitesolid.png");
            _favImgTheme.Add("favorite1whitesolid.png");
        }
        
        if(playlistView.ItemsSource == null) { return; }
        await LoadToListView();
    }


    private Brush _primaryColor;
    public Brush PrimaryColor
    {
        get => _primaryColor;
        set
        {
            if (_primaryColor == value) { return; }
            _primaryColor = value;
            OnPropertyChanged(nameof(PrimaryColor));
        }
    }
    private Color _secondaryColor;
    public Color SecondaryColor
    {
        get => _secondaryColor;
        set
        {
            if (_secondaryColor == value) { return; }
            _secondaryColor = value;
            OnPropertyChanged(nameof(SecondaryColor));
        }
    }

    private string _backwardSolid;
    public string BackwardSolid
    {
        get => _backwardSolid;
        set
        {
            if (_backwardSolid == value) { return; }
            _backwardSolid = value;
            OnPropertyChanged(nameof(BackwardSolid));
        }
    }

    private string _backward;
    public string Backward
    {
        get => _backward;
        set
        {
            if (_backward == value) { return; }
            _backward= value;
            OnPropertyChanged(nameof(Backward));
        }
    }

    private string _playSolid;
    public string PlaySolid
    {
        get => _playSolid;
        set
        {
            if (_playSolid == value) { return; }
            _playSolid = value;
            OnPropertyChanged(nameof(PlaySolid));
        }
    }

    private string _pauseSolid;
    public string PauseSolid
    {
        get => _pauseSolid;
        set
        {
            if (_pauseSolid == value) { return; }
            _pauseSolid = value;
            OnPropertyChanged(nameof(PauseSolid));
        }
    }

    private string _forward;
    public string Forward
    {
        get => _forward;
        set
        {
            if (_forward == value) { return; }
            _forward = value;
            OnPropertyChanged(nameof(Forward));
        }
    }

    private string _forwardSolid;
    public string ForwardSolid
    {
        get => _forwardSolid;
        set
        {
            if (_forwardSolid == value) { return; }
            _forwardSolid = value;
            OnPropertyChanged(nameof(ForwardSolid));
        }
    }

    private string _plusSolid;
    public string PlusSolid
    {
        get => _plusSolid;
        set
        {
            if (_plusSolid == value) { return; }
            _plusSolid = value;
            OnPropertyChanged(nameof(PlusSolid));
        }
    }

    private string _listSolid;
    public string ListSolid
    {
        get => _listSolid;
        set
        {
            if (_listSolid == value) { return; }
            _listSolid = value;
            OnPropertyChanged(nameof(ListSolid));
        }
    }

    private string _downloadSolid;
    public string DownloadSolid
    {
        get => _downloadSolid;
        set
        {
            if (_downloadSolid == value) { return; }
            _downloadSolid = value;
            OnPropertyChanged(nameof(DownloadSolid));
        }
    }

    private string _replaySolid;
    public string ReplaySolid
    {
        get => _replaySolid;
        set
        {
            if (_replaySolid == value) { return; }
            _replaySolid = value;
            OnPropertyChanged(nameof(ReplaySolid));
        }
    }

    private string _shuffleSolid;
    public string ShuffleSolid
    {
        get => _shuffleSolid;
        set
        {
            if (_shuffleSolid == value) { return; }
            _shuffleSolid = value;
            OnPropertyChanged(nameof(ShuffleSolid));
        }
    }

    private string _eqSolid;
    public string EqSolid
    {
        get => _eqSolid;
        set
        {
            if (_eqSolid == value) { return; }
            _eqSolid = value;
            OnPropertyChanged(nameof(EqSolid));
        }
    }

    private string _homeSolid;
    public string HomeSolid
    {
        get => _homeSolid;
        set
        {
            if (_homeSolid == value) { return; }
            _homeSolid = value;
            OnPropertyChanged(nameof(HomeSolid));
        }
    }

    private string _playlistReturnSolid;
    public string PlaylistReturnSolid
    {
        get => _playlistReturnSolid;
        set
        {
            if (_playlistReturnSolid == value) { return; }
            _playlistReturnSolid = value;
            OnPropertyChanged(nameof(PlaylistReturnSolid));
        }
    }




    /// <summary>
    /// Obsługuje kliknięcie przycisku "Nowa playlista".
    /// Przechodzi do strony PlaylistCreationPage, przekazując listę folderów i listę playlist jako parametry.
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    private async void NewPlaylist_Clicked(object sender, EventArgs e)
    {

        await Navigation.PushAsync(new PlaylistCreationPage(_foldersList, _playlists));
    }



}



/// <summary>
/// ViewModel dla utworu na liście odtwarzania.
/// </summary>
public class PlaylistViewModel : BindableObject
{
    public string Title { get; set; }
    public string Duration { get; set; }
    public string Album { get; set; }
    public string Artist { get; set; }
    public string Path { get; set; }

    public string TitleAndArtist { get; set; }


    public Color SecondaryColor { get; set; }

    public bool IsSelected { get; set; }
    public string TrackInfo { get; set; }

    private Color _bgColor;
    public Color BgColor
    {
        get => _bgColor;
        set
        {
            _bgColor = value;
            OnPropertyChanged(nameof(BgColor));
        }
    }


    private string _favourite;
    public string Favourite
    {
        get => _favourite;
        set
        {
            _favourite = value;
            OnPropertyChanged(nameof(Favourite));
        }
    }
    /// <summary>
    /// Tworzy listę obiektów PlaylistViewModel na podstawie listy utworów AudioTrack z playlisty.
    /// </summary>
    /// <param name="playlist">Playlista, dla której tworzone są obiekty PlaylistViewModel.</param>
    /// <returns>Lista obiektów PlaylistViewModel.</returns>

    internal static List<PlaylistViewModel> CreatePlaylistViewModel(AudioPlaylist playlist)
    {
        var playlistViewModels = new List<PlaylistViewModel>();

        foreach (var track in playlist.Tracks)
        {
            var trackViewModel = new PlaylistViewModel
            {
                Title = track.GetTitle(),
                Album = track.GetAlbum(),
                Artist = track.GetArtist(),
                Path = track.GetFilePath(),

                IsSelected = false,
                BgColor = null

            };

            playlistViewModels.Add(trackViewModel);
        }

        return playlistViewModels;
    }

   
}

public class MultiplePlaylistViewModel : BindableObject
{
    
    public string Path { get; set; }

    public string Name { get; set; }

    public Color SecondaryColor { get; set; }

    internal static List<MultiplePlaylistViewModel> CreatePlaylistViewModel(List<AudioPlaylist> playlists, Color color)
    {
        var playlistViewModels = new List<MultiplePlaylistViewModel>();

        foreach (var playlist in playlists)
        {
            var trackViewModel = new MultiplePlaylistViewModel
            {
                Name = playlist.Name,
                Path = playlist.Path,
                SecondaryColor =color
            };

            playlistViewModels.Add(trackViewModel);
        }

        return playlistViewModels;
    }


}






