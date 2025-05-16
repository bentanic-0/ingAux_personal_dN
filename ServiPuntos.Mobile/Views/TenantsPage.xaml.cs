using Microsoft.Maui.Controls;
using ServiPuntos.Mobile.ViewModels;

namespace ServiPuntos.Mobile.Views;

public partial class TenantsPage : ContentPage
{
    public TenantsPage(TenantsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
