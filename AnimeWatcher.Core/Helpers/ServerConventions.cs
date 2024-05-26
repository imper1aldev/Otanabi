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
            PossibleNames = new string[] { "sw", "SW", "OKRU", "streamwish" }
        },
        new Convention
        {
            Name= "Yourupload",
            PossibleNames = new string[] { "yu","YourUpload","yourUpload","yourupload" }
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
