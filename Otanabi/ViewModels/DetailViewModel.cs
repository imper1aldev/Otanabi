using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Otanabi.Contracts.Services;
using Otanabi.Contracts.ViewModels;
using Otanabi.Core.Anilist.Enums;
using Otanabi.Core.Anilist.Models;
using Otanabi.Core.Models;
using Otanabi.Core.Services;
using Otanabi.Services;
using Windows.System;

namespace Otanabi.ViewModels;

public partial class DetailViewModel : ObservableRecipient, INavigationAware
{
    private readonly AnilistService _anilistService = new();
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly SearchAnimeService _searchAnimeService = new();
    private readonly INavigationService _navigationService;
    private readonly ILocalSettingsService _localSettingsService;

    public ObservableCollection<Provider> Providers { get; } = new ObservableCollection<Provider>();

    [ObservableProperty]
    private Media selectedMedia;

    [ObservableProperty]
    private Provider selectedProvider;

    [ObservableProperty]
    private BitmapImage bannerImage;

    [ObservableProperty]
    private string link;

    [ObservableProperty]
    private bool isLoaded = false;

    public string StatusString
    {
        get
        {
            if (IsLoaded)
            {
                return SelectedMedia.Status switch
                {
                    MediaStatus.Finished => "Finished",
                    MediaStatus.Releasing => "Releasing",
                    MediaStatus.NotYetReleased => "Not Yet Released",
                    MediaStatus.Cancelled => "Cancelled",
                    _ => "Unknown",
                };
            }

            return "";
        }
    }

    public bool IsPlayeable
    {
        get
        {
            if (IsLoaded)
            {
                return SelectedMedia.Status switch
                {
                    MediaStatus.Finished => true,
                    MediaStatus.Releasing => true,
                    MediaStatus.NotYetReleased => false,
                    MediaStatus.Cancelled => false,
                    _ => false,
                };
            }
            return false;
        }
    }

    public DetailViewModel(INavigationService navigationService, ILocalSettingsService localSettingsService)
    {
        _navigationService = navigationService;
        _localSettingsService = localSettingsService;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    }

    public void OnNavigatedFrom() { }

    public async void OnNavigatedTo(object parameter)
    {
        await GetProviders();
        if (parameter is Media media)
        {
            await LoadMediaAsync(media.Id);
        }
        else
        {
            await LoadMediaAsync(175443);
        }
    }

    private async Task LoadMediaAsync(int id)
    {
        var data = await _anilistService.GetMediaByIdAsync(id);
        BannerImage = data.BannerImage != null ? new BitmapImage(new Uri(data.BannerImage)) : new BitmapImage(new Uri(data.CoverImage.ExtraLarge));

        Link = $"https://anilist.co/anime/{data.Id}";
        SelectedMedia = data;
        IsLoaded = true;
        OnPropertyChanged(nameof(StatusString));
        OnPropertyChanged(nameof(IsPlayeable));
    }

    [RelayCommand]
    private void OpenLink()
    {
        _dispatcherQueue.TryEnqueue(async () => await Launcher.LaunchUriAsync(new Uri(Link)));
    }

    private async Task GetProviders()
    {
        if (Providers.Count == 0)
        {
            Providers.Clear();
            var provs = _searchAnimeService.GetProviders();
            foreach (var item in provs)
            {
                Providers.Add(item);
            }
            SelectedProvider = provs[0];
        }

        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task SearchReferences()
    {
        var data = await SearchEngineService.SearchByName(SelectedMedia.Title, SelectedProvider);
        ;
    }

    /*
     the chapters need to be based on the following rules
     * 1): If is finised get all the chapters SelectedMedia.Episodes
     * 2): if the status is airing only show the chapters less than SelectedMedia.nextAiringEpisode.episode i'll check if is possible to add the nextairing with a timer but make it without a eventclick
     * 3): if the status is notyetreleased show a counter with the time left to the first episode
     * 4): if is a movie or a single cap ova/ona show only 1 item with the prefix "Episode-#"
     * 5): if is a (movie/ona/ova)  with multiple parts (most common in ovas/onas) show the prefix "Episode-#"
    */
}
