// Quita cualquier "using ServiPuntos.Application;" o similar que traiga el namespace Application
using Microsoft.Maui.Controls;    // <-- el tipo Application vive aquí

namespace ServiPuntos.Mobile
{
	public partial class App : Microsoft.Maui.Controls.Application
	// ahora Application es el tipo de MAUI
	{
		public App()
		{
			InitializeComponent();

			// Arrancamos con tu Shell
			MainPage = new AppShell();
		}
	}
}
