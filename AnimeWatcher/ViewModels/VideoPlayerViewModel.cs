using System.Collections.ObjectModel;
using System.Timers;
using AnimeWatcher.Contracts.Services;
using AnimeWatcher.Contracts.ViewModels;
using AnimeWatcher.Core.Models;
using AnimeWatcher.Core.Services;
using AnimeWatcher.Models.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Juro.Core.Models;
using LibVLCSharp.Platforms.Windows;
using LibVLCSharp.Shared;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;

namespace AnimeWatcher.ViewModels;

public partial class VideoPlayerViewModel : ObservableRecipient, INavigationAware
{
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly INavigationService _navigationService;
    private readonly IWindowPresenterService _windowPresenterService;
    private Chapter selectedChapter;
    private History selectedHistory;
    private Provider selectedProvider;
    public ObservableCollection<Chapter> ChapterList { get; } = new ObservableCollection<Chapter>();

    //private List<Chapter> chapterList;

    private static System.Timers.Timer MainTimerForSave;
    private readonly DatabaseService dbService = new();
    private readonly SearchAnimeService _searchAnimeService = new();
    private readonly SelectSourceService _selectSourceService = new();
    private LibVLC libVLC;
    private MediaPlayer mediaPlayer;
    private string videoUrl = "Empty";
    private bool controlsVisibility = true;
    private string AppCurTitle = "";

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
        new()
        {
            Interval = TimeSpan.FromSeconds(1),
        };

    public VideoPlayerViewModel(
        INavigationService navigationService,
        IWindowPresenterService windowPresenterService
    )
    {
        _navigationService = navigationService;
        _windowPresenterService = windowPresenterService;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        _windowPresenterService.WindowPresenterChanged += OnWindowPresenterChanged;
        _windowEx = App.MainWindow;
        AppCurTitle = _windowEx.Title;
        //each 3 seconds it will save the current play time
        MainTimerForSave = new System.Timers.Timer(3000);
        MainTimerForSave.Elapsed += SaveProgressByTime;
        MainTimerForSave.AutoReset = true;
        MainTimerForSave.Enabled = true;
    }

    private void OnWindowPresenterChanged(object? sender, EventArgs e)
    {
        if (sender is not IWindowPresenterService windowPresenter)
        {
            return;
        }

        if (windowPresenter.IsFullScreen)
        {
            controlsHideTimer.Tick += Timer_Tick;
        }
        else
        {
            controlsHideTimer.Stop();
            controlsHideTimer.Tick -= Timer_Tick;
            ShowControls();
        }

        OnPropertyChanged(nameof(IsNotFullScreen));
        OnPropertyChanged(nameof(ControlsVisibility));
        OnPropertyChanged(nameof(RowSpan));
    }

    private async void SaveProgressByTime(object source, ElapsedEventArgs e)
    {
        if (selectedHistory != null)
        {
            try
            {
                if (Player != null)
                {
                    await dbService.UpdateProgress(selectedHistory.Id, Player.Time);
                }
            } catch (Exception) { }
        }
    }

    //getters and setters
    private LibVLC? LibVLC
    {
        get => libVLC;
        set => libVLC = value;
    }
    public MediaPlayer? Player
    {
        get => mediaPlayer;
        set => SetProperty(ref mediaPlayer, value);
    }
    public string VideoUrl
    {
        get => videoUrl;
        set => SetProperty(ref videoUrl, value);
    }
    public int RowSpan => _windowPresenterService.IsFullScreen ? 2 : 1;
    public bool LoadPlayer => VideoUrl != "Empty";
    public bool ControlsVisibility
    {
        get => controlsVisibility;
        set => SetProperty(ref controlsVisibility, value);
    }
    public InputSystemCursor ProtectedCursor
    {
        get; private set;
    }
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
        if (Player is not null && LibVLC is not null)
        {
            Player.Play(null);
            IsErrorVideo = false;
            LoadingVideo = true;
            //ControlsVisibility = false;
            TimeLong = 0;
            selectedHistory = await dbService.GetOrCreateHistoryByCap(chapter.Id);
            SelectedIndex = selectedChapter.ChapterNumber - 1;
            var videoSources = await _searchAnimeService.GetVideoSources(
                chapter.Url,
                selectedProvider
            );
            var data = await _selectSourceService.SelectSourceAsync(videoSources);
            VideoUrl = data.Item1;
            activeCC = data.Item2;

            ChapterName = $"{animeTitle}  Ep# {chapter.ChapterNumber}";
            if (VideoUrl != "")
            {
                await LoadMediaAsync(LibVLC, Player, VideoUrl, selectedHistory,AddCC, activeCC);
                IsCCactive = !string.IsNullOrEmpty(activeCC) ;
            }
            else
            {
                IsErrorVideo = true;
            } 
            //ControlsVisibility = true;
            OnPropertyChanged(nameof(IsEnablePrev));
            OnPropertyChanged(nameof(IsEnableNext));
            _windowEx.Title = ChapterName;
            LoadingVideo = false;
        }
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
    private async Task Initialized(InitializedEventArgs eventArgs)
    {
        //if (VideoUrl == "Empty")
        //{
        //    return;
        //}
        LibVLC = new LibVLC(true, eventArgs.SwapChainOptions);
        Player = new MediaPlayer(LibVLC);

        // await LoadMediaAsync(LibVLC, Player, VideoUrl, selectedHistory);
        await LoadVideo(selectedChapter);
        AttachPlayerEvents(Player);
        await Task.CompletedTask;
    }

