using Juro.Providers.Anime;
using Otanabi.Core.Helpers;
using Otanabi.Core.Models;
using Otanabi.Extensions.Contracts.Extractors;

namespace Otanabi.Extensions.Extractors;

public class AnimepaheExtractor : IExtractor
{
    internal ServerConventions _serverConventions = new();
    internal readonly int extractorId = 3;
    internal readonly string sourceName = "Animepahe";
    internal readonly string originUrl = "https://animepahe.com";
    internal readonly bool Persistent = false;
    internal readonly string Type = "ANIME";

    public IProvider GenProvider() =>
        new Provider
        {
            Id = extractorId,
            Name = sourceName,
            Url = originUrl,
            Type = Type,
            Persistent = Persistent,
        };

    public async Task<IAnime[]> MainPageAsync(int page = 1, Tag[]? tags = null)
    {
        var animeList = new List<Anime>();
        var provider = new AnimePahe();

        var animes = await provider.GetAiringAsync(page);
        foreach (var item in animes)
        {
            var anime = new Anime();
            anime.RemoteID = item.Id;
            anime.Title = item.Title;
            anime.Cover = item.Image;
            anime.Url = item.Id;
            anime.Provider = (Provider)GenProvider();
            anime.ProviderId = anime.Provider.Id;

            animeList.Add(anime);
        }

        await Task.CompletedTask;
        return animeList.ToArray();
    }

    public async Task<IAnime[]> SearchAnimeAsync(string searchTerm, int page, Tag[]? tags = null)
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
            anime.Type = GetAnimeTypeByStr(item.Type);
            anime.Provider = (Provider)GenProvider();
            anime.ProviderId = anime.Provider.Id;
            animeList.Add(anime);
        }

        await Task.CompletedTask;
        return animeList.ToArray();
    }

    public async Task<IAnime> GetAnimeDetailsAsync(string requestUrl)
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
        anime.Type = GetAnimeTypeByStr(animeInfo.Type);
        anime.GenreStr = string.Join(",", animeInfo.Genres);
        anime.Provider = (Provider)GenProvider();
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

    public async Task<IVideoSource[]> GetVideoSources(string requestUrl)
    {
        var videoSources = new List<VideoSource>();
        var provider = new AnimePahe();

        var videoServers = await provider.GetVideoServersAsync(requestUrl);
        var selected = videoServers.Where(vc => vc.Name.Contains("1080") || vc.Name.Contains("720")).FirstOrDefault();
        if (selected != null)
        {
            var videos = await provider.GetVideosAsync(selected);

            foreach (var video in videos)
            {
                var vSouce = new VideoSource
                {
                    Server = "Juro",
                    Title = "Juro",
                    Code = video.VideoUrl,
                    Url = video.VideoUrl,
                    IsLocalSource = true
                };
                videoSources.Add(vSouce);

                Juro.Providers.Aniskip.AniskipClient aniskipClient = new Juro.Providers.Aniskip.AniskipClient();
            }
        }
        await Task.CompletedTask;
        return videoSources.ToArray();
    }

    private static AnimeType GetAnimeTypeByStr(string strType)
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

    public Tag[] GetTags()
    {
        return Array.Empty<Tag>();
    }
}
