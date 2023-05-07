

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

    internal AudioListMessage(List<AudioPlaylist> playlist)
    {
        Playlist = playlist; 
    }
}
public partial class PlaylistCreationPage : ContentPage
{
	private List<string> _folders = new List<string>();
   // List<PlaylistViewModel> trackViewModels = new List<PlaylistViewModel>();
    private AudioPlaylist mainPlaylist = new AudioPlaylist();
    private List<PlaylistViewModel> _trackViewModels;
    private List<AudioPlaylist> _playlists = new List<AudioPlaylist>();



    public IList<string> _selectedTracks { get; set; } = new List<string>();
    internal PlaylistCreationPage(List<string> Folders, List<AudioPlaylist> AudioPlaylists)
	{

		_folders = Folders;
        _playlists = AudioPlaylists;
        LoadFromDirectory();
        InitializeComponent();
        this.Disappearing += CreationPage_Disappearing;
        Task.Run(async () => { await LoadToListView(); });
        playlistView.ItemTapped += PlaylistView_ItemTapped;


    }
    /// <summary>
    /// Przesyła dane tj lista folderów i lista Playlist po zniknięciu strony
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    private void CreationPage_Disappearing(object sender, EventArgs e)
    {
        StringListMessage folderMessage = new StringListMessage(_folders);
        WeakReferenceMessenger.Default.Send(folderMessage);

        AudioListMessage playlistMessage = new AudioListMessage(_playlists);
        WeakReferenceMessenger.Default.Send(playlistMessage);

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
            item.BgColor = Color.FromArgb("3f48cc");
        }

    }
    /// <summary>
    /// przechodzi na stronę z ustawieniami 
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    private async void settingsButtonClicked(object sender, EventArgs e)
	{

        
    }

	private void NewPlaylist_Clicked(object sender, EventArgs e)
	{
    
    }


    /// <summary>
    /// Powraca do strony głównej programu
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    public async void mainButtonClicked(object sender, EventArgs e)
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
    /// Zapisuje do playlistę do pliku M3U 
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


                using var stream = new MemoryStream(Encoding.Default.GetBytes(newPlaylist.SaveToM3U()));
                await using var fileStream = System.IO.File.Create(fullPath);
                await stream.CopyToAsync(fileStream);
                _playlists.Add(newPlaylist);
                SaveToJson();


            }
            else
                await Toast.Make("Playlist name cannot be blank", ToastDuration.Short, 14).Show();
        });
    }




    private void SaveToJson()
    {
      
        var appSettingsPath = System.IO.Path.Combine(System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GNOM"), "appSettings.json");

        var foldersSettings = new Configuration
        {
            FolderList = _folders,
            AudioPlaylists = _playlists
        };

        var json = JsonConvert.SerializeObject(foldersSettings, Newtonsoft.Json.Formatting.Indented);

        System.IO.File.WriteAllText(appSettingsPath, json);
      
    }


}