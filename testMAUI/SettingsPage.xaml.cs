namespace testMAUI;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();
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
}