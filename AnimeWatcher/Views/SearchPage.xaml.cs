using AnimeWatcher.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace AnimeWatcher.Views;

public sealed partial class SearchPage : Page
{
    public SearchViewModel ViewModel
    {
        get;
    }

    public SearchPage()
    {
        ViewModel = App.GetService<SearchViewModel>();
        InitializeComponent();
    }
}
