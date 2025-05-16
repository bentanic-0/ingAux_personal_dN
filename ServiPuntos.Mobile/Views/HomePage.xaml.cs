namespace ServiPuntos.Mobile.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
        var config = Services.TenantConfigService.Config;
        LogoImage.Source = $"Resources/Tenants/{config.TenantId}/{config.Logo}";
        AppNameLabel.Text = config.Nombre;
        this.BackgroundColor = Color.FromArgb(config.ColorPrimario);
    }
}
