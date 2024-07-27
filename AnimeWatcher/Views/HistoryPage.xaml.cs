using AnimeWatcher.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace AnimeWatcher.Views;

public sealed partial class HistoryPage : Page
{
    public HistoryViewModel ViewModel
    {
        get;
    }

    public HistoryPage()
    {
        ViewModel = App.GetService<HistoryViewModel>();
        InitializeComponent();
    }
}
