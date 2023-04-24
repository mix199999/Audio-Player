using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;

namespace testMAUI;

public partial class SettingsPage : ContentPage
{

    private CancellationToken cancellationToken = new CancellationToken();
    private int _indexPath=-1;
    private int countStart;
    private int countEnd;
    List<string> _foldersList = new List<string>();

    internal SettingsPage(List<string> folderlist)
	{

        InitializeComponent();
        this.Disappearing += SettingsPage_Disappearing;
        this._foldersList = folderlist;
        countStart = _foldersList.Count;
        countEnd = countStart;
        LoadDataToPathView();
        pathListView.ItemTapped += PathListView_ItemTapped;



    }

    private void PathListView_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        _indexPath = e.ItemIndex;
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

    public async void mainButtonClicked(object sender, EventArgs e)
	{

		await Navigation.PopAsync();

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
                LoadDataToPathView();

               countEnd++;
            }
        }
        else
        {
            await Toast.Make($"The folder was not picked with error: {result.Exception.Message}").Show(cancellationToken);
        }
    }

    private void RemoveFolderBt_Tapped(object sender, TappedEventArgs e)
    {
       
        var selectedItem = pathListView.SelectedItem;
        if (selectedItem != null) 
        {
            _foldersList.RemoveAt(_indexPath);
            LoadDataToPathView();

        }
    }

    private void LoadDataToPathView()
    {
        pathListView.ItemsSource = _foldersList.Select(directory => new
        {Path = directory.ToString() }) ;
    }
}