using AnimeWatcher.Core.Models;

namespace AnimeWatcher.Core.Helpers;
public class ServerConventions
{
    internal List<Convention> Conventions = new()
    {
        new Convention
        {
            Name = "Okru",
            PossibleNames = new string[] { "ok-ru", "okru", "OKRU" }
        },new Convention
        {
            Name = "Streamwish",
            PossibleNames = new string[] { "sw", "SW", "Streamwish", "streamwish" }
        },
        new Convention
        {
            Name= "Juro",
            PossibleNames = new string[] { "juro" }
        }
    };


    public string GetServerName(string serverName)
    {
        var convention = "";
        try
        {
            convention = Conventions.First(e => e.PossibleNames.Contains(serverName)).Name;
        } catch (Exception)
        {

        }
        return convention;
    }


}
