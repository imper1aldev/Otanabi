using AnimeWatcher.Contracts.Services;
using AnimeWatcher.Core.Models;
using AnimeWatcher.ViewModels;

using CommunityToolkit.WinUI.UI.Animations;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace AnimeWatcher.Views;

public sealed partial class SearchDetailPage : Page
{
    public SearchDetailViewModel ViewModel
    {
        get;
    }

    public SearchDetailPage()
    {
        ViewModel = App.GetService<SearchDetailViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e); 
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);
        if (e.NavigationMode == NavigationMode.Back)
        {
            
        }
    }
    private void ListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem != null)
        {
            ViewModel.OpenPlayer((Chapter)e.ClickedItem);
        }
         
    }
}
