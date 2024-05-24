using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeWatcher.Core.Models;
public class Favorite
{

    public int AnimeId{ get; set; }

    public Anime Anime { get; set; }
    #nullable enable
    public string? CategoryId { get; set; }


}
