using System.Text;
using System.Text.RegularExpressions;
using AnimeWatcher.Core.Helpers;
using AnimeWatcher.Core.Models;
using AnimeWatcher.Extensions.Contracts.Extractors;
using HtmlAgilityPack;
using Juro.Providers.Anime;
using Newtonsoft.Json.Linq;
using ScrapySharp.Extensions;
using ScrapySharp.Network;

namespace AnimeWatcher.Extensions.Extractors;

public class HstreamExtractor : IExtractor
{
    internal ServerConventions _serverConventions = new();
    internal readonly int extractorId = 4;
    internal readonly string sourceName = "Hstream";
    internal readonly string originUrl = "https://hstream.moe";
    internal readonly bool Persistent = true;
    internal readonly string Type = "ANIME";

    //internal readonly HttpService HService =  new ();
    private static readonly HttpClient client = new HttpClient();

    public string GetSourceName() => sourceName;

    public string GetUrl() => originUrl;

    public IProvider GenProvider() =>
        new Provider
        {
            Id = extractorId,
            Name = sourceName,
            Url = originUrl,
            Type = Type,
            Persistent = Persistent
        };

    public async Task<IAnime[]> MainPageAsync(int page = 1)
    {
        return await SearchAnimeAsync("", page);
    }

    public async Task<IAnime[]> SearchAnimeAsync(string searchTerm, int page)
    {
        var animeList = new List<Anime>();

        var url = string.Concat(originUrl, $"/search/?page={page}&view=poster");
        if (searchTerm != "")
        {
            url = string.Concat(
                originUrl,
                $"/search/?page={page}&view=poster&s={Uri.EscapeDataString(searchTerm)}"
            );
        }

        var oWeb = new HtmlWeb();
        var doc = await oWeb.LoadFromWebAsync(url);
        oWeb.UseCookies = true;

        var nodes = doc.DocumentNode.SelectNodes(
            "/html/body/div/main/div[2]/div[2]/div/div/div/div"
        );

        foreach (var node in nodes)
        {
            Anime anime = new();

            var img = node.CssSelect("div a img").First();
            var pttitle = img.GetAttributeValue("alt");
            var pturl = img.GetAttributeValue("src");
            var temp = pttitle.Split(" - ");
            var temp2 = pturl.Split("/");

            anime.Title = temp[0];
            anime.Url = string.Concat(originUrl, "/", temp2[2], "/", temp2[3]);
            anime.Cover = string.Concat(originUrl, img.Attributes["src"].Value);
            anime.Type = AnimeType.OVA;
            anime.Status = "";
            anime.Provider = (Provider)GenProvider();
            anime.ProviderId = anime.Provider.Id;

            animeList.Add(anime);
        }
        return animeList.ToArray();
    }

    public async Task<IAnime> GetAnimeDetailsAsync(string requestUrl)
    {
        var browser = new ScrapingBrowser { Encoding = Encoding.UTF8 };
        var webPage = await browser.NavigateToPageAsync(new Uri(requestUrl));
        var doc = webPage.Html.CssSelect("body").First();
        var img = doc.SelectSingleNode(".//div/main/div/div/div[1]/div[1]/div[1]/img");

        var geners = doc.CssSelect("ul.list-none > li > a")
            .Select(x => Regex.Replace(x.InnerText, @"\t|\n|\r", ""))
            .ToList();
        var anime = new Anime
        {
            Url = requestUrl,
            Title = Regex.Replace(
                doc.SelectSingleNode(".//div/main/div/div/div[1]/div[1]/div[2]/h1").InnerText,
                @"\t|\n|\r",
                ""
            ),
            Cover = string.Concat(originUrl, img.Attributes["src"].Value),
            Description = doc.SelectSingleNode(
                ".//div/main/div/div/div[1]/div[1]/div[2]/p[2]"
            ).InnerText,
            Type = AnimeType.OVA,
            Status = "",
            GenreStr = string.Join(",", geners),
            RemoteID = requestUrl.Replace("/", "")
        };

        anime.Provider = (Provider)GenProvider();
        anime.ProviderId = anime.Provider.Id;

        var chapters = new List<Chapter>();

        var nodes = doc.SelectNodes(".//div/main/div/div/div[1]/div[2]/div/div");

        var i = 1;
        foreach (var node in nodes)
        {
            var chapter = new Chapter
            {
                Url = node.CssSelect("a").First().GetAttributeValue("href"),
                ChapterNumber = i,
                Name = node.CssSelect("a div p").First().InnerText,
                Extraval = node.CssSelect("a").First().GetAttributeValue("href")
            };
            chapters.Add(chapter);
            i++;
        }

        anime.Chapters = chapters;

        await Task.CompletedTask;
        return anime;
    }

    public async Task<IVideoSource[]> GetVideoSources(string requestUrl)
    {
        var videoSources = new List<VideoSource>();

        var response = await client.GetAsync(requestUrl);
        var responseBody = await response.Content.ReadAsStringAsync();

        var doc = new HtmlDocument();
        doc.LoadHtml(responseBody);

        var cookies = response.Headers.GetValues("Set-Cookie");
        var token = cookies.First(cookie => cookie.Contains("XSRF-TOKEN")).Split('=')[1];
        token = token.Replace("; expires", "");
        var episodeId = doc
            .DocumentNode.SelectSingleNode("//input[@id='e_id']")
            .GetAttributeValue("value", "");

        var newHeaders = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{originUrl}/player/api"),
            Headers =
            {
                { "Referer", response.RequestMessage.RequestUri.ToString() },
                { "Origin", originUrl },
                { "X-Requested-With", "XMLHttpRequest" },
                { "X-XSRF-TOKEN", Uri.UnescapeDataString(token) }
            },
            Content = new StringContent(
                $"{{\"episode_id\": \"{episodeId}\"}}",
                Encoding.UTF8,
                "application/json"
            )
        };

        var apiResponse = await client.SendAsync(newHeaders);
        var apiResponseBody = await apiResponse.Content.ReadAsStringAsync();
        var data = JObject.Parse(apiResponseBody);

        var sD = (JArray)data["stream_domains"];
        var streamDomains = sD.ToObject<List<string>>();

        var resolutions = new List<string> { "720", "1080" };

        var urlBase =
            streamDomains[new Random().Next(streamDomains.Count)]
            + "/"
            + (string)data["stream_url"];

        foreach (var resolution in resolutions)
        {
            var dest = string.Concat(urlBase, "/", resolution, "/manifest.mpd");
            var vsouce = new VideoSource
            {
                Server = "Juro",
                Title = "Juro",
                Code = dest,
                Url = dest,
                Subtitle = $"{urlBase}/eng.ass"
            };
            videoSources.Add(vsouce);
        }

        return videoSources.ToArray();
    }
}
