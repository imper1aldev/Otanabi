using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeWatcher.Core.Models;
public class Category
{
    #nullable enable
    public  int? Id {get;set;}
    #nullable enable
    public string? Name {get;set;}
    #nullable enable
    public int? Order { get;set; }
    #nullable enable
    public Favorite[]? Favorites {get;set;}
}
