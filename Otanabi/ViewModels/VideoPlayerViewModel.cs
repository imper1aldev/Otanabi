﻿using System.Collections.ObjectModel;
using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Otanabi.Contracts.Services;
using Otanabi.Contracts.ViewModels;
using Otanabi.Converters;
using Otanabi.Core.Models;
using Otanabi.Core.Services;
using Otanabi.Models.Enums;
using Otanabi.UserControls;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.System;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;

namespace Otanabi.ViewModels;

public partial class VideoPlayerViewModel : ObservableRecipient, INavigationAware
{
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly INavigationService _navigationService;
    private readonly IWindowPresenterService _windowPresenterService;
    private readonly WindowEx _windowEx;

    private Chapter selectedChapter;
    private History selectedHistory;
    private Provider selectedProvider;

    private static System.Timers.Timer? MainTimerForSave;
    private static System.Timers.Timer? RewindTimer;
    private static System.Timers.Timer? FastTimer;

    private bool IsFastTimerRunning = false;
    private bool IsRewindTimerRunning = false;
    private readonly int forwardTime = 1000;

    private const int rewindOffset10s = 10;
    private const int rewindOffset3s = 3;
    private const int rewindOffset60s = 60;

    private readonly DatabaseService dbService = new();
    private readonly SearchAnimeService _searchAnimeService = new();
    private readonly SelectSourceService _selectSourceService = new();
    private readonly LoggerService logger = new();

    private MediaSource videoUrl;
    private MediaPlaybackItem MpItem;
    private MediaPlayerElement MPE;
    private readonly string AppCurTitle = "";
    private bool IsPaused = false;
    private bool IsDisposed = false;
    public ObservableCollection<Chapter> ChapterList { get; } = [];

    [ObservableProperty]
    public ObservableCollection<string> servers = [];

    [ObservableProperty]
    private string selectedServer;

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

    [ObservableProperty]
    private bool fastVisible = false;

    [ObservableProperty]
    private bool rewindVisible = false;

    [ObservableProperty]
    private bool loadingVideo = false;

    private string animeTitle = "";

    private bool activeCc = false;

    private DateTime _lastClickTime;
    private const int DoubleClickThreshold = 200;
    private DateTime _lastChangedCap;
    private const int ChangeChapThreshold = 2000;

    public VideoPlayerViewModel(INavigationService navigationService, IWindowPresenterService windowPresenterService)
    {
        _navigationService = navigationService;
        _windowPresenterService = windowPresenterService;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        _windowPresenterService = windowPresenterService;
        _windowPresenterService.WindowPresenterChanged += OnWindowPresenterChanged;

        _windowEx = App.MainWindow;
        AppCurTitle = _windowEx.Title;
        //each 4 seconds it will save the current play time
        MainTimerForSave = new System.Timers.Timer(4000);
        MainTimerForSave.AutoReset = true;
        MainTimerForSave.Enabled = true;
        MainTimerForSave.Elapsed += SaveProgressByTime;

        /* rewind and fastforward timer definitions*/

        RewindTimer = new System.Timers.Timer(forwardTime);
        RewindTimer.AutoReset = false;
        RewindTimer.Elapsed += HideRewind;

        FastTimer = new System.Timers.Timer(forwardTime);
        FastTimer.AutoReset = false;
        FastTimer.Elapsed += HideFast;

        /* END*/
    }

