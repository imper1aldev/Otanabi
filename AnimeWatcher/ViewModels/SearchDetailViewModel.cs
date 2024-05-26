using System.Collections.ObjectModel;
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
    private readonly DatabaseService _databaseService = new ();
    private readonly SelectSourceService _selectSourceService = new();
    private readonly INavigationService _navigationService;

    public ObservableCollection<Chapter> ChapterList { get; } = new ObservableCollection<Chapter>();

    [ObservableProperty]
    private Anime selectedAnime;

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
            SelectedAnime = await _searchAnimeService.GetAnimeDetailsAsync(anime); 
            foreach(var chapter in SelectedAnime.Chapters.OrderByDescending((a)=>a.ChapterNumber ) )
            {
                ChapterList.Add(chapter);
            } 

        }
    }




    public void OnNavigatedFrom()
    {
    }


    public async void OpenPlayer(Chapter chapter)
    {
        //try
        //{

        var videoSources = await _searchAnimeService.GetVideoSources(chapter.Url, SelectedAnime.Provider);
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
        OrderIcon = orderedList ?"\uE74B": "\uE74A" ;
        ChapterList.Clear();
        if (orderedList)
        {
            foreach (var chapter in SelectedAnime.Chapters.Reverse())
            {
                ChapterList.Add(chapter);
            }
        }else
        {
            foreach (var chapter in SelectedAnime.Chapters)
            {
                ChapterList.Add(chapter);
            }
        }
    }


    [RelayCommand]
    private async void FavoriteFun() {
        
       await _databaseService.AddToFavorites(SelectedAnime);
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
}
