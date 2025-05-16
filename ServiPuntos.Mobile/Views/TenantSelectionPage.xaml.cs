namespace ServiPuntos.Mobile.Views;

public partial class TenantSelectionPage : ContentPage
{
    public TenantSelectionPage()
    {
        InitializeComponent();
    }

    private async void OnAncapClicked(object sender, EventArgs e)
    {
        await Services.TenantConfigService.LoadTenantConfigAsync("ancap");
        Application.Current.MainPage = new NavigationPage(new HomePage());
    }
    private async void OnAxionClicked(object sender, EventArgs e)
    {
        await Services.TenantConfigService.LoadTenantConfigAsync("axion");
        Application.Current.MainPage = new NavigationPage(new HomePage());
    }
}
