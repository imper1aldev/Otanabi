using HtmlAgilityPack;
using Otanabi.Core.Models;
using Otanabi.Extensions.Contracts.VideoExtractors;

namespace Otanabi.Extensions.VideoExtractors;

public class StreamtapeExtractor : IVideoExtractor
{
    public async Task<SelectedSource> GetStreamAsync(string url)
    {
        try
        {
            var baseUrl = "https://streamtape.com/e/";
            var normalizedUrl = url.StartsWith(baseUrl)
                ? url
                : $"{baseUrl}{url.Split('/', StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(3)}";

            if (string.IsNullOrWhiteSpace(normalizedUrl))
            {
                return new();
            }

            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(normalizedUrl);

            var scriptNode = doc.DocumentNode
                .SelectSingleNode("//script[contains(text(), 'robotlink')]");

            if (scriptNode != null)
            {
                var script = scriptNode.InnerText;

                var part1 = script.SubstringAfter("document.getElementById('robotlink').innerHTML = '")
                                  .SubstringBefore("'");
                var part2 = script.SubstringAfter("+ ('xcd")
                                  .SubstringBefore("'");

                var videoUrl = $"https:{part1}xcd{part2}";

                return new(videoUrl, null, null);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting Streamtape video URL: {ex.Message}");
        }

        return new();
    }
}