using ZeroQL.Client;

//using Otanabi.Core.AnilistModels.Enums;

namespace Otanabi.Core.AnilistModels;

public interface ActivityUnion { }

public interface LikeableUnion { }

public interface NotificationUnion { }

public class StreamingEpisode
{
    public object Title
    {
        get;
        set;
    }
    public object Thumbnail
    {
        get;
        set;
    }
    public object Url
    {
        get;
        set;
    }
    public object Site
    {
        get;
        set;
    }
}
public class ActivityLikeNotification
{
    public ActivityUnion Activity { get; set; }
    public int ActivityId { get; set; }
    public string Context { get; set; }
    public int? CreatedAt { get; set; }
    public int Id { get; set; }
    public NotificationType? Type { get; set; }
    public User User { get; set; }
    public int UserId { get; set; }
}

public class ActivityMentionNotification
{
    public ActivityUnion Activity { get; set; }
    public int ActivityId { get; set; }
    public string Context { get; set; }
    public int? CreatedAt { get; set; }
    public int Id { get; set; }
    public NotificationType? Type { get; set; }
    public User User { get; set; }
    public int UserId { get; set; }
}

public class ActivityMessageNotification
{
    public int ActivityId { get; set; }
    public string Context { get; set; }
    public int? CreatedAt { get; set; }
    public int Id { get; set; }
    public MessageActivity Message { get; set; }
    public NotificationType? Type { get; set; }
    public User User { get; set; }
    public int UserId { get; set; }
}

public class ActivityReply
{
    public int? ActivityId { get; set; }
    public int CreatedAt { get; set; }
    public int Id { get; set; }
    public bool? IsLiked { get; set; }
    public int LikeCount { get; set; }
    public List<User> likes { get; set; }
    public string Text { get; set; }
    public User User { get; set; }
    public int? UserId { get; set; }
}

public class ActivityReplyLikeNotification
{
    public ActivityUnion Activity { get; set; }
    public int ActivityId { get; set; }
    public string Context { get; set; }
    public int? CreatedAt { get; set; }
    public int Id { get; set; }
    public NotificationType? Type { get; set; }
    public User User { get; set; }
    public int UserId { get; set; }
}

public class ActivityReplyNotification
{
    public ActivityUnion Activity { get; set; }
    public int ActivityId { get; set; }
    public string Context { get; set; }
    public int? CreatedAt { get; set; }
    public int Id { get; set; }
    public NotificationType? Type { get; set; }
    public User User { get; set; }
    public int UserId { get; set; }
}

public class ActivityReplySubscribedNotification
{
    public ActivityUnion Activity { get; set; }
    public int ActivityId { get; set; }
    public string Context { get; set; }
    public int? CreatedAt { get; set; }
    public int Id { get; set; }
    public NotificationType? Type { get; set; }
    public User User { get; set; }
    public int UserId { get; set; }
}

public class AiringNotification
{
    public int AnimeId { get; set; }
    public List<string> contexts { get; set; }
    public int? CreatedAt { get; set; }
    public int Episode { get; set; }
    public int Id { get; set; }
    public Media Media { get; set; }
    public NotificationType? Type { get; set; }
}

public class AiringProgression
{
    public double? Episode { get; set; }
    public double? Score { get; set; }
    public int? Watching { get; set; }
}

public class AiringSchedule
{
    public int AiringAt { get; set; }
    public int Episode { get; set; }
    public int Id { get; set; }
    public Media Media { get; set; }
    public int MediaId { get; set; }
    public int TimeUntilAiring { get; set; }
}

public class AiringScheduleConnection
{
    public List<AiringScheduleEdge> edges { get; set; }
    public List<AiringSchedule> nodes { get; set; }
    public PageInfo PageInfo { get; set; }
}

public class AiringScheduleEdge
{
    public int? Id { get; set; }
    public AiringSchedule Node { get; set; }
}

public class AiringScheduleInput
{
    public int? AiringAt { get; set; }
    public int? Episode { get; set; }
    public int? TimeUntilAiring { get; set; }
}

public class AniChartHighlightInput
{
    public string Highlight { get; set; }
    public int? MediaId { get; set; }
}

public class AniChartUser
{
    public ActivityUnion Highlights { get; set; }
    public ActivityUnion Settings { get; set; }
    public User User { get; set; }
}

public class Character
{
    public string Age { get; set; }
    public string BloodType { get; set; }
    public FuzzyDate DateOfBirth { get; set; }
    public string Description { get; set; }
    public int? Favourites { get; set; }
    public string Gender { get; set; }
    public int Id { get; set; }
    public CharacterImage Image { get; set; }
    public bool IsFavourite { get; set; }
    public bool IsFavouriteBlocked { get; set; }
    public MediaConnection Media { get; set; }
    public string ModNotes { get; set; }
    public CharacterName Name { get; set; }
    public string SiteUrl { get; set; }

    [Obsolete("No data available")]
    public int? UpdatedAt { get; set; }
}

public class CharacterConnection
{
    public List<CharacterEdge> edges { get; set; }
    public List<Character> nodes { get; set; }
    public PageInfo PageInfo { get; set; }
}

public class CharacterEdge
{
    public int? FavouriteOrder { get; set; }
    public int? Id { get; set; }
    public List<Media> media { get; set; }
    public string Name { get; set; }
    public Character Node { get; set; }
    public CharacterRole? Role { get; set; }
    public List<StaffRoleType> voiceActorRoles { get; set; }
    public List<Staff> voiceActors { get; set; }
}

public class CharacterImage
{
    public string Large { get; set; }
    public string Medium { get; set; }
}

public class CharacterName
{
    public List<string> alternative { get; set; }
    public List<string> alternativeSpoiler { get; set; }
    public string First { get; set; }
    public string Full { get; set; }
    public string Last { get; set; }
    public string Middle { get; set; }
    public string Native { get; set; }
    public string UserPreferred { get; set; }
}

