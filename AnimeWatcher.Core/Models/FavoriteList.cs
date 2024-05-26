
using SQLite;

namespace AnimeWatcher.Core.Models;
public class FavoriteList
{
    [AutoIncrement,PrimaryKey]
    #nullable enable
    public  int? Id {get;set;}
    #nullable enable
    public string? Name {get;set;}
    #nullable enable
    public int? Order { get;set; }
    #nullable enable
    [Ignore]
    public Favorite[]? Favorites {get;set;}
}
