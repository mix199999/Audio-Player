using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;

namespace testMAUI;

public partial class SettingsPage : ContentPage
{

    private CancellationToken cancellationToken = new CancellationToken();

    private int countStart;
    private int countEnd;
    List<string> _foldersList = new List<string>();
	public SettingsPage(List<string> folderlist)
	{
        
        

        InitializeComponent();
        this.Disappearing += SettingsPage_Disappearing;
        this._foldersList = folderlist;
        countStart = _foldersList.Count;
        countEnd = countStart;
        pathListView.ItemsSource = _foldersList.Select(directory => new
        {
            Path = directory.ToString()
        });

    }


    private async void Button_Clicked(object sender, TappedEventArgs e)
    {
        await PickFolder(cancellationToken);
    }

    private void SettingsPage_Disappearing(object sender, EventArgs e)
    {
        if(countStart == countEnd) { _foldersList = null; }        
        MessagingCenter.Send(this, "FoldersList", _foldersList);

    }

    public void mainButtonClicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();

	}

    public void showPathOptions(object sender, EventArgs e)
    {
        pathOptions.IsVisible = true;
		secondOption.IsVisible = false;
		thirdOption.IsVisible = false;	
    }
    public void showSecondOption(object sender, EventArgs e)
    {
        pathOptions.IsVisible = false;
        secondOption.IsVisible = true;
        thirdOption.IsVisible = false;
    }
    public void showThirdOption(object sender, EventArgs e)
    {
        pathOptions.IsVisible = false;
        secondOption.IsVisible = false;
        thirdOption.IsVisible = true;
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
                pathListView.ItemsSource = _foldersList.Select(directory => new
                {
                     Path = directory.ToString()
                }) ;

               countEnd++;
            }
        }
        else
        {
            await Toast.Make($"The folder was not picked with error: {result.Exception.Message}").Show(cancellationToken);
        }
    }








}