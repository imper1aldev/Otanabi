using AnimeWatcher.Core.Models;
using AnimeWatcher.Core.Services;
using AnimeWatcher.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace AnimeWatcher.Views;

public sealed partial class FavoritesPage : Page
{
    DatabaseService dbService = new();
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
        ClearFlyout();
        
        var fList = await dbService.GetFavoriteLists();
        var counter = 0;
        foreach (var f in fList)
        {
            counter++;
            SelectorBarItem newItem = new();
            newItem.Text = f.Name;
            newItem.IsSelected = counter == 1 ? true : false;
            newItem.Tag = f.Id;
            FavoriteListBar.Items.Add(newItem);

            ComboBoxItem boxItem = new();
            boxItem.Content = f.Name;
            boxItem.Tag = f.Id;
            FavCombob.Items.Add(boxItem);

        }
    }

    protected async override void OnNavigatedTo(NavigationEventArgs e)
    {
        await loadFavoriteList();
        base.OnNavigatedTo(e);
    }

    private void FavoriteListBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        if (sender.SelectedItem != null)
        {
            ViewModel.GetAnimesByFavList(sender.SelectedItem.Tag);
        }

    }

    private async void Button_add_fav_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (txtNew.Text.Length > 3)
        {
            await dbService.CreateFavorite(txtNew.Text); 
            await loadFavoriteList();
        }
    }

    private async void Button_update_fav_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (FavCombob.SelectedItem != null && FavTxtUpdate.Text.Length > 3 && FavTxtUpdate.Text.Length < 60)
        {
            var favoriteL = new FavoriteList();
            var data = (ComboBoxItem)FavCombob.SelectedItem;
            favoriteL.Id = (int)data.Tag;
            favoriteL.Name = FavTxtUpdate.Text;

            await dbService.UpdateFavorite(favoriteL);
            await loadFavoriteList();

        }
    }
    private void ClearFlyout()
    {
        FavTxtUpdate.Text = "";
        txtNew.Text = "";
        FavCombob.SelectedValue = null;
        FavCombob.Items.Clear();
        FavEditFlyout.Hide();

    }

    private async void Button_delete_fav_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (FavCombob.SelectedItem != null)
        {
            var data = (ComboBoxItem)FavCombob.SelectedItem;
            var id = (int)data.Tag;
            if (id > 1)
            {
                var favoriteL = new FavoriteList();
                favoriteL.Id = id;

                await dbService.DeleteFavorite(favoriteL);
                await loadFavoriteList();
            }
        }

    }
}
