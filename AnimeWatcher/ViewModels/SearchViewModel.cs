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

namespace AnimeWatcher.ViewModels;

public partial class SearchViewModel : ObservableRecipient, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly SearchAnimeService _seachAnimeService = new();

    public ObservableCollection<Anime> Source { get; } = new ObservableCollection<Anime>();

    [ObservableProperty]
    private Visibility loadingResults = Visibility.Collapsed;

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

        await SearchManga("");

        // TODO: Replace with real data.
        //var data = await _sampleDataService.GetContentGridDataAsync();
        //foreach (var item in data)
        //{
        //    Source.Add(item);
        //}
    }

    public async void OnAutoComplete(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        LoadingResults = Visibility.Visible;
        NoResults = Visibility.Collapsed;
        VisibleResults = Visibility.Collapsed;
        Source.Clear();
        var queryText = args.QueryText.ToString();
        await SearchManga(queryText);

        LoadingResults = Visibility.Collapsed;
    }

    public async Task SearchManga(string query)
    {


        var data = await _seachAnimeService.SearchAnimeAsync(query);
        if (data.Count() == 0)
        {
            NoResults = Visibility.Visible;
        }
        else
        {
            foreach (var item in data)
            {
                Source.Add(item);

            }
             VisibleResults = Visibility.Visible;

        }

    }

    public void OnNavigatedFrom()
    {
    }

    [RelayCommand]
    private void OnItemClick(Anime? clickedItem)
    {
        if (clickedItem != null)
        {
            _navigationService.NavigateTo(typeof(SearchDetailViewModel).FullName!, clickedItem.url);
        }
    }
}