public class CharacterNameInput
{
    public List<string> alternative { get; set; }
    public List<string> alternativeSpoiler { get; set; }
    public string First { get; set; }
    public string Last { get; set; }
    public string Middle { get; set; }
    public string Native { get; set; }
}

public class CharacterSubmission
{
    public User Assignee { get; set; }
    public Character Character { get; set; }
    public int? CreatedAt { get; set; }
    public int Id { get; set; }
    public bool? Locked { get; set; }
    public string Notes { get; set; }
    public string Source { get; set; }
    public SubmissionStatus? Status { get; set; }
    public Character Submission { get; set; }
    public User Submitter { get; set; }
}

public class CharacterSubmissionConnection
{
    public List<CharacterSubmissionEdge> edges { get; set; }
    public List<CharacterSubmission> nodes { get; set; }
    public PageInfo PageInfo { get; set; }
}

public class CharacterSubmissionEdge
{
    public CharacterSubmission Node { get; set; }
    public CharacterRole? Role { get; set; }
    public List<StaffSubmission> submittedVoiceActors { get; set; }
    public List<Staff> voiceActors { get; set; }
}

public class Deleted
{
    public bool? deleted { get; set; }
}

public class Favourites
{
    public MediaConnection Anime { get; set; }
    public CharacterConnection Characters { get; set; }
    public MediaConnection Manga { get; set; }
    public StaffConnection Staff { get; set; }
    public StudioConnection Studios { get; set; }
}

public class FollowingNotification
{
    public string Context { get; set; }
    public int? CreatedAt { get; set; }
    public int Id { get; set; }
    public NotificationType? Type { get; set; }
    public User User { get; set; }
    public int UserId { get; set; }
}

public class FormatStats
{
    public int? Amount { get; set; }
    public MediaFormat? Format { get; set; }
}

public class FuzzyDate
{
    public int? Day { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
}

public class FuzzyDateInput
{
    public int? Day { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
}

public class GenreStats
{
    public int? Amount { get; set; }
    public string Genre { get; set; }
    public int? MeanScore { get; set; }
    public int? TimeWatched { get; set; }
}

public class InternalPage
{
    public List<ActivityUnion> activities { get; set; }
    public List<ActivityReply> activityReplies { get; set; }
    public List<AiringSchedule> airingSchedules { get; set; }
    public List<CharacterSubmission> characterSubmissions { get; set; }
    public List<Character> characters { get; set; }
    public List<User> followers { get; set; }
    public List<User> following { get; set; }
    public List<User> likes { get; set; }
    public List<Media> media { get; set; }
    public List<MediaList> mediaList { get; set; }
    public List<MediaSubmission> mediaSubmissions { get; set; }
    public List<MediaTrend> mediaTrends { get; set; }
    public List<ModAction> modActions { get; set; }
    public List<NotificationUnion> notifications { get; set; }
    public PageInfo PageInfo { get; set; }
    public List<Recommendation> recommendations { get; set; }
    public List<Report> reports { get; set; }
    public List<Review> reviews { get; set; }
    public List<RevisionHistory> revisionHistory { get; set; }
    public List<Staff> staff { get; set; }
    public List<StaffSubmission> staffSubmissions { get; set; }
    public List<Studio> studios { get; set; }
    public List<ThreadComment> threadComments { get; set; }
    public List<Thread> threads { get; set; }
    public List<User> userBlockSearch { get; set; }
    public List<User> users { get; set; }
}

public class ListActivity
{
    public int CreatedAt { get; set; }
    public int Id { get; set; }
    public bool? IsLiked { get; set; }
    public bool? IsLocked { get; set; }
    public bool? IsPinned { get; set; }
    public bool? IsSubscribed { get; set; }
    public int LikeCount { get; set; }
    public List<User> likes { get; set; }
    public Media Media { get; set; }
    public string Progress { get; set; }
    public List<ActivityReply> replies { get; set; }
    public int ReplyCount { get; set; }
    public string SiteUrl { get; set; }
    public string Status { get; set; }
    public ActivityType? Type { get; set; }
    public User User { get; set; }
    public int? UserId { get; set; }
}

public class ListActivityOption
{
    public bool? Disabled { get; set; }
    public MediaListStatus? Type { get; set; }
}

public class ListActivityOptionInput
{
    public bool? Disabled { get; set; }
    public MediaListStatus? Type { get; set; }
}

public class ListScoreStats
{
    public int? MeanScore { get; set; }
    public int? StandardDeviation { get; set; }
}

public class Media
{
    public AiringScheduleConnection AiringSchedule { get; set; }
    public bool? AutoCreateForumThread { get; set; }
    public int? AverageScore { get; set; }
    public string BannerImage { get; set; }
    public int? Chapters { get; set; }
    public CharacterConnection Characters { get; set; }
    public object CountryOfOrigin { get; set; }
    public MediaCoverImage CoverImage { get; set; }
    public string Description { get; set; }
    public int? Duration { get; set; }
    public FuzzyDate EndDate { get; set; }
    public int? Episodes { get; set; }
    public List<MediaExternalLink> externalLinks { get; set; }
    public int? Favourites { get; set; }
    public MediaFormat? Format { get; set; }
    public string?[]? Genres { get; set; }
    public string Hashtag { get; set; }
    public int Id { get; set; }
    public int? IdMal { get; set; }
    public bool? IsAdult { get; set; }
    public bool IsFavourite { get; set; }
    public bool IsFavouriteBlocked { get; set; }
    public bool? IsLicensed { get; set; }
    public bool? IsLocked { get; set; }
    public bool? IsRecommendationBlocked { get; set; }
    public bool? IsReviewBlocked { get; set; }
    public int? MeanScore { get; set; }
    public MediaList MediaListEntry { get; set; }
    public string ModNotes { get; set; }
    public AiringSchedule NextAiringEpisode { get; set; }
    public int? Popularity { get; set; }
    public List<MediaRank> rankings { get; set; }
    public RecommendationConnection Recommendations { get; set; }
    public MediaConnection Relations { get; set; }
    public ReviewConnection Reviews { get; set; }
    public MediaSeason? Season { get; set; }

