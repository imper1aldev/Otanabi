using AnimeWatcher.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace AnimeWatcher.Views;

public sealed partial class FavoritesPage : Page
{
    public FavoritesViewModel ViewModel
    {
        get;
    }

    public FavoritesPage()
    {
        ViewModel = App.GetService<FavoritesViewModel>();
        InitializeComponent();
    }
}
