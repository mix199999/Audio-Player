using CommunityToolkit.Maui.Views;

namespace testMAUI;

public partial class PopupTrackInfo : Popup
{
    public event EventHandler<string>? PlaylistSaved;

    public PopupTrackInfo()
	{
		InitializeComponent();
		
	}



    private void SavePlaylistButton_Clicked(object sender, EventArgs e)
    {
        string playlistName = PlaylistNameEntry.Text;
        PlaylistSaved?.Invoke(this, playlistName);
        this.Close();
    }

}