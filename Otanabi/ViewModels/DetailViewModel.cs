using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Otanabi.Contracts.ViewModels;
//using ZeroQL.Client;

using Otanabi.Core.AnilistModels;
using Otanabi.Core.Services;
using Windows.System;

namespace Otanabi.ViewModels;

public partial class DetailViewModel : ObservableRecipient, INavigationAware
{
    private AnilistService _anilistService = new();
    private readonly DispatcherQueue _dispatcherQueue;

    [ObservableProperty]
    private Media selectedMedia;

    [ObservableProperty]
    private BitmapImage bannerImage;

    [ObservableProperty]
    private string link;

    [ObservableProperty]
    private bool isLoaded=false;

    public DetailViewModel() {
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();}

    public void OnNavigatedFrom() { }

    public async void OnNavigatedTo(object parameter)
    {
        if (parameter is Media media)
        {
            await LoadMediaAsync(media.Id);
        }
        else
        {
            await LoadMediaAsync(175443);
        }
    }

    private async Task LoadMediaAsync(int id)
    {
        var data = await _anilistService.GetMediaAsync(id);
        if(data.BannerImage != null)
        { 
            BannerImage =new BitmapImage(new Uri(data.BannerImage));
        }
        else
        { 
            BannerImage =new BitmapImage(new Uri(data.CoverImage.ExtraLarge));
        }

        Link =$"https://anilist.co/anime/{data.Id}";
        SelectedMedia = (Media)data;
        isLoaded=true;
    } 

     [RelayCommand]
    public void OpenLink()
    {
        _dispatcherQueue.TryEnqueue(async() => await Launcher.LaunchUriAsync(new Uri(Link)));
    }
}
