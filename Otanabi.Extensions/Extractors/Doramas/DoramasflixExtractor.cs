using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using AngleSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Otanabi.Core.Helpers;
using Otanabi.Core.Models;
using Otanabi.Extensions.Contracts.Extractors;
using Otanabi.Extensions.Models.Doramasflix;

namespace Otanabi.Extensions.Extractors;

public class DoramasflixExtractor : IExtractor
{
    internal ServerConventions _serverConventions = new();
    internal readonly int extractorId = 13;
    internal readonly string sourceName = "DoramasFlix";
    internal static readonly string baseUrl = "https://doramasflix.in";
    internal readonly bool Persistent = true;
    internal readonly string Type = "DORAMA";

    #region API tools
    internal readonly string apiUrl = "https://sv1.fluxcedene.net/api/gql";
    internal static readonly string accessPlatform = "RxARncfg1S_MdpSrCvreoLu_SikCGMzE1NzQzODc3NjE2MQ==";
    internal readonly string mediaType = "application/json";
    #endregion

    private readonly IBrowsingContext _agClient;
    private readonly HttpClient _httpClient = new();

    public DoramasflixExtractor()
    {
        var config = Configuration.Default.WithDefaultLoader();
        _agClient = BrowsingContext.New(config);
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
        var tagValue = tags?.FirstOrDefault()?.Value;
        string json;
        if (!string.IsNullOrWhiteSpace(searchTerm) && string.IsNullOrWhiteSpace(tagValue))
        {
            if (page > 1)
            {
                return animeList.ToArray();
            }
            json = await SendRequest(RequestQueries.searchQuery.Replace("{0}", searchTerm.Replace(" ", "+")));
        }
        else if (!string.IsNullOrWhiteSpace(tagValue))
        {
            if (tagValue == "peliculas")
            {
                json = await SendRequest(RequestQueries.moviesQuery.Replace("{0}", $"{page}"));
            }
            else if (tagValue == "variedades")
            {
                json = await SendRequest(RequestQueries.varietiesQuery.Replace("{0}", $"{page}"));
            }
            else if (tagValue == "doramas")
            {
                json = await SendRequest(RequestQueries.latestUpdatesQuery.Replace("{0}", $"{page}"));
            }
            else
            {
                if (page > 1)
                {
                    return animeList.ToArray();
                }
                json = await SendRequest(RequestQueries.searchByGenreQuery.Replace("{0}", tagValue));
            }
        }
        else
        {
            json = await SendRequest(RequestQueries.latestUpdatesQuery.Replace("{0}", $"{page}"));
        }

        var prov = (Provider)GenProvider();

        if (!string.IsNullOrWhiteSpace(searchTerm) && string.IsNullOrWhiteSpace(tagValue))
        {
            var data = JsonConvert.DeserializeObject<SearchModel>(json);
            var pagination = (data.Data.SearchDorama ?? []).Concat(data.Data.SearchMovie ?? []);

            foreach (var animeObject in pagination)
            {
                var urlImg = animeObject.PosterPath ?? animeObject.Poster;
                var animeType = animeObject.Typename.ToLower() switch
                {
                    "dorama" => AnimeType.TV,
                    "episode" => AnimeType.TV,
                    "movie" => AnimeType.MOVIE,
                    _ => AnimeType.OTHER,
                };

                animeList.Add(new()
                {
                    Provider = prov,
                    ProviderId = prov.Id,
                    Title = $"{animeObject.Name}{(!string.IsNullOrEmpty(animeObject.NameEs) ? $" ({animeObject.NameEs})" : "")}",
                    Cover = ExternalOrInternalImg(urlImg, true),
                    Url = UrlSolverByType(animeObject.Typename, animeObject.Slug, animeObject.Id),
                    Type = animeType,
                });
            }
        }
        else if (!string.IsNullOrWhiteSpace(tagValue) && tagValue is not ("peliculas" or "variedades" or "doramas"))
        {
            var data = JsonConvert.DeserializeObject<ListDoramasModel>(json);
            foreach (var animeObject in data.Data.ListDoramas)
            {
                var urlImg = animeObject.PosterPath ?? animeObject.Poster;
                var animeType = animeObject.Typename.ToLower() switch
                {
                    "dorama" => AnimeType.TV,
                    "episode" => AnimeType.TV,
                    "movie" => AnimeType.MOVIE,
                    _ => AnimeType.OTHER,
                };

                animeList.Add(new()
                {
                    Provider = prov,
                    ProviderId = prov.Id,
                    Title = $"{animeObject.Name}{(!string.IsNullOrEmpty(animeObject.NameEs) ? $" ({animeObject.NameEs})" : "")}",
                    Cover = ExternalOrInternalImg(urlImg, true),
                    Url = UrlSolverByType(animeObject.Typename, animeObject.Slug, animeObject.Id),
                    Description = animeObject.Overview,
                    Type = animeType,
                });
            }
        }
        else
        {
            var data = JsonConvert.DeserializeObject<PaginationModel>(json);
            var pagination = data.Data.PaginationMovie ?? data.Data.PaginationDorama;
            foreach (var animeObject in pagination.Items)
            {
                var urlImg = animeObject.PosterPath ?? animeObject.Poster;
                var animeType = animeObject.Typename.ToLower() switch
                {
                    "dorama" => AnimeType.TV,
                    "episode" => AnimeType.TV,
                    "movie" => AnimeType.MOVIE,
                    _ => AnimeType.OTHER,
                };

                animeList.Add(new()
                {
                    Provider = prov,
                    ProviderId = prov.Id,
                    Title = $"{animeObject.Name}{(!string.IsNullOrEmpty(animeObject.NameEs) ? $" ({animeObject.NameEs})" : "")}",
                    Cover = ExternalOrInternalImg(urlImg, true),
                    Url = UrlSolverByType(animeObject.Typename, animeObject.Slug, animeObject.Id),
                    Description = animeObject.Overview,
                    Type = animeType,
                });
            }
        }

        return animeList.ToArray();
    }

