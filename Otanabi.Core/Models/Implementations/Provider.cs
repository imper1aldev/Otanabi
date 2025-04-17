using SQLite;

namespace Otanabi.Core.Models;

public class Provider : IProvider
{
    [PrimaryKey]
    public int Id
    {
        get; set;
    }
    public string Name
    {
        get; set;
    }
    public string Url
    {
        get; set;
    }

    //this property means that the url data changes over time, only applies to animepahe
    public bool Persistent
    {
        get; set;
    }
    public string Type
    {
        get; set;
    }
    public bool IsNsfw
    {
        get; set;
    }

    [Ignore]
    public Anime[]? Animes
    {
        get; set;
    }
}
