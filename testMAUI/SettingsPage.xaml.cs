using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;

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

    

        private Color _frameBackground;
    public Color FrameBackground
    {
        get => _frameBackground;
        set
        {
            if (_frameBackground == value) { return; }
            _frameBackground = value;
            OnPropertyChanged(nameof(FrameBackground));
        }
    }
    private Color _inputBackground;
    public Color InputBackground
    {
        get => _inputBackground;
        set
        {
            if (_inputBackground == value) { return; }
            _inputBackground = value;
            OnPropertyChanged(nameof(InputBackground));
        }
    }
    private Color _inputColor;
    public Color InputColor
    {
        get => _inputColor;
        set
        {
            if (_inputColor == value) { return; }
            _inputColor = value;
            OnPropertyChanged(nameof(InputColor));
        }
    }


   
    private CancellationToken cancellationToken = new CancellationToken();
    private int _indexPath = -1;
    private int countStart;
    private int countEnd;
    List<string> _foldersList = new List<string>();
    Theme grad;
    List<string> buttons = new();

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

    private string _minus;
    public string Minus
    {
        get => _minus;
        set
        {
            if (_minus == value) { return; }
            _minus = value;
            OnPropertyChanged(nameof(Minus));
        }
    }

    private string _path;
    public string FolderPath
    {
        get => _path;
        set
        {
            if (_path == value) { return; }
            _path = value;
            OnPropertyChanged(nameof(Path));
        }
    }

    internal SettingsPage(List<string> folderlist, Theme theme)
    {

        this._foldersList = folderlist;
       
        InitializeComponent();
      
        this.Disappearing += SettingsPage_Disappearing;
       
        countStart = _foldersList.Count;
        countEnd = countStart;
   
        pathListView.ItemTapped += PathListView_ItemTapped;
        grad = theme;
        GradientEntry.IsEnabled = grad.Gradient;
        GradientOptions.IsVisible = grad.Gradient;

        PrimaryColorEntry.Completed += OnPrimaryColorEntryCompleted;
        GradientEntry.Completed += OnGradientEntryCompleted;
        SecondaryColorEntry.Completed += OnSecondaryColorEntryCompleted;
        GradientCheck.CheckedChanged += OnGradientCheckCheckedChanged;
        GradientFlipCheckBox.CheckedChanged += OnGradientFlipCheckedChanged;
        GradientHtoVCheckBox.CheckedChanged += OnGradientHtoVCheckedChanged;
        DarkButtonsCheckBox.CheckedChanged += OnDarkButtonsCheckedChanged;
        SaveButton.Clicked += OnSaveButtonClicked;

        GradientCheck.IsChecked = grad.Gradient ? true : false;
        GradientFlipCheckBox.IsChecked = grad.Flip ? true : false;
        GradientHtoVCheckBox.IsChecked = grad.HtoV ? true : false;
        DarkButtonsCheckBox.IsChecked = grad.DarkButtons ? true : false;
        PrimaryColorEntry.Text = grad.PrimaryColor;
        SecondaryColorEntry.Text = grad.SecondaryColor;
        GradientEntry.Text = grad.GradientColor;
        buttons = grad.GetButtons();

        BackwardSolid = buttons[0];
        PlaySolid = buttons[2];
        ForwardSolid = buttons[5];
        PlusSolid = buttons[6];
        Minus = buttons[14];
        if (grad.Gradient) { PrimaryColor = grad.GetGradient(); }
        else PrimaryColor = new SolidColorBrush(Color.FromArgb(grad.PrimaryColor));
        SecondaryColor = Color.FromArgb(grad.SecondaryColor);
        BindingContext = this;
        LoadDataToPathView();

        InputBackground = Color.FromArgb("ffffff");
        InputColor = Color.FromArgb("000000");
        FrameBackground = Color.FromArgb(grad.GradientColor);
    }

    private void OnGradientCheckCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        // aktualizacja gradientu przy checkchanged, tak jak flip i HtoV
        // usuniecie parametrow z GetGradient (bierze wszystko z obieku, samego siebie?)
        grad.Gradient = e.Value;
        if (e.Value)
        {
            GradientEntry.IsEnabled = true;
            GradientOptions.IsVisible = true;
            PrimaryColor = grad.GetGradient();
        }
        else
        {
            GradientEntry.IsEnabled = false;
            GradientOptions.IsVisible = false;
            PrimaryColor = new SolidColorBrush(Color.FromArgb(grad.PrimaryColor));
        }
    }

    private void OnPrimaryColorEntryCompleted(object sender, EventArgs e)
    {
        Entry entry = sender as Entry;
        if (entry.Text == null) { return; }
        bool isValidHexColorCode = Regex.IsMatch(entry.Text, "^[0-9A-Fa-f]{6}$");

        if (isValidHexColorCode && entry.Text != grad.SecondaryColor)
        {
            if (PrimaryEntryError.HeightRequest == 20)
            {
                //var animation = new Microsoft.Maui.Controls.Animation(v => PrimaryEntryError.HeightRequest = v, 20, 0, Easing.CubicOut); // sometimes works, sometimes doesnt, cool
                //animation.Commit(PrimaryEntryError, "HideErrorAnimation1", 16, 500, Easing.CubicOut);
                PrimaryEntryError.HeightRequest = 0;
            }
            //PrimaryColor = new SolidColorBrush(Color.FromArgb(entry.Text));
            if (GradientCheck.IsChecked)
            {
                grad.PrimaryColor = entry.Text;
                grad.GradientColor = GradientEntry.Text;
                PrimaryColor = grad.GetGradient();
            }
            else
            {
                grad.PrimaryColor = entry.Text;
                SolidColorBrush scb = new SolidColorBrush(Color.FromArgb(grad.PrimaryColor));
                PrimaryColor = scb;
            }
        }
        else
        {
            if (PrimaryEntryError.HeightRequest == 20) { return; }
            //var animation = new Microsoft.Maui.Controls.Animation(v => PrimaryEntryError.HeightRequest = v, 0, 20, Easing.CubicOut); // sometimes works, sometimes doesnt, cool
            //animation.Commit(PrimaryEntryError, "ShowErrorAnimation1", 16, 500, Easing.CubicOut);
            PrimaryEntryError.HeightRequest = 20;
        }
    }

    private void OnGradientEntryCompleted(object sender, EventArgs e)
    {
        Entry entry = sender as Entry;
        if (entry.Text == null) { return; }
        bool isValidHexColorCode = Regex.IsMatch(entry.Text, "^[0-9A-Fa-f]{6}$");

        if (isValidHexColorCode)
        {
            if (GradientEntryError.HeightRequest == 20)
            {
                //var animation = new Microsoft.Maui.Controls.Animation(v => GradientEntryError.HeightRequest = v, 20, 0, Easing.CubicOut); // sometimes works, sometimes doesnt, cool
                //animation.Commit(GradientEntryError, "HideErrorAnimation3", 16, 500, Easing.CubicOut);
                GradientEntryError.HeightRequest = 0;
            }
            //PrimaryColor = new SolidColorBrush(Color.FromArgb(entry.Text));
            grad.PrimaryColor = PrimaryColorEntry.Text;
            grad.GradientColor = entry.Text;
            PrimaryColor = grad.GetGradient();
        }
        else
        {
            if (GradientEntryError.HeightRequest == 20) { return; }
            //var animation = new Microsoft.Maui.Controls.Animation(v => GradientEntryError.HeightRequest = v, 0, 20, Easing.CubicOut); // sometimes works, sometimes doesnt, cool
            //animation.Commit(GradientEntryError, "ShowErrorAnimation3", 16, 500, Easing.CubicOut);
            GradientEntryError.HeightRequest = 20;
        }
    }

    private void OnGradientFlipCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        grad.Flip = e.Value;
        PrimaryColor = grad.GetGradient();
    }

    private void OnGradientHtoVCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        grad.HtoV = e.Value;
        PrimaryColor = grad.GetGradient();
    }

    private void OnDarkButtonsCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        grad.DarkButtons = e.Value;
        buttons = grad.GetButtons();
        BackwardSolid = buttons[0];
        PlaySolid = buttons[2];
        ForwardSolid = buttons[5];
    }

    private void OnSecondaryColorEntryCompleted(object sender, EventArgs e)
    {
        Entry entry = sender as Entry;
        if (entry.Text == null) { return; }
        bool isValidHexColorCode = Regex.IsMatch(entry.Text, "^[0-9A-Fa-f]{6}$");

        if (isValidHexColorCode && entry.Text != grad.PrimaryColor)
        {
            if (SecondaryEntryError.HeightRequest == 20)
            {
                //var animation = new Microsoft.Maui.Controls.Animation(v => SecondaryEntryError.HeightRequest = v, 20, 0, Easing.CubicOut); // doesnt work at all
                //animation.Commit(SecondaryEntryError, "HideErrorAnimation2", 16, 500, Easing.CubicOut);
                SecondaryEntryError.HeightRequest = 0;
            }
            grad.SecondaryColor = entry.Text;
            SecondaryColor = Color.FromArgb(entry.Text);
        }
        else
        {
            if (SecondaryEntryError.HeightRequest == 20) { return; }
            //var animation = new Microsoft.Maui.Controls.Animation(v => SecondaryEntryError.HeightRequest = v, 0, 20, Easing.CubicOut); // as above, enlighten me how to make it work : - )
            //animation.Commit(SecondaryEntryError, "ShowErrorAnimation2", 16, 500, Easing.CubicOut);
            SecondaryEntryError.HeightRequest = 20;
        }
    }

    private void OnSaveButtonClicked(object sender, EventArgs e) => UpdateJsonTheme();

    private void UpdateJsonTheme()
    {
        var tmp = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GNOM");
        var appSettingsPath = Path.Combine(tmp, "appSettings.json");
        string json = File.ReadAllText(appSettingsPath);
        dynamic jsonObj = JsonConvert.DeserializeObject(json);

        jsonObj["Theme"] = JToken.FromObject(grad);

        var output = JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);

        System.IO.File.WriteAllText(appSettingsPath, output);
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
        if (countStart == countEnd) { _foldersList = null; }
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

    private  void LoadDataToPathView()
    {

      
           pathListView.ItemsSource= FolderlistViewModel.CreatePlaylistViewModel(_foldersList, Color.FromArgb(grad.SecondaryColor));
     


       
    }
}

public static class ContentPageExtensions
{
    public static Task<bool> WaitForPageClosedAsync(this ContentPage page)
    {
        var completionSource = new TaskCompletionSource<bool>();

        page.Disappearing += (sender, e) => completionSource.SetResult(true);

        return completionSource.Task;
    }
}


public class FolderlistViewModel : BindableObject
{



    public Color SecondaryColor { get; set; }

    public string FolderPath { get; set; }
   

    internal static List<FolderlistViewModel> CreatePlaylistViewModel(List<string> paths, Color color)
    {
        var pathsList = new List<FolderlistViewModel>();

        foreach (var path in paths)
        {
            var pathViewModel = new FolderlistViewModel
            {
                FolderPath = path,
                SecondaryColor = color
               
            };

            pathsList.Add(pathViewModel);
        }

        return pathsList;
    }


}