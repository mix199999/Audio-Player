
using CommunityToolkit.Maui.Views;
using testMAUI;
using NAudio.Extras;
namespace GNOM;

public partial class EqPopup : Popup
{
    public event EventHandler<EqualizerBand[]> EqualizerSettingsSaved;
    private Brush _primaryColor;
    private EqualizerBand[] CurrentEqualizerSettings;
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

    private Theme _theme;
	internal EqPopup(Theme theme)
	{
     InitializeComponent();
		_theme = theme;
        LoadColors();
        BindingContext = this;
        this.SecondaryColor = Color.FromArgb(theme.SecondaryColor);

    }


    private void LoadColors()
    {
        if (_theme.Gradient) { PrimaryColor = _theme.GetGradient(); } else { PrimaryColor = new SolidColorBrush(Color.FromArgb(_theme.PrimaryColor)); }
        SecondaryColor = Color.FromArgb(_theme.SecondaryColor);
    }

    void SaveEqualizerButton_Clicked(object sender, EventArgs e)
	{
        CurrentEqualizerSettings = new EqualizerBand[]
         {
            new EqualizerBand{Frequency=32, Bandwidth= 0.8f, Gain = (float)Slider1.Value},
            new EqualizerBand { Frequency = 64, Bandwidth = 0.8f, Gain = (float)Slider2.Value },
            new EqualizerBand { Frequency = 125, Bandwidth = 0.8f, Gain = (float)Slider3.Value },
            new EqualizerBand { Frequency = 250, Bandwidth = 0.8f, Gain = (float)Slider4.Value },
            new EqualizerBand { Frequency = 500, Bandwidth = 0.8f, Gain = (float)Slider5.Value },
            new EqualizerBand { Frequency = 1000, Bandwidth = 0.8f, Gain = (float)Slider6.Value },
            new EqualizerBand { Frequency = 2000, Bandwidth = 0.8f, Gain = (float)Slider7.Value },
            new EqualizerBand { Frequency = 4000, Bandwidth = 0.8f, Gain = (float)Slider8.Value },
            new EqualizerBand { Frequency = 8000, Bandwidth = 0.8f, Gain = (float)Slider9.Value },
            new EqualizerBand { Frequency = 16000, Bandwidth = 0.8f, Gain = (float)Slider10.Value }

         };
        OnEqualizerSettingsSaved(CurrentEqualizerSettings);
        OnEqualizerSettingsSaved(CurrentEqualizerSettings);
    }
    internal virtual void OnEqualizerSettingsSaved(EqualizerBand[] settings)
    {
        EqualizerSettingsSaved?.Invoke(this, settings);
    }


}

