using System.Diagnostics;
using System.Dynamic;
using System.Net;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using AnimeWatcher.Core.Contracts.Extractors;
using AnimeWatcher.Core.Helpers;
using AnimeWatcher.Core.Models;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using ScrapySharp.Exceptions;
using ScrapySharp.Extensions;
using ScrapySharp.Network;

namespace AnimeWatcher.Core.Extractors;
public class AnimepacheExtractor : IExtractor
{
    internal ServerConventions _serverConventions = new();
    internal readonly int extractorId = 3;
    internal readonly string sourceName = "Animepache";
    internal readonly string originUrl = "https://animepahe.com";
    internal readonly bool Secured = true;
    internal readonly string Type = "ANIME";

    public string GetSourceName() => sourceName;
    public string GetUrl() => originUrl;
    public Provider GenProvider() => new() { Id = extractorId, Name = sourceName, Url = originUrl, Type = Type, Secured = Secured };

    private async Task<WebPage> interceptorDDOS(string mainPUrl)
    {
        WebPage returnedPage = null;
        var browser = new ScrapingBrowser { Encoding = Encoding.UTF8 };
        try
        {
            returnedPage = await browser.NavigateToPageAsync(new Uri(mainPUrl));
            return returnedPage;
        } catch (WebException)
        {
            var wellknow = await browser.NavigateToPageAsync(new Uri("https://check.ddos-guard.net/check.js"));
            var wellknowResponse = wellknow.Content.SubstringBetween("'", "'");
            var setcok = wellknow.RawResponse.Headers.Where(t => t.Key == "Set-Cookie").FirstOrDefault();
            var coval = setcok.Value;
            browser.Headers.Add("set-cookie", setcok.Value.ToString());
            var tested = await browser.NavigateToPageAsync(new Uri(originUrl + wellknowResponse));
            var data = tested.Content;
            var tested2 = await browser.NavigateToPageAsync(new Uri(mainPUrl));
            return tested2;
        }
    }

    public async Task<Anime[]> MainPageAsync(int page = 1)
    {
        var animeList = new List<Anime>();
        var mainPUrl = string.Concat(originUrl, "/api?m=airing&page=", page);
        var data = await interceptorDDOS(mainPUrl);
        var jdata = JObject.Parse(data);
        foreach (var vsource in jdata["data"])
        {
            Anime anime = new();
            anime.Title = (string)vsource["anime_title"];
            anime.Cover = (string)vsource["snapshot"];
            anime.RemoteID = (string)vsource["anime_id"];
            anime.Url = $"/a/{anime.RemoteID}";
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
        var mainPUrl = string.Concat(originUrl, "/api?m=search&l=8&q=", HttpUtility.UrlEncode(searchTerm), "&page=", page);
        var data = await interceptorDDOS(mainPUrl);
        var jdata = JObject.Parse(data);
        foreach (var vsource in jdata["data"])
        {
            Anime anime = new();
            anime.Title = (string)vsource["title"];
            anime.Cover = (string)vsource["poster"];
            anime.RemoteID = (string)vsource["id"];
            anime.Url = $"/a/{anime.RemoteID}";
            anime.Type = getAnimeTypeByStr((string)vsource["type"]);

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

        var mainPUrl = string.Concat(originUrl, requestUrl);

        var data = await interceptorDDOS(mainPUrl);
        var session_id = data.Content.ToString().SubstringBetween("let id = \"", "\";");
        session_id = Regex.Unescape(session_id);


        var originid = requestUrl.Replace("/a/", "");

        anime.Url = requestUrl;
        anime.Title = data.Html.CssSelect("div.title-wrapper h1 span").First().InnerText;
        anime.Cover = data.Html.CssSelect("div.anime-poster a").First().GetAttributeValue("href");
        anime.Description = data.Html.CssSelect("div.anime-synopsis").First().InnerText;
        anime.Provider = GenProvider();
        anime.ProviderId = anime.Provider.Id;
        anime.Type = getAnimeTypeByStr("");
        anime.RemoteID = originid;


        var chapters = new List<Chapter>();
        var first = await FirstChapterIter(session_id, originid);
        chapters = first.Item1.ToList();
        var lastpage = first.Item2;

        if (lastpage > 1)
        {
            for (var i = 2; i <= lastpage; i++)
            {
                var chaps = await GetChapters(i, session_id, originid);
                chapters.Concat(chaps.ToList());

                //avoid to be target as ddos
                await Task.Delay(300);
            }

        }

        anime.Chapters = chapters.ToArray();
        await Task.CompletedTask;
        return anime;
    }
    // for chapter url is necessary to create a coded string to search its session
    // 1) animeId-episode-page

    private async Task<(Chapter[], int)> FirstChapterIter(string sessionId, string animeId)
    {

        var mainPUrl = string.Concat(originUrl, "/api?m=release&id=", sessionId, "&sort=episode_desc&page=", 1);
        var data = await interceptorDDOS(mainPUrl);
        var jparsed = JObject.Parse(data.Content);

        var last_page = (int)jparsed["last_page"];
        var chapters = new List<Chapter>();
        foreach (var item in jparsed["data"])
        {
            Chapter chapter = new Chapter();
            chapter.Url = string.Concat(animeId, "-", (string)item["episode"], "-", 1);
            chapter.ChapterNumber = (int)item["episode"];
            chapter.Name = string.Concat("Ep #", (string)item["episode"]);
            chapters.Add(chapter);

        }


        return (chapters.ToArray(), last_page);
    }
    private async Task<Chapter[]> GetChapters(int page, string sessionId, string animeId)
    {
        //https://animepahe.ru/api?m=release&id=121ca70d-ed3f-5534-87c7-a4477b7738b8&sort=episode_desc&page=1

        var mainPUrl = string.Concat(originUrl, "/api?m=release&id=", sessionId, "&sort=episode_desc&page=", page);
        var data = await interceptorDDOS(mainPUrl);
        var jparsed = JObject.Parse(data.Content);
        var chapters = new List<Chapter>();

        foreach (var item in jparsed["data"])
        {
            Chapter chapter = new Chapter();
            chapter.Url = string.Concat(animeId, "-", (string)item["episode"], "-", page);
            chapter.ChapterNumber = (int)item["episode"];
            chapter.Name = string.Concat("Ep #", (string)item["episode"]);
            chapters.Add(chapter);
        }
        return chapters.ToArray();
    }

    private async Task<string> GetSessionId(string animeId)
    {
        //current pathern
        
        var ss = $"/a/{animeId}";
        var mainPUrl = string.Concat(originUrl, ss);
        var data = await interceptorDDOS(mainPUrl);
        var session_id = data.Content.ToString().SubstringBetween("let id = \"", "\";");
        session_id = Regex.Unescape(session_id);

        await Task.CompletedTask;
        return session_id;
    }
     

    public async Task<VideoSource[]> GetVideoSources(string requestUrl)
    { 
        //current pathern        
        //animeId-episode-page
        var videoSources = new List<VideoSource>();
        var animeId=requestUrl.Split("-")[0];
        var page = requestUrl.Split("-")[2];
        var episode = requestUrl.Split("-")[1];
        var sessionId=await GetSessionId(animeId);



        var chaps= await GetChapters(int.Parse(page),sessionId,animeId);

        var detail = chaps.Where(c=>c.ChapterNumber==int.Parse(episode)).FirstOrDefault();

        //TODO : detail contains the sessionId,and the chapter session id
        // next , scrape the data to get the url from pahe , and then all the logic to get the video from kwik





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
