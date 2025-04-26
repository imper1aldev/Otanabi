using System.Text.Json.Serialization;

namespace Otanabi.Extensions.Models.Hstream;
public class PlayerApiResponse
{
    [JsonPropertyName("legacy")]
    public int Legacy { get; set; } = 0;

    [JsonPropertyName("resolution")]
    public string Resolution { get; set; } = "4k";

    [JsonPropertyName("stream_url")]
    public string StreamUrl
    {
        get; set;
    }

    [JsonPropertyName("stream_domains")]
    public List<string> StreamDomains
    {
        get; set;
    }
}