    [Obsolete("")]
    public int? SeasonInt { get; set; }
    public int? SeasonYear { get; set; }
    public string SiteUrl { get; set; }
    public MediaSource? Source { get; set; }
    public StaffConnection Staff { get; set; }
    public FuzzyDate StartDate { get; set; }
    public MediaStats Stats { get; set; }
    public MediaStatus? Status { get; set; }
    public List<MediaStreamingEpisode> StreamingEpisodes { get; set; }
    public StudioConnection Studios { get; set; }
    public List<string> synonyms { get; set; }
    public List<MediaTag> tags { get; set; }
    public MediaTitle Title { get; set; }
    public MediaTrailer Trailer { get; set; }
    public int? Trending { get; set; }
    public MediaTrendConnection Trends { get; set; }
    public MediaType? Type { get; set; }
    public int? UpdatedAt { get; set; }
    public int? Volumes { get; set; }
}

public class MediaCharacter
{
    public Character Character { get; set; }
    public string CharacterName { get; set; }
    public string DubGroup { get; set; }
    public int? Id { get; set; }
    public CharacterRole? Role { get; set; }
    public string RoleNotes { get; set; }
    public Staff VoiceActor { get; set; }
}

public class MediaConnection
{
    public List<MediaEdge> edges { get; set; }
    public List<Media> nodes { get; set; }
    public PageInfo PageInfo { get; set; }
}

public class MediaCoverImage
{
    public string Color { get; set; }
    public string ExtraLarge { get; set; }
    public string Large { get; set; }
    public string Medium { get; set; }
}

public class MediaDataChangeNotification
{
    public string Context { get; set; }
    public int? CreatedAt { get; set; }
    public int Id { get; set; }
    public Media Media { get; set; }
    public int MediaId { get; set; }
    public string Reason { get; set; }
    public NotificationType? Type { get; set; }
}

public class MediaDeletionNotification
{
    public string Context { get; set; }
    public int? CreatedAt { get; set; }
    public string DeletedMediaTitle { get; set; }
    public int Id { get; set; }
    public string Reason { get; set; }
    public NotificationType? Type { get; set; }
}

public class MediaEdge
{
    public string CharacterName { get; set; }
    public CharacterRole? CharacterRole { get; set; }
    public List<Character> characters { get; set; }
    public string DubGroup { get; set; }
    public int? FavouriteOrder { get; set; }
    public int? Id { get; set; }
    public bool IsMainStudio { get; set; }
    public Media Node { get; set; }
    public MediaRelation? RelationType { get; set; }
    public string RoleNotes { get; set; }
    public string StaffRole { get; set; }
    public List<StaffRoleType> voiceActorRoles { get; set; }
    public List<Staff> voiceActors { get; set; }
}

public class MediaExternalLink
{
    public string Color { get; set; }
    public string Icon { get; set; }
    public int Id { get; set; }
    public bool? IsDisabled { get; set; }
    public string Language { get; set; }
    public string Notes { get; set; }
    public string Site { get; set; }
    public int? SiteId { get; set; }
    public ExternalLinkType? Type { get; set; }
    public string Url { get; set; }
}

public class MediaExternalLinkInput
{
    public int Id { get; set; }
    public string Site { get; set; }
    public string Url { get; set; }
}

public class MediaList
{
    public object AdvancedScores { get; set; }
    public FuzzyDate CompletedAt { get; set; }
    public int? CreatedAt { get; set; }
    public object CustomLists { get; set; }
    public bool? HiddenFromStatusLists { get; set; }
    public int Id { get; set; }
    public Media Media { get; set; }
    public int MediaId { get; set; }
    public string Notes { get; set; }
    public int? Priority { get; set; }
    public bool? @private { get; set; }
    public int? Progress { get; set; }
    public int? ProgressVolumes { get; set; }
    public int? Repeat { get; set; }
    public double? Score { get; set; }
    public FuzzyDate StartedAt { get; set; }
    public MediaListStatus? Status { get; set; }
    public int? UpdatedAt { get; set; }
    public User User { get; set; }
    public int UserId { get; set; }
}

public class MediaListCollection
{
    [Obsolete("Not GraphQL spec compliant, use lists field instead.")]
    public List<List<MediaList>> customLists { get; set; }
    public bool? HasNextChunk { get; set; }
    public List<MediaListGroup> lists { get; set; }

    [Obsolete("Not GraphQL spec compliant, use lists field instead.")]
    public List<List<MediaList>> statusLists { get; set; }
    public User User { get; set; }
}

public class MediaListGroup
{
    public List<MediaList> entries { get; set; }
    public bool? IsCustomList { get; set; }
    public bool? IsSplitCompletedList { get; set; }
    public string Name { get; set; }
    public MediaListStatus? Status { get; set; }
}

public class MediaListOptions
{
    public MediaListTypeOptions AnimeList { get; set; }
    public MediaListTypeOptions MangaList { get; set; }
    public string RowOrder { get; set; }
    public ScoreFormat? ScoreFormat { get; set; }

    [Obsolete("No longer used")]
    public object SharedTheme { get; set; }

    [Obsolete("No longer used")]
    public bool? SharedThemeEnabled { get; set; }

    [Obsolete("No longer used")]
    public bool? UseLegacyLists { get; set; }
}

public class MediaListOptionsInput
{
    public List<string> advancedScoring { get; set; }
    public bool? AdvancedScoringEnabled { get; set; }
    public List<string> customLists { get; set; }
    public List<string> sectionOrder { get; set; }
    public bool? SplitCompletedSectionByFormat { get; set; }
    public string Theme { get; set; }
}

public class MediaListTypeOptions
{
    public List<string> advancedScoring { get; set; }
    public bool? AdvancedScoringEnabled { get; set; }
    public List<string> customLists { get; set; }
    public List<string> sectionOrder { get; set; }
    public bool? SplitCompletedSectionByFormat { get; set; }

