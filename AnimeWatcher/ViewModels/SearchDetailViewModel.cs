using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
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
    private readonly DatabaseService _databaseService = new();
    private readonly SelectSourceService _selectSourceService = new();
    private readonly INavigationService _navigationService;

    public ObservableCollection<Chapter> ChapterList { get; } = new ObservableCollection<Chapter>();
    public ObservableCollection<FavoriteList> FavLists { get; } = new ObservableCollection<FavoriteList>();

    [ObservableProperty]
    private bool isLoadingVideo = false;

    [ObservableProperty]
    private bool isLoadingFav = true;

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
    }

    public async void OnNavigatedTo(object parameter)
    {

        GC.Collect();
        IsLoadingFav = true;
        var favoriteLists = await _databaseService.GetFavoriteLists();
        foreach (var fav in favoriteLists)
        {
            FavLists.Add(fav);
        }


        if (parameter is Anime anime)
        {
            SelectedAnime = await _searchAnimeService.GetAnimeDetailsAsync(anime);
            await checkFavorite();
            foreach (var chapter in SelectedAnime.Chapters.OrderByDescending((a) => a.ChapterNumber))
            {
                ChapterList.Add(chapter);
            }

        }
        IsLoadingFav = false;
    }




    public void OnNavigatedFrom()
    {
    }


    public async void OpenPlayer(Chapter chapter)
    {
        IsLoadingVideo = true;
        try
        {
            var videoSources = await _searchAnimeService.GetVideoSources(chapter.Url, SelectedAnime.Provider);
            var videoUrl = await _selectSourceService.SelectSourceAsync(videoSources, "YourUpload");
            var history = await _databaseService.GetOrCreateHistoryByCap(chapter.Id); 
            dynamic data= new ExpandoObject();
            data.History=history;
            data.Url=videoUrl;
            data.Chapter=chapter;

            if (string.IsNullOrEmpty(videoUrl))
            {
                throw new Exception(ErrorMessage = "Can't extract the video URL");
            }
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
        var res = await _databaseService.AddToFavorites(SelectedAnime, action);

        IsFavorite = res == "added" ? true : false;
        FavStatus = IsFavorite ? "\uE8D9" : "\uE728";
        FavText = IsFavorite ? "Remove from Favorites" : "Add to Favorites";
        IsLoadingFav = false;
    }

    private async Task checkFavorite()
    {
        IsFavorite = await _databaseService.IsFavorite(SelectedAnime.Id);
        // Icon TO FAV "\uE728"
        // Icon TO UNFAV "\uE8D9"  
        FavStatus = IsFavorite ? "\uE8D9" : "\uE728";
        FavText = IsFavorite ? "Remove from Favorites" : "Add to Favorites";
    }


    public static void ReverseObservableCollection<T>(ObservableCollection<T> collection)
    {
        for (int i = 0, j = collection.Count - 1; i < j; i++, j--)
        {
            var temp = collection[i];
            collection[i] = collection[j];
            collection[j] = temp;
        }
    }
    [RelayCommand]
    private async void ChangeFavLists(object param)
    {
        var idList = new List<int>();

        if (param is ListBox box)
        {
            foreach (var item in box.SelectedItems)
            {

                if (item is FavoriteList lt)
                {
                    idList.Add(lt.Id);
                }

            }
        }
        if (idList.Count > 0)
        {
            await _databaseService.UpdateAnimeList(SelectedAnime.Id, idList);

        }

    }
}