    private static async Task LoadMediaAsync(
        LibVLC lib,
        MediaPlayer Pp,
        string url,
        History lcHistory,
        Action vc ,
        string subtitle = ""
    )
    {
        await Task.Run(async () =>
        {
            var mediaOptions = new[] { ":network-caching=60000", ":live-caching=60000" };
            using var media = new Media(lib, new Uri(url), mediaOptions);

            Pp.Play(media);

            if (subtitle != "")
            {
                vc(); 
            }
            /*Recover for last seen
            if (lcHistory != null && lcHistory.SecondsWatched > 0)
            {
                Pp.Time = lcHistory.SecondsWatched;
            }*/
            await Task.CompletedTask;
        });
    }

    [RelayCommand]
    private void PointerMoved(PointerRoutedEventArgs? args)
    {
        if (_windowPresenterService.IsFullScreen)
        {
            if (ControlsVisibility == false)
            {
                ShowControls();
            }
            else
            {
                controlsHideTimer.Stop();
                controlsHideTimer.Start();
            }
        }
    }

    //end relay commands

    //Methods
    private void Timer_Tick(object? sender, object e)
    {
        HideControls();
        controlsHideTimer.Stop();
    }

    private void ShowControls()
    {
        ControlsVisibility = true; 
    }

    private void HideControls()
    {
        ControlsVisibility = false;
        //IsChapPanelOpen = false;
    }

    //end methods

    public void Dispose()
    {
        _windowEx.Title = AppCurTitle;
        MainTimerForSave.Stop();
        MainTimerForSave.Dispose();
        _dispatcherQueue.TryEnqueue(() =>
        {
            var mediaPlayer = Player;
            Player = null;
            mediaPlayer?.Dispose();
            LibVLC?.Dispose();
            LibVLC = null;

            GC.Collect();
        });
    }

    // player view related
    private const int rewindOffset10s = 10000;
    private const int rewindOffset3s = 3000;
    private const int rewindOffset60s = 60000;
    private int previousVolume;
    private const int volumeStep = 5;
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

    public long TimeLong
    {
        get => Player != null ? Player.Time : -1;
        set => SetProperty(Player.Time, value, Player, (u, n) => u.Time = n);
    }
    public string TimeString => TimeSpan.FromMilliseconds(TimeLong).ToString(@"hh\:mm\:ss");
    public long TotalTimeLong => Player != null ? Player.Length : -1;
    public string TotalTimeString =>
        TimeSpan.FromMilliseconds(TotalTimeLong).ToString(@"hh\:mm\:ss");
    public int Volume
    {
        get => Player != null ? Player.Volume : -1;
        set => SetProperty(Player.Volume, value, Player, (u, n) => u.Volume = n);
    }
    public bool IsPlaying => Player != null ? Player.IsPlaying : false;

    [RelayCommand]
    public void VolumeUp()
    {
        if (Volume <= 200)
        {
            Volume += volumeStep;
        }
    }

    [RelayCommand]
    public void VolumeDown()
    {
        if (Volume >= volumeStep)
        {
            Volume -= volumeStep;
        }
    }

    [RelayCommand]
    public void Mute()
    {
        if (Volume == 0)
        {
            Volume = previousVolume;
        }
        else
        {
            previousVolume = Volume;
            Volume = 0;
        }
    }

    [RelayCommand]
    public void PlayPause()
    {
        if (Player == null)
        {
            return;
        }

        if (!IsPlaying)
        {
            Player.Play();
        }
        else
        {
            Player.Pause();
        }
    }

    [RelayCommand]
    public void Stop()
    {
        if (Player == null)
        {
            return;
        }

        Player.Stop();
    }

    [RelayCommand]
    private void ScrollChanged(PointerRoutedEventArgs args)
    {
        //var delta = args.GetCurrentPoint(null).Properties.MouseWheelDelta;
        //if (delta > 0)
        //{
        //    VolumeUp();
        //}
        //else
        //{
        //    VolumeDown();
        //}
        args.Handled = true;
    }

