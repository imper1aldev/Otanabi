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

    private ObservableMediaPlayerWrapper mediaPlayerWrapper;
    private Visibility controlsVisibility;

    private readonly DispatcherTimer controlsHideTimer = new()
    {
        Interval = TimeSpan.FromSeconds(1),
    };


    public VideoPlayerViewModel(INavigationService navigationService,IWindowPresenterService windowPresenterService)
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

    public ObservableMediaPlayerWrapper MediaPlayerWrapper
    {
        get => mediaPlayerWrapper;
        set => SetProperty(ref mediaPlayerWrapper, value);
    }
    public Visibility ControlsVisibility
    {
        get => controlsVisibility;
        set => SetProperty(ref controlsVisibility, value);
    }
    public bool IsNotFullScreen => !_windowPresenterService.IsFullScreen;


    public void OnNavigatedTo(object parameter) {
     
        if( parameter is string url)
        {
            VideoUrl= url;
        }
     }
    public void OnNavigatedFrom(){
        
        Dispose();
        }

    //end getters and setters


    // relay commands
    [RelayCommand]
    private void Initialized(InitializedEventArgs eventArgs)
    {
        if (VideoUrl == "Empty")
        {
            Debug.WriteLine("Skipping LibVLC initialization, because no media file specified.");
            return;
        }

        Debug.WriteLine("Initializing LibVLC");

        LibVLC = new LibVLC(true, eventArgs.SwapChainOptions);
        Player = new MediaPlayer(LibVLC);

        var media = new Media(LibVLC,new Uri(VideoUrl));
        Player.Play(media);
        Debug.WriteLine("Starting playback of '{0}'", VideoUrl);

        MediaPlayerWrapper = new ObservableMediaPlayerWrapper(Player, _dispatcherQueue);
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

    [RelayCommand]
    private void PlayPause()
    {
        MediaPlayerWrapper?.PlayPause();
    }

    [RelayCommand]
    private void Stop()
    {
        MediaPlayerWrapper?.Stop();
    }

    [RelayCommand]
    private void Mute()
    {
        MediaPlayerWrapper?.Mute();
    }

    [RelayCommand]
    private void FullScreen()
    {
        _windowPresenterService.ToggleFullScreen();
    }

    [RelayCommand]
    private void VolumeDown()
    {
        MediaPlayerWrapper?.VolumeDown();
    }

    [RelayCommand]
    private void VolumeUp()
    {
        MediaPlayerWrapper?.VolumeUp();
    }

    [RelayCommand]
    private void ScrollChanged(PointerRoutedEventArgs args)
    {
        var delta = args.GetCurrentPoint(null).Properties.MouseWheelDelta;
        if (delta > 0)
        {
            MediaPlayerWrapper?.VolumeUp();
        }
        else
        {
            MediaPlayerWrapper?.VolumeDown();
        }
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
                case VirtualKeyModifiers.Menu://10s
                    MediaPlayerWrapper?.FastForward(RewindMode.Normal);
                    break;
                case VirtualKeyModifiers.Control://60s
                    MediaPlayerWrapper?.FastForward(RewindMode.Long);
                    break;
                case VirtualKeyModifiers.Shift://3s
                    MediaPlayerWrapper?.FastForward(RewindMode.Short);
                    break;
            }
            keyboardAcceleratorInvokedEventArgs.Handled = true;
        }
        else
        {
            MediaPlayerWrapper?.FastForward(RewindMode.Normal);
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
                case VirtualKeyModifiers.Menu://10s
                    MediaPlayerWrapper?.Rewind(RewindMode.Normal);
                    break;
                case VirtualKeyModifiers.Control://60s
                    MediaPlayerWrapper?.Rewind(RewindMode.Long);
                    break;
                case VirtualKeyModifiers.Shift://3s
                    MediaPlayerWrapper?.Rewind(RewindMode.Short);
                    break;
            }
            keyboardAcceleratorInvokedEventArgs.Handled = true;
        }
        else
        {
            MediaPlayerWrapper?.Rewind(RewindMode.Normal);
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
}
