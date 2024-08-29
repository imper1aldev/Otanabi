using System.Collections.ObjectModel;
using System.Timers;
using AnimeWatcher.Contracts.Services;
using AnimeWatcher.Contracts.ViewModels;
using AnimeWatcher.Core.Models;
using AnimeWatcher.Core.Services;
using AnimeWatcher.Models.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.System;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;

namespace AnimeWatcher.ViewModels;

public partial class VideoPlayerViewModel : ObservableRecipient, INavigationAware
{
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly INavigationService _navigationService;
    private readonly IWindowPresenterService _windowPresenterService;
    private readonly LoggerService logger = new();
    private Chapter selectedChapter;
    private History selectedHistory;
    private Provider selectedProvider;
    public ObservableCollection<Chapter> ChapterList { get; } = new ObservableCollection<Chapter>();

    //private List<Chapter> chapterList;

    private static System.Timers.Timer MainTimerForSave;
    private readonly DatabaseService dbService = new();
    private readonly SearchAnimeService _searchAnimeService = new();
    private readonly SelectSourceService _selectSourceService = new();

    private MediaSource videoUrl;
    private MediaPlayer ActiveMP;
    private string AppCurTitle = "";
    private bool IsPaused=false;

    [ObservableProperty]
    private bool isChapPanelOpen = false;

    [ObservableProperty]
    private bool isCCactive = false;

    [ObservableProperty]
    private bool isErrorVideo = false;

    [ObservableProperty]
    private int selectedIndex = 0;

    [ObservableProperty]
    private string chapterName = "";

    private string animeTitle = "";
    private string activeCC = "";

    private DateTime _lastClickTime;
    private const int DoubleClickThreshold = 200;
    private DateTime _lastChangedCap;
    private const int ChangeChapThreshold = 2000;

    private WindowEx _windowEx;

    //this thing will block the interface to prevent problems
    [ObservableProperty]
    private bool loadingVideo = false;

    private readonly DispatcherTimer controlsHideTimer =
        new() { Interval = TimeSpan.FromSeconds(1), };

    public VideoPlayerViewModel(
        INavigationService navigationService,
        IWindowPresenterService windowPresenterService
    )
    {
        _navigationService = navigationService;
        _windowPresenterService = windowPresenterService;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        _windowEx = App.MainWindow;
        AppCurTitle = _windowEx.Title;
        //each 4 seconds it will save the current play time
        MainTimerForSave = new System.Timers.Timer(4000);
        MainTimerForSave.Elapsed += SaveProgressByTime;
        MainTimerForSave.AutoReset = true;
        MainTimerForSave.Enabled = true;
    }

    private async void SaveProgressByTime(object source, ElapsedEventArgs e)
    {
        if (selectedHistory != null)
        {
            try
            {
                if (ActiveMP != null)
                {
                    await dbService.UpdateProgress(selectedHistory.Id,ActiveMP.PlaybackSession.Position.Ticks);
                }
            }
            catch (Exception) { }
        }
    }

    public MediaSource VideoUrl
    {
        get => videoUrl;
        set => SetProperty(ref videoUrl, value);
    }
    public int RowSpan => _windowPresenterService.IsFullScreen ? 2 : 1;
    public bool LoadPlayer => VideoUrl != null;
    public bool IsNotFullScreen => !_windowPresenterService.IsFullScreen;

    public void OnNavigatedTo(object parameter)
    {
        dynamic param = parameter as dynamic;

        //VideoUrl = param.Url;
        //ChapterName = param.ChapterName;
        animeTitle = param.AnimeTitle;
        if (
            param.History is History hs
            && param.Chapter is Chapter ch
            && param.Provider is Provider prov
            && param.ChapterList is List<Chapter> chapters
        )
        {
            selectedHistory = hs;
            selectedProvider = prov;
            selectedChapter = ch;
            foreach (Chapter chapter in chapters)
            {
                ChapterList.Add(chapter);
            }
        }
    }

