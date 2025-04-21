using System.Net;
using System.Web;
using AngleSharp;
using Otanabi.Core.Helpers;
using Otanabi.Core.Models;
using Otanabi.Extensions.Contracts.Extractors;

namespace Otanabi.Extensions.Extractors;

public class HentailaExtractor : IExtractor
{
    internal ServerConventions _serverConventions = new();
    internal readonly int extractorId = 12;
    internal readonly string sourceName = "HentaiLA";
    internal readonly string baseUrl = "https://www5.hentaila.com";
    internal readonly bool Persistent = true;
    internal readonly string Type = "ANIME";

    private readonly IBrowsingContext _client;

    public HentailaExtractor()
    {
        var config = Configuration.Default.WithDefaultLoader();
        _client = BrowsingContext.New(config);
    }

    public IProvider GenProvider() =>
        new Provider
        {
            Id = extractorId,
            Name = sourceName,
            Url = baseUrl,
            Type = Type,
            Persistent = Persistent,
            IsNsfw = true,
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
            url = $"{baseUrl}/animes?buscar={HttpUtility.UrlEncode(searchTerm)}&pag={page}";
        }
        else if (tags != null && tags.Length > 0)
        {
            url = $"{baseUrl}/animes?genero={GenerateTagString(tags)}&pag={page}";
        }
        else
        {
            url = $"{baseUrl}/directorio?filter=recent&p={page}";
        }

