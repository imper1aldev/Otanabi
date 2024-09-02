using SQLite;

namespace Otanabi.Core.Models;

public class AnimexFavorite
{
    [AutoIncrement, PrimaryKey]
    public int Id { get; set; }
    public int AnimeId { get; set; }
    public int FavoriteListId { get; set; }

    [Ignore]
    public Anime Anime { get; set; }

    [Ignore]
    public FavoriteList FavoriteList { get; set; }
}
