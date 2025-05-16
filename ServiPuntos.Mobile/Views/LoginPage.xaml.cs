public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
        var config = TenantConfigService.Config;
        LogoImage.Source = $"resource://ServiPuntos.Mobile.Resources.Tenants.{config.TenantId}.{config.Logo}";
        AppNameLabel.Text = config.NombreApp;
        this.BackgroundColor = Color.FromArgb(config.ColorPrimario);
    }
}
