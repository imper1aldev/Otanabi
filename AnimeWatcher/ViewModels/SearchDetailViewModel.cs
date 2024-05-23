using System.Diagnostics;
using AnimeWatcher.Contracts.Services;
using AnimeWatcher.Contracts.ViewModels;
using AnimeWatcher.Core.Contracts.Services;
using AnimeWatcher.Core.Models;
using AnimeWatcher.Core.Services;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AnimeWatcher.ViewModels;

public partial class SearchDetailViewModel : ObservableRecipient, INavigationAware
{
    private readonly SearchAnimeService _searchAnimeService = new();
    private readonly SelectSourceService _selectSourceService = new();
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private Anime? item;

    [ObservableProperty]
    private string orderIcon="&#xE74B;";

    private Boolean orderedList=false;

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
        if (parameter is string url)
        {
            var data = await _searchAnimeService.GetAnimeDetailsAsync(url);
            Item = data;

        }
    }

    public void OnNavigatedFrom()
    {
    }


    public async void OpenPlayer(Chapter chapter)
    {
        try
        {
            /*
            var videoSources = await _searchAnimeService.GetVideoSources(chapter.url);
            var chosedSource = videoSources.First(vs => vs.server == "okru");
            if (chosedSource == null)
                return;

            var videoUrl = await _searchAnimeService.GetStreamOKURO(chosedSource.checkedUrl);
            _navigationService.NavigateTo(typeof(VideoPlayerViewModel).FullName!, videoUrl);
            */

            var videoSources = await _searchAnimeService.GetVideoSources(chapter.url);
            var videoUrl = await _selectSourceService.SelectSourceAsync(videoSources,"YourUpload");
            if (string.IsNullOrEmpty(videoUrl))
            {
                throw new Exception(ErrorMessage = "Can't extract the video URL");
            }
            _navigationService.NavigateTo(typeof(VideoPlayerViewModel).FullName!, videoUrl);
        } catch (Exception e)
        {
            ErrorMessage = e.Message.ToString();
            ErrorActive = true;
            return;
        }

    }
    [RelayCommand]
    private void OrderChapterList()
    {
        orderedList=!orderedList;
        var simil=(bool order)=>order?"&#xE74B;":"&#xE74A;";
        OrderIcon= simil(orderedList);

    }
}