    private async Task LoadVideo(Chapter chapter)
    {
        if (ActiveMP != null)
        {
            ActiveMP.Source = null;
            IsPaused=true;
        }

        await Task.Delay(200);

        IsErrorVideo = false;
        LoadingVideo = true;
        selectedHistory = await dbService.GetOrCreateHistoryByCap(chapter.Id);
        SelectedIndex = selectedChapter.ChapterNumber - 1;
        var videoSources = await _searchAnimeService.GetVideoSources(chapter.Url, selectedProvider);
        var data = await _selectSourceService.SelectSourceAsync(videoSources);

        activeCC = data.Item2;

        ChapterName = $"{animeTitle}  Ep# {chapter.ChapterNumber}";
        if (!string.IsNullOrEmpty(data.Item1))
        {
            VideoUrl = MediaSource.CreateFromUri(new Uri(data.Item1));
            IsCCactive = !string.IsNullOrEmpty(activeCC);
            if (ActiveMP != null)
            {
                ActiveMP.Source = VideoUrl;
                IsPaused=false;
            }
            await Task.Delay(200);
        }
        else
        {
            IsErrorVideo = true;
            IsPaused=true;
        }
        _windowEx.Title = ChapterName;
        LoadingVideo = false;
    }

    public void OnNavigatedFrom()
    {
        Dispose();
    }

    //end getters and setters


    // relay commands

    [RelayCommand]
    private async Task RetryLoad()
    {
        await LoadVideo(selectedChapter);
    }

    [RelayCommand]
    private async Task Initialized(RoutedEventArgs eventArgs)
    {
        await LoadVideo(selectedChapter);
        await Task.CompletedTask;
    }

    //end methods

   

    public bool IsEnablePrev => selectedChapter.ChapterNumber > 1;
    public bool IsEnableNext
    {
        get
        {
            var maxchap = ChapterList is null
                ? 1
                : ChapterList.MaxBy(x => x.ChapterNumber).ChapterNumber;

            return selectedChapter.ChapterNumber < maxchap;
        }
    }

    [RelayCommand]
    private void FullScreen()
    {
        _windowPresenterService.ToggleFullScreen();
    }

    [RelayCommand]
    private void ClickFull()
    {
        DateTime now = DateTime.Now;
        TimeSpan interval = now - _lastClickTime;

        if (interval.TotalMilliseconds <= DoubleClickThreshold)
        {
            _windowPresenterService.ToggleFullScreen();
        }

        _lastClickTime = now;
    }

    [RelayCommand]
    private void SkipIntro()
    {
        try
        {
            ActiveMP.PlaybackSession.Position += TimeSpan.FromSeconds(79);
        }
        catch (Exception)
        {
            logger.LogInfo("current time cannot be skipped");
        }
    }

    [RelayCommand]
    private async Task NextChapter()
    {
        var next = selectedChapter.ChapterNumber + 1;
        await ChangeChapterByPos(next);
    }

    [RelayCommand]
    private async Task PrevChapter()
    {
        var prev = selectedChapter.ChapterNumber - 1;
        await ChangeChapterByPos(prev);
    }

    private async Task ChangeChapterByPos(int pos)
    {
        DateTime now = DateTime.Now;
        TimeSpan interval = now - _lastChangedCap;

        if (interval.TotalMilliseconds > ChangeChapThreshold)
        {
            Chapter changedChap = ChapterList.FirstOrDefault(c => c.ChapterNumber == pos);
            if (changedChap != null)
            {
                selectedChapter = changedChap;
                await LoadVideo(selectedChapter);
            }
        }
        _lastChangedCap = now;
    }

    [RelayCommand]
    private void ToggleChapPanel()
    {
        IsChapPanelOpen = !IsChapPanelOpen;
    }

    [RelayCommand]
    private async void onClickChap(ItemClickEventArgs args)
    {
        if (args.ClickedItem is Chapter ct && ct.ChapterNumber != selectedChapter.ChapterNumber)
        {
            selectedChapter = ct;
            await LoadVideo(selectedChapter);
        }
    }

    [RelayCommand]
    private void onClosePane(dynamic test)
    {
        IsChapPanelOpen = false;
    }
    [RelayCommand]
    private void PlayPause()
    {
        if (IsPaused)
        {
            ActiveMP.Play();
            IsPaused=false;
        }
        else
        {
            ActiveMP.Pause();
            IsPaused=true;
        }
    }

    public void setMediaPlayer(MediaPlayer mediaPlayer)
    {
        ActiveMP = mediaPlayer;
    }


     public void Dispose()
    {
        _windowEx.Title = AppCurTitle;
        MainTimerForSave.Stop();
        MainTimerForSave.Dispose();
        ActiveMP?.Dispose();
        _dispatcherQueue.TryEnqueue(() =>
        {
            GC.Collect();
        });
    }
}
