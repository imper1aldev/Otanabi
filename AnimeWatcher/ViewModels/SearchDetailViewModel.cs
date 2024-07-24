using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using AnimeWatcher.Contracts.Services;
using AnimeWatcher.Contracts.ViewModels;
using AnimeWatcher.Core.Models;
using AnimeWatcher.Core.Services;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;
namespace AnimeWatcher.ViewModels;

public partial class SearchDetailViewModel : ObservableRecipient, INavigationAware
{
    private readonly SearchAnimeService _searchAnimeService = new();
    private readonly DatabaseService _Db = new();
    private readonly SelectSourceService _selectSourceService = new();
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly INavigationService _navigationService;

    public ObservableCollection<Chapter> ChapterList { get; } = new ObservableCollection<Chapter>();
    public ObservableCollection<FavoriteList> FavLists { get; } = new ObservableCollection<FavoriteList>();

    [ObservableProperty]
    private bool isLoadingVideo = false;

    [ObservableProperty]
    private bool isLoadingFav = true;

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private bool forceLoad = false;


    [ObservableProperty]
    public string favText = "";

    [ObservableProperty]
    public List<FavoriteList> favoritelistsSelected;


    [ObservableProperty]
    private Anime selectedAnime;

    [ObservableProperty]
    private Chapter[]? chapters;

    [ObservableProperty]
    private string favStatus = "\uE728";

    [ObservableProperty]
    private bool isFavorite = false;

    [ObservableProperty]
    private string orderIcon = "\uE74B";

    private Boolean orderedList = false;

    [ObservableProperty]
    public Boolean errorActive = false;

    [ObservableProperty]
    public string errorMessage = "";

    public SearchDetailViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    }

    public async void OnNavigatedTo(object parameter)
    {
        GC.Collect();
        IsLoadingFav = true;
        IsLoading = true;
        var favoriteLists = await _Db.GetFavoriteLists();
        foreach (var fav in favoriteLists)
        {
            FavLists.Add(fav);
        }
        if (parameter is Anime anime)
        {
            if (anime.Url != null)
            {
                await GetAnimeFromDB(anime);
                var bw = new BackgroundWorker();
                bw.DoWork += (sender, args) => _dispatcherQueue.TryEnqueue(async () =>
                {
                    IsLoading = true;
                    await UpsertAnime(anime);
                    IsLoading = false;
                });
                bw.RunWorkerAsync();
            }

            IsLoadingFav = false;
            IsLoading = false;
        }
    }
    private void setLoader(bool value)
    {
        IsLoading = value;
    }

    private async Task GetAnimeFromDB(Anime request)
    {
        var anime = await _Db.GetAnimeOnDB(request);
        if (anime != null)
        {
            SelectedAnime = anime;
            await checkFavorite();

            if (SelectedAnime.Chapters == null)
                return;

            foreach (var chapter in SelectedAnime.Chapters.OrderByDescending((a) => a.ChapterNumber))
            {
                ChapterList.Add(chapter);
            }

        }


    }
    private async Task UpsertAnime(Anime request, bool force = false)
    {
        ForceLoad = false;
        try
        {
            var anime = await _Db.UpsertAnime(request, force);
            if (anime != null)
            {
                SelectedAnime = anime;

                await checkFavorite();
                ChapterList.Clear();
                if (SelectedAnime.Chapters == null)
                    return;

                foreach (var chapter in SelectedAnime.Chapters.OrderByDescending((a) => a.ChapterNumber))
                {
                    ChapterList.Add(chapter);
                }
            }
        } catch (Exception e)
        {
            ErrorMessage = e.Message.ToString();
            ErrorActive = true; 
        } finally
        {
            ForceLoad = true;
        }
    }

    [RelayCommand]
    private void ForceUpsert()
    {
        var bw = new BackgroundWorker();
        bw.DoWork += (sender, args) => _dispatcherQueue.TryEnqueue(async () =>
        {
            IsLoading = true;
            await UpsertAnime(SelectedAnime, true);
            IsLoading = false;
        });
        bw.RunWorkerAsync();
    }


    public void OnNavigatedFrom()
    {
    }


    public async void OpenPlayer(Chapter chapter)
    {
        IsLoadingVideo = true;
        try
        {
            //var videoSources = await _searchAnimeService.GetVideoSources(chapter.Url, SelectedAnime.Provider);
            //var videoUrl = await _selectSourceService.SelectSourceAsync(videoSources);
            var history = await _Db.GetOrCreateHistoryByCap(chapter.Id);
            dynamic data = new ExpandoObject();
            data.History = history;
            //data.Url = videoUrl;
            data.Chapter = chapter;
            //data.ChapterName = $"{SelectedAnime.Title}  Ep# {chapter.ChapterNumber}";
            data.AnimeTitle=SelectedAnime.Title;
            data.ChapterList=SelectedAnime.Chapters.ToList();
            data.Provider=SelectedAnime.Provider;
            //if (string.IsNullOrEmpty(videoUrl))
            //{
            //    throw new Exception(ErrorMessage = "Can't extract the video URL");
            //}
            _navigationService.NavigateTo(typeof(VideoPlayerViewModel).FullName!, data);
            IsLoadingVideo = false;
        } catch (Exception e)
        {
            IsLoadingVideo = false;
            ErrorMessage = e.Message.ToString();
            ErrorActive = true;
            return;
        }
    }
    [RelayCommand]
    private void OrderChapterList()
    {
        ChapterList.Clear();
        orderedList = !orderedList;
        OrderIcon = orderedList ? "\uE74A" : "\uE74B";

        if (orderedList)
        {
            foreach (var chapter in SelectedAnime.Chapters)
            {
                ChapterList.Add(chapter);
            }
        }
        else
        {
            foreach (var chapter in SelectedAnime.Chapters.Reverse())
            {
                ChapterList.Add(chapter);
            }
        }
    }


    [RelayCommand]
    private async Task FavoriteFun()
    {
        IsLoadingFav = true;
        var action = IsFavorite ? "remove" : "add";
        var res = await _Db.AddToFavorites(SelectedAnime, action);

        IsFavorite = res == "added" ? true : false;
        FavStatus = IsFavorite ? "\uE8D9" : "\uE728";
        FavText = IsFavorite ? "Remove from Favorites" : "Add to Favorites";
        IsLoadingFav = false;
    }

    private async Task checkFavorite()
    {
        IsFavorite = await _Db.IsFavorite(SelectedAnime.Id);
        // Icon TO FAV "\uE728"
        // Icon TO UNFAV "\uE8D9"  
        FavStatus = IsFavorite ? "\uE8D9" : "\uE728";
        FavText = IsFavorite ? "Remove from Favorites" : "Add to Favorites";
    }

    [RelayCommand]
    private async Task ChangeFavLists(object param)
    {
        var idList = new List<int>();
        if (param is ListBox box)
        {
            foreach (var item in box.SelectedItems)
            {
                if (item is FavoriteList lt)
                    idList.Add(lt.Id);
            }
        }
        if (idList.Count > 0)
            await _Db.UpdateAnimeList(SelectedAnime.Id, idList);
    }
}
