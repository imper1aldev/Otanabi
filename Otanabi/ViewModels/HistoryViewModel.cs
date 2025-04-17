using System.Collections.ObjectModel;
using System.Dynamic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Otanabi.Contracts.Services;
using Otanabi.Contracts.ViewModels;
using Otanabi.Core.Models;
using Otanabi.Core.Services;

namespace Otanabi.ViewModels;

public partial class HistoryViewModel : ObservableRecipient, INavigationAware
{
    private readonly DatabaseService dbService = new();
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    public Boolean errorActive = false;

    [ObservableProperty]
    public string errorMessage = "";

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

    public async void OnNavigatedFrom()
    {
        // Histories.Clear();
        //await LoadHistory();
    }

    public async void OnNavigatedTo(object parameter)
    {
        await LoadHistory();
    }

    [RelayCommand]
    public async Task LoadHistory()
    {
        if (IsLoading && noData)
            return;

        IsLoading = true;
        var history = await dbService.GetAllHistoriesAsync();
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
        IsLoading = false;
    }

    [RelayCommand]
    public async void PrepareVideo(History param)
    {
        if (IsLoadingVideo)
            return;

        IsLoadingVideo = true;

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
        try
        {
            dynamic data = new ExpandoObject();
            data.History = history;
            data.Chapter = selectedChapter;
            data.AnimeTitle = selectedAnime.Title;
            data.ChapterList = selectedAnime.Chapters.ToList();
            data.Provider = selectedAnime.Provider;
            _navigationService.NavigateTo(typeof(VideoPlayerViewModel).FullName!, data);
            IsLoadingVideo = false;
        }
        catch (Exception e)
        {
            IsLoadingVideo = false;
            ErrorMessage = e.Message.ToString();
            ErrorActive = true;
            return;
        }
    }

    [RelayCommand]
    public async Task DeleteHistoryById(int id)
    {
        var hs = Histories.Where(h => h.Id == id).First();
        Histories.Remove(hs);
        await dbService.DeleteFromHistory(id);
    }
}
