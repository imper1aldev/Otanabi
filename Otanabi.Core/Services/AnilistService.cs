using System.Linq;
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

    public async Task<(AnilistModels.Media[], PageInfo)> GetSeasonal(
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
        sortFilter ??= new MediaSort[] { MediaSort.PopularityDesc };

        

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
                        selector: m => new AnilistModels.Media
                        {
                            Id = m.Id,
                            IdMal = m.IdMal,
                            Title = m.Title(t => new AnilistModels.MediaTitle
                            {
                                Romaji = t.Romaji(),
                                Native = t.Native(),
                                English = t.English(),
                            }),
                            Status = m.Status(),
                            Description =m.Description(),
                            Genres = m.Genres,
                            CoverImage = m.CoverImage(ci => new AnilistModels.MediaCoverImage{ ExtraLarge = ci.ExtraLarge }),
                        }
                    ),
                }
            )
        );
         
        foreach (var media in response.Data.Media)
        {
            media.Description = RemoveHtmlTags(media.Description);
        }


        return (response.Data.Media, response.Data.PageInfo);
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

    public async Task<AnilistModels.Media> GetMediaAsync(int id) {
         

        var response = await _client.Query(q =>
            q.Media(
                id: id,
                selector: m => new  {
                    Title = m.Title(t => new  {
                        Romaji = t.Romaji(),
                        Native = t.Native(),
                        English = t.English(),
                    }),
                    BannerImage = m.BannerImage,
                    CoverImage = m.CoverImage(ci => new  { ExtraLarge = ci.ExtraLarge, Color = ci.Color }),
                    Description = m.Description(),
                    Genres = m.Genres,
                    MeanScore = m.MeanScore,
                    Popularity = m.Popularity,
                    AverageScore = m.AverageScore,
                    Status = m.Status(),
                    Format = m.Format,
                    StartDate = m.StartDate(sd => new { Year = sd.Year, Month = sd.Month, Day = sd.Day }),
                    EndDate = m.EndDate(ed => new { Year = ed.Year, Month = ed.Month, Day = ed.Day }),
                    Duration = m.Duration,
                    Episodes = m.Episodes,
                    Season = m.Season,
                    SeasonYear = m.SeasonYear,
                    streamingEpisodes = m.StreamingEpisodes(selector:se => new { Title = se.Title, Site = se.Site, Thumbnail = se.Thumbnail, Url = se.Url }),
                }
            )
        );

        //response.Data.Description = RemoveHtmlTags(response.Data.Description);

        var data = response.Data;
        List<AnilistModels.MediaStreamingEpisode> episodes = [];
        foreach (var item in data.streamingEpisodes)
        {
            var episode= new AnilistModels.MediaStreamingEpisode{Site=item.Site,Thumbnail=item.Thumbnail,Title=item.Title,Url=item.Url,Number= (int)GetEpisodeNumber(item.Title) };
             episodes.Add(episode);
        }


        if(episodes.Count== 0 )
        {
            for (int i = 0; i < data.Episodes; i++)
            {
               var episode = new AnilistModels.MediaStreamingEpisode { Site = "", Thumbnail = "//Assets/OtanabiSplash.png", Title = $"Episode {i+1}", Url = "", Number = i+1 };
             episodes.Add(episode);

            }
        }


        var anime= new AnilistModels.Media
        {
            Id = id,
            Title = new AnilistModels.MediaTitle{ English = response.Data.Title.English,Romaji=data.Title.Romaji,Native=data.Title.Native},
            BannerImage = data.BannerImage,
            CoverImage = new AnilistModels.MediaCoverImage{ Color=data.CoverImage.Color,ExtraLarge=data.CoverImage.ExtraLarge },
            Description = RemoveHtmlTags(data.Description),
            Genres = data.Genres,
            MeanScore = data.MeanScore,
            Popularity = data.Popularity,
            AverageScore = data.AverageScore,
            Status = data.Status,
            Format =  data.Format,
            StartDate = new AnilistModels.FuzzyDate{ Day=data.StartDate.Day, Month=data.StartDate.Month,Year=data.StartDate.Year },
            EndDate = new AnilistModels.FuzzyDate{ Day=data.EndDate.Day, Month=data.EndDate.Month,Year=data.EndDate.Year },
            Duration = data.Duration,
            Episodes = data.Episodes,
            Season = data.Season,
            SeasonYear  = data.SeasonYear,
            StreamingEpisodes = episodes.OrderBy(x=>x.Number).ToList(),
        };


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
}
