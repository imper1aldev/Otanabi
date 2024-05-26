using SQLite;

namespace AnimeWatcher.Core.Models;
public class Provider
{
    [AutoIncrement,PrimaryKey]
    public int Id { get; set; }
    #nullable enable
    public string? Name { get; set; }
    #nullable enable
    public string? Url { get; set; }
    #nullable enable
    [Ignore]
    public Anime[]? Animes { get; set; }
}