    private void HideRewind(object source, ElapsedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            RewindVisible = false;
            if (RewindTimer != null)
            {
                RewindTimer.Stop();
                IsRewindTimerRunning = false;
            }
        });
    }

    private void HideFast(object source, ElapsedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            FastVisible = false;
            if (FastTimer != null)
            {
                FastTimer.Stop();
                IsFastTimerRunning = false;
            }
        });
    }

    private void SaveProgressByTime(object source, ElapsedEventArgs e)
    {
        if (selectedHistory != null)
        {
            try
            {
                _dispatcherQueue.TryEnqueue(async () =>
                {
                    if (MPE != null && MPE.MediaPlayer != null)
                    {
                        var seconds = (long)MPE.MediaPlayer.PlaybackSession.Position.TotalSeconds;
                        await dbService.UpdateProgress(selectedHistory.Id, seconds);
                        //Debug.WriteLine($"Progres ssaved seconds {seconds}  transformed: {TimeSpan.FromSeconds(seconds).ToString(@"\.hh\:mm\:ss")}");
                    }
                });
            }
            catch (Exception ex)
            {
                logger.LogError("UpdateProgress error :", ex.Message);
            }
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

        animeTitle = param.AnimeTitle;
        if (
            (param.History is History hs)
            && param.Chapter is Chapter ch
            && param.Provider is Provider prov
            && param.ChapterList is List<Chapter> chapters
        )
        {
            if (hs.Id != 0)
            {
                selectedHistory = hs;
            }

            selectedProvider = prov;
            selectedChapter = ch;
            foreach (Chapter chapter in chapters)
            {
                ChapterList.Add(chapter);
            }
        }
    }

    private async void SetStateForNexAndPrev()
    {
        if (selectedChapter.ChapterNumber == 1)
        {
            IsEnabledPrev = false;
            IsEnabledNext = true;
        }
        else
        {
            var maxChap = ChapterList.Max(c => c.ChapterNumber);
            if (selectedChapter.ChapterNumber == maxChap)
            {
                IsEnabledNext = false;
                IsEnabledPrev = true;
            }
            else
            {
                IsEnabledPrev = true;
                IsEnabledNext = true;
            }
        }
        _dispatcherQueue.TryEnqueue(() =>
        {
            OnPropertyChanged(nameof(IsEnabledPrev));
            OnPropertyChanged(nameof(IsEnabledNext));
        });
    }

    private async Task LoadVideo(Chapter chapter)
    {
        var prevVolume = 0.3;

        App.AppState.TryGetValue("Volume", out var stateVolume);
        if (stateVolume != null)
        {
            prevVolume = (double)stateVolume;
        }

        var isMuted = false;
        if (MPE.MediaPlayer != null)
        {
            prevVolume = MPE.MediaPlayer.Volume;
            isMuted = MPE.MediaPlayer.IsMuted;
            MPE.Source = null;
            MpItem = null;
            IsPaused = true;
            GC.Collect();
        }

        IsErrorVideo = false;
        LoadingVideo = true;
        if (App.AppState.TryGetValue("Incognito", out var incognito) && (bool)incognito)
        {
            selectedHistory = null;
        }
        else
        {
            selectedHistory = await dbService.GetOrCreateHistoryByCap(chapter.Id);
        }

        SelectedIndex = selectedChapter.ChapterNumber - 1;
        var videoSources = await _searchAnimeService.GetVideoSources(chapter.Url, selectedProvider);
        if (videoSources != null && (videoSources.FirstOrDefault(e => e.Server == SelectedServer) ?? videoSources[0]) is VideoSource item)
        {
            SelectedServer = item.Server;
        }
        //(streamUrl, subUrl, headers)
        var data = await _selectSourceService.SelectSourceAsync(videoSources, SelectedServer);

        if (data != null && data.Server != SelectedServer)
        {
            SelectedServer = data.Server;
        }

        activeCc = data.Subtitles.Count != 0;

        ChapterName = $"{animeTitle}  Ep# {chapter.ChapterNumber}";
        if (!string.IsNullOrEmpty(data.StreamUrl))
        {
            VideoUrl = MediaSource.CreateFromUri(new Uri(data.StreamUrl));
            if (MPE != null)
            {
                MpItem = new MediaPlaybackItem(VideoUrl);
                LoadServers();
                if (activeCc)
                {
                    try
                    {
                        foreach (var track in data.Subtitles)
                        {
                            var srtPath = await AssSubtitleSource.SaveSrtToTempFolderAsync(track.File);
                            var timedTextSource = TimedTextSource.CreateFromUri(new Uri(srtPath));
                            timedTextSource.Resolved += (sender, args) =>
                            {
                                if (args.Error != null)
                                {
                                    logger.LogInfo(message: $"Error on timed text source for track {track.Label}: {args.Error}");
                                }
                                foreach (var timedTrack in args.Tracks)
                                {
                                    timedTrack.Label = string.IsNullOrEmpty(timedTrack.Label) ? track.Label : timedTrack.Label;
                                }
                            };
                            MpItem.Source.ExternalTimedTextSources.Add(timedTextSource);
                        }
                        MpItem.TimedMetadataTracksChanged += (sender, args) =>
                        {
                            MpItem.TimedMetadataTracks.SetPresentationMode(0, TimedMetadataTrackPresentationMode.PlatformPresented);
                        };
                    }
                    catch (Exception e)
                    {
                        logger.LogError("Could not load CC or set the CC ", e.Message.ToString());
                    }
                }
                _dispatcherQueue.TryEnqueue(() =>
                {
                    // MPE.MediaPlayer = new MediaPlayer();
                    if (!IsDisposed)
                    {
                        MPE.SetMediaPlayer(
                            new MediaPlayer()
                            {
                                AutoPlay = true,
                                Source = MpItem,
                                Volume = prevVolume,
                                IsMuted = isMuted,
                            }
                        );
                        MPE.Source = MpItem;
                        if (MPE.MediaPlayer != null)
                        {
                            MPE.MediaPlayer.VolumeChanged += (sender, args) =>
                            {
                                _dispatcherQueue.TryEnqueue(() =>
                                {
                                    if (!IsDisposed && MPE != null && MPE.MediaPlayer != null)
                                    {
                                        App.AppState["Volume"] = MPE.MediaPlayer.Volume;
                                    }
                                });
                            };
                            MPE.MediaPlayer.MediaOpened += (sender, args) =>
                            {
                                _dispatcherQueue.TryEnqueue(() =>
                                {
                                    SetStateForNexAndPrev();
                                    LoadingVideo = false;
                                });
                            };
                            MPE.MediaPlayer.Play();
                            if (MPE.MediaPlayer.CanSeek && selectedHistory.SecondsWatched > 0)
                            {
                                seekTo(selectedHistory.SecondsWatched - 2);
                            }
                        }
                    }
                });
                IsPaused = false;
            }
        }
        else
        {
            IsErrorVideo = true;
            IsPaused = true;
            LoadingVideo = false;
        }
        _windowEx.Title = ChapterName;
    }

    private async Task LoadServers()
    {
        try
        {
            var videoSources = await _searchAnimeService.GetVideoSources(selectedChapter.Url, selectedProvider);
            Servers.Clear();
            foreach (var source in videoSources)
            {
                if (!string.IsNullOrEmpty(source.Server))
                {
                    Servers.Add(source.Server);
                }
            }

            if (MPE?.TransportControls is AnimeMediaTransportControls controls)
            {
                controls.UpdateServers(Servers, SelectedServer);
            }
        }
        catch (Exception e)
        {
            logger.LogError("Could not load servers ", e.Message.ToString());
        }
    }

    public void OnNavigatedFrom()
    {
        Dispose();
    }

    [RelayCommand]
    private async Task SelectServer(string server)
    {
        if (string.IsNullOrEmpty(server))
        {
            return;
        }

        SelectedServer = server;
        await LoadVideo(selectedChapter);
    }

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

    [ObservableProperty]
    private bool isEnabledPrev = true;

    [ObservableProperty]
    private bool isEnabledNext = true;

    [RelayCommand]
    private void FullScreen()
    {
        _windowPresenterService.ToggleFullScreen();
    }

    [RelayCommand]
    private void ClickFull()
    {
        var now = DateTime.Now;
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
            MPE.MediaPlayer.PlaybackSession.Position += TimeSpan.FromSeconds(79);
        }
        catch (Exception)
        {
            logger.LogInfo("current time cannot be skipped");
        }
    }

    [RelayCommand]
    private async Task NextChapter()
    {
        if (IsEnabledNext && LoadingVideo == false)
        {
            var next = selectedChapter.ChapterNumber + 1;
            await ChangeChapterByPos(next);
        }
    }

    [RelayCommand]
    private async Task PrevChapter()
    {
        if (IsEnabledPrev && LoadingVideo == false)
        {
            var prev = selectedChapter.ChapterNumber - 1;
            await ChangeChapterByPos(prev);
        }
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
            MPE.MediaPlayer.Play();
            IsPaused = false;
        }
        else
        {
            MPE.MediaPlayer.Pause();
            IsPaused = true;
        }
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
        if (FastTimer != null)
        {
            if (IsFastTimerRunning)
            {
                FastTimer.Interval = forwardTime;
            }
            else
            {
                FastVisible = true;
                FastTimer.Start();
                IsFastTimerRunning = true;
            }
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
        if (MPE != null && MPE.MediaPlayer != null)
        {
            MPE.MediaPlayer.PlaybackSession.Position += TimeSpan.FromSeconds(offset);
        }
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
        if (RewindTimer != null)
        {
            if (IsFastTimerRunning)
            {
                RewindTimer.Interval = forwardTime;
            }
            else
            {
                RewindVisible = true;
                RewindTimer.Start();
                IsRewindTimerRunning = true;
            }
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
        if (MPE != null && MPE.MediaPlayer != null)
        {
            if (MPE.MediaPlayer.PlaybackSession.Position > TimeSpan.FromSeconds(offset + 1))
            {
                MPE.MediaPlayer.PlaybackSession.Position -= TimeSpan.FromSeconds(offset);
            }
        }
    }

    public void setMediaPlayer(MediaPlayerElement mediaPlayerElement)
    {
        MPE = mediaPlayerElement;
    }

    public void seekTo(long msec)
    {
        try
        {
            MPE.MediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(msec);
        }
        catch (Exception)
        {
            logger.LogInfo("current time cannot be seek");
        }
    }

    private void OnWindowPresenterChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(IsNotFullScreen));
    }

    public void Dispose()
    {
        _windowEx.Title = AppCurTitle;
        IsDisposed = true;
        _windowPresenterService.WindowPresenterChanged -= OnWindowPresenterChanged;
        _dispatcherQueue.TryEnqueue(() =>
        {
            MainTimerForSave.Stop();
            MainTimerForSave.Dispose();
            MPE.Source = null;
            if (MPE.MediaPlayer != null)
            {
                MPE.MediaPlayer.Pause();
                MPE.MediaPlayer.Source = null;
            }
            //cant dispose the media player because it will crash the app
            GC.Collect();
        });
    }
}
