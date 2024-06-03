using SQLite;

namespace AnimeWatcher.Core.Models;
public class Provider
{
    [AutoIncrement,PrimaryKey]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }

    public bool Secured{get; set; }
    public string Type { get; set; }

    [Ignore]
    public Anime[]? Animes { get; set; }
}
