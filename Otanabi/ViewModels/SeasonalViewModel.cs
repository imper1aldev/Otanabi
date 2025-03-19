using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Otanabi.Core.Models;
using Otanabi.Core.Services;

namespace Otanabi.ViewModels;

public partial class SeasonalViewModel : ObservableRecipient
{
    private AnilistService _anilistService = new();
    public ObservableCollection<Anime> AnimeList { get; } = new ObservableCollection<Anime>();

    public SeasonalViewModel() { }

    [RelayCommand]
    private async void LoadSeasonalAnimes()
    {
        // Load seasonal animes
        var response = await _anilistService.GetSeasonal();

        // Add animes to the list
        foreach (var anime in response)
        {
            AnimeList.Add(anime);
        }
    }
}
