using HtmlAgilityPack;
using Otanabi.Core.Models;
using Otanabi.Extensions.Contracts.VideoExtractors;

namespace Otanabi.Extensions.VideoExtractors;
public class YourUploadExtractor : IVideoExtractor
{
    public async Task<SelectedSource> GetStreamAsync(string url)
    {
        var newHeaders = new HttpClient().DefaultRequestHeaders;
        newHeaders.Add("Referer", "https://www.yourupload.com/");
        try
        {
            HtmlWeb oWeb = new();
            var document = await oWeb.LoadFromWebAsync(url);
            var baseData = document.DocumentNode.Descendants()
                .FirstOrDefault(x => x.Name == "script" && x.InnerText?.Contains("jwplayerOptions") == true);
            if (baseData != null)
            {
                var basicUrl = baseData.InnerText.SubstringAfter("file: '").SubstringBefore("',");
                return new(basicUrl, newHeaders);
            }
            return new();
        }
        catch (Exception)
        {
            return new();
        }
    }
}
