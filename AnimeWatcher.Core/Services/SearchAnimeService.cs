using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;
using AnimeWatcher.Core.Models;
using AnimeWatcher.Core.Helpers;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using ScrapySharp.Extensions;
using System.Xml.Linq;
using System;

namespace AnimeWatcher.Core.Services;
public class SearchAnimeService
{
    internal ServerConventions _serverConventions = new();
    internal readonly string originUrl = "https://www3.animeflv.net";

    public async Task<ICollection<Anime>> SearchAnimeAsync(string searchTerm)
    {
        List<Anime> animeList = new List<Anime>();

        var url = string.Concat(originUrl, "/browse?q=", HttpUtility.UrlEncode(searchTerm));

        HtmlWeb oWeb = new HtmlWeb();
        HtmlDocument doc = await oWeb.LoadFromWebAsync(url);

        foreach (var nodo in doc.DocumentNode.CssSelect(".Anime"))
        {
            Anime anime = new();

            var innerNodes = nodo.Descendants("a").First();
            var image = nodo.SelectSingleNode(".//div/figure/img");
            var title = nodo.SelectSingleNode(".//h3");
            var link = innerNodes.GetAttributeValue("href");
            
            anime.url = link;
            anime.title = title.InnerText;
            anime.cover = image.GetAttributeValue("src");
            var tempType = nodo.SelectSingleNode(".//a/div/span").InnerText;
            switch (tempType)
        {   
            case "OVA":
                anime.type=AnimeType.OVA;
                break;
            case"Anime" :
                anime.type=AnimeType.TV;
                break;
            case"Película":
                anime.type=AnimeType.MOVIE;
                break;
            case"Especial":
                anime.type=AnimeType.SPECIAL;
                break;
            default: 
                anime.type=AnimeType.TV;
                break;
        }
            animeList.Add(anime);
        }
        return animeList;

    }

    public async Task<Anime> GetAnimeDetailsAsync(string requestUrl)
    {
        Anime anime = new();

        var url = string.Concat(originUrl, requestUrl);
        HtmlWeb oWeb = new HtmlWeb();
        HtmlDocument doc = await oWeb.LoadFromWebAsync(url);

        var node = doc.DocumentNode.SelectSingleNode("/html/body");


        anime.title = node.CssSelect("div.Wrapper > div > div > div.Ficha.fchlt > div.Container > h1").First().InnerText;
        var coverTmp = node.CssSelect("div.Wrapper > div > div > div.Container > div > aside > div.AnimeCover > div > figure > img").First().GetAttributeValue("src");
        anime.cover = string.Concat(originUrl, coverTmp);
        anime.description = node.CssSelect("div.Wrapper > div > div > div.Container > div > main > section").First().CssSelect("div.Description > p").First().InnerText;
        
        var tempType= node.CssSelect("div.Wrapper > div > div > div.Ficha.fchlt > div.Container > span").First().InnerText;
        switch (tempType)
        {   
            case "OVA":
                anime.type=AnimeType.OVA;
                break;
            case"Anime" :
                anime.type=AnimeType.TV;
                break;
            case"Película":
                anime.type=AnimeType.MOVIE;
                break;
            case"Especial":
                anime.type=AnimeType.SPECIAL;
                break;
            default: 
                anime.type=AnimeType.TV;
                break;
        }
        
        anime.status = node.CssSelect("div.Wrapper > div > div > div.Container > div > aside > p > span").First().InnerText;

        var identifier = GetUriIdentify(node.InnerText , anime.status);
        anime.Chapters = GetChaptersByregex(node.InnerText, identifier);
        return anime;
    }
    private string[] GetUriIdentify(string text ,string aStatus)
    {
        var pattern = @"anime_info = (\[.*])";
        var identifier = "";
        var chapUri = "";
        var chapName = "";

        var match = Regex.Match(text, pattern);
        if (match.Success)
        {
            var special = match.Groups[1].Value;
            /*
            special = special.Replace("[", "").Replace("]", "").Replace(@"""", "");
            var data = special.Split(new string[] { "," }, StringSplitOptions.None);
            */
            var data = match.Groups[1].Value.Trim('[', ']').Split(',');
            List<string> dataList = new List<string>();

            foreach (var value in data)
            {
                // Remove quotes and trim extra whitespaces
                var trimmedValue = value.Trim('"');
                dataList.Add(trimmedValue);
            }

            foreach (var item in dataList.GetRange(1, dataList.Count - 3))
            {
                chapName += item;
            }

            identifier = dataList[0];
            if(aStatus=="En emision" ) {
                chapUri = dataList.GetRange(dataList.Count-2 , 1)[0];
            }else
            {
                chapUri = dataList.GetRange(dataList.Count-1 , 1)[0];
            }

        }

        return new string[] { identifier, chapUri, chapName };
    }
    private Chapter[] GetChaptersByregex(string text, string[] chapIdentifier)
    {

        var pattern = @"episodes = (\[\[.*\].*])";
        var chapters = new List<Chapter>();
        var match = Regex.Match(text, pattern);
        if (match.Success)
        {
            var innerArrays = match.Groups[1].Value.Split(new string[] { "],[" }, StringSplitOptions.None);
            var chaptherOrder = 0;
            foreach (var innerArray in innerArrays)
            {
                chaptherOrder++;
                Chapter chapter = new Chapter();
                /*var justnummber = innerArray.Replace("[","");
                justnummber=justnummber.Replace("]","");
                string[] numbers=justnummber.Split(',');
                int[] intArray=Array.ConvertAll(numbers,int.Parse);
                */
                chapter.url = string.Concat("/ver/", chapIdentifier[1], "-", chaptherOrder);
                chapter.chapter=chaptherOrder;
                chapter.animeId = int.Parse(chapIdentifier[0]);
                chapter.name = string.Concat(chapIdentifier[2], " ", chaptherOrder);

                chapters.Add(chapter);
            }
        }
        return chapters.ToArray();
    }

    private VideoSource[] getSorcesRegex(string text)
    {
        var pattern = @"var videos = \{""SUB"":(.*?)\};";
        var sources = new List<VideoSource>();
        var match = Regex.Match(text, pattern);
        if (match.Success)
        {
            var prefab = match.Groups[0].Value.Replace("var videos =", "").Replace(";", "");
            var prefJson = JObject.Parse(prefab);

            foreach (var vsource in prefJson["SUB"])
            {
                var serverName = "";
                //(string)vsource["server"]
                serverName = _serverConventions.GetServerName((string)vsource["server"]);

                if (string.IsNullOrEmpty(serverName))
                {
                    continue;
                }

                var vSouce = new VideoSource();
                vSouce.server = serverName;
                vSouce.code = (string)vsource["code"];
                vSouce.url = (string)vsource["url"];
                vSouce.ads = (int)vsource["ads"];
                vSouce.title = (string)vsource["title"];
                vSouce.allow_mobile = (bool)vsource["allow_mobile"];
                sources.Add(vSouce);

            }
        }
        return sources.ToArray();
    }

    public async Task<VideoSource[]> GetVideoSources(string requestUrl)
    {

        var url = string.Concat(originUrl, requestUrl);
        HtmlWeb oWeb = new HtmlWeb();
        HtmlDocument doc = await oWeb.LoadFromWebAsync(url);
        var node = doc.DocumentNode.SelectSingleNode("/html/body");

        var sources = getSorcesRegex(node.InnerText);

        return sources;

    }


}
