using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Otanabi.Contracts.ViewModels;
using Otanabi.Core.Models;
using Otanabi.Core.Services;
using ZeroQL.Client;

namespace Otanabi.ViewModels;

public partial class SeasonalViewModel : ObservableRecipient, INavigationAware
{
    private AnilistService _anilistService = new();
    public ObservableCollection<Anime> AnimeList { get; } = new ObservableCollection<Anime>();

    public int[] years = Enumerable.Range(2009, ((DateTime.Now.Year + 2) - 2009)).Reverse().ToArray();

    [ObservableProperty]
    private int selectedYear = DateTime.Now.Year;

    private MediaSeason selectedSeason;

    [ObservableProperty]
    private SelectorBarItem selectedSeasonBar;

    private SelectorBarItem[] selectorBars;

    public SeasonalViewModel() { }

    [RelayCommand]
    private async void LoadSeasonalAnimes()
    {
        // Load seasonal animes
        await LoadData(selectedSeason, SelectedYear, 1);
    }

    public async void OnNavigatedTo(object parameter) { }

    public void OnNavigatedFrom() { }

    private async Task LoadData(MediaSeason season, int year, int page = 1)
    {
        var response = await _anilistService.GetSeasonal(season: season, seasonYear: year, page: page);

        // Add animes to the list
        foreach (var anime in response.Item1)
        {
            AnimeList.Add(anime);
        }
    }

    [RelayCommand]
    private void test()
    {
        Console.WriteLine(selectorBars);
        Console.WriteLine(SelectedSeasonBar);
    }

    [RelayCommand]
    private async Task LoadedView(SelectorBar selectorBar)
    {
        selectorBars = selectorBar.Items.ToArray();
        AnimeList.Clear();
        LoadCurrentSeason();
        await LoadData(selectedSeason, SelectedYear, 1);
    }

    [RelayCommand]
    private async void SeasonChanged()
    {
        if (selectedSeasonBar != null)
        {
            var season = SelectedSeasonBar.Name switch
            {
                "SelectorSpring" => MediaSeason.Spring,
                "SelectorSummer" => MediaSeason.Summer,
                "SelectorFall" => MediaSeason.Fall,
                "SelectorWinter" => MediaSeason.Winter,
                _ => MediaSeason.Winter,
            };

            AnimeList.Clear();
            await LoadData(season, SelectedYear);
        }
    }

    private void LoadCurrentSeason()
    {
        var currDate = DateTime.Now.Date;
        var month = currDate.Month;
        var day = currDate.Day;
        if ((month == 3 && day >= 20) || month == 4 || month == 5 || (month == 6 && day < 21))
        {
            SelectedSeasonBar = selectorBars.FirstOrDefault(x => x.Name == "SelectorSpring");
            selectedSeason = MediaSeason.Spring;
        }
        else if ((month == 6 && day >= 21) || month == 7 || month == 8 || (month == 9 && day < 22))
        {
            SelectedSeasonBar = selectorBars.FirstOrDefault(x => x.Name == "SelectorSummer");
            selectedSeason = MediaSeason.Summer;
        }
        else if ((month == 9 && day >= 22) || month == 10 || month == 11 || (month == 12 && day < 21))
        {
            SelectedSeasonBar = selectorBars.FirstOrDefault(x => x.Name == "SelectorFall");
            selectedSeason = MediaSeason.Fall;
        }
        else
        {
            SelectedSeasonBar = selectorBars.FirstOrDefault(x => x.Name == "SelectorWinter");
            selectedSeason = MediaSeason.Winter;
        }
    }
}