    [Obsolete("This field has not yet been fully implemented and may change without warning")]
    public object Theme { get; set; }
}

public class MediaMergeNotification
{
    public string Context { get; set; }
    public int? CreatedAt { get; set; }
    public List<string> deletedMediaTitles { get; set; }
    public int Id { get; set; }
    public Media Media { get; set; }
    public int MediaId { get; set; }
    public string Reason { get; set; }
    public NotificationType? Type { get; set; }
}

public class MediaRank
{
    public bool? AllTime { get; set; }
    public string Context { get; set; }
    public MediaFormat Format { get; set; }
    public int Id { get; set; }
    public int Rank { get; set; }
    public MediaSeason? Season { get; set; }
    public MediaRankType Type { get; set; }
    public int? Year { get; set; }
}

public class MediaStats
{
    [Obsolete("Replaced by MediaTrends")]
    public List<AiringProgression> airingProgression { get; set; }
    public List<ScoreDistribution> scoreDistribution { get; set; }
    public List<StatusDistribution> statusDistribution { get; set; }
}

public class MediaStreamingEpisode
{
    public string Site { get; set; }
    public string Thumbnail { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }
    public int Number
    {
        get; set;
    }
}

public class MediaSubmission
{
    public User Assignee { get; set; }
    public List<string> changes { get; set; }
    public List<MediaSubmissionComparison> characters { get; set; }
    public int? CreatedAt { get; set; }
    public List<MediaSubmissionComparison> externalLinks { get; set; }
    public int Id { get; set; }
    public bool? Locked { get; set; }
    public Media Media { get; set; }
    public string Notes { get; set; }
    public List<MediaEdge> relations { get; set; }
    public string Source { get; set; }
    public List<MediaSubmissionComparison> staff { get; set; }
    public SubmissionStatus? Status { get; set; }
    public List<MediaSubmissionComparison> studios { get; set; }
    public Media Submission { get; set; }
    public User Submitter { get; set; }
    public object SubmitterStats { get; set; }
}

public class MediaSubmissionComparison
{
    public MediaCharacter Character { get; set; }
    public MediaExternalLink ExternalLink { get; set; }
    public StaffEdge Staff { get; set; }
    public StudioEdge Studio { get; set; }
    public MediaSubmissionEdge Submission { get; set; }
}

public class MediaSubmissionEdge
{
    public Character Character { get; set; }
    public string CharacterName { get; set; }
    public CharacterRole? CharacterRole { get; set; }
    public Character CharacterSubmission { get; set; }
    public string DubGroup { get; set; }
    public MediaExternalLink ExternalLink { get; set; }
    public int? Id { get; set; }
    public bool? IsMain { get; set; }
    public Media Media { get; set; }
    public string RoleNotes { get; set; }
    public Staff Staff { get; set; }
    public string StaffRole { get; set; }
    public Staff StaffSubmission { get; set; }
    public Studio Studio { get; set; }
    public Staff VoiceActor { get; set; }
    public Staff VoiceActorSubmission { get; set; }
}

public class MediaTag
{
    public string Category { get; set; }
    public string Description { get; set; }
    public int Id { get; set; }
    public bool? IsAdult { get; set; }
    public bool? IsGeneralSpoiler { get; set; }
    public bool? IsMediaSpoiler { get; set; }
    public string Name { get; set; }
    public int? Rank { get; set; }
    public int? UserId { get; set; }
}

public class MediaTitle
{
    public string English { get; set; }
    public string Native { get; set; }
    public string Romaji { get; set; }
    public string UserPreferred { get; set; }
}

public class MediaTitleInput
{
    public string English { get; set; }
    public string Native { get; set; }
    public string Romaji { get; set; }
}

public class MediaTrailer
{
    public string Id { get; set; }
    public string Site { get; set; }
    public string Thumbnail { get; set; }
}

public class MediaTrend
{
    public int? AverageScore { get; set; }
    public int Date { get; set; }
    public int? Episode { get; set; }
    public int? InProgress { get; set; }
    public Media Media { get; set; }
    public int MediaId { get; set; }
    public int? Popularity { get; set; }
    public bool Releasing { get; set; }
    public int Trending { get; set; }
}

public class MediaTrendConnection
{
    public List<MediaTrendEdge> edges { get; set; }
    public List<MediaTrend> nodes { get; set; }
    public PageInfo PageInfo { get; set; }
}

public class MediaTrendEdge
{
    public MediaTrend Node { get; set; }
}

public class MessageActivity
{
    public int CreatedAt { get; set; }
    public int Id { get; set; }
    public bool? IsLiked { get; set; }
    public bool? IsLocked { get; set; }
    public bool? IsPrivate { get; set; }
    public bool? IsSubscribed { get; set; }
    public int LikeCount { get; set; }
    public List<User> likes { get; set; }
    public string Message { get; set; }
    public User Messenger { get; set; }
    public int? MessengerId { get; set; }
    public User Recipient { get; set; }
    public int? RecipientId { get; set; }
    public List<ActivityReply> replies { get; set; }
    public int ReplyCount { get; set; }
    public string SiteUrl { get; set; }
    public ActivityType? Type { get; set; }
}

public class ModAction
{
    public int CreatedAt { get; set; }
    public string Data { get; set; }
    public int Id { get; set; }
    public User Mod { get; set; }
    public int? ObjectId { get; set; }
    public string ObjectType { get; set; }
    public ModActionType? Type { get; set; }
    public User User { get; set; }
}

public class Mutation
{
    public Deleted DeleteActivity { get; set; }
    public Deleted DeleteActivityReply { get; set; }
    public Deleted DeleteCustomList { get; set; }
    public Deleted DeleteMediaListEntry { get; set; }
    public Deleted DeleteReview { get; set; }
    public Deleted DeleteThread { get; set; }
    public Deleted DeleteThreadComment { get; set; }
    public Review RateReview { get; set; }
    public ActivityReply SaveActivityReply { get; set; }
    public ListActivity SaveListActivity { get; set; }
    public MediaList SaveMediaListEntry { get; set; }
    public MessageActivity SaveMessageActivity { get; set; }
    public Recommendation SaveRecommendation { get; set; }
    public Review SaveReview { get; set; }
    public TextActivity SaveTextActivity { get; set; }
    public Thread SaveThread { get; set; }
    public ThreadComment SaveThreadComment { get; set; }
    public ActivityUnion ToggleActivityPin { get; set; }
    public ActivityUnion ToggleActivitySubscription { get; set; }
    public Favourites ToggleFavourite { get; set; }
    public User ToggleFollow { get; set; }
    public List<User> toggleLike { get; set; }
    public LikeableUnion ToggleLikeV2 { get; set; }
    public Thread ToggleThreadSubscription { get; set; }
    public object UpdateAniChartHighlights { get; set; }
    public object UpdateAniChartSettings { get; set; }
    public Favourites UpdateFavouriteOrder { get; set; }
    public List<MediaList> updateMediaListEntries { get; set; }
    public User UpdateUser { get; set; }
}

public class NotificationOption
{
    public bool? Enabled { get; set; }
    public NotificationType? Type { get; set; }
}

public class NotificationOptionInput
{
    public bool? Enabled { get; set; }
    public NotificationType? Type { get; set; }
}

public class Page
{
    public List<ActivityUnion> activities { get; set; }
    public List<ActivityReply> activityReplies { get; set; }
    public List<AiringSchedule> airingSchedules { get; set; }
    public List<Character> characters { get; set; }
    public List<User> followers { get; set; }
    public List<User> following { get; set; }
    public List<User> likes { get; set; }
    public List<Media> media { get; set; }
    public List<MediaList> mediaList { get; set; }
    public List<MediaTrend> mediaTrends { get; set; }
    public List<NotificationUnion> notifications { get; set; }
    public PageInfo PageInfo { get; set; }
    public List<Recommendation> recommendations { get; set; }
    public List<Review> reviews { get; set; }
    public List<Staff> staff { get; set; }
    public List<Studio> studios { get; set; }
    public List<ThreadComment> threadComments { get; set; }
    public List<Thread> threads { get; set; }
    public List<User> users { get; set; }
}

public class PageInfo
{
    public int? CurrentPage { get; set; }
    public bool? HasNextPage { get; set; }
    public int? LastPage { get; set; }
    public int? PerPage { get; set; }
    public int? Total { get; set; }
}

public class ParsedMarkdown
{
    public string Html { get; set; }
}

public class Query
{
    public ActivityUnion Activity { get; set; }
    public ActivityReply ActivityReply { get; set; }
    public AiringSchedule AiringSchedule { get; set; }
    public AniChartUser AniChartUser { get; set; }
    public Character Character { get; set; }
    public List<MediaExternalLink> externalLinkSourceCollection { get; set; }
    public User Follower { get; set; }
    public User Following { get; set; }
    public List<string> genreCollection { get; set; }
    public User Like { get; set; }
    public ParsedMarkdown Markdown { get; set; }
    public Media Media { get; set; }
    public MediaList MediaList { get; set; }
    public MediaListCollection MediaListCollection { get; set; }
    public List<MediaTag> mediaTagCollection { get; set; }
    public MediaTrend MediaTrend { get; set; }
    public NotificationUnion Notification { get; set; }
    public Page Page { get; set; }
    public Recommendation Recommendation { get; set; }
    public Review Review { get; set; }
    public SiteStatistics SiteStatistics { get; set; }
    public Staff Staff { get; set; }
    public Studio Studio { get; set; }
    public Thread Thread { get; set; }
    public List<ThreadComment> threadComment { get; set; }
    public User User { get; set; }
    public User Viewer { get; set; }
}

public class Recommendation
{
    public int Id { get; set; }
    public Media Media { get; set; }
    public Media MediaRecommendation { get; set; }
    public int? Rating { get; set; }
    public User User { get; set; }
    public RecommendationRating? UserRating { get; set; }
}

public class RecommendationConnection
{
    public List<RecommendationEdge> edges { get; set; }
    public List<Recommendation> nodes { get; set; }
    public PageInfo PageInfo { get; set; }
}

public class RecommendationEdge
{
    public Recommendation Node { get; set; }
}

public class RelatedMediaAdditionNotification
{
    public string Context { get; set; }
    public int? CreatedAt { get; set; }
    public int Id { get; set; }
    public Media Media { get; set; }
    public int MediaId { get; set; }
    public NotificationType? Type { get; set; }
}

public class Report
{
    public bool? Cleared { get; set; }
    public int? CreatedAt { get; set; }
    public int Id { get; set; }
    public string Reason { get; set; }
    public User Reported { get; set; }
    public User Reporter { get; set; }
}

public class Review
{
    public string Body { get; set; }
    public int CreatedAt { get; set; }
    public int Id { get; set; }
    public Media Media { get; set; }
    public int MediaId { get; set; }
    public MediaType? MediaType { get; set; }
    public bool? @private { get; set; }
    public int? Rating { get; set; }
    public int? RatingAmount { get; set; }
    public int? Score { get; set; }
    public string SiteUrl { get; set; }
    public string Summary { get; set; }
    public int UpdatedAt { get; set; }
    public User User { get; set; }
    public int UserId { get; set; }
    public ReviewRating? UserRating { get; set; }
}

public class ReviewConnection
{
    public List<ReviewEdge> edges { get; set; }
    public List<Review> nodes { get; set; }
    public PageInfo PageInfo { get; set; }
}

public class ReviewEdge
{
    public Review Node { get; set; }
}

public class RevisionHistory
{
    public RevisionHistoryAction? Action { get; set; }
    public object Changes { get; set; }
    public Character Character { get; set; }
    public int? CreatedAt { get; set; }
    public MediaExternalLink ExternalLink { get; set; }
    public int Id { get; set; }
    public Media Media { get; set; }
    public Staff Staff { get; set; }
    public Studio Studio { get; set; }
    public User User { get; set; }
}

public class ScoreDistribution
{
    public int? Amount { get; set; }
    public int? Score { get; set; }
}

public class SiteStatistics
{
    public SiteTrendConnection Anime { get; set; }
    public SiteTrendConnection Characters { get; set; }
    public SiteTrendConnection Manga { get; set; }
    public SiteTrendConnection Reviews { get; set; }
    public SiteTrendConnection Staff { get; set; }
    public SiteTrendConnection Studios { get; set; }
    public SiteTrendConnection Users { get; set; }
}

public class SiteTrend
{
    public int Change { get; set; }
    public int Count { get; set; }
    public int Date { get; set; }
}

public class SiteTrendConnection
{
    public List<SiteTrendEdge> edges { get; set; }
    public List<SiteTrend> nodes { get; set; }
    public PageInfo PageInfo { get; set; }
}

public class SiteTrendEdge
{
    public SiteTrend Node { get; set; }
}

public class Staff
{
    public int? Age { get; set; }
    public string BloodType { get; set; }
    public MediaConnection CharacterMedia { get; set; }
    public CharacterConnection Characters { get; set; }
    public FuzzyDate DateOfBirth { get; set; }
    public FuzzyDate DateOfDeath { get; set; }
    public string Description { get; set; }
    public int? Favourites { get; set; }
    public string Gender { get; set; }
    public string HomeTown { get; set; }
    public int Id { get; set; }
    public StaffImage Image { get; set; }
    public bool IsFavourite { get; set; }
    public bool IsFavouriteBlocked { get; set; }

