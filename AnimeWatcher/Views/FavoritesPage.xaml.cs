using System.Diagnostics;
using AnimeWatcher.Contracts.ViewModels;
using AnimeWatcher.Core.Services;
using AnimeWatcher.ViewModels;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SharpDX.Direct3D11;

namespace AnimeWatcher.Views;

public sealed partial class FavoritesPage : Page
{
    DatabaseService dbeService = new();
    public FavoritesViewModel ViewModel
    {
        get;
    }

    public FavoritesPage()
    {
        ViewModel = App.GetService<FavoritesViewModel>();
        InitializeComponent();
    }

    private async Task loadFavoriteList()
    {
        FavoriteListBar.Items.Clear();
        var fList = await dbeService.GetFavoriteLists();
        var counter = 0;
        foreach (var f in fList)
        {
            counter++;
            SelectorBarItem newItem = new SelectorBarItem();
            newItem.Text = f.Name;

            newItem.IsSelected = counter == 1 ? true : false;
            newItem.Tag = f.Id;
            FavoriteListBar.Items.Add(newItem);
        }
    }

    protected async override void OnNavigatedTo(NavigationEventArgs e)
    {
        await loadFavoriteList();
        base.OnNavigatedTo(e);
    }

    private void FavoriteListBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        ViewModel.GetAnimesByFavList(sender.SelectedItem.Tag);
    }
}
