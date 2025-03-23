using System.Text.RegularExpressions;
using Otanabi.Core.Models;
using ZeroQL.Client;

namespace Otanabi.Core.Services;

public class AnilistService
{
    private readonly ZeroQLClient _client;

    public AnilistService()
    {
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("https://graphql.anilist.co");
        _client = new ZeroQLClient(httpClient);
    }

    public async Task<(List<Anime>, PageInfo)> GetSeasonal(
        int page = 1,
        MediaSeason season = MediaSeason.Fall,
        int seasonYear = 2024,
        MediaStatus? status = MediaStatus.Finished,
        bool isAdult = false,
        MediaType type = MediaType.Anime,
        MediaFormat format = MediaFormat.Tv
    )
    {
        var sortFilter = new[] { MediaSort.PopularityDesc };
        var response = await _client.Query(q =>
            q.Page(
                page: page,
                selector: p => new
                {
                    PageInfo = p.PageInfo(pi => new PageInfo { HasNextPage = pi.HasNextPage, Total = pi.Total }),
                    Media = p.Media(
                        season: season,
                        seasonYear: seasonYear,
                        format: format,
                        format_not: MediaFormat.Ona,
                        isAdult: isAdult,
                        type: type,
                        sort: sortFilter,
                        selector: m => new
                        {
                            Id = m.Id,
                            IdMal = m.IdMal,
                            Title = m.Title(t => new
                            {
                                Romaji = t.Romaji(),
                                Native = t.Native(),
                                English = t.English(),
                            }),
                            Status = m.Status(),
                            Description = m.Description(),
                            Genres = m.Genres,
                            CoverImage = m.CoverImage(ci => new { ExtraLarge = ci.ExtraLarge }),
                        }
                    ),
                }
            )
        );
        var animes = new List<Anime>();
        var mediaItems = response.Data.Media;

        foreach (var media in response.Data.Media)
        {
            animes.Add(
                new Anime()
                {
                    Title = media.Title.Romaji,
                    Cover = media.CoverImage.ExtraLarge,
                    Description = RemoveHtmlTags(media.Description),
                    GenreStr = string.Join(",", media.Genres),
                }
            );
        }

        return (animes, response.Data.PageInfo);
    }

    public async Task<(List<Anime>, PageInfo)> GetSeasonalFullDetail(
        int page = 1,
        MediaSeason season = MediaSeason.Fall,
        int seasonYear = 2024,
        MediaStatus? status = MediaStatus.Finished,
        bool isAdult = false
    )
    {
        var response = await _client.Query(q =>
            q.Page(
                page: page,
                selector: p => new
                {
                    PageInfo = p.PageInfo(pi => new PageInfo { HasNextPage = pi.HasNextPage, Total = pi.Total }),

                    Media = p.Media(
                        season: season,
                        seasonYear: seasonYear,
                        format: MediaFormat.Tv,
                        format_not: MediaFormat.Ona,
                        //status: status,
                        //episodes_greater: 12,
                        isAdult: isAdult,
                        type: MediaType.Anime,
                        selector: m => new
                        {
                            Id = m.Id,
                            IdMal = m.IdMal,
                            Title = m.Title(t => new
                            {
                                Romaji = t.Romaji(),
                                Native = t.Native(),
                                English = t.English(),
                            }),
                            StartDate = m.StartDate(sd => new
                            {
                                Year = sd.Year,
                                Month = sd.Month,
                                Day = sd.Day,
                            }),
                            EndDate = m.EndDate(ed => new
                            {
                                Year = ed.Year,
                                Month = ed.Month,
                                Day = ed.Day,
                            }),
                            Status = m.Status(),
                            Season = m.Season,
                            Format = m.Format,
                            Genres = m.Genres,
                            Synonyms = m.Synonyms,
                            Duration = m.Duration,
                            Popularity = m.Popularity,
                            Episodes = m.Episodes,
                            Source = m.Source(version: 2),
                            CountryOfOrigin = m.CountryOfOrigin,
                            Hashtag = m.Hashtag,
                            AverageScore = m.AverageScore,
                            SiteUrl = m.SiteUrl,
                            Description = m.Description(),
                            BannerImage = m.BannerImage,
                            IsAdult = m.IsAdult,
                            CoverImage = m.CoverImage(ci => new { ExtraLarge = ci.ExtraLarge, Color = ci.Color }),
                            Trailer = m.Trailer(t => new
                            {
                                Id = t.Id,
                                Site = t.Site,
                                Thumbnail = t.Thumbnail,
                            }),
                            ExternalLinks = m.ExternalLinks(el => new
                            {
                                Site = el.Site,
                                Icon = el.Icon,
                                Color = el.Color,
                                Url = el.Url,
                            }),
                            Rankings = m.Rankings(r => new
                            {
                                Rank = r.Rank,
                                Type = r.Type,
                                Season = r.Season,
                                AllTime = r.AllTime,
                            }),
                            Studios = m.Studios(
                                isMain: true,
                                selector: s =>
                                    s.Nodes(n => new
                                    {
                                        Id = n.Id,
                                        Name = n.Name,
                                        SiteUrl = n.SiteUrl,
                                    })
                            ),
                            Relations = m.Relations(r =>
                                r.Edges(e => new
                                {
                                    RelationType = e.RelationType(version: 2),
                                    Node = e.Node(n => new
                                    {
                                        Id = n.Id,
                                        Title = n.Title(t => new
                                        {
                                            Romaji = t.Romaji(),
                                            Native = t.Native(),
                                            English = t.English(),
                                        }),
                                        SiteUrl = n.SiteUrl,
                                    }),
                                })
                            ),
                            AiringSchedule = m.AiringSchedule(
                                notYetAired: true,
                                perPage: 2,
                                selector: a => a.Nodes(n => new { Episode = n.Episode, AiringAt = n.AiringAt })
                            ),
                        }
                    ),
                }
            )
        );

        var animes = new List<Anime>();
        var mediaItems = response.Data.Media;

        foreach (var media in mediaItems)
        {
            animes.Add(
                new Anime()
                {
                    Title = media.Title.Romaji,
                    Cover = media.CoverImage.ExtraLarge,
                    Description = RemoveHtmlTags(media.Description),
                    GenreStr = string.Join(",", media.Genres),
                }
            );
        }

        return (animes, response.Data.PageInfo);
    }

    public async Task<AnilistModels.Media> GetMediaAsync(int id)
    {
        var response = await _client.Query(q =>
            q.Media(
                id: id,
                selector: m => new AnilistModels.Media()
                {
                    Title = m.Title(t => new AnilistModels.MediaTitle
                    {
                        Romaji = t.Romaji(),
                        Native = t.Native(),
                        English = t.English(),
                    }),
                    BannerImage = m.BannerImage,
                    CoverImage = m.CoverImage(ci => new AnilistModels.MediaCoverImage { ExtraLarge = ci.ExtraLarge, Color = ci.Color }),
                    Description = m.Description(),
                    Genres = m.Genres,
                    MeanScore = m.MeanScore,
                    Popularity = m.Popularity,
                    AverageScore = m.AverageScore,
                    Status = m.Status(),
                    Format = m.Format,
                }
            )
        );
        /*Mutations to solve some problems*/

        response.Data.Description = RemoveHtmlTags(response.Data.Description);

        /*End Mutations*/

        return response.Data;
    }

    private static string RemoveHtmlTags(string html)
    {
        return string.IsNullOrEmpty(html) ? html : Regex.Replace(html, @"<.*?>", string.Empty);
    }
}
