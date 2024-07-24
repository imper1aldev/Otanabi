using System.Collections.ObjectModel;
using System.Dynamic;
using AnimeWatcher.Contracts.Services;
using AnimeWatcher.Contracts.ViewModels;
using AnimeWatcher.Core.Models;
using AnimeWatcher.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AnimeWatcher.ViewModels;

public partial class HistoryViewModel : ObservableRecipient, INavigationAware
{
    private readonly DatabaseService dbService = new();
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    public Boolean errorActive = false;

    [ObservableProperty]
    public string errorMessage = "";


    private int currPage = 1;
    [ObservableProperty]
    private bool isLoading = false;
    [ObservableProperty]
    private bool isLoadingVideo = false;

    private bool noData = false;

    public ObservableCollection<History> Histories { get; } = new ObservableCollection<History>();


    public HistoryViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    public void OnNavigatedFrom()
    {
        noData=false;
        Histories.Clear();
        LoadHistory();
    }
    public void OnNavigatedTo(object parameter)
    {
        LoadHistory();
    }
    [RelayCommand]
    public async void LoadHistory()
    {
        if (IsLoading && noData)
            return;


        IsLoading = true;
        var history = await dbService.GetHistoriesAsync(currPage, 10);
        if (history != null)
        {
            foreach (var item in history)
            {
                Histories.Add(item);
            }
        }
        else
        {
            noData = true;
        }
        currPage++;
        IsLoading = false;
    }

    [RelayCommand]
    public async void PrepareVideo(History param)
    {
        if(IsLoadingVideo)
            return;


        var selectedHistory = param;
        var selectedChapter = param.Chapter;
        var selectedAnime = param.Chapter.Anime;

        var updatedAnime = await CheckAnimeUpdates(param.Chapter.Anime);

        if (updatedAnime != null)
        {
            selectedAnime = updatedAnime;

        }
        else
        {
            var chapList = await dbService.GetChaptersByAnime(selectedAnime.Id);
            selectedAnime.Chapters = chapList;
        }

        await OpenPlayer(selectedHistory, selectedChapter, selectedAnime);
    }
    private async Task<Anime> CheckAnimeUpdates(Anime request)
    {
        var anime = await dbService.UpsertAnime(request);
        if (anime != null)
        {
            anime.Chapters = anime.Chapters.OrderBy((a) => a.ChapterNumber).ToList();
        }
        return anime;
    }


    public async Task OpenPlayer(History history, Chapter selectedChapter, Anime selectedAnime)
    {
        IsLoadingVideo = true;
        try
        {
            dynamic data = new ExpandoObject();
            data.History = history;
            data.Chapter = selectedChapter;
            data.AnimeTitle = selectedAnime.Title;
            data.ChapterList = selectedAnime.Chapters.ToList();
            data.Provider = selectedAnime.Provider;
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
}
