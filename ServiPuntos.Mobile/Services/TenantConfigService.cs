using System.Text.Json;
using ServiPuntos.Mobile.Models;

namespace ServiPuntos.Mobile.Services
{
    public static class TenantConfigService
    {
        public static TenantConfig Config { get; private set; } = new TenantConfig();

        public static async Task LoadTenantConfigAsync(string tenantId)
        {
            var file = $"Resources/Tenants/{tenantId}/config.json";
            using var stream = await FileSystem.OpenAppPackageFileAsync(file);
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            Config = JsonSerializer.Deserialize<TenantConfig>(json) ?? new TenantConfig();
        }
    }
}
