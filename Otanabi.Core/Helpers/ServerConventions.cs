using Otanabi.Core.Models;

namespace Otanabi.Core.Helpers;

public class ServerConventions
{
    internal List<Convention> Conventions =
        [
            new Convention
            {
                Name = "Okru",
                PossibleNames = ["ok-ru", "okru", "OKRU"]
            },
            new Convention
            {
                Name = "Streamwish",
                PossibleNames = ["sw", "SW", "Streamwish", "streamwish"]
            },
            new Convention
            {
                Name = "Streamtape",
                PossibleNames = ["stape", "Stape","Streamtape","streamtape"]
            },
            new Convention {
                Name = "Juro",
                PossibleNames = ["juro"]
            },
            new Convention {
                Name = "Mp4Upload",
                PossibleNames = ["mp4", "mp4upload"]
            }
        ];

    public string GetServerName(string serverName)
    {
        var convention = "";
        try
        {
            convention = Conventions.First(e => e.PossibleNames.Contains(serverName)).Name;
        }
        catch (Exception) { }
        return convention;
    }
}
