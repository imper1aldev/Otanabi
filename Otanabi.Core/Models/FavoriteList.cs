using SQLite;

namespace Otanabi.Core.Models;

public class FavoriteList
{
    [AutoIncrement, PrimaryKey]
    public int Id
    {
        get; set;
    }
    public string Name
    {
        get; set;
    }
    public int Placement
    {
        get; set;
    }
}
