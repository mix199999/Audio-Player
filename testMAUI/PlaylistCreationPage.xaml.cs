using Bertuzzi.MAUI.MultiSelectListView;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
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
    private MultiSelectObservableCollection<PlaylistViewModel> _trackViewModels;
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
       // playlistView.ItemTapped += PlaylistView_ItemTapped;


    }

    private void CreationPage_Disappearing(object sender, EventArgs e)
    {
        StringListMessage folderMessage = new StringListMessage(_folders);
        WeakReferenceMessenger.Default.Send(folderMessage);

        AudioListMessage playlistMessage = new AudioListMessage(_playlists);
        WeakReferenceMessenger.Default.Send(playlistMessage);



    }

    private void PlaylistView_ItemTapped(object sender, ItemTappedEventArgs e)
    {

        if (e.Item == null) return;

        var item = (PlaylistViewModel)e.Item;


        if(_selectedTracks.Contains(item.Path))
        {
            _selectedTracks.Remove(item.Path);
        }
        else if(!_selectedTracks.Contains(item.Path))
        {
            _selectedTracks.Add(item.Path);
        }

    }

    private void settingsButtonClicked(object sender, EventArgs e)
	{

	}

	private void NewPlaylist_Clicked(object sender, EventArgs e)
	{ }

	private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
	{

      try
        {
            var item = _trackViewModels[e.SelectedItemIndex];

            if (_selectedTracks.Contains(item.Data.Path))
            {
                _selectedTracks.Remove(item.Data.Path);
            }
            else if (!_selectedTracks.Contains(item.Data.Path))
            {
                _selectedTracks.Add(item.Data.Path);
            }
        }
        catch(Exception ) 
        { }




    }

    public async void mainButtonClicked(object sender, EventArgs e)
    {

        await Navigation.PopAsync();

    }


    private async Task LoadToListView()
    {
        await Task.Delay(500);
        await Dispatcher.DispatchAsync(async () => {

            playlistView.ItemsSource = null;


            _trackViewModels = new MultiSelectObservableCollection<PlaylistViewModel>();
            foreach (var track in mainPlaylist.Tracks)
            {
                var trackViewModel = new PlaylistViewModel
                {
                    Title = track.GetTitle(),
                    Duration = track.GetDuration().ToString("mm\\:ss"),
                    Album = track.GetAlbum(),
                    Artist = track.GetArtist(),
                    Path = track.GetFilePath(),
                    TrackInfo = $"{track.GetTitle()} - {track.GetArtist()}  [{track.GetAlbum()}]",


                };

                _trackViewModels.Add(trackViewModel);

            }

            playlistView.ItemsSource = _trackViewModels;

        });


    }

    private void LoadFromDirectory()
    {
        mainPlaylist.Tracks.Clear();
        foreach (var Folder in _folders)
        {
            mainPlaylist.LoadFromDirectory(Folder);

        }
        Task.Run(async () => { await LoadToListView(); });
    }


    private async void SavePlaylist_Clicked(object sender, EventArgs e) 
    {
        var test = _selectedTracks;

        await Dispatcher.DispatchAsync(() => {
            var popup = new PopupTrackInfo();
            popup.PlaylistSaved += OnPlaylistSaved;
            this.ShowPopup(popup);
        });


    }


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

    public ICommand SaveSelectedItem => new Command<PlaylistViewModel>(filePath =>
    {
        if (_selectedTracks.Contains(filePath.Path))
        {
            _selectedTracks.Remove(filePath.Path);
        }
        else if (!_selectedTracks.Contains(filePath.Path))
        {
            _selectedTracks.Add(filePath.Path);
        }
    });


    private void SaveToJson()
    {
        var appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appSettings.json");


        var foldersSettings = new Configuration
        {
            FolderList = _folders,
            AudioPlaylists = _playlists
        };

        var json = JsonConvert.SerializeObject(foldersSettings, Newtonsoft.Json.Formatting.Indented);

        System.IO.File.WriteAllText(appSettingsPath, json);
      
    }
}