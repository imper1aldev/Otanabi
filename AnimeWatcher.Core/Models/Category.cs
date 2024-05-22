using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeWatcher.Core.Models;
public class Category
{
    public  int id {get;set;}
    public string name {get;set;}
    public int order { get;set; }
    public ICollection<Favorite> favorites {get;set;}
}
