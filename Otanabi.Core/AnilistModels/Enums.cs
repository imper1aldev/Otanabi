namespace Otanabi.Core.AnilistModels.Enums;

public enum ActivitySort
{
    Id,
    IdDesc,
    Pinned,
}

public enum ActivityType
{
    Text,
    AnimeList,
    MangaList,
    Message,
    MediaList,
}

public enum AiringSort
{
    Id,
    IdDesc,
    MediaId,
    MediaIdDesc,
    Time,
    TimeDesc,
    Episode,
    EpisodeDesc,
}

public enum CharacterRole
{
    Main,
    Supporting,
    Background,
}

public enum CharacterSort
{
    Id,
    IdDesc,
    Role,
    RoleDesc,
    SearchMatch,
    Favourites,
    FavouritesDesc,
    Relevance,
}

public enum ExternalLinkMediaType
{
    Anime,
    Manga,
    Staff,
}

public enum ExternalLinkType
{
    Info,
    Streaming,
    Social,
}

public enum LikeableType
{
    Thread,
    ThreadComment,
    Activity,
    ActivityReply,
}

public enum MediaFormat
{
    Tv,
    TvShort,
    Movie,
    Special,
    Ova,
    Ona,
    Music,
    Manga,
    Novel,
    OneShot,
}

public enum MediaListSort
{
    MediaId,
    MediaIdDesc,
    Score,
    ScoreDesc,
    Status,
    StatusDesc,
    Progress,
    ProgressDesc,
    ProgressVolumes,
    ProgressVolumesDesc,
    Repeat,
    RepeatDesc,
    Priority,
    PriorityDesc,
    StartedOn,
    StartedOnDesc,
    FinishedOn,
    FinishedOnDesc,
    AddedTime,
    AddedTimeDesc,
    UpdatedTime,
    UpdatedTimeDesc,
    MediaTitleRomaji,
    MediaTitleRomajiDesc,
    MediaTitleEnglish,
    MediaTitleEnglishDesc,
    MediaTitleNative,
    MediaTitleNativeDesc,
    MediaPopularity,
    MediaPopularityDesc,
}

public enum MediaListStatus
{
    Current,
    Planning,
    Completed,
    Dropped,
    Paused,
    Repeating,
}

public enum MediaRankType
{
    Rated,
    Popular,
}

public enum MediaRelation
{
    Adaptation,
    Prequel,
    Sequel,
    Parent,
    SideStory,
    Character,
    Summary,
    Alternative,
    SpinOff,
    Other,
    Source,
    Compilation,
    Contains,
}

public enum MediaSeason
{
    Winter,
    Spring,
    Summer,
    Fall,
}

public enum MediaSort
{
    Id,
    IdDesc,
    TitleRomaji,
    TitleRomajiDesc,
    TitleEnglish,
    TitleEnglishDesc,
    TitleNative,
    TitleNativeDesc,
    Type,
    TypeDesc,
    Format,
    FormatDesc,
    StartDate,
    StartDateDesc,
    EndDate,
    EndDateDesc,
    Score,
    ScoreDesc,
    Popularity,
    PopularityDesc,
    Trending,
    TrendingDesc,
    Episodes,
    EpisodesDesc,
    Duration,
    DurationDesc,
    Status,
    StatusDesc,
    Chapters,
    ChaptersDesc,
    Volumes,
    VolumesDesc,
    UpdatedAt,
    UpdatedAtDesc,
    SearchMatch,
    Favourites,
    FavouritesDesc,
}

public enum MediaSource
{
    Original,
    Manga,
    LightNovel,
    VisualNovel,
    VideoGame,
    Other,
    Novel,
    Doujinshi,
    Anime,
    WebNovel,
    LiveAction,
    Game,
    Comic,
    MultimediaProject,
    PictureBook,
}

public enum MediaStatus
{
    Finished,
    Releasing,
    NotYetReleased,
    Cancelled,
    Hiatus,
}

