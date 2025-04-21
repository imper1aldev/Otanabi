using HtmlAgilityPack;
using Otanabi.Core.Models;
using Otanabi.Extensions.Contracts.VideoExtractors;

namespace Otanabi.Extensions.VideoExtractors;

public class YourUploadExtractor : IVideoExtractor
{
    private readonly HttpClient _client = new();

    public async Task<SelectedSource> GetStreamAsync(string url)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Referrer", "https://www.yourupload.com/");
            request.Headers.Referrer = new Uri("https://www.yourupload.com/");

            var response = await _client.SendAsync(request);
            var html = await response.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var baseData = doc.DocumentNode.Descendants()
                .FirstOrDefault(x => x.Name == "script" && x.InnerText?.Contains("jwplayerOptions") == true)?.InnerText;

            if (!string.IsNullOrEmpty(baseData))
            {
                var basicUrl = baseData.SubstringAfter("file: '").SubstringBefore("',");
                return new(basicUrl, request.Headers)
                {
                    UseVlcProxy = true
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[YourUploadExtractor] Error extracting video URL: {ex.Message}");
        }
        return new();
    }
}
