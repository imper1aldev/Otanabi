using SQLite;

namespace Otanabi.Core.Models;

public class History
{
    [AutoIncrement, PrimaryKey]
    public int Id { get; set; }
    public DateTime WatchedDate { get; set; }
    public long SecondsWatched { get; set; }
    public int ChapterNumber { get; set; }
    public int AnimeId { get; set; }

    [Ignore]
    public Anime Anime { get; set; }

    [Ignore]
    public string TimeString => TimeSpan.FromSeconds(SecondsWatched).ToString(@"hh\:mm\:ss");
}
