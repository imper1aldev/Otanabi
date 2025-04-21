using System.Net;
using System.Web;
using AngleSharp;
using AngleSharp.Dom;
using Newtonsoft.Json;
using Otanabi.Core.Helpers;
using Otanabi.Core.Models;
using Otanabi.Extensions.Contracts.Extractors;
using Otanabi.Extensions.Models.Cuevanaeu;

namespace Otanabi.Extensions.Extractors;

public class CuevanaeuExtractor : IExtractor
{
    internal ServerConventions _serverConventions = new();
    internal readonly int extractorId = 11;
    internal readonly string sourceName = "CuevanaEu";
    internal readonly string baseUrl = "https://www.cuevana3.eu";
    internal readonly bool Persistent = true;
    internal readonly string Type = "MOVIE";

    private readonly IBrowsingContext _client;

    public CuevanaeuExtractor()
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
            url = $"{baseUrl}/search?q={HttpUtility.UrlEncode(searchTerm)}";
        }
        else if (tags != null && tags.Length > 0)
        {
            var genre = tags.FirstOrDefault()?.Value;
            url = $"{baseUrl}/{genre}/page/{page}";
        }
        else
        {
            url = $"{baseUrl}/peliculas/estrenos/page/{page}";
        }

        var doc = await _client.OpenAsync(url);
        var prov = (Provider)GenProvider();
        var script = doc.Scripts.FirstOrDefault(s => s.TextContent.Contains("{\"props\":{\"pageProps\":{"))?.TextContent;
        var responseJson = JsonConvert.DeserializeObject<PopularAnimeList>(script);
        foreach (var animeItem in responseJson.Props.PageProps.Movies)
        {
            var preSlug = animeItem.Url?.Slug ?? "";
            var type = preSlug.StartsWith("series") ? "ver-serie" : "ver-pelicula";
            animeList.Add(new()
            {
                Title = animeItem.Titles?.Name ?? "",
                Cover = animeItem.Images?.Poster?.Replace("/original/", "/w200/") ?? "",
                Url = $"{baseUrl}/{type}/{animeItem.Slug?.Name}",
                Provider = prov,
                ProviderId = prov.Id,
                Type = preSlug.StartsWith("series") ? AnimeType.TV : AnimeType.MOVIE,
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
        var script = doc.Scripts.FirstOrDefault(s => s.TextContent.Contains("{\"props\":{\"pageProps\":{"))?.TextContent;
        var responseJson = JsonConvert.DeserializeObject<AnimeEpisodesList>(script);
        Anime anime;
        if (requestUrl.Contains("/ver-serie/"))
        {
            var data = responseJson.Props?.PageProps?.ThisSerie;
            anime = new()
            {
                Provider = prov,
                ProviderId = prov.Id,
                Url = requestUrl,
                Title = data?.Titles?.Name ?? "",
                Description = data?.Overview ?? "",
                Status = "Desconocido",
                GenreStr = string.Join(",", data?.Genres?.Select(g => g.Name)),
                RemoteID = requestUrl.Replace("/", ""),
                Cover = data?.Images?.Poster?.Replace("/original/", "/w500/"),
                Type = AnimeType.TV,

                Chapters = await GetChapters(requestUrl)
            };
        }
        else
        {
            var data = responseJson.Props?.PageProps?.ThisMovie;
            anime = new()
            {
                Provider = prov,
                ProviderId = prov.Id,
                Url = requestUrl,
                Title = data?.Titles?.Name ?? "",
                Description = data?.Overview ?? "",
                Status = "Finalizado",
                GenreStr = string.Join(",", data?.Genres?.Select(g => g.Name)),
                RemoteID = requestUrl.Replace("/", ""),
                Cover = data?.Images?.Poster?.Replace("/original/", "/w500/"),
                Type = AnimeType.MOVIE,

                Chapters = await GetChapters(requestUrl)
            };
        }

        return anime;
    }

    private async Task<List<Chapter>> GetChapters(string requestUrl)
    {
        var chapters = new List<Chapter>();
        var doc = await _client.OpenAsync(requestUrl);
        if (requestUrl.Contains("/ver-serie/"))
        {
            var script = doc.Scripts.FirstOrDefault(s => s.TextContent.Contains("{\"props\":{\"pageProps\":{"))?.TextContent;
            var responseJson = JsonConvert.DeserializeObject<AnimeEpisodesList>(script);
            foreach (var ep in responseJson.Props?.PageProps?.ThisSerie?.Seasons?.SelectMany(c => c.Episodes))
            {
                chapters.Add(new()
                {
                    ChapterNumber = ep.Number ?? 0,
                    Name = $"T{ep.Slug?.Season} - Episodio {ep.Slug?.Episode}",
                    Url = $"{baseUrl}/episodio/{ep.Slug?.Name}-temporada-{ep.Slug?.Season}-episodio-{ep.Slug?.Episode}",
                    ReleaseDate = DateTime.TryParse(ep.ReleaseDate, out var d) ? d.ToString("dd/MM/yyyy") : ""
                });
            }
        }
        else
        {
            ///document.querySelector('.home__slider_content > div.genres.rating > span:nth-child(1) > a').textContent
            chapters.Add(new()
            {
                ChapterNumber = 1,
                Name = doc.QuerySelector("header .Title")?.TextContent?.Trim(),
                Url = requestUrl,
                ReleaseDate = doc.QuerySelector(".meta span:nth-child(1)")?.TextContent,
                Extraval = doc.QuerySelector(".meta span:nth-child(2)")?.TextContent
            });
        }

        return chapters.OrderBy(x => x.ChapterNumber).ToList();
    }

    public async Task<IVideoSource[]> GetVideoSources(string requestUrl)
    {
        var sources = new List<VideoSource>();
        var doc = await _client.OpenAsync(requestUrl);
        var script = doc.Scripts.FirstOrDefault(s => s.TextContent.Contains("{\"props\":{\"pageProps\":{"))?.TextContent;
        var responseJson = JsonConvert.DeserializeObject<AnimeEpisodesList>(script);
        var data = new Videos();

        if (requestUrl.Contains("/episodio/"))
        {
            data = responseJson.Props?.PageProps?.Episode?.Videos;
        }
        else
        {
            data = responseJson.Props?.PageProps?.ThisMovie?.Videos;
        }

        var serverLists = new List<IEnumerable<Server>>
        {
            data.Latino,
            data.Spanish,
            data.English,
            data.Japanese
        };

        var activeServers = serverLists.FirstOrDefault(servers => servers != null && servers.Any());
        if (activeServers != null)
        {
            foreach (var server in activeServers)
            {
                try
                {
                    var video = await GetVideoInfo(server.Result, server.Cyberlocker);
                    sources.Add(video);
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        return sources.OrderByDescending(s => s.Server).ToArray();
    }

    private async Task<VideoSource> GetVideoInfo(string requestUrl, string cyberlocker)
    {
        var serverDoc = await _client.OpenAsync(requestUrl);
        var urls = serverDoc.Scripts.FirstOrDefault(s => s.TextContent.Contains("var message"))?.TextContent;
        var url = serverDoc.Scripts.FirstOrDefault(s => s.TextContent.Contains("var message"))
                            ?.TextContent?.SubstringAfter("var url = '")?.SubstringBefore("'") ?? "";
        var serverName = _serverConventions.GetServerName(cyberlocker);

        return new VideoSource()
        {
            Server = serverName,
            Title = serverName,
            Url = url,
        };
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
}