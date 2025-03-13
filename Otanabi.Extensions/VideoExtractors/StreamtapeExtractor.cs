using Otanabi.Core.Models;
using Otanabi.Extensions.Contracts.VideoExtractors;

namespace Otanabi.Extensions.VideoExtractors;

public class StreamtapeExtractor : IVideoExtractor
{
    private static readonly HttpClient client = new();

    public async Task<SelectedSource> GetStreamAsync(string url)
    {
        var streamUrl = "";
        var newHeaders = new HttpClient().DefaultRequestHeaders;
        try
        {
            var baseUrl = "https://streamtape.com/e/";
            var newUrl = url.StartsWith(baseUrl)
                ? url
                : baseUrl + url.Split('/').ElementAtOrDefault(4);

            if (newUrl == null)
            {
                return new(streamUrl, newHeaders);
            }

            var response = await client.GetStringAsync(newUrl);
            var scriptData = response.SubstringAfter("document.getElementById('robotlink').innerHTML = ").SubstringBefore(";");
            var baseVideo = scriptData.SubstringBetween("'//", "'+");
            var toex = scriptData.SubstringBetween("('xcd", "').substring");
            var videoUrl = $"https://{baseVideo}" + toex;
            streamUrl = videoUrl;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            // Handle exceptions as needed
        }
        return new(streamUrl, newHeaders);
    }

}