    public async Task<IAnime> GetAnimeDetailsAsync(string requestUrl)
    {
        var doc = await _agClient.OpenAsync(requestUrl);
        if (doc.StatusCode != HttpStatusCode.OK)
        {
            throw new Exception("Anime could not be found");
        }

        var prov = (Provider)GenProvider();
        Anime anime = new();
        anime.Provider = prov;
        anime.ProviderId = prov.Id;
        anime.RemoteID = requestUrl.Replace("/", "");
        var scriptData = doc.Scripts.FirstOrDefault(x => x.TextContent.Contains("{\"props\":{\"pageProps\":{"))?.TextContent;
        try
        {
            var json = JObject.Parse(scriptData);
            var apolloState = (JObject?)json["props"]?["pageProps"]?["apolloState"];

            var doramaEntry = apolloState.Properties()
                .FirstOrDefault(p => System.Text.RegularExpressions.Regex.IsMatch(p.Name, @"\b(?:Movie|Dorama):[a-zA-Z0-9]+"));

            var dorama = (JObject?)doramaEntry.Value;

            var genres = string.Join(",", apolloState.Properties()
                .Where(p => p.Name.Contains("genres"))
                .Select(p => p.Value?["name"]?.ToString())
                .Where(n => !string.IsNullOrEmpty(n)));

            //var title = $"{dorama["name"]} ({dorama["name_es"]})";
            //var network = apolloState.Properties().FirstOrDefault(p => p.Name.Contains("networks"))?.Value?["name"]?.ToString() ?? "";
            //var artist = dorama["cast"]?["json"]?.FirstOrDefault()?["name"]?.ToString();

            var id = dorama["_id"]?.ToString();
            var type = dorama["__typename"]?.ToString()?.ToLower() ?? "";
            var poster = dorama["poster_path"]?.ToString() ?? "";
            var urlImg = !string.IsNullOrEmpty(poster) ? poster : (dorama["poster"]?.ToString() ?? "");
            var title = $"{dorama["name"]}{(!string.IsNullOrEmpty(dorama["name_es"]?.ToString()) ? $" ({dorama["name_es"]})" : "")}";
            var description = dorama["overview"]?.ToString()?.Trim() ?? "";

            var animeType = type switch
            {
                "dorama" => AnimeType.TV,
                "episode" => AnimeType.TV,
                "movie" => AnimeType.MOVIE,
                _ => AnimeType.OTHER,
            };
            anime.Type = animeType;
            anime.Title = title;
            anime.Description = description;
            anime.GenreStr = genres;
            anime.Status = type == "movie" ? "Finalizado" : "Desconocido";
            anime.Cover = ExternalOrInternalImg(urlImg);

            anime.Url = UrlSolverByType(type, dorama["slug"]?.ToString(), id);
            anime.Chapters = await GetChapters(anime.Url);
        }
        catch
        {
            throw new Exception("Anime could not be found");
        }
        return anime;
    }