    [Obsolete("Replaced with languageV2")]
    public StaffLanguage? Language { get; set; }
    public string LanguageV2 { get; set; }
    public string ModNotes { get; set; }
    public StaffName Name { get; set; }
    public List<string> primaryOccupations { get; set; }
    public string SiteUrl { get; set; }
    public Staff staff { get; set; }
    public MediaConnection StaffMedia { get; set; }
    public string SubmissionNotes { get; set; }
    public int? SubmissionStatus { get; set; }
    public User Submitter { get; set; }

    [Obsolete("No data available")]
    public int? UpdatedAt { get; set; }
    public List<int?> yearsActive { get; set; }
}

public class StaffConnection
{
    public List<StaffEdge> edges { get; set; }
    public List<Staff> nodes { get; set; }
    public PageInfo PageInfo { get; set; }
}

public class StaffEdge
{
    public int? FavouriteOrder { get; set; }
    public int? Id { get; set; }
    public Staff Node { get; set; }
    public string Role { get; set; }
}

public class StaffImage
{
    public string Large { get; set; }
    public string Medium { get; set; }
}

public class StaffName
{
    public List<string> alternative { get; set; }
    public string First { get; set; }
    public string Full { get; set; }
    public string Last { get; set; }
    public string Middle { get; set; }
    public string Native { get; set; }
    public string UserPreferred { get; set; }
}

public class StaffNameInput
{
    public List<string> alternative { get; set; }
    public string First { get; set; }
    public string Last { get; set; }
    public string Middle { get; set; }
    public string Native { get; set; }
}

public class StaffRoleType
{
    public string DubGroup { get; set; }
    public string RoleNotes { get; set; }
    public Staff VoiceActor { get; set; }
}

public class StaffStats
{
    public int? Amount { get; set; }
    public int? MeanScore { get; set; }
    public Staff Staff { get; set; }
    public int? TimeWatched { get; set; }
}

public class StaffSubmission
{
    public User Assignee { get; set; }
    public int? CreatedAt { get; set; }
    public int Id { get; set; }
    public bool? Locked { get; set; }
    public string Notes { get; set; }
    public string Source { get; set; }
    public Staff Staff { get; set; }
    public SubmissionStatus? Status { get; set; }
    public Staff Submission { get; set; }
    public User Submitter { get; set; }
}

public class StatusDistribution
{
    public int? Amount { get; set; }
    public MediaListStatus? Status { get; set; }
}

public class Studio
{
    public int? Favourites { get; set; }
    public int Id { get; set; }
    public bool IsAnimationStudio { get; set; }
    public bool IsFavourite { get; set; }
    public MediaConnection Media { get; set; }
    public string Name { get; set; }
    public string SiteUrl { get; set; }
}

public class StudioConnection
{
    public List<StudioEdge> edges { get; set; }
    public List<Studio> nodes { get; set; }
    public PageInfo PageInfo { get; set; }
}

public class StudioEdge
{
    public int? FavouriteOrder { get; set; }
    public int? Id { get; set; }
    public bool IsMain { get; set; }
    public Studio Node { get; set; }
}

public class StudioStats
{
    public int? Amount { get; set; }
    public int? MeanScore { get; set; }
    public Studio Studio { get; set; }
    public int? TimeWatched { get; set; }
}

public class TagStats
{
    public int? Amount { get; set; }
    public int? MeanScore { get; set; }
    public MediaTag Tag { get; set; }
    public int? TimeWatched { get; set; }
}

public class TextActivity
{
    public int CreatedAt { get; set; }
    public int Id { get; set; }
    public bool? IsLiked { get; set; }
    public bool? IsLocked { get; set; }
    public bool? IsPinned { get; set; }
    public bool? IsSubscribed { get; set; }
    public int LikeCount { get; set; }
    public List<User> likes { get; set; }
    public List<ActivityReply> replies { get; set; }
    public int ReplyCount { get; set; }
    public string SiteUrl { get; set; }
    public string Text { get; set; }
    public ActivityType? Type { get; set; }
    public User User { get; set; }
    public int? UserId { get; set; }
}

public class Thread
{
    public string Body { get; set; }
    public List<ThreadCategory> categories { get; set; }
    public int CreatedAt { get; set; }
    public int Id { get; set; }
    public bool? IsLiked { get; set; }
    public bool? IsLocked { get; set; }
    public bool? IsSticky { get; set; }
    public bool? IsSubscribed { get; set; }
    public int LikeCount { get; set; }
    public List<User> likes { get; set; }
    public List<Media> mediaCategories { get; set; }
    public int? RepliedAt { get; set; }
    public int? ReplyCommentId { get; set; }
    public int? ReplyCount { get; set; }
    public User ReplyUser { get; set; }
    public int? ReplyUserId { get; set; }
    public string SiteUrl { get; set; }
    public string Title { get; set; }
    public int UpdatedAt { get; set; }
    public User User { get; set; }
    public int UserId { get; set; }
    public int? ViewCount { get; set; }
}

public class ThreadCategory
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class ThreadComment
{
    public object ChildComments { get; set; }
    public string Comment { get; set; }
    public int CreatedAt { get; set; }
    public int Id { get; set; }
    public bool? IsLiked { get; set; }
    public bool? IsLocked { get; set; }
    public int LikeCount { get; set; }
    public List<User> likes { get; set; }
    public string SiteUrl { get; set; }
    public Thread Thread { get; set; }
    public int? ThreadId { get; set; }
    public int UpdatedAt { get; set; }
    public User User { get; set; }
    public int? UserId { get; set; }
}

public class ThreadCommentLikeNotification
{
    public ThreadComment Comment { get; set; }
    public int CommentId { get; set; }
    public string Context { get; set; }
    public int? CreatedAt { get; set; }
    public int Id { get; set; }
    public Thread Thread { get; set; }
    public NotificationType? Type { get; set; }
    public User User { get; set; }
    public int UserId { get; set; }
}

public class ThreadCommentMentionNotification
{
    public ThreadComment Comment { get; set; }
    public int CommentId { get; set; }
    public string Context { get; set; }
    public int? CreatedAt { get; set; }
    public int Id { get; set; }
    public Thread Thread { get; set; }
    public NotificationType? Type { get; set; }
    public User User { get; set; }
    public int UserId { get; set; }
}

public class ThreadCommentReplyNotification
{
    public ThreadComment Comment { get; set; }
    public int CommentId { get; set; }
    public string Context { get; set; }
    public int? CreatedAt { get; set; }
    public int Id { get; set; }
    public Thread Thread { get; set; }
    public NotificationType? Type { get; set; }
    public User User { get; set; }
    public int UserId { get; set; }
}

public class ThreadCommentSubscribedNotification
{
    public ThreadComment Comment { get; set; }
    public int CommentId { get; set; }
    public string Context { get; set; }
    public int? CreatedAt { get; set; }
    public int Id { get; set; }
    public Thread Thread { get; set; }
    public NotificationType? Type { get; set; }
    public User User { get; set; }
    public int UserId { get; set; }
}

public class ThreadLikeNotification
{
    public ThreadComment Comment { get; set; }
    public string Context { get; set; }
    public int? CreatedAt { get; set; }
    public int Id { get; set; }
    public Thread Thread { get; set; }
    public int ThreadId { get; set; }
    public NotificationType? Type { get; set; }
    public User User { get; set; }
    public int UserId { get; set; }
}

public class User
{
    public string About { get; set; }
    public UserAvatar Avatar { get; set; }
    public string BannerImage { get; set; }
    public object Bans { get; set; }
    public int? CreatedAt { get; set; }
    public string DonatorBadge { get; set; }
    public int? DonatorTier { get; set; }
    public Favourites Favourites { get; set; }
    public int Id { get; set; }
    public bool? IsBlocked { get; set; }
    public bool? IsFollower { get; set; }
    public bool? IsFollowing { get; set; }
    public MediaListOptions MediaListOptions { get; set; }
    public List<ModRole?> moderatorRoles { get; set; }

