using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using Otanabi.Core.Helpers;
using Otanabi.Core.Models;
using Otanabi.Extensions.Contracts.Extractors;
using ScrapySharp.Extensions;
using ScrapySharp.Network;

namespace Otanabi.Extensions.Extractors;

public class HstreamExtractor : IExtractor
{
    internal ServerConventions _serverConventions = new();
    internal readonly int extractorId = 4;
    internal readonly string sourceName = "Hstream";
    internal readonly string originUrl = "https://hstream.moe";
    internal readonly bool Persistent = true;
    internal readonly string Type = "ANIME";

    //internal readonly HttpService HService =  new ();
    private static readonly HttpClient client = new();

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

    public async Task<IAnime[]> MainPageAsync(int page = 1, Tag[]? tags = null)
    {
        return await SearchAnimeAsync("", page, tags);
    }

    public async Task<IAnime[]> SearchAnimeAsync(string searchTerm, int page, Tag[]? tags = null)
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
        if (tags != null && tags.Length > 0)
        {
            url += $"&{GenerateTagString(tags)}";
        }

        var oWeb = new HtmlWeb();
        var doc = await oWeb.LoadFromWebAsync(url);
        oWeb.UseCookies = true;

        var nodes = doc.DocumentNode.SelectNodes(
            "/html/body/div/main/div[2]/div[2]/div/div/div/div"
        );
        foreach (var node in nodes)
        {
            try
            {
                var img = node.CssSelect("div a img").First();
                var pttitle = img.GetAttributeValue("alt");
                var pturl = img.GetAttributeValue("src");
                var temp = pttitle.Split(" - ");
                var temp2 = pturl.Split("/");

                Anime anime = new()
                {
                    Title = temp[0],
                    Url = string.Concat(originUrl, "/", temp2[2], "/", temp2[3]),
                    Cover = string.Concat(originUrl, img.Attributes["src"].Value),
                    Type = AnimeType.OVA,
                    Status = "",
                    Provider = (Provider)GenProvider()
                };
                anime.ProviderId = anime.Provider.Id;
                animeList.Add(anime);
            }
            catch (Exception)
            {
                continue;
            }
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
                Subtitles = [
                    new() {
                        File = $"{urlBase}/eng.ass",
                        Label = "English"
                    }
                ]
            };
            videoSources.Add(vsouce);
        }

        return videoSources.ToArray();
    }

    public static string GenerateTagString(Tag[] tags)
    {
        var result = "";
        for (var i = 0; i < tags.Length; i++)
        {
            result += $"tags[{i}]={tags[i].Value}";
            if (i < tags.Length - 1)
            {
                result += "&";
            }
        }
        return result;
    }

    public Tag[] GetTags()
    {
        return
        [
            new() { Name = "3D", Value = "3d" },
            new() { Name = "4K", Value = "4k" },
            new() { Name = "Ahegao", Value = "ahegao" },
            new() { Name = "Anal", Value = "anal" },
            new() { Name = "Bdsm", Value = "bdsm" },
            new() { Name = "Big Boobs", Value = "big-boobs" },
            new() { Name = "Blow Job", Value = "blow-job" },
            new() { Name = "Bondage", Value = "bondage" },
            new() { Name = "Boob Job", Value = "boob-job" },
            new() { Name = "Censored", Value = "censored" },
            new() { Name = "Comedy", Value = "comedy" },
            new() { Name = "Cosplay", Value = "cosplay" },
            new() { Name = "Creampie", Value = "creampie" },
            new() { Name = "Dark Skin", Value = "dark-skin" },
            new() { Name = "Elf", Value = "elf" },
            new() { Name = "Facial", Value = "facial" },
            new() { Name = "Fantasy", Value = "fantasy" },
            new() { Name = "Filmed", Value = "filmed" },
            new() { Name = "Foot Job", Value = "foot-job" },
            new() { Name = "Futanari", Value = "futanari" },
            new() { Name = "Gangbang", Value = "gangbang" },
            new() { Name = "Glasses", Value = "glasses" },
            new() { Name = "Hand Job", Value = "hand-job" },
            new() { Name = "Harem", Value = "harem" },
            new() { Name = "Horror", Value = "horror" },
            new() { Name = "Incest", Value = "incest" },
            new() { Name = "Inflation", Value = "inflation" },
            new() { Name = "Lactation", Value = "lactation" },
            new() { Name = "Loli", Value = "loli" },
            new() { Name = "Maid", Value = "maid" },
            new() { Name = "Masturbation", Value = "masturbation" },
            new() { Name = "Milf", Value = "milf" },
            new() { Name = "Mind Break", Value = "mind-break" },
            new() { Name = "Mind Control", Value = "mind-control" },
            new() { Name = "Monster", Value = "monster" },
            new() { Name = "Nekomimi", Value = "nekomimi" },
            new() { Name = "Ntr", Value = "ntr" },
            new() { Name = "Nurse", Value = "nurse" },
            new() { Name = "Orgy", Value = "orgy" },
            new() { Name = "Pov", Value = "pov" },
            new() { Name = "Pregnant", Value = "pregnant" },
            new() { Name = "Public Sex", Value = "public-sex" },
            new() { Name = "Rape", Value = "rape" },
            new() { Name = "Reverse Rape", Value = "reverse-rape" },
            new() { Name = "Rimjob", Value = "rimjob" },
            new() { Name = "Scat", Value = "scat" },
            new() { Name = "School Girl", Value = "school-girl" },
            new() { Name = "Shota", Value = "shota" },
            new() { Name = "Small Boobs", Value = "small-boobs" },
            new() { Name = "Succubus", Value = "succubus" },
            new() { Name = "Swim Suit", Value = "swim-suit" },
            new() { Name = "Teacher", Value = "teacher" },
            new() { Name = "Tentacle", Value = "tentacle" },
            new() { Name = "Threesome", Value = "threesome" },
            new() { Name = "Toys", Value = "toys" },
            new() { Name = "Trap", Value = "trap" },
            new() { Name = "Tsundere", Value = "tsundere" },
            new() { Name = "Ugly Bastard", Value = "ugly-bastard" },
            new() { Name = "Uncensored", Value = "uncensored" },
            new() { Name = "Vanilla", Value = "vanilla" },
            new() { Name = "Virgin", Value = "virgin" },
            new() { Name = "X-Ray,", Value = "x-ray" },
            new() { Name = "Yuri", Value = "yuri" }
        ];
    }
}
