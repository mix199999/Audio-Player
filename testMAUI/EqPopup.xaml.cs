
using CommunityToolkit.Maui.Views;
using testMAUI;
using NAudio.Extras;
namespace GNOM;

public partial class EqPopup : Popup
{
    public event EventHandler<EqualizerBand[]> EqualizerSettingsSaved;
    public event EventHandler<KeyValuePair<int,float>> SingleBandChanged;

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
	internal EqPopup(Theme theme, EqualizerBand[] equalizerBands)
	{

        
        InitializeComponent();
		_theme = theme;
        CurrentEqualizerSettings = equalizerBands;
        AssignCurrentEqualizerSettingsToSliders();
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
   
    }


    private void AssignCurrentEqualizerSettingsToSliders()
    {
        Slider1.Value = CurrentEqualizerSettings[0].Gain;
        Slider2.Value = CurrentEqualizerSettings[1].Gain;
        Slider3.Value = CurrentEqualizerSettings[2].Gain;
        Slider4.Value = CurrentEqualizerSettings[3].Gain;
        Slider5.Value = CurrentEqualizerSettings[4].Gain;
        Slider6.Value = CurrentEqualizerSettings[5].Gain;
        Slider7.Value = CurrentEqualizerSettings[6].Gain;
        Slider8.Value = CurrentEqualizerSettings[7].Gain;
        Slider9.Value = CurrentEqualizerSettings[8].Gain;
        Slider10.Value = CurrentEqualizerSettings[9].Gain;
    }
    internal virtual void OnEqualizerSettingsSaved(EqualizerBand[] settings)
    {
        EqualizerSettingsSaved?.Invoke(this, settings);
    }

    private void HoverBegan(object sender, PointerEventArgs e)
    {
        if (sender is Button button)
        {
            button.BackgroundColor = Color.FromRgba(0, 0, 0, 20);
        }
    }

    private void HoverEnded(object sender, PointerEventArgs e)
    {
        if (sender is Button button)
        {
            button.BackgroundColor = Color.FromRgba(0, 0, 0, 0);
        }
    }

    private void Slider1_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        OnSliderValueChanged(0, (float)e.NewValue);
    }

    private void Slider2_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        OnSliderValueChanged(1, (float)e.NewValue);
    }

    private void Slider3_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        OnSliderValueChanged(2, (float)e.NewValue);
    }

    private void Slider4_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        OnSliderValueChanged(3, (float)e.NewValue);
    }

    private void Slider5_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        OnSliderValueChanged(4, (float)e.NewValue);
    }

    private void Slider6_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        OnSliderValueChanged(5, (float)e.NewValue);
    }

    private void Slider7_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        OnSliderValueChanged(6, (float)e.NewValue);
    }

    private void Slider8_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        OnSliderValueChanged(7, (float)e.NewValue);
    }

    private void Slider9_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        OnSliderValueChanged(8, (float)e.NewValue);
    }

    private void Slider10_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        OnSliderValueChanged(9, (float)e.NewValue);
    }

    protected virtual void OnSliderValueChanged(int bandIndex, float newValue)
    {
        var band = new KeyValuePair<int, float>(bandIndex, newValue);
     

        SingleBandChanged?.Invoke(this, band);
    }
}