        var doc = await _client.OpenAsync(url);
        var prov = (Provider)GenProvider();
        foreach (var element in doc.QuerySelectorAll("div.columns main section.section div.grid.hentais article.hentai"))
        {
            animeList.Add(new()
            {
                Title = element.QuerySelector("header.h-header h2")?.TextContent?.Trim(),
                Cover = element.QuerySelector("div.h-thumb figure img").GetImageUrl(),
                Url = element.QuerySelector("a").GetAbsoluteUrl("href"),
                Provider = prov,
                ProviderId = prov.Id,
                Type = AnimeType.OTHER
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
        Anime anime = new()
        {
            Provider = prov,
            ProviderId = prov.Id,
            Url = requestUrl,
            Title = doc.QuerySelector("article.hentai-single header.h-header h1")?.TextContent?.Trim(),
            Description = doc.QuerySelector("article.hentai-single div.h-content p")?.TextContent?.TrimAll(),
            Status = doc.QuerySelectorAll("article.hentai-single span.status-on").Any() ? "En Emisión" : "Finalizado",
            GenreStr = string.Join(",", doc.QuerySelectorAll("article.hentai-single footer.h-footer nav.genres a.btn.sm").Select(x => WebUtility.HtmlDecode(x.TextContent?.Trim())).ToList()),
            RemoteID = requestUrl.Replace("/", ""),
            Cover = doc.QuerySelector("div.h-thumb figure img").GetImageUrl(),
            Type = AnimeType.OTHER,

            Chapters = await GetChapters(requestUrl)
        };
        return anime;
    }

    private async Task<List<Chapter>> GetChapters(string requestUrl)
    {
        var _httpClient = new HttpClient();
        var doc = await _client.OpenAsync(requestUrl);
        var animeId = doc.Url.SubstringAfter("hentai-").ToLower().TrimAll();
        var chapters = new List<Chapter>();
        foreach (var chapter in doc.QuerySelectorAll("div.episodes-list article"))
        {
            var numEp = chapter.QuerySelector("a")
                                .GetAbsoluteUrl("href")
                                .SubstringAfter($"/ver/{animeId}-")
                                .Replace($"/ver/{animeId}-", "");

            chapters.Add(new()
            {
                ChapterNumber = numEp.ToIntOrNull() ?? 0,
                Name = $"Episodio {numEp}",
                Url = $"{baseUrl}/ver/{animeId}-{numEp}"
            });
        }
        return chapters.OrderBy(x => x.ChapterNumber).ToList();
    }

    public async Task<IVideoSource[]> GetVideoSources(string requestUrl)
    {
        var sources = new List<VideoSource>();
        var doc = await _client.OpenAsync(requestUrl);
        var scriptElement = doc.Scripts.FirstOrDefault(s => s.TextContent.Contains("var videos = ["))?.TextContent;
        var videoServers = scriptElement.Split("videos = ")[1].Split(";")[0].Replace("[[", "").Replace("]]", "");
        var videoServerList = videoServers.Split("],[");
        foreach (var it in videoServerList)
        {
            var server = it.Split(',').Select(a => a.Replace("\"", "")).ToList();
            var urlServer = server[1].Replace("\\/", "/");
            var name = server[0];
            var serverName = _serverConventions.GetServerName(name);
            sources.Add(new()
            {
                Server = serverName,
                Title = serverName,
                Url = urlServer,
            });
        }
        return sources.Where(x => !string.IsNullOrEmpty(x.Server)).ToArray();
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
            new() { Name = "Acción", Value = "accion" },
            new() { Name = "Aenime", Value = "aenime" },
            new() { Name = "Anime Latino", Value = "anime-latino" },
            new() { Name = "Artes Marciales", Value = "artes-marciales" },
            new() { Name = "Aventura", Value = "aventura" },
            new() { Name = "Aventuras", Value = "aventuras" },
            new() { Name = "Blu-ray", Value = "blu-ray" },
            new() { Name = "Carreras", Value = "carreras" },
            new() { Name = "Castellano", Value = "castellano" },
            new() { Name = "Ciencia Ficción", Value = "ciencia-ficcion" },
            new() { Name = "Comedia", Value = "comedia" },
            new() { Name = "Cyberpunk", Value = "cyberpunk" },
            new() { Name = "Demencia", Value = "demencia" },
            new() { Name = "Dementia", Value = "dementia" },
            new() { Name = "Demonios", Value = "demonios" },
            new() { Name = "Deportes", Value = "deportes" },
            new() { Name = "Drama", Value = "drama" },
            new() { Name = "Ecchi", Value = "ecchi" },
            new() { Name = "Escolares", Value = "escolares" },
            new() { Name = "Espacial", Value = "espacial" },
            new() { Name = "Fantasía", Value = "fantasia" },
            new() { Name = "Gore", Value = "gore" },
            new() { Name = "Harem", Value = "harem" },
            new() { Name = "Historia paralela", Value = "historia-paralela" },
            new() { Name = "Historico", Value = "historico" },
            new() { Name = "Horror", Value = "horror" },
            new() { Name = "Infantil", Value = "infantil" },
            new() { Name = "Josei", Value = "josei" },
            new() { Name = "Juegos", Value = "juegos" },
            new() { Name = "Latino", Value = "latino" },
            new() { Name = "Lucha", Value = "lucha" },
            new() { Name = "Magia", Value = "magia" },
            new() { Name = "Mecha", Value = "mecha" },
            new() { Name = "Militar", Value = "militar" },
            new() { Name = "Misterio", Value = "misterio" },
            new() { Name = "Monogatari", Value = "monogatari" },
            new() { Name = "Música", Value = "musica" },
            new() { Name = "Parodia", Value = "parodia" },
            new() { Name = "Parodias", Value = "parodias" },
            new() { Name = "Policía", Value = "policia" },
            new() { Name = "Psicológico", Value = "psicologico" },
            new() { Name = "Recuentos de la vida", Value = "recuentos-de-la-vida" },
            new() { Name = "Recuerdos de la vida", Value = "recuerdos-de-la-vida" },
            new() { Name = "Romance", Value = "romance" },
            new() { Name = "Samurai", Value = "samurai" },
            new() { Name = "Seinen", Value = "seinen" },
            new() { Name = "Shojo", Value = "shojo" },
            new() { Name = "Shonen", Value = "shonen" },
            new() { Name = "Shoujo", Value = "shoujo" },
            new() { Name = "Shounen", Value = "shounen" },
            new() { Name = "Sobrenatural", Value = "sobrenatural" },
            new() { Name = "Superpoderes", Value = "superpoderes" },
            new() { Name = "Suspenso", Value = "suspenso" },
            new() { Name = "Terror", Value = "terror" },
            new() { Name = "Vampiros", Value = "vampiros" },
            new() { Name = "Yaoi", Value = "yaoi" },
            new() { Name = "Yuri", Value = "yuri" },
        ];
    }

    #region Private methods

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

    #endregion
}