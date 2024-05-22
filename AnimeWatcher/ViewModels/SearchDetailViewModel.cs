using System.Diagnostics;
using AnimeWatcher.Contracts.Services;
using AnimeWatcher.Contracts.ViewModels;
using AnimeWatcher.Core.Contracts.Services;
using AnimeWatcher.Core.Models;
using AnimeWatcher.Core.Services;

using CommunityToolkit.Mvvm.ComponentModel;

namespace AnimeWatcher.ViewModels;

public partial class SearchDetailViewModel : ObservableRecipient, INavigationAware
{
    private readonly SearchAnimeService _searchAnimeService = new();
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private Anime? item;


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
        Debug.WriteLine(chapter);
        var videoSources = await _searchAnimeService.GetVideoSources(chapter.url);

        var chosedSource = videoSources.FirstOrDefault(vs => vs.server == "okru");
        if (chosedSource == null)
            return;

        var videoUrl = await _searchAnimeService.GetStreamOKURO(chosedSource.checkedUrl);
         
        _navigationService.NavigateTo(typeof(VideoPlayerViewModel).FullName!, videoUrl);
    }
}
