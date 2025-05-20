using SQLite;

namespace Otanabi.Core.Models;

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
    public long TotalSeconds
    {
        get; set;
    }
    public int ChapterId
    {
        get; set;
    }
    public bool IsManuallyCompleted
    {
        get; set;
    }

    [Ignore]
    public Chapter Chapter
    {
        get; set;
    }

    [Ignore]
    public string TimeString => TimeSpan.FromSeconds(SecondsWatched).ToString(@"hh\:mm\:ss");

    [Ignore]
    public bool IsWatchedCompleted => IsManuallyCompleted || (TotalSeconds > 0 && (double)SecondsWatched / TotalSeconds >= 0.85);

    [Ignore]
    public string TotalTimeString => TimeSpan.FromSeconds(TotalSeconds).ToString(@"hh\:mm\:ss");
}
