public partial class TenantSelectionPage : ContentPage
{
    public TenantSelectionPage()
    {
        InitializeComponent();
    }

    private async void OnAncapClicked(object sender, EventArgs e)
    {
        await TenantConfigService.LoadTenantConfigAsync("ancap");
        Application.Current.MainPage = new NavigationPage(new LoginPage());
    }

    private async void OnAxionClicked(object sender, EventArgs e)
    {
        await TenantConfigService.LoadTenantConfigAsync("axion");
        Application.Current.MainPage = new NavigationPage(new LoginPage());
    }
}
