using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeWatcher.Core.Models;
public class Chapter
{
    public int id {get;set;}
    public int animeId {get;set;}
    public int chapter {get;set;}
    public string name {get;set;}
    public int historyId {get;set;}
    public string url {get;set; } 
    public History history {get;set;} 
    
}
