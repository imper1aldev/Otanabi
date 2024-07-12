using System.Diagnostics;
using AnimeWatcher.Core.Contracts.Extractors;
using AnimeWatcher.Core.Helpers;
using AnimeWatcher.Core.Models;
using Juro.Providers.Anime;
namespace AnimeWatcher.Core.Extractors;
public class AnimepacheExtractor : IExtractor
{
    internal ServerConventions _serverConventions = new();
    internal readonly int extractorId = 3;
    internal readonly string sourceName = "Animepache";
    internal readonly string originUrl = "https://animepahe.com";
    internal readonly bool Persistent = false;
    internal readonly string Type = "ANIME";

    public string GetSourceName() => sourceName;
    public string GetUrl() => originUrl;
    public Provider GenProvider() => new() { Id = extractorId, Name = sourceName, Url = originUrl, Type = Type, Persistent = Persistent };



    public async Task<Anime[]> MainPageAsync(int page = 1)
    {
        var animeList = new List<Anime>();
        //var client = new AnimeClient();
        var provider = new AnimePahe();

        var animes = await provider.GetAiringAsync(page);
        foreach (var item in animes)
        {
            var anime = new Anime();
            anime.RemoteID = item.Id;
            anime.Title = item.Title;
            anime.Cover = item.Image;
            anime.Url = item.Id;
            anime.Provider = GenProvider();
            anime.ProviderId = anime.Provider.Id;

            animeList.Add(anime);

        }

        await Task.CompletedTask;
        return animeList.ToArray();
    }
    public async Task<Anime[]> SearchAnimeAsync(string searchTerm, int page)
    {
        var animeList = new List<Anime>(); 
        var provider = new AnimePahe();
        var animes = await provider.SearchAsync(searchTerm);
        foreach (var item in animes)
        {
            var anime = new Anime();
            anime.RemoteID = item.Id;
            anime.Title = item.Title;
            anime.Cover = item.Image;
            anime.Url = item.Id;
            anime.Provider = GenProvider();
            anime.ProviderId = anime.Provider.Id;
            animeList.Add(anime);
        }

        await Task.CompletedTask;
        return animeList.ToArray();
    }

    public async Task<Anime> GetAnimeDetailsAsync(string requestUrl)
    {
        Anime anime = new();
        var provider = new AnimePahe();
        var animeInfo = await provider.GetAnimeInfoAsync(requestUrl);
        var episodes = await provider.GetEpisodesAsync(requestUrl);

        anime.Title = animeInfo.Title;

        anime.Cover = animeInfo.Image;
        anime.Description = animeInfo.Summary;
        anime.RemoteID = animeInfo.Id;
        anime.Url = animeInfo.Id;
        anime.Type = getAnimeTypeByStr(animeInfo.Type);
        anime.Provider = GenProvider();
        anime.ProviderId = anime.Provider.Id;

        var chapters = new List<Chapter>();
        var i = 1;
        foreach (var ep in episodes)
        {

            var chapter = new Chapter();
            chapter.Url = ep.Link;
            chapter.ChapterNumber = i;
            chapter.Name = ep.Name;
            chapter.Extraval = ep.Id;
            chapters.Add(chapter);
            i++;
        }
        anime.Chapters = chapters.ToArray();

        await Task.CompletedTask;
        return anime;
    }


    public async Task<Models.VideoSource[]> GetVideoSources(string requestUrl)
    {
        var videoSources = new List<Models.VideoSource>();
        var provider = new AnimePahe();

        var videoServers = await provider.GetVideoServersAsync(requestUrl);
        var selected = videoServers.Where(vc => vc.Name.Contains("1080") || vc.Name.Contains("720")).FirstOrDefault();
        if (selected != null)
        {
            var videos = await provider.GetVideosAsync(selected);

            foreach (var video in videos)
            {
                var vSouce = new Models.VideoSource();
                vSouce.Server = "Juro";
                vSouce.Title = "Juro";
                vSouce.Code = video.VideoUrl;
                vSouce.Url = video.VideoUrl;
                videoSources.Add(vSouce);
            }
        }
        Debug.WriteLine(videoServers);

        await Task.CompletedTask;
        return videoSources.ToArray();
    }
    private AnimeType getAnimeTypeByStr(string strType)
    {
        switch (strType)
        {
            case "OVA":
                return AnimeType.OVA;
            case "TV":
                return AnimeType.TV;
            case "Movie":
                return AnimeType.MOVIE;
            case "Special":
                return AnimeType.SPECIAL;
            default:
                return AnimeType.TV;
        }
    }
}
