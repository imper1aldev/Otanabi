using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Otanabi.Contracts.Services;
using Otanabi.Contracts.ViewModels;
using Otanabi.Core.Models;
using Otanabi.Core.Services;

namespace Otanabi.ViewModels;

public partial class FavoritesViewModel : ObservableRecipient, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly DatabaseService dbService = new();
    public ObservableCollection<Anime> FavoriteAnimes { get; } = new ObservableCollection<Anime>();

    public ObservableCollection<SelectorBarItem> FavBarItems { get; } = new ObservableCollection<SelectorBarItem>();

    public ObservableCollection<FavoriteList> FavoriteList { get; } = new ObservableCollection<FavoriteList>();

    [ObservableProperty]
    private int _currentFavId = 0;

    [ObservableProperty]
    private string _newFavName = string.Empty;

    [ObservableProperty]
    private string _updateFavName = string.Empty;

    [ObservableProperty]
    private FavoriteList _selectedToUpdate;

    public FavoritesViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    public void OnNavigatedFrom()
    {
    }

    public async void OnNavigatedTo(object parameter)
    {
        await LoadFavoriteList(true);
    }

    public event EventHandler FavoreListChanged;

    [RelayCommand]
    private async Task AnimeOnListChanged()
    {
        await GetAnimesByFavList(CurrentFavId);
    }

    [RelayCommand]
    public async Task GetAnimesByFavList(object param)
    {
        var favId = 0;
        if (param is SelectorBar sl)
        {
            if (sl.SelectedItem != null && sl.SelectedItem.Tag != null)
            {
                favId = (int)sl.SelectedItem.Tag;
            }
            else
            {
                if (FavoriteList.Count > 0)
                {
                    await GetAnimesByFavList(FavoriteList.First().Id);
                }
            }
        }
        else if (param is int tmpID)
        {
            favId = tmpID;
        }
        FavoriteAnimes.Clear();
        if (favId == 0)
        {
            return;
        }
        var data = await dbService.GetFavAnimeByList(favId);
        CurrentFavId = favId;
        OnPropertyChanged(nameof(CurrentFavId));
        foreach (var anime in data)
        {
            FavoriteAnimes.Add(anime);
        }
        OnPropertyChanged(nameof(FavoriteAnimes));
    }

    [RelayCommand]
    private void AnimeClick(object param)
    {
        if (param != null && param is Anime anime)
        {
            _navigationService.NavigateTo(typeof(SearchDetailViewModel).FullName!, anime);
        }
    }

    private async Task LoadFavoriteList(bool firstLoad = false)
    {
        FavBarItems.Clear();
        FavoriteList.Clear();
        var fList = await dbService.GetFavoriteLists();
        var counter = 0;
        foreach (var f in fList)
        {
            counter++;
            SelectorBarItem newItem =
                new()
                {
                    Text = $"{f.Name}",
                    IsSelected = counter == 1 ? true : false,
                    Tag = f.Id,
                };
            FavBarItems.Add(newItem);
            FavoriteList.Add(f);
        }
        if (firstLoad)
        {
            await GetAnimesByFavList(fList[0].Id);
        }
        else
        {
            OnPropertyChanged(nameof(FavBarItems));
            var id = FavBarItems.First(x => x.IsSelected == true).Tag;
            await GetAnimesByFavList((id));
        }
        FavoreListChanged?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private async Task AddFavorite()
    {
        if (NewFavName.Length > 3)
        {
            await dbService.CreateFavorite(NewFavName);
            await LoadFavoriteList();
            NewFavName = "";
            UpdateFavName = "";
        }
    }

    [RelayCommand]
    public async Task UpdateFavorite(object param)
    {
        if (param is string newName && SelectedToUpdate != null)
        {
            if (newName.Length > 3 && newName.Length < 60)
            {
                var favoriteL = FavoriteList.First(x => x.Id == SelectedToUpdate.Id);
                favoriteL.Name = newName;

                await dbService.UpdateFavorite(favoriteL);
                await LoadFavoriteList();
                UpdateFavName = "";
            }
        }
    }

    [RelayCommand]
    private async Task DeleteFavorite()
    {
        var favToDelete = new FavoriteList { Id = SelectedToUpdate.Id };
        await dbService.DeleteFavorite(favToDelete);
        await LoadFavoriteList();
        UpdateFavName = "";
    }

    [RelayCommand]
    private void SetUpdatedName()
    {
        var fav = FavoriteList.FirstOrDefault(x => x.Id == SelectedToUpdate.Id);

        if (fav != null)
        {
            UpdateFavName = fav.Name;
        }
    }
}
