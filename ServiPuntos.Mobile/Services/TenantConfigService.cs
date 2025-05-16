using System.Text.Json;
public static class TenantConfigService
{
    public static TenantConfig Config { get; private set; }

    public static async Task LoadTenantConfigAsync(string tenantId)
    {
        var assembly = typeof(TenantConfigService).Assembly;
        var resourceName = $"ServiPuntos.Mobile.Resources.Tenants.{tenantId}.config.json";

        using Stream stream = assembly.GetManifestResourceStream(resourceName);
        using StreamReader reader = new StreamReader(stream);
        string json = await reader.ReadToEndAsync();
        Config = JsonSerializer.Deserialize<TenantConfig>(json);
    }
}
