using System.Diagnostics;
using AnimeWatcher.Contracts.Services;
using AnimeWatcher.Contracts.ViewModels;
using AnimeWatcher.Core.Contracts.Services;
using AnimeWatcher.Core.Models;
using AnimeWatcher.Core.Services;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;

namespace AnimeWatcher.ViewModels;

public partial class SearchDetailViewModel : ObservableRecipient, INavigationAware
{
    private readonly SearchAnimeService _searchAnimeService = new();
    private readonly SelectSourceService _selectSourceService = new();
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private Anime anime;

    [ObservableProperty]
    private Chapter[]? chapters;

    [ObservableProperty]
    private string orderIcon = "\uE74A";

    private Boolean orderedList = false;

    [ObservableProperty]
    public Boolean errorActive = false;

    [ObservableProperty]
    public string errorMessage = "";

    public SearchDetailViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        GC.Collect();

        if (parameter is Anime anime)
        {
            Anime = await _searchAnimeService.GetAnimeDetailsAsync(anime); 

        }
    }




    public void OnNavigatedFrom()
    {
    }


    public async void OpenPlayer(Chapter chapter)
    {
        //try
        //{

        var videoSources = await _searchAnimeService.GetVideoSources(chapter.Url, Anime.Provider);
        var videoUrl = await _selectSourceService.SelectSourceAsync(videoSources, "YourUpload");
        if (string.IsNullOrEmpty(videoUrl))
        {
            throw new Exception(ErrorMessage = "Can't extract the video URL");
        }
        _navigationService.NavigateTo(typeof(VideoPlayerViewModel).FullName!, videoUrl);

        //} catch (Exception e)
        //{
        //    ErrorMessage = e.Message.ToString();
        //    ErrorActive = true;
        //    return;
        //}

    }
    [RelayCommand]
    private void OrderChapterList()
    {
        orderedList = !orderedList;
        OrderIcon = orderedList ? "\uE74A" : "\uE74B";

    }
}
