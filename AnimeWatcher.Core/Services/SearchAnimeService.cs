using AnimeWatcher.Core.Models;
using AnimeWatcher.Core.Helpers;
namespace AnimeWatcher.Core.Services;
public class SearchAnimeService
{
    private readonly ClassReflectionHelper _classReflectionHelper = new();  

    public Provider[] GetProviders()
    {
        return  _classReflectionHelper.GetProviders();
    }

    public async Task<Anime[]> MainPageAsync(Provider provider,int page=1)
    {
        var rexflex = _classReflectionHelper.GetMethodFromProvider("MainPageAsync", provider);
        var method = rexflex.Item1;
        var instance = rexflex.Item2;
        var animesTmp = await (Task<Anime[]>)method.Invoke(instance, new object[] { page });

        return animesTmp.ToArray();
    }

    public async Task<Anime[]> SearchAnimeAsync(string searchTerm,int page, Provider provider)
    {
        var rexflex = _classReflectionHelper.GetMethodFromProvider("SearchAnimeAsync", provider);
        var method = rexflex.Item1;
        var instance = rexflex.Item2;
        var animesTmp = await (Task<Anime[]>)method.Invoke(instance, new object[] { searchTerm , page });

        return animesTmp.ToArray();
    }
    public async Task<Anime> GetAnimeDetailsAsync(Anime animeReq)
    {
        var rexflex = _classReflectionHelper.GetMethodFromProvider("GetAnimeDetailsAsync", animeReq.Provider);
        var method = rexflex.Item1;
        var instance = rexflex.Item2;
        var animesDet = await (Task<Anime>)method.Invoke(instance, new object[] { animeReq.Url });

        /*
        var tmp = await DBservice.CreateMinimalAnime(animesDet);
        animesDet.Id=tmp.Id;
        animesDet.Chapters = tmp.Chapters;
        */

        return animesDet;
    }
    public async Task<VideoSource[]> GetVideoSources(string requestUrl, Provider provider)
    {
        var rexflex = _classReflectionHelper.GetMethodFromProvider("GetVideoSources", provider);
        var method = rexflex.Item1;
        var instance = rexflex.Item2;
        var videoSources = await (Task<VideoSource[]>)method.Invoke(instance, new object[] { requestUrl });

        return videoSources.ToArray();
    }

}
