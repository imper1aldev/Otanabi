using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using Otanabi.Core.Helpers;
using Otanabi.Core.Models;
using Otanabi.Extensions.Contracts.Extractors;

namespace Otanabi.Extensions.Extractors;

public class PelisplustoExtractor : IExtractor
{
    internal ServerConventions _serverConventions = new();
    internal readonly int extractorId = 5;
    internal readonly string sourceName = "PelisPlusTo";
    internal readonly string originUrl = "https://ww3.pelisplus.to";
    internal readonly bool Persistent = true;
    internal readonly string Type = "MOVIE";

    private readonly IBrowsingContext _client;

    public PelisplustoExtractor()
    {
        var config = Configuration.Default.WithDefaultLoader();
        _client = BrowsingContext.New(config);
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

        string url;
        if (!string.IsNullOrEmpty(searchTerm))
        {
            if (page > 1)
            {
                return animeList.ToArray();
            }
            url = $"{originUrl}/api/search/{Uri.EscapeDataString(searchTerm)}";
        }
        else if (tags != null && tags.Length > 0)
        {
            var genre = tags.FirstOrDefault()?.Value;
            url = $"{originUrl}/{genre}?page={page}";
        }
        else
        {
            url = $"{originUrl}/peliculas?page={page}";
        }

        var doc = await _client.OpenAsync(url);
        var prov = (Provider)GenProvider();
        foreach (var nodo in doc.QuerySelectorAll("article.item"))
        {
            var cover = nodo.QuerySelector("a .item__image picture img")?.GetAttribute("data-src");
            animeList.Add(new Anime()
            {
                Provider = prov,
                ProviderId = prov.Id,
                Title = nodo.QuerySelector("a h2")?.TextContent?.TrimAll(),
                Url = nodo.QuerySelector("a").GetAttribute("href").AddOrUpdateParameter("cover", cover),
                Cover = cover,
                Type = GetAnimeTypeByStr(nodo.QuerySelector("a .item__image span")?.TextContent)
            });
        }
        return animeList.ToArray();
    }

    public async Task<IAnime> GetAnimeDetailsAsync(string requestUrl)
    {
        var doc = await _client.OpenAsync(requestUrl);

        if (doc.StatusCode != HttpStatusCode.OK)
        {
            throw new Exception("Anime could not be found");
        }

        var prov = (Provider)GenProvider();
        var cover = requestUrl.GetParameter("cover")?.Replace("/w342/", "/w500/");
        cover ??= Regex.Match(doc.QuerySelector(".bg").GetAttribute("style"), @"url\((?:'|"")?(.*?)(?:'|"")?\)").Groups[1].Value;

        requestUrl = requestUrl.RemoveParameter("cover");
        Anime anime = new()
        {
            Provider = prov,
            ProviderId = prov.Id,
            Url = requestUrl,
            Title = doc.QuerySelector(".home__slider_content div h1.slugh1")?.TextContent?.TrimAll(),
            Description = doc.QuerySelector(".home__slider_content .description p")?.TextContent?.TrimAll(),
            Status = "Finalizado",
            GenreStr = string.Join(",", doc.QuerySelectorAll(".home__slider_content div:nth-child(5) > a").Select(x => WebUtility.HtmlDecode(x.TextContent))),
            RemoteID = requestUrl.Replace("/", ""),
            Cover = cover,
            Type = GetAnimeTypeByStr(requestUrl.Split("/")[3]),
            Chapters = GetChapters(requestUrl, doc)
        };
        return anime;
    }

    public async Task<IVideoSource[]> GetVideoSources(string requestUrl)
    {
        var doc = await _client.OpenAsync(requestUrl);
        if (doc.StatusCode != HttpStatusCode.OK)
        {
            throw new Exception("Anime could not be found");
        }

        var sources = new List<VideoSource>();
        var regIsUrl = new Regex(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)");
        foreach (var li in doc.QuerySelectorAll(".bg-tabs ul li"))
        {
            var parentDiv = li.ParentElement?.ParentElement;
            var buttonText = parentDiv?.QuerySelector("button")?.TextContent?.Trim().ToLower();
            var prefix = GetLang(buttonText);
            var server = li.QuerySelector("span").TextContent?.SubstringBefore("-").Trim();

            var encoded = li.GetAttribute("data-server");
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
                var playerDoc = await _client.OpenAsync(urlToUse);
                var scriptNode = playerDoc.Scripts.FirstOrDefault(x => x.TextContent.Contains("window.onload"));
                var scriptContent = scriptNode?.TextContent ?? "";

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
        return sources.OrderByDescending(s => s.Server).ToArray();
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

    private static List<Chapter> GetChapters(string requestUrl, IDocument doc)
    {
        var chapters = new List<Chapter>();
        if (requestUrl.Contains("/pelicula/"))
        {
            var title = doc.QuerySelector(".home__slider_content div h1.slugh1")?.TextContent;
            var year = Regex.Match(title ?? "", @"\(([^)]*)\)").Groups[1].Value;
            chapters.Add(new Chapter()
            {
                Url = requestUrl,
                ChapterNumber = 1,
                Name = title,
                ReleaseDate = year
            });
        }
        else
        {
            var scriptNode = doc.Scripts.FirstOrDefault(node => node.TextContent.Contains("const seasonUrl ="));

            if (scriptNode == null)
            {
                return chapters;
            }

            var scriptContent = scriptNode.TextContent;

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