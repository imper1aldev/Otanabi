
using SQLite;

namespace AnimeWatcher.Core.Models;
public class Favorite
{
    [AutoIncrement,PrimaryKey]
    public int Id { get; set; }
    public int AnimeId{ get; set; }
    [Ignore]
    public Anime Anime { get; set; }
    #nullable enable
    public string? CategoryId { get; set; }


}