    private async Task<List<Chapter>> GetChapters(string requestUrl)
    {
        var id = requestUrl.GetParameter("id");
        var chapters = new List<Chapter>();
        if (requestUrl.Contains("peliculas-online"))
        {
            chapters.Add(new Chapter()
            {
                ChapterNumber = 1,
                Name = "Película",
                Url = requestUrl
            });
        }
        else
        {
            var seasonsJson = await SendRequest(RequestQueries.seasonsQuery.Replace("{0}", id));
            var seasonData = JsonConvert.DeserializeObject<SeasonModel>(seasonsJson);
            foreach (var season in seasonData.Data.ListSeasons)
            {
                var episodesJson = await SendRequest(RequestQueries.episodesQuery.Replace("{0}", id).Replace("{1}", season.SeasonNumber.ToString("N0")));
                var episodesData = JsonConvert.DeserializeObject<EpisodeModel>(episodesJson);
                foreach (var episodeObject in episodesData.Data.ListEpisodes
                    .Where(x => string.IsNullOrWhiteSpace(x.AirDate) || (DateTime.TryParse(x.AirDate, out var date) && date.Date <= DateTime.Now.Date)))
                {
                    chapters.Add(new Chapter()
                    {
                        ChapterNumber = (int)episodeObject.EpisodeNumber,
                        Name = $"T{episodeObject.SeasonNumber} - E{episodeObject.EpisodeNumber} " + (!string.IsNullOrEmpty(episodeObject.Name) ? $"- Capítulo {episodeObject.EpisodeNumber}" : $"- {episodeObject.Name}"),
                        Url = UrlSolverByType("episode", episodeObject.Slug),
                        ReleaseDate = DateTime.TryParse(episodeObject.AirDate, out var releaseDate) ? releaseDate.ToString("dd/MM/yyyy") : ""
                    });
                }
            }
        }

        return chapters.OrderBy(x => x.ChapterNumber).ToList();
    }

