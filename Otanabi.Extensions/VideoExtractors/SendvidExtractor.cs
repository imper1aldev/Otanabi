using HtmlAgilityPack;
using Otanabi.Core.Models;
using Otanabi.Extensions.Contracts.VideoExtractors;

namespace Otanabi.Extensions.VideoExtractors;

public class SendvidExtractor : IVideoExtractor
{
    private readonly HttpClient _client;

    public SendvidExtractor(HttpClient client)
    {
        _client = client;
    }

    public async Task<SelectedSource> GetStreamAsync(string url)
    {
        try
        {
            var response = await _client.GetAsync(url);
            var html = await response.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var sourceNode = doc.DocumentNode.SelectSingleNode("//source[@id='video_source']");
            var masterUrl = sourceNode?.GetAttributeValue("src", null);

            if (string.IsNullOrWhiteSpace(masterUrl))
            {
                return new();
            }

            Console.WriteLine($"[SendvidExtractor] Master URL: {masterUrl}");

            return new(masterUrl, null, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SendvidExtractor] Error extracting stream: {ex.Message}");
            return new();
        }
    }
}
