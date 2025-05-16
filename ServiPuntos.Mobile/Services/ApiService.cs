public class ApiService
{
    private readonly HttpClient _client;

    public ApiService()
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri(TenantConfigService.Config.ApiUrl);
        _client.DefaultRequestHeaders.Add("X-Tenant-ID", TenantConfigService.Config.TenantId);
    }

    public async Task<Usuario> LoginAsync(string username, string password)
    {
        var data = new { username, password };
        var response = await _client.PostAsJsonAsync("/api/auth/login", data);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Usuario>();
    }

    // Otros métodos...
}
