using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using Otanabi.Core.Helpers;
using Otanabi.Core.Models;
using Otanabi.Extensions.Contracts.Extractors;
using ScrapySharp.Extensions;
using static Otanabi.Extensions.Utils.HtmlNodeExtensions;
using static ScrapySharp.Core.Token;

namespace Otanabi.Extensions.Extractors;

public class AnimefenixExtractor : IExtractor
{
    internal ServerConventions _serverConventions = new();
    internal readonly int extractorId = 7;
    internal readonly string sourceName = "AnimeFenix";
    internal readonly string originUrl = "https://animefenix2.tv";
    internal readonly bool Persistent = true;
    internal readonly string Type = "ANIME";

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

        string url;
        if (!string.IsNullOrEmpty(searchTerm))
        {
            url = $"{originUrl}/search?s={HttpUtility.UrlEncode(searchTerm)}&page={page}";
        }
        else if (tags != null && tags.Length > 0)
        {
            url = $"{originUrl}/{GenerateTagString(tags)}?page={page}";
        }
        else
        {
            url = $"{originUrl}/directorio/anime?p={page}&estado=2";
        }

        var oWeb = new HtmlWeb();
        var doc = await oWeb.LoadFromWebAsync(url);

        foreach (var nodo in doc.DocumentNode.CssSelect(".grid-animes li article a"))
        {
            Anime anime = new();
            anime.Title = nodo.SelectNodes("//p[not(contains(@class, 'gray'))]").FirstOrDefault()?.InnerText;
            anime.Cover = nodo.CssSelect(".main-img img").FirstOrDefault()?.GetAttributeValue("src");
            anime.Url = nodo.GetAbsoluteUrl("href", originUrl);
            anime.Provider = (Provider)GenProvider();
            anime.ProviderId = anime.Provider.Id;
            var type = nodo.CssSelect(".tipo").FirstOrDefault()?.InnerText?.Trim();
            anime.Type = GetAnimeTypeByStr(type.ToLower());
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
        anime.Title = node.CssSelect("h1.text-4xl")?.FirstOrDefault()?.InnerText?.Trim();
        anime.Description = node.CssSelect(".mb-6 p.text-gray-300")?.FirstOrDefault()?.InnerText?.Trim();
        anime.Cover = node.CssSelect("#blur_img")?.FirstOrDefault()?.GetImageUrl(originUrl);
        anime.Cover ??= node.CssSelect("#anime_image")?.FirstOrDefault()?.GetImageUrl(originUrl);
        anime.Provider = (Provider)GenProvider();
        anime.ProviderId = anime.Provider.Id;
        anime.Status = node.CssSelect(".relative .rounded")?.FirstOrDefault()?.InnerText?.Trim();
        var genres = node.CssSelect(".flex-wrap a").Select(x => WebUtility.HtmlDecode(x.InnerText?.Trim())).ToList();
        anime.GenreStr = string.Join(",", genres);
        anime.RemoteID = requestUrl.Replace("/", "");
        anime.Type = GetAnimeTypeByStr(node.CssSelect(".text-gray-300 .mb-2")?.FirstOrDefault()?.InnerText?.ToLower()?.SubstringAfter("tipo:")?.Trim());
        anime.Chapters = [];
        foreach (var item in node.CssSelect(".divide-y li > a"))
        {
            var title = item.CssSelect(".font-semibold")?.FirstOrDefault()?.InnerText?.Trim();
            anime.Chapters.Add(new Chapter()
            {
                Url = item.GetAbsoluteUrl("href", originUrl),
                ChapterNumber = title.SubstringAfter("Episodio").ToIntOrNull() ?? 0,
                Name = title,
            });
        }
        return anime;
    }

    public async Task<IVideoSource[]> GetVideoSources(string requestUrl)
    {
        var oWeb = new HtmlWeb();
        var doc = await oWeb.LoadFromWebAsync(requestUrl);

        if (oWeb.StatusCode != HttpStatusCode.OK)
            throw new Exception("Page could not be loaded");

        var sources = new List<VideoSource>();

        var scriptNode = doc.DocumentNode
            .SelectNodes("//script")
            ?.FirstOrDefault(s => s.InnerText.Contains("var tabsArray"));

        if (scriptNode == null)
        {
            return [];
        }

        var data = scriptNode.InnerText;

        // Extraer iframes y obtener el parámetro "id" del src
        var urls = data
            .Split(new[] { "<iframe" }, StringSplitOptions.RemoveEmptyEntries)
            .Skip(1) // el primero no tiene iframe
            .Select(part => part.Split(["src='"], StringSplitOptions.None).ElementAtOrDefault(1))
            .Where(src => src != null)
            .Select(src => src.Split('\'')[0]) // cortar en la primera comilla simple
            .Select(src => src.Split(["redirect.php?id="], StringSplitOptions.None).LastOrDefault()?.Trim())
            .Where(id => !string.IsNullOrEmpty(id))
            .ToList();

        foreach (var url in urls.Order())
        {
            var serverName = _serverConventions.GetServerName(GetDomainName(url));
            if (!string.IsNullOrEmpty(serverName))
            {
                sources.Add(new VideoSource()
                {
                    Server = serverName,
                    Title = serverName,
                    Url = url,
                });
            }
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
            new() { Name = "Doramas", Value = "generos/dorama"},
            new() { Name = "Animes", Value = "animes"},
            new() { Name = "Acción", Value = "generos/accion"},
            new() { Name = "Animación", Value = "generos/animacion"},
            new() { Name = "Aventura", Value = "generos/aventura"},
            new() { Name = "Ciencia Ficción", Value = "generos/ciencia-ficcion"},
            new() { Name = "Comedia", Value = "generos/comedia"},
            new() { Name = "Crimen", Value = "generos/crimen"},
            new() { Name = "Documental", Value = "generos/documental"},
            new() { Name = "Drama", Value = "generos/drama"},
            new() { Name = "Fantasía", Value = "generos/fantasia"},
            new() { Name = "Foreign", Value = "generos/foreign"},
            new() { Name = "Guerra", Value = "generos/guerra"},
            new() { Name = "Historia", Value = "generos/historia"},
            new() { Name = "Misterio", Value = "generos/misterio"},
            new() { Name = "Pelicula de Televisión", Value = "generos/pelicula-de-la-television"},
            new() { Name = "Romance", Value = "generos/romance"},
            new() { Name = "Suspense", Value = "generos/suspense"},
            new() { Name = "Terror", Value = "generos/terror"},
            new() { Name = "Western", Value = "generos/western"},
        ];
    }

    #region Private methods

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
            "ova" => AnimeType.OVA,
            "anime" or "doramas" or "serie" or "manhwa" or "live action" or "tv" => AnimeType.TV,
            "película" or "pelicula" => AnimeType.MOVIE,
            "especial" => AnimeType.SPECIAL,
            _ => AnimeType.OTHER,
        };
    }

    public static string GetDomainName(string url)
    {
        try
        {
            var uri = new UriBuilder(url).Uri;
            var host = uri.Host;
            var parts = host.Split('.');
            if (parts.Length >= 2)
            {
                return parts[^2];
            }
            return host;
        }
        catch
        {
            return "invalid";
        }
    }

    #endregion
}