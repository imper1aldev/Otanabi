using System.Diagnostics;
using System.Reflection;
using AnimeWatcher.Core.Contracts.Extractors;
using AnimeWatcher.Core.Models;
using AnimeWatcher.Core.Helpers;
using ScrapySharp.Html.Parsing;
namespace AnimeWatcher.Core.Services;
public class SearchAnimeService
{
    private readonly ClassReflectionHelper _classReflectionHelper = new();

    public Provider[] GetProviders()
    {
        var providers = new List<Provider>();
        var namesp = "AnimeWatcher.Core.Extractors";

        var data = _classReflectionHelper.ExtractAssembliesOnlyClass(namesp);
        foreach (var cls in data)
        {
            var props = _classReflectionHelper.GetProviderClassName(cls.FullName);
            providers.Add(new Provider { Name = props.Item1, Url = props.Item2 });
        }
        return providers.ToArray();

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
