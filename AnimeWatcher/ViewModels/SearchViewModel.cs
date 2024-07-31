using System.Collections.ObjectModel;

using AnimeWatcher.Contracts.Services;
using AnimeWatcher.Contracts.ViewModels;
using AnimeWatcher.Core.Models;
using AnimeWatcher.Core.Services;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;

namespace AnimeWatcher.ViewModels;

public partial class SearchViewModel : ObservableRecipient, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly SearchAnimeService _searchAnimeService = new();
    private readonly ILocalSettingsService _localSettingsService;

    private string currQuery = string.Empty;
    private int currPage = 1;
    public ObservableCollection<Anime> Source { get; } = new ObservableCollection<Anime>();

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private bool noResults = false;

    [ObservableProperty]
    private Provider selectedProvider = new();

    public ObservableCollection<Provider> Providers { get; } = new ObservableCollection<Provider>();


    public SearchViewModel(
        INavigationService navigationService, ILocalSettingsService localSettingsService
        )
    {
        _navigationService = navigationService;
        _localSettingsService = localSettingsService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        // Source.Clear();
        if (Source.Count == 0)
        {
            await GetProviders();
            var provdef = await _localSettingsService.ReadSettingAsync<int>("ProviderId");

            if (provdef != 0)
            {
                var tmp = Providers.FirstOrDefault(p => p.Id == provdef);
                if (tmp != null)
                    SelectedProvider = tmp;
            }
            await Task.CompletedTask;
            await LoadMainAnimePage();
        }

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

        Source.Clear();
        var queryText = args.QueryText.ToString();
        currQuery = queryText;
        currPage = 1;
        if (currQuery == "")
            await LoadMainAnimePage();
        else
            await SearchManga(queryText);
    }
    public async Task LoadMainAnimePage()
    {
        IsLoading = true;
        NoResults = false;
        var data = await _searchAnimeService.MainPageAsync(SelectedProvider, currPage);
        if (data.Count() == 0)
        {
            NoResults = true;
            IsLoading = false;
            return;
        }
        foreach (var item in data)
        {
            Source.Add(item);
        }
        IsLoading = false;
    }
    public async Task SearchManga(string query)
    {
        NoResults = false;
        IsLoading = true;
        var data = await _searchAnimeService.SearchAnimeAsync(query, currPage, SelectedProvider);
        if (data.Count() == 0)
        {
            NoResults = true;
            IsLoading = false;
            return;
        }
        foreach (var item in data)
        {
            Source.Add(item);
        }
        IsLoading = false;
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
    private async Task LoadMore()
    {
        if (IsLoading)
        {
            return;
        }

        currPage++;
        if (currQuery == "")
            await LoadMainAnimePage();
        else
            await SearchManga(currQuery);

    }
}
