using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using ServiPuntos.Core.Entities;
using ServiPuntos.Core.Interfaces;

namespace ServiPuntos.Mobile.Services;

public class TenantService : ITenantService
{
    readonly HttpClient _http;
    readonly ILogger<TenantService> _logger;

    public TenantService(IHttpClientFactory httpFactory, ILogger<TenantService> logger)
    {
        _http = httpFactory.CreateClient("api");
        _logger = logger;
    }

    public async Task<IEnumerable<Tenant>> GetAllAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<IEnumerable<Tenant>>("api/tenant")
                   ?? Array.Empty<Tenant>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tenants");
            throw;
        }
    }

    public async Task<Tenant?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _http.GetFromJsonAsync<Tenant>($"api/tenant/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener tenant {id}");
            throw;
        }
    }

    public Task AddAsync(Tenant tenant)
        => _http.PostAsJsonAsync("api/tenant", tenant);

    public Task UpdateAsync(Tenant tenant)
        => _http.PutAsJsonAsync($"api/tenant/{tenant.Id}", tenant);

    public Task DeleteAsync(Guid id)
        => _http.DeleteAsync($"api/tenant/{id}");
}
