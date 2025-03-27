using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Otanabi.Contracts.Services;
using Otanabi.Contracts.ViewModels;
using Otanabi.Core.Anilist.Enums;
using Otanabi.Core.Anilist.Models;
using Otanabi.Core.Services;
using Otanabi.Services;

namespace Otanabi.ViewModels;

public partial class SearchViewModel : ObservableRecipient, INavigationAware
{
    private readonly AnilistService _anilistService = new();
    public MediaSeason[] Seasons { get; } = Enum.GetValues<MediaSeason>().ToArray();
    public MediaFormat[] Formats { get; } = Enum.GetValues<MediaFormat>().ToArray();

    public MediaStatus[] Statuses { get; } = Enum.GetValues<MediaStatus>().ToArray();

    [ObservableProperty]
    private List<string> genres = new();

    public ObservableCollection<Media> SoruceMedia { get; } = new ObservableCollection<Media>();

    public int[] Years
    {
        get
        {
            var currYear = DateTime.Now.Year;
            return Enumerable.Range(1998, ((currYear + 2) - 1998)).Reverse().ToArray();
        }
    }

    [ObservableProperty]
    private Nullable<MediaSeason> selectedSeason;

    [ObservableProperty]
    private Nullable<MediaStatus> selectedStatus;

    public ObservableCollection<MediaFormat> SelectedFormats = new();

    public string GetSelectedFormats
    {
        get
        {
            if (SelectedFormats == null || SelectedFormats.Count == 0)
            {
                return "Select Formats";
            }
            return SelectedFormats.Count >= 2 ? $"{SelectedFormats[0]} , +{SelectedFormats.Count - 1} " : string.Join(", ", SelectedFormats);
        }
    }

    public ObservableCollection<string> SelectedGenres = new();

    public string GetSelectedGenres
    {
        get
        {
            if (SelectedGenres == null || SelectedGenres.Count == 0)
            {
                return "Select Genres";
            }
            return SelectedGenres.Count >= 2 ? $"{SelectedGenres[0]} , +{SelectedGenres.Count - 1} " : string.Join(", ", SelectedGenres);
        }
    }

    [ObservableProperty]
    private Nullable<int> selectedYear;

    public SearchViewModel()
    {
        SelectedGenres.CollectionChanged += (s, e) => OnPropertyChanged(nameof(GetSelectedGenres));
        SelectedFormats.CollectionChanged += (s, e) => OnPropertyChanged(nameof(GetSelectedFormats));
    }

    public void OnNavigatedFrom() { }

    public async void OnNavigatedTo(object parameter)
    {
        await GetTags();
    }

    private async Task GetTags()
    {
        Genres = new List<string>();
        var data = await _anilistService.GetTagsASync();
        foreach (var item in data)
        {
            Genres.Add(item);
        }
        OnPropertyChanged(nameof(Genres));
    }

    public void UpdateCollection(IList<object> selectedItems, string type)
    {
        if (type == "Genres")
        {
            SelectedGenres.Clear();
            foreach (var item in selectedItems)
            {
                SelectedGenres.Add(item.ToString());
            }
        }
        else
        {
            SelectedFormats.Clear();
            foreach (var item in selectedItems)
            {
                SelectedFormats.Add((MediaFormat)item);
            }
        }
    }

    public async void OnSearch(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        //




        var queryText = args.QueryText.ToString();
        var data = await _anilistService.SearchMedia(
            page: 2,
            season: SelectedSeason,
            status: SelectedStatus,
            searchTerm: queryText,
            year: SelectedYear,
            formats: SelectedFormats.Count > 0 ? SelectedFormats.ToArray() : null,
            genres: SelectedGenres.Count > 0 ? SelectedGenres.ToArray() : null
        );
        var tt = data;
    }
}