    [Obsolete("Deprecated. Replaced with moderatorRoles field.")]
    public string ModeratorStatus { get; set; }
    public string Name { get; set; }
    public UserOptions Options { get; set; }
    public List<UserPreviousName> previousNames { get; set; }
    public string SiteUrl { get; set; }
    public UserStatisticTypes Statistics { get; set; }

    [Obsolete("Deprecated. Replaced with statistics field.")]
    public UserStats Stats { get; set; }
    public int? UnreadNotificationCount { get; set; }
    public int? UpdatedAt { get; set; }
}

public class UserActivityHistory
{
    public int? Amount { get; set; }
    public int? Date { get; set; }
    public int? Level { get; set; }
}

public class UserAvatar
{
    public string Large { get; set; }
    public string Medium { get; set; }
}

public class UserCountryStatistic
{
    public int ChaptersRead { get; set; }
    public int Count { get; set; }
    public object Country { get; set; }
    public double MeanScore { get; set; }
    public List<int?> mediaIds { get; set; }
    public int MinutesWatched { get; set; }
}

public class UserFormatStatistic
{
    public int ChaptersRead { get; set; }
    public int Count { get; set; }
    public MediaFormat? Format { get; set; }
    public double MeanScore { get; set; }
    public List<int?> mediaIds { get; set; }
    public int MinutesWatched { get; set; }
}

public class UserGenreStatistic
{
    public int ChaptersRead { get; set; }
    public int Count { get; set; }
    public string Genre { get; set; }
    public double MeanScore { get; set; }
    public List<int?> mediaIds { get; set; }
    public int MinutesWatched { get; set; }
}

public class UserLengthStatistic
{
    public int ChaptersRead { get; set; }
    public int Count { get; set; }
    public string Length { get; set; }
    public double MeanScore { get; set; }
    public List<int?> mediaIds { get; set; }
    public int MinutesWatched { get; set; }
}

public class UserModData
{
    public List<User> alts { get; set; }
    public object Bans { get; set; }
    public object Counts { get; set; }
    public string Email { get; set; }
    public object Ip { get; set; }
    public int? Privacy { get; set; }
}

public class UserOptions
{
    public int? ActivityMergeTime { get; set; }
    public bool? AiringNotifications { get; set; }
    public List<ListActivityOption> disabledListActivity { get; set; }
    public bool? DisplayAdultContent { get; set; }
    public List<NotificationOption> notificationOptions { get; set; }
    public string ProfileColor { get; set; }
    public bool? RestrictMessagesToFollowing { get; set; }
    public UserStaffNameLanguage? StaffNameLanguage { get; set; }
    public string Timezone { get; set; }
    public UserTitleLanguage? TitleLanguage { get; set; }
}

public class UserPreviousName
{
    public int? CreatedAt { get; set; }
    public string Name { get; set; }
    public int? UpdatedAt { get; set; }
}

public class UserReleaseYearStatistic
{
    public int ChaptersRead { get; set; }
    public int Count { get; set; }
    public double MeanScore { get; set; }
    public List<int?> mediaIds { get; set; }
    public int MinutesWatched { get; set; }
    public int? ReleaseYear { get; set; }
}

public class UserScoreStatistic
{
    public int ChaptersRead { get; set; }
    public int Count { get; set; }
    public double MeanScore { get; set; }
    public List<int?> mediaIds { get; set; }
    public int MinutesWatched { get; set; }
    public int? Score { get; set; }
}

public class UserStaffStatistic
{
    public int ChaptersRead { get; set; }
    public int Count { get; set; }
    public double MeanScore { get; set; }
    public List<int?> mediaIds { get; set; }
    public int MinutesWatched { get; set; }
    public Staff Staff { get; set; }
}

public class UserStartYearStatistic
{
    public int ChaptersRead { get; set; }
    public int Count { get; set; }
    public double MeanScore { get; set; }
    public List<int?> mediaIds { get; set; }
    public int MinutesWatched { get; set; }
    public int? StartYear { get; set; }
}

public class UserStatisticTypes
{
    public UserStatistics Anime { get; set; }
    public UserStatistics Manga { get; set; }
}

public class UserStatistics
{
    public int ChaptersRead { get; set; }
    public int Count { get; set; }
    public List<UserCountryStatistic> countries { get; set; }
    public int EpisodesWatched { get; set; }
    public List<UserFormatStatistic> formats { get; set; }
    public List<UserGenreStatistic> genres { get; set; }
    public List<UserLengthStatistic> lengths { get; set; }
    public double MeanScore { get; set; }
    public int MinutesWatched { get; set; }
    public List<UserReleaseYearStatistic> releaseYears { get; set; }
    public List<UserScoreStatistic> scores { get; set; }
    public List<UserStaffStatistic> staff { get; set; }
    public double StandardDeviation { get; set; }
    public List<UserStartYearStatistic> startYears { get; set; }
    public List<UserStatusStatistic> statuses { get; set; }
    public List<UserStudioStatistic> studios { get; set; }
    public List<UserTagStatistic> tags { get; set; }
    public List<UserVoiceActorStatistic> voiceActors { get; set; }
    public int VolumesRead { get; set; }
}

public class UserStats
{
    public List<UserActivityHistory> activityHistory { get; set; }
    public ListScoreStats AnimeListScores { get; set; }
    public List<ScoreDistribution> animeScoreDistribution { get; set; }
    public List<StatusDistribution> animeStatusDistribution { get; set; }
    public int? ChaptersRead { get; set; }
    public List<StaffStats> favouredActors { get; set; }
    public List<FormatStats> favouredFormats { get; set; }
    public List<GenreStats> favouredGenres { get; set; }
    public List<GenreStats> favouredGenresOverview { get; set; }
    public List<StaffStats> favouredStaff { get; set; }
    public List<StudioStats> favouredStudios { get; set; }
    public List<TagStats> favouredTags { get; set; }
    public List<YearStats> favouredYears { get; set; }
    public ListScoreStats MangaListScores { get; set; }
    public List<ScoreDistribution> mangaScoreDistribution { get; set; }
    public List<StatusDistribution> mangaStatusDistribution { get; set; }
    public int? WatchedTime { get; set; }
}

public class UserStatusStatistic
{
    public int ChaptersRead { get; set; }
    public int Count { get; set; }
    public double MeanScore { get; set; }
    public List<int?> mediaIds { get; set; }
    public int MinutesWatched { get; set; }
    public MediaListStatus? Status { get; set; }
}

public class UserStudioStatistic
{
    public int ChaptersRead { get; set; }
    public int Count { get; set; }
    public double MeanScore { get; set; }
    public List<int?> mediaIds { get; set; }
    public int MinutesWatched { get; set; }
    public Studio Studio { get; set; }
}

public class UserTagStatistic
{
    public int ChaptersRead { get; set; }
    public int Count { get; set; }
    public double MeanScore { get; set; }
    public List<int?> mediaIds { get; set; }
    public int MinutesWatched { get; set; }
    public MediaTag Tag { get; set; }
}

public class UserVoiceActorStatistic
{
    public int ChaptersRead { get; set; }
    public List<int?> characterIds { get; set; }
    public int Count { get; set; }
    public double MeanScore { get; set; }
    public List<int?> mediaIds { get; set; }
    public int MinutesWatched { get; set; }
    public Staff VoiceActor { get; set; }
}

public class YearStats
{
    public int? Amount { get; set; }
    public int? MeanScore { get; set; }
    public int? Year { get; set; }
}
