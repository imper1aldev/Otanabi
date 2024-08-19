using System.Collections.Immutable;
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

    public ObservableCollection<Tag> Tags { get; } = new ObservableCollection<Tag>();

    private Tag[] OriginalTags=Array.Empty<Tag>();

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
            LoadTags();
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
        var selectedTags = Tags.Where(t => t.IsChecked == true).ToArray();
        var data = await _searchAnimeService.MainPageAsync(SelectedProvider, currPage, selectedTags);
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
        var selectedTags = Tags.Where(t => t.IsChecked == true).ToArray();
        var data = await _searchAnimeService.SearchAnimeAsync(query, currPage, SelectedProvider, selectedTags);
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
    private void LoadTags()
    {
        var filters = _searchAnimeService.GetTags(SelectedProvider);
        if (filters.Length > 0)
        {
            foreach (var item in filters)
            {
                Tags.Add(item);
            }
            OriginalTags= (Tag[])filters.Clone();
        }

    }
    public void OnNavigatedFrom()
    {
    }
    private void ResetData()
    {
        Source.Clear();
        Tags.Clear();
        currQuery = string.Empty;
        currPage = 1;
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
    [RelayCommand]
    private async Task OnProviderChanged()
    {
        ResetData();
        LoadTags();
        await LoadMainAnimePage();
    }
    [RelayCommand]
    private void ResetFilterBox()
    {
        if (OriginalTags.Length > 0)
        {
            Tags.Clear(); 
            foreach (var tag in OriginalTags)
            {
                tag.IsChecked=false;
                Tags.Add(tag);
            } 
        }
    }
}
