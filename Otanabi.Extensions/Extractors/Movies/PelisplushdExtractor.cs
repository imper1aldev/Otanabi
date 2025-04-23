using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using AngleSharp;
using AngleSharp.Dom;
using Newtonsoft.Json.Linq;
using Otanabi.Core.Helpers;
using Otanabi.Core.Models;
using Otanabi.Extensions.Contracts.Extractors;

namespace Otanabi.Extensions.Extractors;

public class PelisplushdExtractor : IExtractor
{
    internal ServerConventions _serverConventions = new();
    internal readonly int extractorId = 6;
    internal readonly string sourceName = "PelisPlusHd";
    internal readonly string originUrl = "https://pelisplushd.bz";
    internal readonly bool Persistent = true;
    internal readonly string Type = "MOVIE";

    private readonly IBrowsingContext _client;

    public PelisplushdExtractor()
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
            url = $"{originUrl}/search?s={HttpUtility.UrlEncode(searchTerm)}&page={page}";
        }
        else if (tags != null && tags.Length > 0)
        {
            var genre = tags.FirstOrDefault()?.Value;
            url = $"{originUrl}/{genre}?page={page}";
        }
        else
        {
            url = $"{originUrl}/series?page={page}";
        }

        var doc = await _client.OpenAsync(url);
        var prov = (Provider)GenProvider();

        foreach (var nodo in doc.QuerySelectorAll("div.Posters a.Posters-link"))
        {
            var type = nodo.QuerySelector(".centrado")?.TextContent?.Trim() ?? url.Split('/')[3];
            var cover = nodo.QuerySelector("img")?.GetAttribute("src")?.Replace("/w154/", "/w200/");
            animeList.Add(new()
            {
                Title = nodo.QuerySelector("div.listing-content p")?.TextContent?.TrimAll(),
                Cover = cover,
                Url = nodo.GetAbsoluteUrl("href").AddOrUpdateParameter("cover", cover),
                Provider = prov,
                ProviderId = prov.Id,
                Type = GetAnimeTypeByStr(type.SubstringBefore("?"))
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
        var cover = requestUrl.GetParameter("cover")?.Replace("/w200/", "/w500/");
        cover ??= doc.QuerySelector("div.card-body div.row div.col-sm-3 img.img-fluid")?.GetAttribute("src")?.Replace("/w154/", "/w500/");

        requestUrl = requestUrl.RemoveParameter("cover");
        var anime = new Anime()
        {
            Url = requestUrl,
            Title = doc.QuerySelector("h1.m-b-5")?.TextContent?.TrimAll(),
            Description = doc.QuerySelector("div.col-sm-4 div.text-large")?.TextContent?.TrimAll(),
            Cover = cover,
            Status = "Finalizado",
            Provider = prov,
            ProviderId = prov.Id,
            GenreStr = string.Join(",", doc.QuerySelectorAll(".d-flex .p-v-20 a span").Select(x => WebUtility.HtmlDecode(x.TextContent?.Trim())).ToList()),
            RemoteID = requestUrl.Replace("/", ""),
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
            throw new Exception("Page could not be loaded");
        }

        var sources = new List<VideoSource>();
        var scriptNode = doc.Scripts.FirstOrDefault(s => s.TextContent.Contains("video[1] ="));
        if (scriptNode == null)
        {
            return sources.ToArray();
        }

        var scriptContent = scriptNode.TextContent;
        var regVideoOpts = new Regex("'(https?://[^']*)'", RegexOptions.Compiled);
        var matches = regVideoOpts.Matches(scriptContent);

        foreach (Match match in matches)
        {
            var optUrl = match.Groups[1].Value;
            var response = await _client.OpenAsync(optUrl);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                continue;
            }

            var iframeNode = response.QuerySelector("iframe");
            List<(string Lang, string OnClick, string Server)> encryptedList;
            var dataScript = response.Scripts.FirstOrDefault(s => s.TextContent.Contains("const dataLink"))?.TextContent;

            if (iframeNode != null && string.IsNullOrEmpty(dataScript))
            {
                encryptedList = [("", iframeNode.GetAttribute("src"), GetDomainName(iframeNode.GetAttribute("src")))];
            }
            else if (!string.IsNullOrEmpty(dataScript))
            {
                var jsonArr = dataScript.SubstringAfter("const dataLink =").SubstringBefore("];") + "]";
                var jsonArray = JArray.Parse(jsonArr);
                var decryptUtf8 = dataScript.Contains("decryptLink(encrypted){");
                var key = dataScript.SubstringAfter("CryptoJS.AES.decrypt(encrypted, '").SubstringBefore("')");
                if (!decryptUtf8)
                {
                    key = dataScript.SubstringAfter("decryptLink(server.link, '").SubstringBefore("'),");
                }

                encryptedList = jsonArray.SelectMany(embed =>
                {
                    var videoLang = embed.Value<string>("video_language");
                    var sortedEmbeds = embed["sortedEmbeds"] as JArray ?? [];
                    return sortedEmbeds.Select(x => (
                        Lang: GetLang(videoLang),
                        OnClick: AESDecryptor.DecryptLink(x.Value<string>("link"), key, decryptUtf8),
                        Server: x.Value<string>("servername")
                    ));
                }).ToList();
            }
            else
            {
                encryptedList = response.QuerySelectorAll("#PlayerDisplay div[class*=\"OptionsLangDisp\"] div[class*=\"ODDIV\"] div[class*=\"OD\"] li")
                    ?.Select(li => (
                        Lang: GetLang(li.GetAttribute("data-lang")),
                        OnClick: li.GetAttribute("onclick"),
                        Server: GetDomainName(li.GetAttribute("onclick"))
                    )).ToList() ?? [];
            }

            foreach (var item in encryptedList)
            {
                try
                {
                    var extractedUrl = item.OnClick
                        .SubstringAfter("go_to_player('")
                        .SubstringAfter("go_to_playerVast('")
                        .SubstringBefore("?cover_url=")
                        .SubstringBefore("')")
                        .SubstringBefore("',")
                        .SubstringBefore("?poster")
                        .SubstringBefore("?c_poster=")
                        .SubstringBefore("?thumb=")
                        .SubstringBefore("#poster=");

                    string finalUrl;
                    var regIsUrl = new Regex(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)");
                    if (!regIsUrl.IsMatch(extractedUrl))
                    {
                        var decodedBytes = Convert.FromBase64String(extractedUrl);
                        finalUrl = Encoding.UTF8.GetString(decodedBytes);
                    }
                    else if (extractedUrl.Contains("?data="))
                    {
                        var nestedDoc = await _client.OpenAsync(extractedUrl);
                        finalUrl = nestedDoc.QuerySelector("iframe")?.GetAttribute("src") ?? "";
                    }
                    else
                    {
                        finalUrl = extractedUrl;
                    }

                    var serverName = _serverConventions.GetServerName(item.Server);
                    sources.Add(new VideoSource()
                    {
                        Server = serverName,
                        Title = $"{item.Lang} {serverName}",
                        Url = finalUrl
                    });
                }
                catch
                {
                    continue;
                }
            }
        }

        return sources.OrderByDescending(s => s.Server).ToArray();
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

    private static List<Chapter> GetChapters(string requestUrl, IDocument doc)
    {
        var chapters = new List<Chapter>();
        if (requestUrl.Contains("/pelicula/"))
        {
            var title = doc.QuerySelector("h1.m-b-5")?.TextContent?.Trim();
            var dateNode = doc.QuerySelectorAll(".sectionDetail")
                .FirstOrDefault(x => x.TextContent.TrimAll().Contains("Fecha de estreno", StringComparison.OrdinalIgnoreCase));
            var yearText = dateNode?.Owner.TextContent?.Trim();
            var year = DateTime.TryParseExact(yearText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
                ? d.ToString("dd/MM/yyyy")
                : Regex.Match(title ?? "", @"\(([^)]*)\)").Groups[1].Value;

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
            var index = 1;
            foreach (var item in doc.QuerySelectorAll("div.tab-content div a"))
            {
                chapters.Add(new Chapter()
                {
                    ChapterNumber = index,
                    Name = item.TextContent?.TrimAll(),
                    Url = item.GetAttribute("href"),
                });
                index++;
            }
        }
        return chapters;
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
            "Película" or "pelicula" => AnimeType.MOVIE,
            "Especial" => AnimeType.SPECIAL,
            _ => AnimeType.TV,
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