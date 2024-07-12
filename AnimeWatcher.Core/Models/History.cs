using SQLite;

namespace AnimeWatcher.Core.Models;
public class History
{
    [AutoIncrement, PrimaryKey]
    public int Id
    {
        get; set;
    }
    public DateTime WatchedDate
    {
        get; set;
    }
    public long SecondsWatched
    {
        get; set;
    }
    public int ChapterId
    {
        get; set;
    }
    [Ignore]
    public string TimeString => TimeSpan.FromMilliseconds(SecondsWatched).ToString(@"hh\:mm\:ss");
}