public enum MediaTrendSort
{
    Id,
    IdDesc,
    MediaId,
    MediaIdDesc,
    Date,
    DateDesc,
    Score,
    ScoreDesc,
    Popularity,
    PopularityDesc,
    Trending,
    TrendingDesc,
    Episode,
    EpisodeDesc,
}

public enum MediaType
{
    Anime,
    Manga,
}

public enum ModActionType
{
    Note,
    Ban,
    Delete,
    Edit,
    Expire,
    Report,
    Reset,
    Anon,
}

public enum ModRole
{
    Admin,
    LeadDeveloper,
    Developer,
    LeadCommunity,
    Community,
    DiscordCommunity,
    LeadAnimeData,
    AnimeData,
    LeadMangaData,
    MangaData,
    LeadSocialMedia,
    SocialMedia,
    Retired,
    CharacterData,
    StaffData,
}

public enum NotificationType
{
    ActivityMessage,
    ActivityReply,
    Following,
    ActivityMention,
    ThreadCommentMention,
    ThreadSubscribed,
    ThreadCommentReply,
    Airing,
    ActivityLike,
    ActivityReplyLike,
    ThreadLike,
    ThreadCommentLike,
    ActivityReplySubscribed,
    RelatedMediaAddition,
    MediaDataChange,
    MediaMerge,
    MediaDeletion,
}

public enum RecommendationRating
{
    NoRating,
    RateUp,
    RateDown,
}

public enum RecommendationSort
{
    Id,
    IdDesc,
    Rating,
    RatingDesc,
}

public enum ReviewRating
{
    NoVote,
    UpVote,
    DownVote,
}

public enum ReviewSort
{
    Id,
    IdDesc,
    Score,
    ScoreDesc,
    Rating,
    RatingDesc,
    CreatedAt,
    CreatedAtDesc,
    UpdatedAt,
    UpdatedAtDesc,
}

public enum RevisionHistoryAction
{
    Create,
    Edit,
}

public enum ScoreFormat
{
    Point100,
    Point10Decimal,
    Point10,
    Point5,
    Point3,
}

public enum SiteTrendSort
{
    Date,
    DateDesc,
    Count,
    CountDesc,
    Change,
    ChangeDesc,
}

public enum StaffLanguage
{
    Japanese,
    English,
    Korean,
    Italian,
    Spanish,
    Portuguese,
    French,
    German,
    Hebrew,
    Hungarian,
}

public enum StaffSort
{
    Id,
    IdDesc,
    Role,
    RoleDesc,
    Language,
    LanguageDesc,
    SearchMatch,
    Favourites,
    FavouritesDesc,
    Relevance,
}

public enum StudioSort
{
    Id,
    IdDesc,
    Name,
    NameDesc,
    SearchMatch,
    Favourites,
    FavouritesDesc,
}

public enum SubmissionSort
{
    Id,
    IdDesc,
}

public enum SubmissionStatus
{
    Pending,
    Rejected,
    PartiallyAccepted,
    Accepted,
}

public enum ThreadCommentSort
{
    Id,
    IdDesc,
}

public enum ThreadSort
{
    Id,
    IdDesc,
    Title,
    TitleDesc,
    CreatedAt,
    CreatedAtDesc,
    UpdatedAt,
    UpdatedAtDesc,
    RepliedAt,
    RepliedAtDesc,
    ReplyCount,
    ReplyCountDesc,
    ViewCount,
    ViewCountDesc,
    IsSticky,
    SearchMatch,
}

public enum UserSort
{
    Id,
    IdDesc,
    Username,
    UsernameDesc,
    WatchedTime,
    WatchedTimeDesc,
    ChaptersRead,
    ChaptersReadDesc,
    SearchMatch,
}

public enum UserStaffNameLanguage
{
    RomajiWestern,
    Romaji,
    Native,
}

public enum UserStatisticsSort
{
    Id,
    IdDesc,
    Count,
    CountDesc,
    Progress,
    ProgressDesc,
    MeanScore,
    MeanScoreDesc,
}

public enum UserTitleLanguage
{
    Romaji,
    English,
    Native,
    RomajiStylised,
    EnglishStylised,
    NativeStylised,
}
