using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;
using AnimeWatcher.Core.Models;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using ScrapySharp.Extensions;

namespace AnimeWatcher.Core.Services;
public class SearchAnimeService
{
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


        var identifier = GetUriIdentify(node.InnerText);
        anime.Chapters = GetChaptersByregex(node.InnerText, identifier);
        return anime;
    }
    private string[] GetUriIdentify(string text)
    {
        var pattern = @"anime_info = (\[.*])";
        var identifier = "";
        var chapUri = "";
        var chapName = "";

        var match = Regex.Match(text, pattern);
        if (match.Success)
        {
            var special = match.Groups[1].Value;
            special = special.Replace("[", "").Replace("]", "").Replace(@"""", "");
            var data = special.Split(new string[] { "," }, StringSplitOptions.None);
            identifier = data[0];
            chapUri = data[2];
            chapName = data[1];
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
                chapter.animeId = int.Parse(chapIdentifier[0]);
                chapter.name = string.Concat(chapIdentifier[2], " ", chaptherOrder);

                chapters.Add(chapter);
            }
        }
        return chapters.ToArray();
    }

    private videoSource[] getSorcesRegex(string text)
    {
        var pattern = @"var videos = \{""SUB"":(.*?)\};";
        var sources = new List<videoSource>();
        var match = Regex.Match(text, pattern);
        if (match.Success)
        {
            var prefab = match.Groups[0].Value.Replace("var videos =", "").Replace(";", "");
            var prefJson = JObject.Parse(prefab);

            foreach (var vsource in prefJson["SUB"])
            {
                var vSouce = new videoSource();

                vSouce.server = (string)vsource["server"];
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

    public async Task<videoSource[]> GetVideoSources(string requestUrl)
    {

        var url = string.Concat(originUrl, requestUrl);
        HtmlWeb oWeb = new HtmlWeb();
        HtmlDocument doc = await oWeb.LoadFromWebAsync(url);
        var node = doc.DocumentNode.SelectSingleNode("/html/body");

        var sources = getSorcesRegex(node.InnerText);

        return sources;

    }
    public async Task<string> GetStreamOKURO(string url)
    {
        // var url = "https://ok.ru/videoembed/947875089023";
        var streaminUrl = "";
        HtmlWeb oWeb = new HtmlWeb();
        HtmlDocument doc = await oWeb.LoadFromWebAsync(url);


        var values = doc.DocumentNode.SelectSingleNode("/html/body/div[2]/div").GetAttributeValue("data-options").Replace("&quot;", "\"");
        Debug.WriteLine(values);
        dynamic contourManifest = JObject.Parse(values);
        var metadata = (string)contourManifest.flashvars["metadata"];
        var meta2 = JObject.Parse(metadata);
        var videos = meta2["videos"];
        foreach (var video in videos)
        {
            if ((string)video["name"] == "hd")
            {
                streaminUrl = (string)video["url"];
            }
        }

        return streaminUrl;
    }

}
