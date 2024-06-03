using SQLite;

namespace AnimeWatcher.Core.Models;
public class Chapter
{
    [AutoIncrement,PrimaryKey]    
    public int Id {get;set;} 
    public int AnimeId {get;set;}
    public int ChapterNumber {get;set;}
    public string Name {get;set;}
    public string Url {get;set; }
    public string Extraval {get;set;}
    [Ignore]
    public History History {get;set;}
    
    
}
