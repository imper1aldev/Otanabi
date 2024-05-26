using SQLite;

namespace AnimeWatcher.Core.Models;
public class Chapter
{
    [AutoIncrement,PrimaryKey]
    #nullable enable
    public int? Id {get;set;} 
    public int? AnimeId {get;set;}
    #nullable enable
    public int? ChapterNumber {get;set;}
    #nullable enable
    public string? Name {get;set;}
    #nullable enable
    public int? historyId {get;set;}
    #nullable enable
    public string? Url {get;set; }
    #nullable enable
    [Ignore]
    public History? History {get;set;} 
    
}
