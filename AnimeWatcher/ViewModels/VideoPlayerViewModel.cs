using System.Diagnostics;
using AnimeWatcher.Contracts.Services;
using AnimeWatcher.Contracts.ViewModels;
using AnimeWatcher.Models.Enums;
using AnimeWatcher.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibVLCSharp.Platforms.Windows;
using LibVLCSharp.Shared;
using Microsoft.UI.Xaml;
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

    private LibVLC libVLC;
    private MediaPlayer mediaPlayer;
    private string videoUrl = "Empty";

    //private ObservableMediaPlayerWrapper mediaPlayerWrapper;
    private Visibility controlsVisibility;

    private readonly DispatcherTimer controlsHideTimer = new()
    {
        Interval = TimeSpan.FromSeconds(1),
    };


    public VideoPlayerViewModel(INavigationService navigationService, IWindowPresenterService windowPresenterService)
    {
        _navigationService = navigationService;
        _windowPresenterService = windowPresenterService;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();



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

    //public ObservableMediaPlayerWrapper MediaPlayerWrapper
    //{
    //    get => mediaPlayerWrapper;
    //    set => SetProperty(ref mediaPlayerWrapper, value);
    //}
    public Visibility ControlsVisibility
    {
        get => controlsVisibility;
        set => SetProperty(ref controlsVisibility, value);
    }
    public bool IsNotFullScreen => !_windowPresenterService.IsFullScreen;


    public void OnNavigatedTo(object parameter)
    {

        if (parameter is string url)
        {
            VideoUrl = url;
        }
    }
    public void OnNavigatedFrom()
    {

        Dispose();
    }

    //end getters and setters


    // relay commands
    [RelayCommand]
    private async void Initialized(InitializedEventArgs eventArgs)
    {
        if (VideoUrl == "Empty")
        {
            Debug.WriteLine("Skipping LibVLC initialization, because no media file specified.");
            return;
        }

        Debug.WriteLine("Initializing LibVLC");

        LibVLC = new LibVLC(true, eventArgs.SwapChainOptions);
        Player = new MediaPlayer(LibVLC);

        //var mediaOptions = new[] { ":network-caching=3000", ":live-caching=3000" };
        //var media = new Media(LibVLC, new Uri(VideoUrl), mediaOptions);
        //Player.Play(media);
        //Debug.WriteLine("Starting playback of '{0}'", VideoUrl);
        await LoadMediaAsync(LibVLC, Player, VideoUrl);

        AttachPlayerEvents(Player);
        await Task.CompletedTask;
        //MediaPlayerWrapper = new ObservableMediaPlayerWrapper(Player, _dispatcherQueue);
    }
    private static async Task LoadMediaAsync(LibVLC lib, MediaPlayer Pp, string url)
    {
        await Task.Run(() =>
        {

            var mediaOptions = new[] { ":network-caching=3000", ":live-caching=3000" };
            var media = new Media(lib, new Uri(url), mediaOptions);
            Pp.Play(media);
            Debug.WriteLine("Starting playback of '{0}'", url);
        });
    }


    [RelayCommand]
    private void PointerMoved(PointerRoutedEventArgs? args)
    {
        if (_windowPresenterService.IsFullScreen)
        {
            if (ControlsVisibility == Visibility.Collapsed)
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
        ControlsVisibility = Visibility.Visible;
        Debug.WriteLine("Showing controls");
    }

    private void HideControls()
    {
        ControlsVisibility = Visibility.Collapsed;
        Debug.WriteLine("Hiding controls");
    }

    //end methods

    public void Dispose()
    {
        var mediaPlayer = Player;
        Player = null;
        mediaPlayer?.Dispose();
        LibVLC?.Dispose();
        LibVLC = null;
    }


    // player view related
    private const int rewindOffset10s = 10000;
    private const int rewindOffset3s = 3000;
    private const int rewindOffset60s = 60000;
    private int previousVolume;
    private const int volumeStep = 5;
    public long TimeLong
    {
        get => Player != null ? Player.Time : -1;
        set => SetProperty(Player.Time, value, Player, (u, n) => u.Time = n);
    }

    public string TimeString => TimeSpan.FromMilliseconds(TimeLong).ToString(@"hh\:mm\:ss");
    public long TotalTimeLong => Player != null ? Player.Length : -1;

    public string TotalTimeString => TimeSpan.FromMilliseconds(TotalTimeLong).ToString(@"hh\:mm\:ss");

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
        Player.Stop();
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
                case VirtualKeyModifiers.Menu://10s
                    FastForwardInter(RewindMode.Normal);
                    break;
                case VirtualKeyModifiers.Control://60s
                    FastForwardInter(RewindMode.Long);
                    break;
                case VirtualKeyModifiers.Shift://3s
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
                case VirtualKeyModifiers.Menu://10s
                    RewindInter(RewindMode.Normal);
                    break;
                case VirtualKeyModifiers.Control://60s
                    RewindInter(RewindMode.Long);
                    break;
                case VirtualKeyModifiers.Shift://3s
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

        TimeLong -= offset;
    }
    [RelayCommand]
    private void FullScreen()
    {
        _windowPresenterService.ToggleFullScreen();
    }

    private void AttachPlayerEvents(MediaPlayer _player)
    {
        //_player.TimeChanged += (sender, time) => _dispatcherQueue.TryEnqueue(() =>
        //{
        //    OnPropertyChanged(nameof(TimeLong));
        //    OnPropertyChanged(nameof(TimeString));
        //});
        _player.TimeChanged += (sender, time) =>
        {
            try
            {
                _dispatcherQueue.TryEnqueue(() =>{
                    OnPropertyChanged(nameof(TimeLong));
                    OnPropertyChanged(nameof(TimeString));
                });
            } catch (AccessViolationException ex)
            {
                 
            }
        };



        _player.VolumeChanged += (sender, volumeArgs) => _dispatcherQueue.TryEnqueue(() =>
        {
            OnPropertyChanged(nameof(Volume));
        });

        _player.Playing += (sender, args) => _dispatcherQueue.TryEnqueue(() =>
        {
            OnPropertyChanged(nameof(IsPlaying));
        });

        _player.Paused += (sender, args) => _dispatcherQueue.TryEnqueue(() =>
        {
            OnPropertyChanged(nameof(IsPlaying));
        });

        _player.Stopped += (sender, args) => _dispatcherQueue.TryEnqueue(() =>
        {
            OnPropertyChanged(nameof(IsPlaying));
        });

        _player.EndReached += (sender, args) => _dispatcherQueue.TryEnqueue(() =>
        {
            OnPropertyChanged(nameof(IsPlaying));
        });

        _player.LengthChanged += (sender, args) => _dispatcherQueue.TryEnqueue(() =>
        {
            OnPropertyChanged(nameof(TotalTimeLong));
            OnPropertyChanged(nameof(TotalTimeString));
        });
    }
    // end´player view related
}
