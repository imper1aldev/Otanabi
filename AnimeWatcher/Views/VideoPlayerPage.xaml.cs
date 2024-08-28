using AnimeWatcher.ViewModels;

using Microsoft.UI.Xaml.Controls;
using Windows.Media.Playback;

namespace AnimeWatcher.Views;

public sealed partial class VideoPlayerPage : Page
{
    public VideoPlayerViewModel ViewModel
    {
        get;
    }

    public VideoPlayerPage()
    {
        ViewModel = App.GetService<VideoPlayerViewModel>();
        InitializeComponent();
         AMediaPlayer.Loaded += OnPlayerLoaded;
    }
     private void OnPlayerLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (AMediaPlayer != null)
        {
            ViewModel.setMediaPlayer(AMediaPlayer.MediaPlayer);
            ViewModel.InitializedCommand.Execute(null);

        }
    }
}
