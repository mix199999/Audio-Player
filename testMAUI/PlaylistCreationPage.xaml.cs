

using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;
using System.Windows.Input;

namespace testMAUI;

internal class AudioListMessage
{
    public List<AudioPlaylist> Playlist { get; set; }
    public string Name { get; set; }
    internal AudioListMessage(List<AudioPlaylist> playlist, string name)
    {
        Playlist = playlist;
        Name = name;

    }
}




public partial class PlaylistCreationPage : ContentPage
{
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




    private List<string> _folders = new List<string>(); //!< Lista ścieżek do folderó z pliku konfiguracyjnego
    // List<PlaylistViewModel> trackViewModels = new List<PlaylistViewModel>();
    private AudioPlaylist mainPlaylist = new AudioPlaylist(); //!< Lista wszystkich piosenek
    private List<PlaylistViewModel> _trackViewModels;
    private List<AudioPlaylist> _playlists = new List<AudioPlaylist>();
    private Theme _theme;

    public IList<string> _selectedTracks { get; set; } = new List<string>();
    internal PlaylistCreationPage(List<string> Folders, List<AudioPlaylist> AudioPlaylists, Theme theme)
	{

		_folders = Folders;
        _playlists = AudioPlaylists;
        _theme = theme;
        LoadFromDirectory();
        InitializeComponent();
        this.Disappearing += CreationPage_Disappearing;
        Task.Run(async () => { await LoadToListView(); });
        playlistView.ItemTapped += PlaylistView_ItemTapped;
       
        this.SecondaryColor = Color.FromArgb(theme.SecondaryColor);
        this.PrimaryColor = Color.FromArgb(theme.PrimaryColor);
        LoadColors();

        BindingContext = this;
    }

    /// <summary>
    /// Przesyła dane: lista folderów i lista Playlist po zniknięciu strony
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    private void CreationPage_Disappearing(object sender, EventArgs e)
    {
       

    }

    /// <summary>
    /// Przechwytuje zdarzenie kliknięcia na element z ListView
    /// Zależnie od tego czy Path ("Ścieżka") znajduje się (lub nie)  na liście _selectedTracks
    /// dodaje bądź ją usuwa 
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    private void PlaylistView_ItemTapped(object sender, ItemTappedEventArgs e)
    {

        if (e.Item == null)
        {
            return;
        }
        var item = (PlaylistViewModel)e.Item;
        if (_selectedTracks.Contains(item.Path))
        {
            _selectedTracks.Remove(item.Path);
            item.BgColor = null;
        }
        else if (!_selectedTracks.Contains(item.Path))
        {
            _selectedTracks.Add(item.Path);
            item.BgColor = Color.FromArgb(_theme.PrimaryColor);
        }

    }

    /// <summary>
    /// przechodzi na stronę z ustawieniami 
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
  

    /// <summary>
    /// Powraca do strony głównej programu
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    public async void MainButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    /// <summary>
    /// Ładuje utwory do ListView używając modelu PlaylistViewModel
    /// </summary>
    /// <returns>Task</returns>
    private async Task LoadToListView()
    {
        await Task.Delay(500);
   

        playlistView.ItemsSource = null;
        Dispatcher.Dispatch(async () => 
        {
            _trackViewModels = new List<PlaylistViewModel>();
            foreach (var track in mainPlaylist.Tracks)
            {
                var trackViewModel = new PlaylistViewModel
                {
                    Title = track.GetTitle(),
                    Duration = track.GetDuration().ToString("mm\\:ss"),
                    Album = track.GetAlbum(),
                    Artist = track.GetArtist(),
                    Path = track.GetFilePath()

                };

                _trackViewModels.Add(trackViewModel);

            }

            playlistView.ItemsSource = _trackViewModels;
        });

    }

    /// <summary>
    /// wczytuje pliki muzyczne z katalogu i ładuje je do ListView
    /// </summary>
    private void LoadFromDirectory()
    {
        mainPlaylist.Tracks.Clear();
        foreach (var Folder in _folders)
        {
            mainPlaylist.LoadFromDirectory(Folder);

        }
        Task.Run(async () => { await LoadToListView(); });
    }

    /// <summary>
    /// Wywołuje popup-a w którym podaje się nazwę playlisty do zapisania 
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    private async void SavePlaylist_Clicked(object sender, EventArgs e) 
    {
        await Dispatcher.DispatchAsync(() => {
            var popup = new PopupTrackInfo();
            popup.PlaylistSaved += OnPlaylistSaved;
            this.ShowPopup(popup);
        });

    }

    /// <summary>
    /// Zapisuje playlistę do pliku M3U 
    /// </summary>
    /// <param name="sender">obiekt wywołujący zdarzenie</param>
    /// <param name="playlistName">nazwa playlisty</param>
    private async void OnPlaylistSaved(object? sender, string playlistName)
    {

        await Dispatcher.DispatchAsync(async () => {

            if (playlistName != null || playlistName == "")
            {
                string Name = playlistName;
                string Path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

                var fullPath = Path + "\\" + playlistName + ".M3U";
             

                var newPlaylist = new AudioPlaylist()
                {
                    Name = playlistName,
                    Path = fullPath
                };
                foreach (string trackPath in _selectedTracks)
                {

                    newPlaylist.AddTrack(new AudioFile(trackPath));
                }
               
                _playlists.Add(newPlaylist);

                AudioListMessage playlistMessage = new AudioListMessage(_playlists, playlistName);
                WeakReferenceMessenger.Default.Send(playlistMessage);

             

            }
            else
                await Toast.Make("Playlist name cannot be blank", ToastDuration.Short, 14).Show();
        });
    }

    /// <summary>
    /// Zapisuje playlistę do pliku konfiguracyjnego appSettings.json
    /// </summary>
    



    private void LoadColors()
    {
        HomeSolid = _theme.GetButtons()[11];
        if (_theme.Gradient) { PrimaryColor = _theme.GetGradient(); } else { PrimaryColor = new SolidColorBrush(Color.FromArgb(_theme.PrimaryColor)); }
        SecondaryColor = Color.FromArgb(_theme.SecondaryColor);

    }

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

}