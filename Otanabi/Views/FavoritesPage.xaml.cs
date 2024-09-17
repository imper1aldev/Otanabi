using Otanabi.Core.Models;
using Otanabi.Core.Services;
using Otanabi.ViewModels;
using Microsoft.UI.Xaml.Controls; 

namespace Otanabi.Views;

public sealed partial class FavoritesPage : Page
{
    readonly DatabaseService dbService = new();
    public FavoritesViewModel ViewModel
    {
        get;
    }

    public FavoritesPage()
    {
        ViewModel = App.GetService<FavoritesViewModel>();
        InitializeComponent();
    }
     
    private async void OpenConfigDialog(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ConfigFavsDialog.XamlRoot=this.XamlRoot;
        var result = await ConfigFavsDialog.ShowAsync();
    }
}
