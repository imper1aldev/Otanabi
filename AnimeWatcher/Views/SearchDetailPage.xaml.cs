using System.Diagnostics;
using AnimeWatcher.Contracts.Services;
using AnimeWatcher.Core.Models;
using AnimeWatcher.Core.Services;
using AnimeWatcher.ViewModels;

using CommunityToolkit.WinUI.UI.Animations;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace AnimeWatcher.Views;

public sealed partial class SearchDetailPage : Page
{
    private int AnimeId;
    private FavoriteList[] favoriteLists;
    private FavoriteList selectedFList;
    DatabaseService dbService = new();
    public SearchDetailViewModel ViewModel
    {
        get;
    }

    public SearchDetailPage()
    {
        ViewModel = App.GetService<SearchDetailViewModel>();
        InitializeComponent();
    }


    protected async override void OnNavigatedTo(NavigationEventArgs e)
    {

        favoriteLists = await dbService.GetFavoriteLists();
        foreach (var item in favoriteLists)
        {
            FavoriteCombo.Items.Add(item);
        }
        if (e.Parameter is Anime anime)
        {
            AnimeId = anime.Id;
            await LoadAnimeFavList();
        }
        base.OnNavigatedTo(e);
    }
    private async Task LoadAnimeFavList()
    {
        selectedFList = await dbService.GetFavoriteListByAnime(AnimeId);

        if (selectedFList != null)
        {
            var index = 0;
            foreach (FavoriteList item in FavoriteCombo.Items)
            {
                if (selectedFList == null)
                    break;

                if (item.Id == selectedFList.Id)
                {
                    FavoriteCombo.SelectedIndex = index;
                }
                index++;
            }
        }
        else
        {
            //FavoriteCombo.SelectedIndex = 0;
        }
    }
    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);
        if (e.NavigationMode == NavigationMode.Back)
        {

        }
    }
    private void ListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem != null)
        {
            ViewModel.OpenPlayer((Chapter)e.ClickedItem);
        }

    }

    private async void FavoriteCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (selectedFList == null)
        {
            return;
        }
        if (sender is ComboBox cb && cb.SelectedItem is FavoriteList fl && fl.Id != selectedFList.Id)
        {
            await dbService.UpdateAnimeList(AnimeId, fl.Id);
        }

    }

    private async void FavoriteCombo_IsEnabledChanged(object sender, Microsoft.UI.Xaml.DependencyPropertyChangedEventArgs e)
    {
        if (animetxtid.Tag is int aid)
        {
            AnimeId = aid;
        }



        if (sender is ComboBox cb && cb.IsEnabled)
        {
            await LoadAnimeFavList();
        }
    }
}