    public async Task<IVideoSource[]> GetVideoSources(string requestUrl)
    {
        var sources = new List<VideoSource>();
        var document = await _agClient.OpenAsync(requestUrl);

        var scriptElement = document.Scripts.FirstOrDefault(s => s.TextContent.Contains("{\"props\":{\"pageProps\":{")) ?? throw new Exception("No se encontró el script con el JSON esperado.");
        var jsonData = scriptElement.TextContent;

        var root = JObject.Parse(jsonData);

        if (root["props"]?["pageProps"]?["apolloState"] is not JObject apolloState)
        {
            throw new Exception("No se pudo obtener 'apolloState'.");
        }

        // Buscar objeto del episodio o película/dorama
        var episodeItem = apolloState.Properties().FirstOrDefault(x => x.Name.Contains("Episode:"));

        if (episodeItem?.Value is not JObject episode)
        {
            episode = apolloState.Properties()
                .FirstOrDefault(x => Regex.IsMatch(x.Name, @"\b(?:Movie|Dorama):[a-zA-Z0-9]+"))
                ?.Value as JObject;
        }

        var linksOnline = episode?["links_online"]?["json"] as JArray;

        var bMovies = apolloState.Properties().Any(x => x.Name.Contains("ROOT_QUERY.getMovieLinks("));

        if (bMovies && linksOnline == null)
        {
            linksOnline = apolloState.Properties()
                .FirstOrDefault(x => x.Name.Contains("ROOT_QUERY.getMovieLinks("))
                ?.Value?["links_online"]?["json"] as JArray;
        }

        // Procesar linksOnline (si existen)
        if (linksOnline != null)
        {
            foreach (var item in linksOnline)
            {
                if (item is not JObject obj)
                {
                    continue;
                }

                var link = obj["link"]?.ToString();
                var server = obj["server"]?.ToString();
                var lang = GetLang(obj["lang"]?.ToString() ?? "");

                if (!string.IsNullOrEmpty(link))
                {
                    var realLink = await GetRealLinkAsync(link);
                    var serverName = _serverConventions.GetServerName(server);
                    sources.Add(new()
                    {
                        Server = serverName,
                        Title = $"{lang} {serverName}".Trim(),
                        Url = realLink,
                    });
                }
            }
        }
        else
        {
            // Si no hay linksOnline, buscar en ROOT_QUERY.listProblems(
            List<(string Link, string Server, string Lang)> problemLinks = apolloState.Properties()
                .Where(x => x.Name.Contains("ROOT_QUERY.listProblems("))
                .Select(entry =>
                {
                    var server = entry.Value?["server"]?["json"] as JObject;
                    var link = server?["link"]?.ToString();
                    var serverName = server?["server"]?.ToString();
                    var lang = server?["lang"]?.ToString() ?? "";

                    return !string.IsNullOrEmpty(link) ? (link, GetServer(serverName), GetLang(lang)) : ((string, string, string)?)null;
                })
                .Where(x => x != null)
                .DistinctBy(x => x.Value.Item1)
                .Select(x => x.Value)
                .ToList();


            foreach (var problemLink in problemLinks)
            {
                var realLink = await GetRealLinkAsync(problemLink.Link);
                var serverName = _serverConventions.GetServerName(problemLink.Server);
                sources.Add(new()
                {
                    Server = serverName,
                    Title = $"{problemLink.Lang} {serverName}".Trim(),
                    Url = realLink,
                });
            }
        }


        return sources.ToArray();
    }

    public Tag[] GetTags()
    {
        return
        [
            new() { Name = "Doramas", Value = "doramas" },
            new() { Name = "Películas", Value = "peliculas" },
            new() { Name = "Variedades", Value = "variedades" },
            new() { Name = "Acción", Value = "accion" },
            new() { Name = "Aventura", Value = "aventura" },
            new() { Name = "Ciencia ficción", Value = "ciencia-ficcion" },
            new() { Name = "Comedia", Value = "comedia" },
            new() { Name = "Crimen", Value = "crimen" },
            new() { Name = "Documental", Value = "documental" },
            new() { Name = "Drama", Value = "drama" },
            new() { Name = "Familia", Value = "familia" },
            new() { Name = "Fantasía", Value = "fantasia" },
            new() { Name = "History", Value = "history" },
            new() { Name = "Misterio", Value = "misterio" },
            new() { Name = "Music", Value = "music" },
            new() { Name = "Política", Value = "politica" },
            new() { Name = "Romance", Value = "romance" },
            new() { Name = "Soap", Value = "soap" },
            new() { Name = "Terror", Value = "terror" },
            new() { Name = "Thriller", Value = "thriller" },
            new() { Name = "War", Value = "war" }
        ];
    }

    #region Private methods

