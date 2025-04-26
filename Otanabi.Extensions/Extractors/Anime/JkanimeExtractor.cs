using System.Diagnostics;
using System.Text;
using AngleSharp;
using AngleSharp.Html.Dom;
using Newtonsoft.Json.Linq;
using Otanabi.Core.Helpers;
using Otanabi.Core.Models;
using Otanabi.Extensions.Contracts.Extractors;
using ScrapySharp.Extensions;
using ScrapySharp.Network;

namespace Otanabi.Extensions.Extractors;

public class JkanimeExtractor : IExtractor
{
    internal ServerConventions _serverConventions = new();
    internal readonly int extractorId = 2;
    internal readonly string sourceName = "Jkanime";
    internal readonly string originUrl = "https://jkanime.net/";
    internal readonly bool Persistent = true;
    internal readonly string Type = "ANIME";
    private readonly IBrowsingContext _client;

    public JkanimeExtractor()
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
            Persistent = Persistent,
        };

    public async Task<IAnime[]> MainPageAsync(int page = 1, Tag[]? tags = null)
    {
        var animeList = new List<Anime>();
        var mainPUrl = string.Concat(originUrl, "directorio/", page);
        if (tags != null && tags.Length > 0)
        {
            mainPUrl = $"{originUrl}directorio/{page}/{tags.FirstOrDefault()?.Value}/";
        }
        IHtmlScriptElement script = null;
        var i = 0;
        do
        {
            var doc = await _client.OpenAsync(mainPUrl);
            script = doc.Scripts.FirstOrDefault(x => x.TextContent.Contains("var animes ="));
            i++;
        }
        while (script == null && i <= 5);

        var animesdata = script?.TextContent?.SubstringAfter("var animes = ").SubstringBefore("var mode")?.Trim().TrimEnd(';');

        try
        {
            var data = JArray.Parse(animesdata);

            foreach (var nodo in data)
            {
                var anime = new Anime();
                anime.Title = (string)nodo["title"];
                anime.Url = (string)nodo["slug"];
                anime.Provider = (Provider)GenProvider();
                anime.ProviderId = anime.Provider.Id;
                anime.Status = (string)nodo["status"];
                anime.Cover = (string)nodo["image"];
                anime.Type = GetAnimeTypeByStr((string)nodo["type"]);
                animeList.Add(anime);
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }

        return animeList.ToArray();
    }

    public async Task<IAnime[]> SearchAnimeAsync(string searchTerm, int page, Tag[]? tags = null)
    {
        var animeList = new List<Anime>();
        var tmp = searchTerm == "" ? "" : searchTerm.Replace(" ", "_") + "/";
        var mainPUrl = string.Concat(originUrl, "buscar/", tmp, page);
        if (tags != null)
        {
            mainPUrl = $"{originUrl}/directorio/{page}/{tags.FirstOrDefault()?.Value}/";
        }

        var browser = new ScrapingBrowser();
        var webPage = await browser.NavigateToPageAsync(new Uri(mainPUrl));

        var doc = webPage.Html.CssSelect("body").First();

        var removeDomain = (string e) => e.Replace(originUrl, "");

        foreach (var nodo in doc.CssSelect(".anime__page__content .row .col-md-6"))
        {
            var anime = new Anime();
            anime.Title = nodo.CssSelect("h5 a").First().InnerText;
            var temp = nodo.CssSelect(".anime__item a").First();
            anime.Url = removeDomain(temp.GetAttributeValue("href"));
            anime.Cover = nodo.CssSelect(".anime__item__pic").First().GetAttributeValue("data-setbg");
            anime.Provider = (Provider)GenProvider();
            anime.ProviderId = anime.Provider.Id;
            anime.Type = GetAnimeTypeByStr(nodo.CssSelect(".anime__item__text ul li.anime").First().InnerText);
            animeList.Add(anime);
        }

        return animeList.ToArray();
    }

    public async Task<IAnime> GetAnimeDetailsAsync(string requestUrl)
    {
        var mainPUrl = string.Concat(originUrl, requestUrl);
        var browser = new ScrapingBrowser { Encoding = Encoding.UTF8 };
        var webPage = await browser.NavigateToPageAsync(new Uri(mainPUrl));

        var doc = webPage.Html.CssSelect("body").First();

        var anime = new Anime();
        anime.Url = requestUrl;
        anime.Title = doc.CssSelect(".anime__details__title h3").First().InnerText;
        anime.Cover = doc.CssSelect(".anime__details__pic").First().GetAttributeValue("data-setbg");
        anime.Description = doc.CssSelect(".sinopsis").First().InnerText;
        anime.Provider = (Provider)GenProvider();
        anime.ProviderId = anime.Provider.Id;
        var tempType = doc.SelectSingleNode(".//section[2]/div/div[1]/div/div[2]/div/div[2]/div/div[1]/ul/li[1]").InnerText;
        anime.Type = GetAnimeTypeByStr(tempType);

        anime.Status = doc.CssSelect("span.enemision").First().InnerText;

        foreach (var animeData in doc.CssSelect("div.row div.col-lg-6.col-md-6 ul li"))
        {
            var data = animeData.CssSelect("span")?.FirstOrDefault()?.InnerText;
            if (data.Contains("Genero"))
            {
                anime.GenreStr = string.Join(",", animeData.CssSelect("a").Select(x => x.InnerText.Trim()));
                break;
            }
        }

        anime.RemoteID = requestUrl.Replace("/", "");

        //do some magic things
        var lastlink = doc.CssSelect("a.numbers").Last().InnerText;
        var ttr = lastlink.Split(" - ");
        var lastEpisode = ttr[1];

        var lastchap = 1;
        if (!string.IsNullOrEmpty(lastEpisode))
            lastchap = int.Parse(lastEpisode);

        var chapters = new List<Chapter>();
        var preChapterCheck = await _client.OpenAsync(string.Concat(originUrl, requestUrl, "/", 0, "/"));
        if (!preChapterCheck.Url.Contains("404.shtml"))
        {
            chapters.Add(new()
            {
                Url = string.Concat(requestUrl, "/", 0, "/"),
                ChapterNumber = 0,
                Name = $"Episodio 0"
            });
        }

        for (var i = 1; i <= lastchap; i++)
        {
            var chapter = new Chapter();
            chapter.Url = string.Concat(requestUrl, "/", i, "/");
            chapter.ChapterNumber = i;
            chapter.Name = $"Episodio {i}";
            chapters.Add(chapter);
        }
        anime.Chapters = chapters.ToArray();
        await Task.CompletedTask;
        return anime;
    }

    public async Task<IVideoSource[]> GetVideoSources(string requestUrl)
    {
        var videoSource = new List<VideoSource>();
        var mainPUrl = string.Concat(originUrl, requestUrl);

        var browser = new ScrapingBrowser();
        var webPage = await browser.NavigateToPageAsync(new Uri(mainPUrl));
        var doc = webPage.Html.InnerHtml;

        var match = doc.SubstringBetween("var servers = ", ";");

        Debug.WriteLine(match);
        try
        {
            var prefJson = JArray.Parse(match);

            foreach (JObject vsource in prefJson.Children<JObject>())
            {
                var serverName = _serverConventions.GetServerName((string)vsource["server"]);
                if (string.IsNullOrEmpty(serverName))
                {
                    continue;
                }
                var iframe = originUrl + "/c1.php?u=" + (string)vsource["remote"] + "&s=" + serverName.ToLower();
                var tempframe = await browser.NavigateToPageAsync(new Uri(iframe));
                var iframedoc = tempframe.Html.SelectSingleNode(".//iframe").GetAttributeValue("src");
                var vSouce = new VideoSource();
                vSouce.Server = serverName;
                vSouce.Title = serverName;
                vSouce.Code = iframedoc;
                vSouce.Url = iframedoc;
                videoSource.Add(vSouce);
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
        return videoSource.OrderByDescending(s => s.Server).ToArray();
    }

    private static AnimeType GetAnimeTypeByStr(string strType)
    {
        return strType switch
        {
            "OVA" => AnimeType.OVA,
            "ONA" => AnimeType.OVA,
            "TV" => AnimeType.TV,
            "Movie" => AnimeType.MOVIE,
            "Special" => AnimeType.SPECIAL,
            _ => AnimeType.TV,
        };
    }

    public Tag[] GetTags()
    {
        return [
            new() { Name = "Español Latino", Value = "espaol-latino" },
            new() { Name = "Accion", Value = "accion" },
            new() { Name = "Aventura", Value = "aventura" },
            new() { Name = "Autos", Value = "autos" },
            new() { Name = "Comedia", Value = "comedia" },
            new() { Name = "Dementia", Value = "dementia" },
            new() { Name = "Demonios", Value = "demonios" },
            new() { Name = "Misterio", Value = "misterio" },
            new() { Name = "Drama", Value = "drama" },
            new() { Name = "Ecchi", Value = "ecchi" },
            new() { Name = "Fantasìa", Value = "fantasa" },
            new() { Name = "Juegos", Value = "juegos" },
            new() { Name = "Hentai", Value = "hentai" },
            new() { Name = "Historico", Value = "historico" },
            new() { Name = "Terror", Value = "terror" },
            new() { Name = "Magia", Value = "magia" },
            new() { Name = "Artes Marciales", Value = "artes-marciales" },
            new() { Name = "Mecha", Value = "mecha" },
            new() { Name = "Musica", Value = "musica" },
            new() { Name = "Parodia", Value = "parodia" },
            new() { Name = "Samurai", Value = "samurai" },
            new() { Name = "Romance", Value = "romance" },
            new() { Name = "Colegial", Value = "colegial" },
            new() { Name = "Sci-Fi", Value = "sci-fi" },
            new() { Name = "Shoujo Ai", Value = "shoujo-ai" },
            new() { Name = "Shounen Ai", Value = "shounen-ai" },
            new() { Name = "Space", Value = "space" },
            new() { Name = "Deportes", Value = "deportes" },
            new() { Name = "Super Poderes", Value = "super-poderes" },
            new() { Name = "Vampiros", Value = "vampiros" },
            new() { Name = "Yaoi", Value = "yaoi" },
            new() { Name = "Yuri", Value = "yuri" },
            new() { Name = "Harem", Value = "harem" },
            new() { Name = "Cosas de la vida", Value = "cosas-de-la-vida" },
            new() { Name = "Sobrenatural", Value = "sobrenatural" },
            new() { Name = "Militar", Value = "militar" },
            new() { Name = "Policial", Value = "policial" },
            new() { Name = "Psicologico", Value = "psicologico" },
            new() { Name = "Thriller", Value = "thriller" },
            new() { Name = "Isekai", Value = "isekai" }
        ];
    }
}
