using SQLite;

namespace Otanabi.Core.Models;

public class History
{
    [AutoIncrement, PrimaryKey]
    public int Id { get; set; }
    public DateTime WatchedDate { get; set; }
    public long SecondsWatched { get; set; }
    public long TotalMedia { get; set; } = 0;
    public int ChapterNumber { get; set; }
    public int AnimeId { get; set; }

    [Ignore]
    public Anime Anime { get; set; }

    [Ignore]
    public string TimeString => TimeSpan.FromSeconds(SecondsWatched).ToString(@"hh\:mm\:ss");

    [Ignore]
    public string TotalMediaString => TimeSpan.FromSeconds(TotalMedia).ToString(@"hh\:mm\:ss");

    [Ignore]
    public double ProgressPercentage => TotalMedia > 0 ? (double)SecondsWatched / TotalMedia * 100 : 0;
}
