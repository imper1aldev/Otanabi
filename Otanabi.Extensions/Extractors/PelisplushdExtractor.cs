using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using Otanabi.Core.Helpers;
using Otanabi.Core.Models;
using Otanabi.Extensions.Contracts.Extractors;
using ScrapySharp.Extensions;

namespace Otanabi.Extensions.Extractors;

public class PelisplushdExtractor : IExtractor
{
    internal ServerConventions _serverConventions = new();
    internal readonly int extractorId = 6;
    internal readonly string sourceName = "PelisPlusHd";
    internal readonly string originUrl = "https://pelisplushd.bz";
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
            url = $"{originUrl}/series?page={page}";
        }

        var oWeb = new HtmlWeb();
        var doc = await oWeb.LoadFromWebAsync(url);

        foreach (var nodo in doc.DocumentNode.CssSelect("div.Posters a.Posters-link"))
        {
            Anime anime = new();
            anime.Title = nodo.CssSelect("div.listing-content p").FirstOrDefault()?.InnerText?.TrimAll();
            anime.Cover = nodo.CssSelect("img").FirstOrDefault()?.GetAttributeValue("src");
            anime.Url = nodo.GetAttributeValue("href").Replace("/w154/", "/w200/");
            anime.Provider = (Provider)GenProvider();
            anime.ProviderId = anime.Provider.Id;
            var type = nodo.CssSelect(".centrado").FirstOrDefault()?.InnerText?.Trim();
            type ??= url.Split('/')[3];
            anime.Type = GetAnimeTypeByStr(type.SubstringBefore("?"));
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
        anime.Title = node.CssSelect("h1.m-b-5")?.FirstOrDefault()?.InnerText?.TrimAll();
        anime.Description = node.CssSelect("div.col-sm-4 div.text-large")?.FirstOrDefault()?.InnerText?.TrimAll();
        var img = FetchUrls(node.CssSelect(".img-fluid")?.FirstOrDefault().OuterHtml).FirstOrDefault();
        anime.Cover = img?.Replace("/w154/", "/w500/");
        anime.Provider = (Provider)GenProvider();
        anime.ProviderId = anime.Provider.Id;
        anime.Status = "Finalizado";
        var genres = node.CssSelect(".d-flex .p-v-20 a span").Select(x => WebUtility.HtmlDecode(x.InnerText?.Trim())).ToList();
        anime.GenreStr = string.Join(",", genres);
        anime.RemoteID = requestUrl.Replace("/", "");
        anime.Type = GetAnimeTypeByStr(requestUrl.Split("/")[3]);
        anime.Chapters = GetChapters(requestUrl, doc);
        return anime;
    }

    public async Task<IVideoSource[]> GetVideoSources(string requestUrl)
    {
        var oWeb = new HtmlWeb();
        var doc = await oWeb.LoadFromWebAsync(requestUrl);

        if (oWeb.StatusCode != HttpStatusCode.OK)
            throw new Exception("Page could not be loaded");

        var sources = new List<VideoSource>();
        var scriptNode = doc.DocumentNode.SelectSingleNode("//script[contains(text(),'video[1] =')]");
        if (scriptNode == null)
        {
            return sources.ToArray();
        }

        var scriptContent = scriptNode.InnerText;
        var regVideoOpts = new Regex("'(https?://[^']*)'", RegexOptions.Compiled);
        var matches = regVideoOpts.Matches(scriptContent);

        foreach (Match match in matches)
        {
            var optUrl = match.Groups[1].Value;
            var response = await oWeb.LoadFromWebAsync(optUrl);
            if (oWeb.StatusCode != HttpStatusCode.OK)
            {
                continue;
            }

            var iframeNode = response.DocumentNode.SelectSingleNode("//iframe");
            List<(string Lang, string OnClick, string Server)> encryptedList;
            var dataScript = response.DocumentNode.SelectSingleNode("//script[contains(text(),'const dataLink')]")?.InnerText;

            if (iframeNode != null && string.IsNullOrEmpty(dataScript))
            {
                encryptedList = [("", iframeNode.GetAttributeValue("src", ""), GetDomainName(iframeNode.GetAttributeValue("src", "")))];
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
                encryptedList = response.DocumentNode
                    .SelectNodes("//div[contains(@id,'PlayerDisplay')]//div[contains(@class,'OptionsLangDisp')]//div[contains(@class,'ODDIV')]//div[contains(@class,'OD')]//li")
                    ?.Select(li => (
                        Lang: GetLang(li.GetAttributeValue("data-lang", "")),
                        OnClick: li.GetAttributeValue("onclick", ""),
                        Server: GetDomainName(li.GetAttributeValue("onclick", ""))
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
                        var nestedDoc = await oWeb.LoadFromWebAsync(extractedUrl);
                        finalUrl = nestedDoc.DocumentNode.SelectSingleNode("//iframe")?.GetAttributeValue("src", "") ?? "";
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
            });
        }
        else
        {
            var index = 1;
            foreach (var item in doc.DocumentNode.CssSelect("div.tab-content div a"))
            {
                var chapter = new Chapter()
                {
                    ChapterNumber = index,
                    Name = item.InnerText?.TrimAll(),
                    Url = item.GetAttributeValue("href"),
                };
                chapters.Add(chapter);
                index++;
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