    [RelayCommand]
    private void FastForward(object args)
    {
        if (args is KeyboardAcceleratorInvokedEventArgs keyboardAcceleratorInvokedEventArgs)
        {
            var modifier = keyboardAcceleratorInvokedEventArgs.KeyboardAccelerator.Modifiers;
            switch (modifier)
            {
                case VirtualKeyModifiers.None:
                case VirtualKeyModifiers.Menu: //10s
                    FastForwardInter(RewindMode.Normal);
                    break;
                case VirtualKeyModifiers.Control: //60s
                    FastForwardInter(RewindMode.Long);
                    break;
                case VirtualKeyModifiers.Shift: //3s
                    FastForwardInter(RewindMode.Short);
                    break;
            }
            keyboardAcceleratorInvokedEventArgs.Handled = true;
        }
        else
        {
            FastForwardInter(RewindMode.Normal);
        }
    }

    private void FastForwardInter(RewindMode mode)
    {
        var offset = mode switch
        {
            RewindMode.Normal => rewindOffset10s,
            RewindMode.Short => rewindOffset3s,
            RewindMode.Long => rewindOffset60s,
            _ => rewindOffset10s,
        };

        TimeLong += offset;
    }

    [RelayCommand]
    private void Rewind(object args)
    {
        if (args is KeyboardAcceleratorInvokedEventArgs keyboardAcceleratorInvokedEventArgs)
        {
            var modifier = keyboardAcceleratorInvokedEventArgs.KeyboardAccelerator.Modifiers;
            switch (modifier)
            {
                case VirtualKeyModifiers.None:
                case VirtualKeyModifiers.Menu: //10s
                    RewindInter(RewindMode.Normal);
                    break;
                case VirtualKeyModifiers.Control: //60s
                    RewindInter(RewindMode.Long);
                    break;
                case VirtualKeyModifiers.Shift: //3s
                    RewindInter(RewindMode.Short);
                    break;
            }
            keyboardAcceleratorInvokedEventArgs.Handled = true;
        }
        else
        {
            RewindInter(RewindMode.Normal);
        }
    }

    private void RewindInter(RewindMode mode)
    {
        var offset = mode switch
        {
            RewindMode.Normal => rewindOffset10s,
            RewindMode.Short => rewindOffset3s,
            RewindMode.Long => rewindOffset60s,
            _ => rewindOffset10s,
        };
        if (TimeLong > (offset + 1))
        {
            TimeLong -= offset;
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
        if (Player == null)
        {
            return;
        }

        var crtm = Player.Time;
        var skip = (long)TimeSpan.FromSeconds(79).TotalMilliseconds;
        Player.Time = crtm + skip;
    }

    [RelayCommand]
    private async void NextChapter()
    {
        var next = selectedChapter.ChapterNumber + 1;
        await ChangeChapterByPos(next);
    }

    [RelayCommand]
    private async void PrevChapter()
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
    private void ChangeSpeed(ComboBox param)
    {
        if (Player == null)
        {
            return;
        }

        var item = (ComboBoxItem)param.SelectedItem;
        Player.SetRate(float.Parse((string)item.Tag));
    }

    [RelayCommand]
    private void ToggleCC()
    {
        IsCCactive = !IsCCactive;
        if (Player != null)
        {
            var tt = Player.SetSpu(IsCCactive ? 0 : -1);
            if (!tt)
            {
                AddCC();
            }
        }
    }
    private void AddCC()
    {
        if (Player == null)
        {
            return;
        }
        if (string.IsNullOrEmpty(activeCC))
        {
            return;
        }
        Player.AddSlave(MediaSlaveType.Subtitle, activeCC, true);
        Player.SetSpu(0);
    }

    private void AttachPlayerEvents(MediaPlayer _player)
    {
        _player.TimeChanged += (sender, time) =>
            _dispatcherQueue.TryEnqueue(() =>
            {
                OnPropertyChanged(nameof(TimeLong));
                OnPropertyChanged(nameof(TimeString));
            });
        _player.VolumeChanged += (sender, volumeArgs) =>
            _dispatcherQueue.TryEnqueue(() =>
            {
                OnPropertyChanged(nameof(Volume));
            });
        _player.Playing += (sender, args) =>
            _dispatcherQueue.TryEnqueue(() =>
            {
                OnPropertyChanged(nameof(IsPlaying));
            });
        _player.Paused += (sender, args) =>
            _dispatcherQueue.TryEnqueue(() =>
            {
                OnPropertyChanged(nameof(IsPlaying));
            });
        _player.Stopped += (sender, args) =>
            _dispatcherQueue.TryEnqueue(() =>
            {
                OnPropertyChanged(nameof(IsPlaying));
            });

        _player.EndReached += (sender, args) =>
            _dispatcherQueue.TryEnqueue(() =>
            {
                OnPropertyChanged(nameof(IsPlaying));
            });

        _player.LengthChanged += (sender, args) =>
            _dispatcherQueue.TryEnqueue(() =>
            {
                OnPropertyChanged(nameof(TotalTimeLong));
                OnPropertyChanged(nameof(TotalTimeString));
            });
    }
    // end´player view related
}
