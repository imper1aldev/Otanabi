using AnimeWatcher.Contracts.Services;
using AnimeWatcher.Contracts.ViewModels;
using AnimeWatcher.Core.Contracts.Services;
using AnimeWatcher.Core.Models;
using AnimeWatcher.Core.Services;

using CommunityToolkit.Mvvm.ComponentModel;

namespace AnimeWatcher.ViewModels;

public partial class SearchDetailViewModel : ObservableRecipient, INavigationAware
{
    private readonly SearchAnimeService _seachAnimeService = new(); 
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
            var data = await _seachAnimeService.GetAnimeDetailsAsync(url);
            Item= data;  
                        
        }
    }

    public void OnNavigatedFrom()
    {
    }

    public void OpenPlayer(Chapter chapter)
    { 
            
            _navigationService.NavigateTo(typeof(VideoPlayerViewModel).FullName!); 
    }
}
