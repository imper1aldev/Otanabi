using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeWatcher.Core.Models;
public class Provider
{
    public int id { get; set; }
    public string name { get; set; }
    public string url { get; set; }
    public ICollection<Anime> animes { get; set; }
}
