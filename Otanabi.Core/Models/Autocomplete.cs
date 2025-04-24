using SQLite;

namespace Otanabi.Core.Models;

public class Autocomplete
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Term { get; set; }
}
