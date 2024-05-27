using System.Collections.ObjectModel;
using System.Windows.Input;

using AnimeWatcher.Contracts.Services;
using AnimeWatcher.Contracts.ViewModels;
using AnimeWatcher.Core.Contracts.Services;
using AnimeWatcher.Core.Models;
using AnimeWatcher.Core.Services;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SharpDX.Direct3D11;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AnimeWatcher.ViewModels;

public partial class SearchViewModel : ObservableRecipient, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly SearchAnimeService _searchAnimeService = new();

    private string currQuery = string.Empty;
    private int currPage = 1;
    public ObservableCollection<Anime> Source { get; } = new ObservableCollection<Anime>();

    [ObservableProperty]
    private Visibility loadingResults = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility loadingMoreResults = Visibility.Collapsed;

    [ObservableProperty]
    private bool loadingMoreResultsBol = false;


    [ObservableProperty]
    private Provider selectedProvider = new();

    public ObservableCollection<Provider> Providers { get; } = new ObservableCollection<Provider>();

    [ObservableProperty]
    private Visibility visibleResults = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility noResults = Visibility.Collapsed;

    public SearchViewModel(
        INavigationService navigationService
        )
    {
        _navigationService = navigationService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        Source.Clear();
        await GetProviders();
        await LoadMainAnimePage();
    }
    private async Task GetProviders()
    {
        var provs = _searchAnimeService.GetProviders();
        foreach (var item in provs)
        {
            Providers.Add(item);
        }
        SelectedProvider = provs[0];
        await Task.CompletedTask;
    }



    public async void OnAutoComplete(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        LoadingResults = Visibility.Visible;
        NoResults = Visibility.Collapsed;
        VisibleResults = Visibility.Collapsed;
        Source.Clear();
        var queryText = args.QueryText.ToString();
        currQuery = queryText;
        currPage = 1;
        if (currQuery == "")
        {
            await LoadMainAnimePage();
        }
        else
        {
            await SearchManga(queryText);
        }


        LoadingResults = Visibility.Collapsed;

    }
    public async Task LoadMainAnimePage()
    {
        LoadingMoreResultsBol = false;
        var data = await _searchAnimeService.MainPageAsync(SelectedProvider, currPage);
        if (data.Count() == 0)
        {
            NoResults = currPage == 1 ? Visibility.Visible : Visibility.Collapsed;
            return;
        }
        foreach (var item in data)
        {
            Source.Add(item);
        }
        VisibleResults = Visibility.Visible;
        LoadingMoreResultsBol = true;
    }
    public async Task SearchManga(string query)
    {
        LoadingMoreResultsBol = false;
        var data = await _searchAnimeService.SearchAnimeAsync(query, currPage, SelectedProvider);
        if (data.Count() == 0)
        {
            NoResults = currPage == 1 ? Visibility.Visible : Visibility.Collapsed;
            return;
        }
        foreach (var item in data)
        {
            Source.Add(item);
        }
        VisibleResults = Visibility.Visible;
        LoadingMoreResultsBol = true;
    }

    public void OnNavigatedFrom()
    {
    }

    [RelayCommand]
    private void OnItemClick(Anime? clickedItem)
    {
        if (clickedItem != null)
        {
            _navigationService.NavigateTo(typeof(SearchDetailViewModel).FullName!, clickedItem);
        }
    }
    [RelayCommand]
    private async void LoadMore()
    {
        LoadingMoreResults = Visibility.Visible;
        currPage++;
        if (currQuery == "")
        {
            await LoadMainAnimePage();
        }
        else
        {
            await SearchManga(currQuery);
        }


        LoadingMoreResults = Visibility.Collapsed;
    }
}
