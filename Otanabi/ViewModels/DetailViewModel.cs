using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
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
using Otanabi.UserControls;
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

    /*Match Anilist detail with provider*/
    private Anime _localAnime;

    /**/

    public ObservableCollection<MediaStreamingEpisode> EpisodeList { get; } = new ObservableCollection<MediaStreamingEpisode>();

    private EpisodeCollectionControl _episodeCollectionControl;

    [ObservableProperty]
    private bool isLoaded = false;

    [ObservableProperty]
    private bool isLoadedMerge = false;

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
        GC.Collect();
        await GetProviders();
        if (parameter is Media media && (SelectedMedia == null || SelectedMedia.Id != media.Id))
        {
            await LoadMediaAsync(media.Id);
        }
    }

    private async Task LoadMediaAsync(int id)
    {
        var data = await _anilistService.GetMediaByIdAsync(id);
        BannerImage = data.BannerImage != null ? new BitmapImage(new Uri(data.BannerImage)) : new BitmapImage(new Uri(data.CoverImage.ExtraLarge));

        Link = $"https://anilist.co/anime/{data.Id}";
        EpisodeList.Clear();

        if (data.Status != MediaStatus.NotYetReleased)
        {
            foreach (var episode in data.StreamingEpisodes)
            {
                EpisodeList.Add(episode);
            }
        }
        else
        {
            IsLoadedMerge = true;
        }

        SelectedMedia = data;
        IsLoaded = true;
        EpisodesLoaded();
        OnPropertyChanged(nameof(StatusString));
        OnPropertyChanged(nameof(IsPlayeable));
        await SearchReferences();
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
            var provs = _searchAnimeService.GetProviders().Where(prov => prov.IsTrackeable).ToList();
            foreach (var item in provs)
            {
                Providers.Add(item);
            }
            SelectedProvider = provs[0];
        }

        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ProviderChanged()
    {
        IsLoadedMerge = false;
        await SearchReferences();
    }

    public void LoadEpisodeComponent(EpisodeCollectionControl episodeCollectionControl)
    {
        _episodeCollectionControl = episodeCollectionControl;
    }

    [RelayCommand]
    private async Task LoadEpisode(MediaStreamingEpisode episode)
    {
        if (_localAnime != null)
        {
            var ep = _localAnime.Chapters.Where(x => x.ChapterNumber == episode.Number).FirstOrDefault();
            if (ep != null)
            {
                Debug.Print($"Episode selected: {ep.Url}");
                await OpenPlayer(ep);
            }
        }
    }

    private void EpisodesLoaded()
    {
        var data = SelectedMedia.NextAiringEpisode;
        _episodeCollectionControl.LoadNextAiringData();
    }

    private async Task SearchReferences()
    {
        var data = await SearchEngineService.SearchByName(SelectedMedia.Title, SelectedProvider);
        var exactMatch = data.Item1;
        var otherMatches = data.Item2;

        if (exactMatch != null)
        {
            _localAnime = await _searchAnimeService.GetAnimeDetailsAsync(exactMatch);
            IsLoadedMerge = true;
        }
    }

    public async Task OpenPlayer(Chapter chapter)
    {
        //IsLoadingVideo = true;
        try
        {
            //App.AppState.TryGetValue("Incognito", out var incognito);  Incognito mode
            dynamic data = new ExpandoObject();
            //data.History = (bool)incognito ? new History() { Id = 0 } : await _Db.GetOrCreateHistoryByCap(chapter.Id);
            data.History = new History() { Id = 0 };
            data.Chapter = chapter;
            data.AnimeTitle = _localAnime.Title;
            data.ChapterList = _localAnime.Chapters.ToList();
            data.Provider = _localAnime.Provider;
            _dispatcherQueue.TryEnqueue(() => _navigationService.NavigateTo(typeof(VideoPlayerViewModel).FullName!, data));

            //IsLoadingVideo = false;
        }
        catch (Exception e)
        {
            //IsLoadingVideo = false;
            //ErrorMessage = e.Message.ToString();
            //ErrorActive = true;
            return;
        }
    }
    /* TODO : Chapter comminucation between detailview to provider to VideoView
     the chapters need to be based on the following rules
     * 1): If is finised get all the chapters SelectedMedia.Episodes
     * 2): if the status is airing only show the chapters less than SelectedMedia.nextAiringEpisode.episode i'll check if is possible to add the nextairing with a timer but make it without a eventclick
     * 3): if the status is notyetreleased show a counter with the time left to the first episode
     * 4): if is a movie or a single cap ova/ona show only 1 item with the prefix "Episode-#"
     * 5): if is a (movie/ona/ova)  with multiple parts (most common in ovas/onas) show the prefix "Episode-#"
    */
}
