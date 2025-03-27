using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Otanabi.Core.Anilist;
using Otanabi.Core.Anilist.Enums;
using Otanabi.Core.Anilist.Models;
using Otanabi.Core.Models;

namespace Otanabi.Core.Services;

public class AnilistService
{
    private readonly AnilistClient _client;

    public AnilistService()
    {
        _client = new AnilistClient();
    }

    public async Task<List<string>> GetTagsASync()
    {
        var query = _client.GetQuery(QueryType.GetTags);

        var response = await _client.SendQueryAsync(query);

        var data = response["data"];

        List<string> temp = new List<string>();

        foreach (var el in data["genres"])
        {
            temp.Add((string)el);
        }
        foreach (var el in data["tags"])
        {
            temp.Add((string)el["name"]);
        }

        return temp;
    }

    public async Task<(Media[], PageInfo)> GetSeasonal(
        int page = 1,
        MediaSeason season = MediaSeason.Fall,
        int seasonYear = 2024,
        MediaStatus? status = MediaStatus.Finished,
        bool isAdult = false,
        MediaType type = MediaType.Anime,
        MediaFormat format = MediaFormat.Tv,
        MediaSort[] sortFilter = null
    )
    {
        sortFilter ??= new MediaSort[] { MediaSort.Popularity_Desc };
        var query = _client.GetQuery(QueryType.Seasonal);
        var variables = new Dictionary<string, object>
        {
            { "perPage", 10 },
            { "page", 1 },
            { "year", seasonYear },
            { "season", season.ToString().ToUpper() },
            { "type", type.ToString().ToUpper() },
            { "format", format.ToString().ToUpper() },
            //{ "sort", sortFilter.Select(x => x.ToString().ToUpper()).ToArray() },
        };
        var response = await _client.SendQueryAsync(query, variables);
        var data = response["data"]["Page"];
        var pdata = data["pageInfo"];
        var pageinfo = new PageInfo { HasNextPage = (bool)pdata["hasNextPage"], Total = (int)pdata["total"] };

        var medias = new List<Media>();
        foreach (var media in data["media"])
        {
            try
            {
                var mediaItem = new Media
                {
                    Id = (int)media["id"],
                    Title = new MediaTitle
                    {
                        English = (string)media["title"]["english"],
                        Romaji = (string)media["title"]["romaji"],
                        Native = (string)media["title"]["native"],
                    },
                    BannerImage = (string)media["bannerImage"],
                    CoverImage = new MediaCoverImage
                    {
                        Color = (string)media["coverImage"]["color"],
                        ExtraLarge = (string)media["coverImage"]["extraLarge"],
                    },
                    Description = RemoveHtmlTags((string)media["description"]),
                    Genres = media["genres"].Select(x => (string)x).ToArray(),
                    Popularity = (int?)media["popularity"] ?? 0,
                    AverageScore = (int?)media["averageScore"] ?? 0,
                    Status = ConvertToMediastatus((string)media["status"]),
                    Format = ConvertToMediaFormat((string)media["format"]),
                    StartDate = new FuzzyDate
                    {
                        Day = (int?)media["startDate"]["day"] ?? 0,
                        Month = (int?)media["startDate"]["month"] ?? 0,
                        Year = (int?)media["startDate"]["year"] ?? 0,
                    },
                    EndDate = new FuzzyDate
                    {
                        Day = (int?)media["endDate"]["day"] ?? 0,
                        Month = (int?)media["endDate"]["month"] ?? 0,
                        Year = (int?)media["endDate"]["year"] ?? 0,
                    },
                    Duration = (int?)media["duration"] ?? 0,
                    Episodes = (int?)media["episodes"] ?? 0,
                    Season = ConvertToMediaSeason((string)media["season"]),
                    SeasonYear = (int?)media["seasonYear"] ?? 0,
                };
                medias.Add(mediaItem);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        return (medias.ToArray(), pageinfo);
    }

    public async Task<(List<Anime>, PageInfo)> GetSeasonalFullDetail(
        int page = 1,
        MediaSeason season = MediaSeason.Fall,
        int seasonYear = 2024,
        MediaStatus? status = MediaStatus.Finished,
        bool isAdult = false
    )
    {
        //var animes = new List<Anime>();
        //var mediaItems = response.Data.Media;

        //foreach (var media in mediaItems)
        //{
        //    animes.Add(
        //        new Anime()
        //        {
        //            Title = media.Title.Romaji,
        //            Cover = media.CoverImage.ExtraLarge,
        //            Description = RemoveHtmlTags(media.Description),
        //            GenreStr = string.Join(",", media.Genres),
        //        }
        //    );
        //}

        return (null, new PageInfo());
    }

    public async Task<(Media[], PageInfo)> SearchMedia(
        int page,
        string searchTerm,
        int? year,
        MediaSeason? season,
        MediaStatus? status,
        MediaFormat[] formats = null,
        string[] genres = null,
        MediaSort[] sortFilter = null,
        bool isAdult = false,
        int perPage = 30
    )
    {
        sortFilter ??= new MediaSort[] { MediaSort.Popularity_Desc };
        var query = _client.GetQuery(QueryType.Search);
        var variables = new Dictionary<string, object>
        {
            { "page", 1 },
            { "perPage", perPage },
            { "type", "ANIME" },
        };
        if (!string.IsNullOrEmpty(searchTerm))
        {
            variables.Add("search", searchTerm);
        }
        if (year != null)
        {
            variables.Add("year", year);
        }
        if (season != null)
        {
            variables.Add("season", season.ToString().ToUpper());
        }
        if (status != null)
        {
            variables.Add("status", status.ToString().ToUpper());
        }
        if (formats != null)
        {
            variables.Add("format", arrayToQuery(formats));
        }
        if (genres != null)
        {
            variables.Add("genres", genres);
        }
        if (sortFilter != null)
        {
            variables.Add("sort", arrayToQuery(sortFilter));
        }

        var response = await _client.SendQueryAsync(query, variables);
        var data = response["data"]["Page"];
        var pdata = data["pageInfo"];
        var pageinfo = new PageInfo { HasNextPage = (bool)pdata["hasNextPage"], Total = (int)pdata["total"] };

        var medias = new List<Media>();
        foreach (var media in data["media"])
        {
            try
            {
                var mediaItem = new Media
                {
                    Id = (int)media["id"],
                    Title = new MediaTitle
                    {
                        English = (string)media["title"]["english"],
                        Romaji = (string)media["title"]["romaji"],
                        Native = (string)media["title"]["native"],
                    },
                    BannerImage = (string)media["bannerImage"],
                    CoverImage = new MediaCoverImage
                    {
                        Color = (string)media["coverImage"]["color"],
                        ExtraLarge = (string)media["coverImage"]["extraLarge"],
                    },
                    Description = RemoveHtmlTags((string)media["description"]),
                    Genres = media["genres"].Select(x => (string)x).ToArray(),
                    Popularity = (int?)media["popularity"] ?? 0,
                    AverageScore = (int?)media["averageScore"] ?? 0,
                    Status = ConvertToMediastatus((string)media["status"]),
                    Format = ConvertToMediaFormat((string)media["format"]),
                    StartDate = new FuzzyDate
                    {
                        Day = (int?)media["startDate"]["day"] ?? 0,
                        Month = (int?)media["startDate"]["month"] ?? 0,
                        Year = (int?)media["startDate"]["year"] ?? 0,
                    },
                    EndDate = new FuzzyDate
                    {
                        Day = (int?)media["endDate"]["day"] ?? 0,
                        Month = (int?)media["endDate"]["month"] ?? 0,
                        Year = (int?)media["endDate"]["year"] ?? 0,
                    },
                    Duration = (int?)media["duration"] ?? 0,
                    Episodes = (int?)media["episodes"] ?? 0,
                    Season = ConvertToMediaSeason((string)media["season"]),
                    SeasonYear = (int?)media["seasonYear"] ?? 0,
                };
                medias.Add(mediaItem);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        return (medias.ToArray(), pageinfo);
    }

    public async Task<Media> GetMediaByIdAsync(int id)
    {
        var variables = new Dictionary<string, object> { { "id", id } };

        var query = _client.GetQuery(QueryType.ById);

        var response = await _client.SendQueryAsync(query, variables);

        var data = response["data"]["Media"];

        var anime = new Media
        {
            Id = id,
            Title = new MediaTitle
            {
                English = (string)data["title"]["english"],
                Romaji = (string)data["title"]["romaji"],
                Native = (string)data["title"]["native"],
            },
            BannerImage = (string)data["bannerImage"],
            CoverImage = new MediaCoverImage { Color = (string)data["coverImage"]["color"], ExtraLarge = (string)data["coverImage"]["extraLarge"] },
            Description = RemoveHtmlTags((string)data["description"]),
            Genres = data["genres"].Select(x => (string)x).ToArray(),
            MeanScore = (int?)data["meanScore"] ?? 0,
            Popularity = (int?)data["popularity"] ?? 0,
            AverageScore = (int?)data["averageScore"] ?? 0,
            Status = ConvertToMediastatus((string)data["status"]),
            Format = ConvertToMediaFormat((string)data["format"]),
            StartDate = new FuzzyDate
            {
                Day = (int?)data["startDate"]["day"] ?? 0,
                Month = (int?)data["startDate"]["month"] ?? 0,
                Year = (int?)data["startDate"]["year"] ?? 0,
            },
            EndDate = new FuzzyDate
            {
                Day = (int?)data["endDate"]["day"] ?? 0,
                Month = (int?)data["endDate"]["month"] ?? 0,
                Year = (int?)data["endDate"]["year"] ?? 0,
            },
            Duration = (int?)data["duration"] ?? 0,
            Episodes = (int?)data["episodes"] ?? 0,
            Season = ConvertToMediaSeason((string)data["season"]),
            SeasonYear = (int?)data["seasonYear"] ?? 0,
        };
        List<MediaStreamingEpisode> episodes = [];
        foreach (var item in data["streamingEpisodes"])
        {
            var episode = new MediaStreamingEpisode
            {
                Site = (string)item["site"],
                Thumbnail = (string)item["thumbnail"],
                Title = (string)item["title"],
                Url = (string)item["url"],
                Number = (int)GetEpisodeNumber((string)item["title"]),
            };
            episodes.Add(episode);
        }

        if (episodes.Count == 0)
        {
            for (int i = 0; i < (int)data["episodes"]; i++)
            {
                var episode = new MediaStreamingEpisode
                {
                    Site = "",
                    Thumbnail = "//Assets/OtanabiSplash.png",
                    Title = $"Episode {i + 1}",
                    Url = "",
                    Number = i + 1,
                };
                episodes.Add(episode);
            }
        }

        anime.StreamingEpisodes = episodes;

        return anime;
    }

    private static string RemoveHtmlTags(string html)
    {
        return string.IsNullOrEmpty(html) ? html : Regex.Replace(html, @"<.*?>", string.Empty);
    }

    private static int? GetEpisodeNumber(string text)
    {
        Match match = Regex.Match(text, @"Episode (\d+)");
        return match.Success ? int.Parse(match.Groups[1].Value) : (int?)null;
    }

    private static MediaStatus ConvertToMediastatus(string param)
    {
        return param switch
        {
            "FINISHED" => MediaStatus.Finished,
            "RELEASING" => MediaStatus.Releasing,
            "NOT_YET_RELEASED" => MediaStatus.NotYetReleased,
            "CANCELLED" => MediaStatus.Cancelled,
            _ => MediaStatus.Finished,
        };
    }

    private static MediaFormat ConvertToMediaFormat(string param)
    {
        return param switch
        {
            "TV" => MediaFormat.Tv,
            "TV_SHORT" => MediaFormat.TvShort,
            "MOVIE" => MediaFormat.Movie,
            "SPECIAL" => MediaFormat.Special,
            "OVA" => MediaFormat.Ova,
            "ONA" => MediaFormat.Ona,
            "MUSIC" => MediaFormat.Music,
            _ => MediaFormat.Tv,
        };
    }

    private static MediaSeason ConvertToMediaSeason(string param)
    {
        return param switch
        {
            "WINTER" => MediaSeason.Winter,
            "SPRING" => MediaSeason.Spring,
            "SUMMER" => MediaSeason.Summer,
            "FALL" => MediaSeason.Fall,
            _ => MediaSeason.Winter,
        };
    }

    private List<string> arrayToQuery(MediaFormat[] formats)
    {
        return arrayToQuery(formats.Select(x => x.ToString().ToUpper()).ToArray());
    }

    private List<string> arrayToQuery(MediaSort[] sort)
    {
        return arrayToQuery(sort.Select(x => x.ToString().ToUpper()).ToArray());
    }

    private List<string> arrayToQuery(string[] arr)
    {
        var test = arr.Select(x => x.ToString().ToUpper()).ToArray();

        return test.ToList();
    }
}
