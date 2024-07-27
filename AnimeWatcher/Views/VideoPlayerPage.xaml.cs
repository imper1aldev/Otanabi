using AnimeWatcher.ViewModels;

using Microsoft.UI.Xaml.Controls;

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

    }
}
