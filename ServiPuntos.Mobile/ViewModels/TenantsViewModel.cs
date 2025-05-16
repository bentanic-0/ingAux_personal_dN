using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServiPuntos.Core.Entities;
using ServiPuntos.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ServiPuntos.Mobile.ViewModels;

public partial class TenantsViewModel : ObservableObject
{
    readonly ITenantService _service;

    public ObservableCollection<Tenant> Tenants { get; } = new();

    public TenantsViewModel(ITenantService service)
    {
        _service = service;
    }

    [RelayCommand]
    public async Task LoadTenantsAsync()
    {
        var list = await _service.GetAllAsync();
        Tenants.Clear();
        foreach (var t in list)
            Tenants.Add(t);
    }
}
