namespace ServiPuntos.Mobile;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		MainPage = new AppShell(); // SOLO esta línea, si usas Shell
	}
}
