using System.Net;
using System.Web;
using HtmlAgilityPack;
using Otanabi.Core.Helpers;
using Otanabi.Core.Models;
using Otanabi.Extensions.Contracts.Extractors;
using ScrapySharp.Extensions;

namespace Otanabi.Extensions.Extractors;

public class XnxxExtractor : IExtractor
{
    internal ServerConventions _serverConventions = new();
    internal readonly int extractorId = 9;
    internal readonly string sourceName = "Xnxx";
    internal readonly string originUrl = "https://www.xnxx.com";
    internal readonly bool Persistent = true;
    internal readonly string Type = "MOVIES";

    public string GetSourceName()
    {
        return sourceName;
    }

    public string GetUrl()
    {
        return originUrl;
    }

    public IProvider GenProvider() =>
        new Provider
        {
            Id = extractorId,
            Name = sourceName,
            Url = originUrl,
            Type = Type,
            Persistent = Persistent,
            IsNsfw = true
        };

    public async Task<IAnime[]> MainPageAsync(int page = 1, Tag[]? tags = null)
    {
        var animeList = (Anime[])await SearchAnimeAsync("", page, tags);
        return animeList.ToArray();
    }

    public async Task<IAnime[]> SearchAnimeAsync(string searchTerm, int page, Tag[]? tags = null)
    {
        var animeList = new List<Anime>();
        string url;
        if (!string.IsNullOrEmpty(searchTerm))
        {
            url = $"{originUrl}/search/hits/{HttpUtility.UrlEncode(searchTerm)}/{page - 1}";
        }
        else
        {
            url = $"{originUrl}/hits/month/{page - 1}";
        }

        var oWeb = new HtmlWeb();
        var doc = await oWeb.LoadFromWebAsync(url);

        foreach (var nodo in doc.DocumentNode.SelectNodes("//div[contains(@id, 'video_') and contains(concat(' ', normalize-space(@class), ' '), ' thumb-block ')]\r\n"))
        {
            Anime anime =
                new()
                {
                    Title = nodo.CssSelect("div.thumb-under > p > a").FirstOrDefault()?.InnerText?.TrimAll(),
                    Url = originUrl + nodo.CssSelect("div.thumb-inside div.thumb > a").First().GetAttributeValue("href"),
                    Cover = nodo.CssSelect("div.thumb-inside div.thumb img").FirstOrDefault()?.GetAttributeValue("data-src"),
                    Provider = (Provider)GenProvider()
                };
            anime.ProviderId = anime.Provider.Id;
            anime.Type = AnimeType.MOVIE;

            animeList.Add(anime);
        }
        return animeList.ToArray();
    }

    public async Task<IAnime> GetAnimeDetailsAsync(string requestUrl)
    {

        var url = string.Concat(requestUrl);
        var oWeb = new HtmlWeb();
        var doc = await oWeb.LoadFromWebAsync(url);

        if (oWeb.StatusCode != HttpStatusCode.OK)
        {
            throw new Exception("Anime could not be found");
        }
        Anime anime = new();
        var node = doc.DocumentNode.SelectSingleNode("/html/body");
        var sourcesJson = doc.DocumentNode.SelectNodes("//script")
            ?.FirstOrDefault(s => s.InnerText.Contains("html5player.setVideoUrl"))?.InnerText ?? "";

        var genres = doc.DocumentNode.CssSelect("#video-content-metadata > div.metadata-row.video-tags > a")
                        .Take(10).Select(x => WebUtility.HtmlDecode(x.InnerText.TrimAll())).ToList();
        anime.Provider = (Provider)GenProvider();
        anime.ProviderId = anime.Provider.Id;
        anime.Type = AnimeType.MOVIE;
        anime.Url = requestUrl;
        anime.Title = node.CssSelect("#video-content-metadata > div.clear-infobar strong")?.FirstOrDefault()?.InnerText?.TrimAll();
        anime.Description = node.CssSelect("#video-content-metadata > p")?.FirstOrDefault()?.InnerText?.TrimAll();
        anime.Status = "Finalizado";
        anime.GenreStr = string.Join(",", genres);
        anime.RemoteID = requestUrl.Replace("/", "");
        anime.Cover = sourcesJson.SubstringAfter("setThumbUrl169('").SubstringBefore("')");

        anime.Chapters =
        [
            new()
            {
                ChapterNumber = 1,
                Url = requestUrl,
                Name = "Video"
            }
        ];
        return anime;
    }

    public async Task<IVideoSource[]> GetVideoSources(string requestUrl)
    {
        var oWeb = new HtmlWeb();
        var doc = await oWeb.LoadFromWebAsync(requestUrl);
        if (oWeb.StatusCode != HttpStatusCode.OK)
        {
            throw new Exception("Anime could not be found");
        }

        var sources = new List<VideoSource>();
        var sourcesJson = doc.DocumentNode.SelectNodes("//script")
            ?.FirstOrDefault(s => s.InnerText.Contains("html5player.setVideoUrl"))
            ?.InnerText ?? "";

        var lowQuality = sourcesJson.SubstringAfter("VideoUrlLow('").SubstringBefore("')");
        var hlsQuality = sourcesJson.SubstringAfter("setVideoHLS('").SubstringBefore("')");
        var highQuality = sourcesJson.SubstringAfter("VideoUrlHigh('").SubstringBefore("')");

        sources.Add(new VideoSource()
        {
            Server = "HLS",
            Title = "HLS",
            Code = hlsQuality,
            Url = hlsQuality,
            IsLocalSource = true
        });

        sources.Add(new VideoSource()
        {
            Server = "High",
            Title = "High",
            Code = highQuality,
            Url = highQuality,
            IsLocalSource = true
        });

        sources.Add(new VideoSource()
        {
            Server = "Low",
            Title = "Low",
            Code = lowQuality,
            Url = lowQuality,
            IsLocalSource = true
        });

        return sources.ToArray();
    }

    public Tag[] GetTags()
    {
        return [];
    }
}