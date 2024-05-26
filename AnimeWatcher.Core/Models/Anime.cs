using SQLite;

namespace AnimeWatcher.Core.Models;
public enum AnimeType { OVA, TV, MOVIE ,SPECIAL,OTHER }
public class Anime
{   
    [PrimaryKey, AutoIncrement]
    public int Id
    {
        get; set;
    }
    public string RemoteID
    {
        get; set;
    }
    public string Cover
    {
        get; set;
    }
    public string Title
    {
        get; set;
    }
    public string Url
    {
        get; set;
    }
    public AnimeType Type
    {
        get; set;
    }
    public string Description
    {
        get; set;
    }
    public int ProviderId
    {
        get; set;
    }
    [Ignore]
    public Provider Provider
    {
        get; set;
    }
    public string Status
    {
        get; set;
    }
    [Ignore]
    public ICollection<Chapter> Chapters
    {
        get; set;
    }

    public string TypeStr=>Type.ToString();
     
}
