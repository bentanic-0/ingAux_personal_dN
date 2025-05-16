using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiPuntos.Core.Interfaces;    // <-- Aquí van tus interfaces (ITenantService, etc.)
using ServiPuntos.Mobile.Services;    // TenantService, AuthService...
using ServiPuntos.Mobile;             // Para Configuration

namespace ServiPuntos.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

		// 1) App + Fuentes
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(f =>
			{
				f.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				f.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// 2) HttpClient "api"
		builder.Services
			.AddHttpClient("api", c => c.BaseAddress = new Uri(Configuration.ApiBaseUrl));

		// 3) Servicios de API
		builder.Services.AddSingleton<ITenantService, TenantService>();
		// builder.Services.AddSingleton<IAuthService, AuthService>();
		// builder.Services.AddSingleton<IPointsService, PointsService>();
		// …

		// 4) Views + ViewModels
		builder.Services.AddTransient<Views.TenantsPage>();
		builder.Services.AddTransient<ViewModels.TenantsViewModel>();

		// 5) Logging en DEBUG
#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
