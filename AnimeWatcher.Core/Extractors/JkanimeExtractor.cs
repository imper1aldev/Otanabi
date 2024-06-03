using AnimeWatcher.Core.Helpers;
using AnimeWatcher.Core.Models;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using AnimeWatcher.Core.Contracts.Extractors;
using AnimeWatcher.Core.Flare;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using FlareSolverrSharp;
using ScrapySharp.Network;
using System.Globalization;
using System.Text;
namespace AnimeWatcher.Core.Extractors;
public class JkanimeExtractor : IExtractor
{
    internal ServerConventions _serverConventions = new();
    internal readonly int extractorId = 2;
    internal readonly string sourceName = "Jkanime";
    internal readonly string originUrl = "https://jkanime.net/";
    internal readonly bool Persistent = true;
    internal readonly string Type = "ANIME";

    public string GetSourceName() => sourceName;
    public string GetUrl() => originUrl;
    public Provider GenProvider() => new() { Id = extractorId, Name = sourceName, Url = originUrl, Type = Type, Persistent = Persistent };

    private readonly FlareService flareService = new();



    public async Task<Anime[]> MainPageAsync(int page = 1)
    {
        var animeList = new List<Anime>();
        var mainPUrl = string.Concat(originUrl, "directorio/", page);

        var browser = new ScrapingBrowser { Encoding = Encoding.UTF8 };
        WebPage webPage = await browser.NavigateToPageAsync(new Uri(mainPUrl));

        var doc = webPage.Html.CssSelect("body").First();
        var removeDomain = (string e) => e.Replace(originUrl, "");

        foreach (var nodo in doc.CssSelect(".custom_item2"))
        {

            var anime = new Anime();
            anime.Title = nodo.CssSelect("h5.card-title a").First().GetAttributeValue("title");
            var temp = nodo.CssSelect("h5.card-title a").First();
            anime.Url = removeDomain(temp.GetAttributeValue("href"));
            anime.Cover = nodo.CssSelect(".custom_thumb2 img").First().GetAttributeValue("src");
            anime.Provider = GenProvider();
            anime.ProviderId = anime.Provider.Id;
            anime.Type = getAnimeTypeByStr(nodo.CssSelect(".card-info p.card-txt").First().InnerText);
            animeList.Add(anime);
        }

        return animeList.ToArray();
    }
    public async Task<Anime[]> SearchAnimeAsync(string searchTerm, int page)
    {
        var animeList = new List<Anime>();
        var tmp = searchTerm == "" ? "" : searchTerm.Replace(" ", "_") + "/";
        var mainPUrl = string.Concat(originUrl, "buscar/", tmp, page);
        var browser = new ScrapingBrowser();
        WebPage webPage = await browser.NavigateToPageAsync(new Uri(mainPUrl));


        var doc = webPage.Html.CssSelect("body").First();

        var removeDomain = (string e) => e.Replace(originUrl, "");

        foreach (var nodo in doc.CssSelect(".anime__page__content .row .col-md-6"))
        {
            var anime = new Anime();
            anime.Title = nodo.CssSelect("h5 a").First().InnerText;
            var temp = nodo.CssSelect(".anime__item a").First();
            anime.Url = removeDomain(temp.GetAttributeValue("href"));
            anime.Cover = nodo.CssSelect(".anime__item__pic").First().GetAttributeValue("data-setbg");
            anime.Provider = GenProvider();
            anime.ProviderId = anime.Provider.Id;
            anime.Type = getAnimeTypeByStr(nodo.CssSelect(".anime__item__text ul li.anime").First().InnerText);
            animeList.Add(anime);
        }



        await Task.CompletedTask;

        return animeList.ToArray();
    }
    public async Task<Anime> GetAnimeDetailsAsync(string requestUrl)
    {
        var mainPUrl = string.Concat(originUrl, requestUrl);
        var browser = new ScrapingBrowser { Encoding = Encoding.UTF8 };
        WebPage webPage = await browser.NavigateToPageAsync(new Uri(mainPUrl));


        var doc = webPage.Html.CssSelect("body").First();

        var anime = new Anime();
        anime.Url = requestUrl;
        anime.Title = doc.CssSelect(".anime__details__title h3").First().InnerText;
        anime.Cover = doc.CssSelect(".anime__details__pic").First().GetAttributeValue("data-setbg");
        anime.Description = doc.CssSelect(".sinopsis").First().InnerText;
        anime.Provider = GenProvider();
        anime.ProviderId = anime.Provider.Id;
        var tempType = doc.SelectSingleNode(".//section[2]/div/div[1]/div/div[2]/div/div[2]/div/div[1]/ul/li[1]").InnerText;
        anime.Type = getAnimeTypeByStr(tempType);

        anime.Status = doc.CssSelect("span.enemision").First().InnerText;

        anime.RemoteID = requestUrl.Replace("/", "");

        //do some magic things
        var lastlink = doc.CssSelect("a.numbers").Last().InnerText;
        var ttr = lastlink.Split(" - ");
        var lastEpisode = ttr[1];

        //var lastEpisode = doc.CssSelect("a#uep").First().GetAttributeValue("href");
        var lastchap = 1;
        if (!string.IsNullOrEmpty(lastEpisode))
            lastchap = int.Parse(lastEpisode);

        var chapters = new List<Chapter>();
        for (var i = 1; i <= lastchap; i++)
        {
            var chapter = new Chapter();
            chapter.Url = string.Concat(requestUrl, i, "/");
            chapter.ChapterNumber = i;
            chapter.Name = string.Concat(requestUrl.Replace("/", ""), " ", i);
            chapters.Add(chapter);
        }
        anime.Chapters = chapters.ToArray();
        await Task.CompletedTask;
        return anime;
    }

    public async Task<VideoSource[]> GetVideoSources(string requestUrl)
    {
        var videoSource = new List<VideoSource>();
        var mainPUrl = string.Concat(originUrl, requestUrl);

        var browser = new ScrapingBrowser();
        WebPage webPage = await browser.NavigateToPageAsync(new Uri(mainPUrl));
        var doc = webPage.Html.CssSelect("body").First().InnerHtml;

        var match = doc.SubstringBetween("var servers = ", ";");

        Debug.WriteLine(match);
        var prefJson = JArray.Parse(match);
        foreach (JObject vsource in prefJson.Children<JObject>())
        {
            var serverName = _serverConventions.GetServerName((string)vsource["server"]);

            if (string.IsNullOrEmpty(serverName))
            {
                continue;
            }
            var vSouce = new VideoSource();
            vSouce.Server = serverName;
            vSouce.Code = Base64Decode((string)vsource["remote"]);
            vSouce.Url = Base64Decode((string)vsource["remote"]);
            vSouce.Title = serverName;
            vSouce.Allow_mobile = false;
            videoSource.Add(vSouce);

        }
        await Task.CompletedTask;
        return videoSource.ToArray();
    }


    private AnimeType getAnimeTypeByStr(string strType)
    {
        switch (strType)
        {
            case "OVA":
                return AnimeType.OVA;
            case "Anime":
                return AnimeType.TV;
            case "Película":
                return AnimeType.MOVIE;
            case "Especial":
                return AnimeType.SPECIAL;
            default:
                return AnimeType.TV;
        }
    }
    public static string Base64Decode(string base64EncodedData)
    {
        var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }
}
