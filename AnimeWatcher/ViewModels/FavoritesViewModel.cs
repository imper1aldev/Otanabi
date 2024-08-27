using System.Collections.ObjectModel;
using AnimeWatcher.Contracts.Services;
using AnimeWatcher.Contracts.ViewModels;
using AnimeWatcher.Core.Models;
using AnimeWatcher.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace AnimeWatcher.ViewModels;

public partial class FavoritesViewModel : ObservableRecipient, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly DatabaseService dbService = new();
    public ObservableCollection<Anime> FavoriteAnimes { get; } = new ObservableCollection<Anime>();
    public FavoritesViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    public void OnNavigatedFrom()
    {
    }
    public void OnNavigatedTo(object parameter)
    {

    }
    public async void GetAnimesByFavList(object parameter)
    {
        if (parameter is int favId)
        {
            FavoriteAnimes.Clear();
            var data = await dbService.GetFavAnimeByList(favId);
            foreach (var anime in data)
            {
                FavoriteAnimes.Add(anime);
            }

        }
    }
    [RelayCommand]
    private void AnimeClick(Anime anime)
    {
        if (anime != null)
        {
            _navigationService.NavigateTo(typeof(SearchDetailViewModel).FullName!, anime);
        }
    }

}
