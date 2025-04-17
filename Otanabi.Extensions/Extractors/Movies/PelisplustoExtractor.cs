using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using Otanabi.Core.Helpers;
using Otanabi.Core.Models;
using Otanabi.Extensions.Contracts.Extractors;
using ScrapySharp.Extensions;

namespace Otanabi.Extensions.Extractors;

public class PelisplustoExtractor : IExtractor
{
    internal ServerConventions _serverConventions = new();
    internal readonly int extractorId = 5;
    internal readonly string sourceName = "PelisPlusTo";
    internal readonly string originUrl = "https://ww3.pelisplus.to";
    internal readonly bool Persistent = true;
    internal readonly string Type = "MOVIE";

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
            Persistent = Persistent
        };

    public async Task<IAnime[]> MainPageAsync(int page = 1, Tag[]? tags = null)
    {
        var animeList = (Anime[])await SearchAnimeAsync("", page, tags);
        return animeList.ToArray();
    }

    public async Task<IAnime[]> SearchAnimeAsync(string searchTerm, int page, Tag[]? tags = null)
    {
        var animeList = new List<Anime>();

        var url = originUrl;

        if (!string.IsNullOrEmpty(searchTerm))
        {
            if (page > 1)
            {
                return animeList.ToArray();
            }
            url += $"/api/search/{HttpUtility.UrlEncode(searchTerm)}";
        }
        else if (tags != null && tags.Length > 0)
        {
            url = string.Concat(originUrl, $"/{GenerateTagString(tags)}", $"?page={page}");
        }
        else
        {
            url = string.Concat(originUrl, "/peliculas", $"?page={page}");
        }

        var oWeb = new HtmlWeb();
        var doc = await oWeb.LoadFromWebAsync(url);

        foreach (var nodo in doc.DocumentNode.CssSelect("article.item"))
        {
            Anime anime =
                new()
                {
                    Title = nodo.CssSelect("a h2").FirstOrDefault()?.InnerText?.TrimAll(),
                    Url = nodo.CssSelect("a").First().GetAttributeValue("href"),
                    Cover = nodo.CssSelect("a .item__image picture img").FirstOrDefault()?.GetAttributeValue("data-src"),
                    Provider = (Provider)GenProvider()
                };
            anime.ProviderId = anime.Provider.Id;
            anime.Type = GetAnimeTypeByStr(nodo.CssSelect("a .item__image span").FirstOrDefault()?.InnerText);

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
        anime.Url = requestUrl;
        anime.Title = node.CssSelect(".home__slider_content div h1.slugh1")?.FirstOrDefault()?.InnerText?.TrimAll();
        anime.Description = node.CssSelect(".home__slider_content .description p")?.FirstOrDefault()?.InnerText?.TrimAll();
        anime.Provider = (Provider)GenProvider();
        anime.ProviderId = anime.Provider.Id;
        anime.Status = "Finalizado";
        var genres = node.SelectNodes("//div[contains(@class,'home__slider_content')]/div[5]/a").Select(x => WebUtility.HtmlDecode(x.InnerText)).ToList();
        anime.GenreStr = string.Join(",", genres);
        anime.RemoteID = requestUrl.Replace("/", "");
        var cover = Regex.Match(node.CssSelect(".bg").FirstOrDefault().GetAttributeValue("style"), @"url\((?:'|"")?(.*?)(?:'|"")?\)").Groups[1].Value;
        anime.Cover = cover;
        anime.Type = GetAnimeTypeByStr(requestUrl.Split("/")[3]);

        anime.Chapters = GetChapters(requestUrl, doc);
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
        var regIsUrl = new Regex(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)");
        foreach (var li in doc.DocumentNode.SelectNodes("//div[contains(@class,'bg-tabs')]//ul/li"))
        {
            var parentDiv = li.ParentNode?.ParentNode;
            var buttonText = parentDiv?.SelectSingleNode(".//button")?.InnerText?.Trim().ToLower();
            var prefix = GetLang(buttonText);
            var server = li.SelectSingleNode("span").InnerText?.SubstringBefore("-").Trim();

            var encoded = li.GetAttributeValue("data-server", "");
            var decodedBytes = Convert.FromBase64String(encoded);
            var decoded = Encoding.UTF8.GetString(decodedBytes);

            string urlToUse;
            if (!regIsUrl.IsMatch(decoded))
            {
                var reEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(encoded));
                urlToUse = $"{originUrl}/player/{reEncoded}";
            }
            else
            {
                urlToUse = decoded;
            }

            string videoUrl;
            if (urlToUse.Contains("/player/"))
            {
                var playerDoc = await oWeb.LoadFromWebAsync(urlToUse);
                var scriptNode = playerDoc.DocumentNode.SelectSingleNode("//script[contains(text(),'window.onload')]");
                var scriptContent = scriptNode?.InnerText ?? "";

                videoUrl = FetchUrls(scriptContent).FirstOrDefault() ?? "";
            }
            else
            {
                videoUrl = urlToUse;
            }

            videoUrl = videoUrl.Replace("https://sblanh.com", "https://lvturbo.com");
            videoUrl = Regex.Replace(videoUrl, @"([a-zA-Z0-9]{0,8}[a-zA-Z0-9_-]+)=https:\/\/ww3\.pelisplus\.to.*", "");

            var serverName = _serverConventions.GetServerName(server);
            sources.Add(new VideoSource()
            {
                Server = serverName,
                Title = $"{prefix} {serverName}",
                Url = videoUrl,
            });
        }
        return sources.ToArray();
    }

    public static string GenerateTagString(Tag[] tags)
    {
        var result = "";
        for (var i = 0; i < tags.Length; i++)
        {
            result += $"{tags[i].Value}";
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
            new() { Name = "Peliculas", Value = "peliculas"},
            new() { Name = "Series", Value = "series"},
            new() { Name = "Doramas", Value = "doramas"},
            new() { Name = "Animes", Value = "animes"},
            new() { Name = "Acción", Value = "genero/accion"},
            new() { Name = "Action & Adventure", Value = "genero/action-adventure"},
            new() { Name = "Animación", Value = "genero/animacion"},
            new() { Name = "Aventura", Value = "genero/aventura"},
            new() { Name = "Bélica", Value = "genero/belica"},
            new() { Name = "Ciencia ficción", Value = "genero/ciencia-ficcion"},
            new() { Name = "Comedia", Value = "genero/comedia"},
            new() { Name = "Crimen", Value = "genero/crimen"},
            new() { Name = "Documental", Value = "genero/documental"},
            new() { Name = "Dorama", Value = "genero/dorama"},
            new() { Name = "Drama", Value = "genero/drama"},
            new() { Name = "Familia", Value = "genero/familia"},
            new() { Name = "Fantasía", Value = "genero/fantasia"},
            new() { Name = "Guerra", Value = "genero/guerra"},
            new() { Name = "Historia", Value = "genero/historia"},
            new() { Name = "Horror", Value = "genero/horror"},
            new() { Name = "Kids", Value = "genero/kids"},
            new() { Name = "Misterio", Value = "genero/misterio"},
            new() { Name = "Música", Value = "genero/musica"},
            new() { Name = "Musical", Value = "genero/musical"},
            new() { Name = "Película de TV", Value = "genero/pelicula-de-tv"},
            new() { Name = "Reality", Value = "genero/reality"},
            new() { Name = "Romance", Value = "genero/romance"},
            new() { Name = "Sci-Fi & Fantasy", Value = "genero/sci-fi-fantasy"},
            new() { Name = "Soap", Value = "genero/soap"},
            new() { Name = "Suspense", Value = "genero/suspense"},
            new() { Name = "Terror", Value = "genero/terror"},
            new() { Name = "War & Politics", Value = "genero/war-politics"},
            new() { Name = "Western", Value = "genero/western"}
        ];
    }

    #region Private methods

    private static List<Chapter> GetChapters(string requestUrl, HtmlDocument doc)
    {
        var chapters = new List<Chapter>();
        if (requestUrl.Contains("/pelicula/"))
        {
            chapters.Add(new Chapter()
            {
                Url = requestUrl,
                ChapterNumber = 1,
                Name = "Película",
                Extraval = doc.DocumentNode.CssSelect(".home__slider_content div h1.slugh1")?.FirstOrDefault()?.InnerText
            });
        }
        else
        {
            var scriptNode = doc.DocumentNode.SelectNodes("//script")?.FirstOrDefault(node => node.InnerText.Contains("const seasonUrl ="));

            if (scriptNode == null)
            {
                return chapters;
            }

            var scriptContent = scriptNode.InnerText;

            var jsonStr = scriptContent.SubstringAfter("seasonsJson = ").SubstringBefore(";");

            if (string.IsNullOrWhiteSpace(jsonStr))
            {
                return chapters;
            }

            var seasonsJson = JsonNode.Parse(jsonStr)?.AsObject();
            if (seasonsJson == null)
            {
                return chapters;
            }

            var index = 0;
            foreach (var entry in seasonsJson)
            {
                var episodeArray = entry.Value.AsArray().Reverse();

                foreach (var episodeElement in episodeArray)
                {
                    index++;
                    var epObj = episodeElement.AsObject();
                    var season = epObj["season"]?.ToString();
                    var ep = epObj["episode"]?.ToString();
                    var title = epObj["title"]?.ToString();

                    var chapter = new Chapter()
                    {
                        ChapterNumber = index,
                        Name = $"T{season} - E{ep} - {title}",
                        Url = $"{requestUrl}/season/{season}/episode/{ep}"
                    };
                    chapters.Add(chapter);
                }
            }
        }
        return chapters;
    }

    private static List<string> FetchUrls(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }
        var linkRegex = new Regex(@"(http|ftp|https):\/\/([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:/~+#-]*[\w@?^=%&/~+#-])");
        return linkRegex.Matches(text).Cast<Match>().Select(m => m.Value.Trim('"')).ToList();
    }

    private static string GetLang(string input)
    {
        if (new[] { "0", "lat" }.Any(x => input.Contains(x))) return "[LAT]";
        if (new[] { "1", "cast" }.Any(x => input.Contains(x))) return "[CAST]";
        if (new[] { "2", "eng", "sub" }.Any(x => input.Contains(x))) return "[SUB]";
        return "";
    }

    private static AnimeType GetAnimeTypeByStr(string strType)
    {
        return strType switch
        {
            "OVA" => AnimeType.OVA,
            "Anime" or "anime" or "doramas" or "serie" => AnimeType.TV,
            "Pelicúla" or "pelicula" => AnimeType.MOVIE,
            "Especial" => AnimeType.SPECIAL,
            _ => AnimeType.TV,
        };
    }

    #endregion
}