    private async Task<string> SendRequest(string query, string api = null)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, api ?? apiUrl);
            request.Headers.Add("authority", "sv1.fluxcedene.net");
            request.Headers.Add("accept", "application/json, text/plain, */*");
            request.Headers.Add("origin", baseUrl);
            request.Headers.Add("referer", $"{baseUrl}/");
            request.Headers.Add("platform", "doramasflix");
            request.Headers.Add("authorization", "Bear");
            request.Headers.Add("x-access-jwt-token", "");
            request.Headers.Add("x-access-platform", accessPlatform);
            request.Headers.Add("Cookie", "_mcnc=1");
            request.Content = new StringContent(query, null, mediaType);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    private static string UrlSolverByType(string type, string slug, string id = "")
    {
        return type.ToLower() switch
        {
            "dorama" => $"{baseUrl}/doramas-online/{slug}?id={id}",
            "episode" => $"{baseUrl}/episodios/{slug}",
            "movie" => $"{baseUrl}/peliculas-online/{slug}?id={id}",
            _ => string.Empty,
        };
    }

    private static string ExternalOrInternalImg(string url, bool isThumb = false)
    {
        if (url.Contains("https"))
        {
            return url;
        }
        else if (isThumb)
        {
            return $"https://image.tmdb.org/t/p/w220_and_h330_face{url}";
        }
        else
        {
            return $"https://image.tmdb.org/t/p/w500{url}";
        }
    }

    public async Task<string> GetRealLinkAsync(string link)
    {
        if (!link.Contains("fkplayer.xyz"))
        {
            return link;
        }

        // Parsear HTML con AngleSharp
        var document = await _agClient.OpenAsync(link);
        var script = document.Scripts.FirstOrDefault(x => x.TextContent.Contains("{\"props\":{\"pageProps\":{"))?.TextContent;

        if (string.IsNullOrWhiteSpace(script))
        {
            return link;
        }

        // Extraer el JSON dentro del script
        var jsonStart = script.IndexOf("{\"props\":");
        var json = script.Substring(jsonStart);
        var parsed = JObject.Parse(json);

        var token = parsed["props"]?["pageProps"]?["token"] ?? parsed["query"]?["token"];
        if (token == null)
        {
            return link;
        }

        // Crear body para la petición POST
        var payload = new JObject { ["token"] = token.ToString() }.ToString();
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        // Crear headers
        var request = new HttpRequestMessage(HttpMethod.Post, "https://fkplayer.xyz/api/decoding");
        request.Headers.Add("origin", $"https://{new Uri(link).Host}");
        request.Content = content;

        // Hacer POST
        var postResponse = await _httpClient.SendAsync(request);
        var responseJson = await postResponse.Content.ReadAsStringAsync();
        var decoded = JObject.Parse(responseJson);

        var base64Link = decoded["link"]?.ToString();
        if (base64Link == null)
        {
            return link;
        }

        // Decodificar Base64
        var bytes = Convert.FromBase64String(base64Link);
        return Encoding.UTF8.GetString(bytes);
    }

    private static string GetLang(string langCode)
    {
        Dictionary<string, string> Languages = new()
        {
            { "36", "[ENG]" },
            { "37", "[CAST]" },
            { "38", "[LAT]" },
            { "192", "[SUB]" },
            { "1327", "[POR]" },
            { "13109", "[COR]" },
            { "13110", "[JAP]" },
            { "13111", "[MAN]" },
            { "13112", "[TAI]" },
            { "13113", "[FIL]" },
            { "13114", "[IND]" },
            { "343422", "[VIET]" },
        };
        return Languages.TryGetValue(langCode, out var lang) ? lang : "";
    }

    private static string GetServer(string serverCode)
    {
        Dictionary<string, string> Servers = new()
        {
            { "1113", "Ok" },
            { "3889", "Mixdrop" },
            { "7286", "Dood" },
            { "8309", "Streamtape" },
            { "1230", "Voe" },
            { "1233", "Uqload" },
            { "1234", "Mp4Upload" },
            { "55532", "Vudeo" },
            { "38585", "Streamwish" },
            { "958695", "Filemoon" },
            { "576857", "VidHide" },
        };
        return Servers.TryGetValue(serverCode, out var name) ? name : "";
    }

    #endregion
}