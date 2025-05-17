using ServiPuntos.Mobile.Models;
using ServiPuntos.Mobile.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ServiPuntos.Mobile.ViewModels
{
    public class TenantSelectorViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TenantConfig> Tenants { get; set; } = new();
        private TenantConfig _selectedTenant;
        public TenantConfig SelectedTenant
        {
            get => _selectedTenant;
            set { _selectedTenant = value; OnPropertyChanged(); }
        }

        public ICommand ContinueCommand { get; }
        public event PropertyChangedEventHandler PropertyChanged;

        private TenantService _tenantService = new TenantService();

        public TenantSelectorViewModel()
        {
            ContinueCommand = new Command(async () => await Continue(), () => SelectedTenant != null);
            LoadTenants();
        }

        public async void LoadTenants()
        {
            var tenants = await _tenantService.GetTenantsAsync();
            Tenants.Clear();
            foreach (var t in tenants)
                Tenants.Add(t);
        }

        private async Task Continue()
        {
            // Aquí podrías navegar a LoginPage, pasando el SelectedTenant
            await App.Current.MainPage.DisplayAlert("Seleccionado", $"Tenant: {SelectedTenant?.Name}", "OK");
            // Navigation logic acá después
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            (ContinueCommand as Command)?.ChangeCanExecute();
        }
    }
}
