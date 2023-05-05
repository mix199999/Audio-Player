using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace testMAUI;
public class StringListMessage
{
    public List<string> Strings { get; set; }

    public StringListMessage(List<string> strings)
    {
        Strings = strings;
    }
}

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
    /// <summary>
    /// Wywoływana po zamknięciu strony
    /// przesyła listę folderów
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    private void SettingsPage_Disappearing(object sender, EventArgs e)
    {
        if(countStart == countEnd) { _foldersList = null; }             
        StringListMessage message = new StringListMessage(_foldersList);
        WeakReferenceMessenger.Default.Send(message);


    }

    public async void mainButtonClicked(object sender, EventArgs e)
	{

		await Navigation.PopAsync();

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

    public void showPathOptions(object sender, EventArgs e)
    {
        pathOptions.IsVisible = true;
		secondOption.IsVisible = false;
		thirdOption.IsVisible = false;	
    }
    public void showInstruction(object sender, EventArgs e)
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
    public void showThirdOption(object sender, EventArgs e)
    {
        pathOptions.IsVisible = false;
        secondOption.IsVisible = false;
        thirdOption.IsVisible = true;
    }

    /// <summary>
    /// Służy do wybrania i dodania katalogu(jeśli jest to możliwe)
    /// </summary>
    /// <param name="cancellationToken">token anulowania</param>
    /// <returns></returns>
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
    /// <summary>
    /// Obsługuje zdarzenie kliknecia przycisku "-"
    /// usuwa ścieżkę do katalogu z listy
    /// </summary>
    /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
    /// <param name="e">Argumenty zdarzenia.</param>
    private void RemoveFolderBt_Tapped(object sender, TappedEventArgs e)
    {
       
        var selectedItem = pathListView.SelectedItem;
        if (selectedItem != null) 
        {
            _foldersList.RemoveAt(_indexPath);
            LoadDataToPathView();

        }
    }
    /// <summary>
    /// Wczytuje katalogi do ListView
    /// </summary>
    private void LoadDataToPathView()
    {
        pathListView.ItemsSource = _foldersList.Select(directory => new
        {Path = directory.ToString() }) ;
    }
}