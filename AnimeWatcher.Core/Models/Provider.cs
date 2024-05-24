using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeWatcher.Core.Models;
public class Provider
{
    public int? Id { get; set; }
    #nullable enable
    public string? Name { get; set; }
    #nullable enable
    public string? Url { get; set; }
    #nullable enable
    public Anime[]? Animes { get; set; }
}
