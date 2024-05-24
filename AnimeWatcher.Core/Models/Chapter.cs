using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeWatcher.Core.Models;
public class Chapter
{
    #nullable enable
    public int? Id {get;set;}
    #nullable enable
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
    public History? History {get;set;} 
    
}
