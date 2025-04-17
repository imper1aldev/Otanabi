namespace Otanabi.Core.Models;

public class Track : ITrack
{
    public Track()
    {

    }

    public Track(string file, string label)
    {
        File = file;
        Label = label;
    }

    public string File
    {
        get; set;
    }
    public string Label
    {
        get; set;
    }
    public string Kind
    {
        get; set;
    }
}
