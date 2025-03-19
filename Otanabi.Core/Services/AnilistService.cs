using Otanabi.Core.Models;
using ZeroQL.Client;

namespace Otanabi.Core.Services;

public class AnilistService
{
    public AnilistService() { }

    public async Task<List<Anime>> GetSeasonal()
    {
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("https://graphql.anilist.co");
        var _client = new ZeroQLClient(httpClient);
        var response = await _client.Query(q =>
            q.Page(
                page: 1,
                selector: p => new
                {
                    PageInfo = p.PageInfo(pi => new PageInfo { HasNextPage = pi.HasNextPage, Total = pi.Total }),

                    Media = p.Media(
                        season: MediaSeason.Fall,
                        seasonYear: 2024,
                        format: MediaFormat.Tv,
                        format_not: MediaFormat.Ona,
                        status: MediaStatus.Finished,
                        episodes_greater: 12,
                        isAdult: false,
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

        for (int i = 0; i < mediaItems.Length; i++)
        {
            var media = mediaItems[i];
            animes.Add(new Anime() { Title = media.Title.Romaji, Cover = media.CoverImage.ExtraLarge });
        }

        return animes;
    }
}
