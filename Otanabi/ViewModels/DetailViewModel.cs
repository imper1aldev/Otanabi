using CommunityToolkit.Mvvm.ComponentModel;
using Otanabi.Contracts.ViewModels;
//using ZeroQL.Client;

using Otanabi.Core.AnilistModels;
using Otanabi.Core.Services;

namespace Otanabi.ViewModels;

public partial class DetailViewModel : ObservableRecipient, INavigationAware
{
    private AnilistService _anilistService = new();

    [ObservableProperty]
    private Media selectedMedia;

    public DetailViewModel() { }

    public void OnNavigatedFrom() { }

    public async void OnNavigatedTo(object parameter)
    {
        if (parameter is int id)
        {
            await LoadMediaAsync(id);
        }
        else
        {
            await LoadMediaAsync(136804);
        }
    }

    private async Task LoadMediaAsync(int id)
    {
        var data = await _anilistService.GetMediaAsync(id);
        SelectedMedia = data;
    }
}
