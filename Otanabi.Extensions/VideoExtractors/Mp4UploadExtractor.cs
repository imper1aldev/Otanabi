using HtmlAgilityPack;
using JsUnpacker;
using Otanabi.Core.Models;
using Otanabi.Extensions.Contracts.VideoExtractors;

namespace Otanabi.Extensions.VideoExtractors;
internal class Mp4UploadExtractor : IVideoExtractor
{
    public async Task<SelectedSource> GetStreamAsync(string url)
    {
        try
        {
            HtmlWeb oWeb = new();
            var doc = await oWeb.LoadFromWebAsync(url);
            var packedScript = doc.DocumentNode.Descendants()
                .FirstOrDefault(x => x.Name == "script" && x.InnerText?.Contains("p,a,c,k,e,d") == true)?.InnerText;
            if (string.IsNullOrEmpty(packedScript))
            {
                packedScript = doc.DocumentNode.Descendants()
                .FirstOrDefault(x => x.Name == "script" && x.InnerText?.Contains("player.src") == true)?.InnerText;
            }
            else
            {
                packedScript = Unpacker.UnpackAndCombine(packedScript);
            }

            var videoUrl = packedScript.SubstringAfter(".src(").SubstringBefore(")")
                .SubstringAfter("src:").SubstringAfter("\"").SubstringBefore("\"");

            return new(videoUrl, null);
        }
        catch (Exception)
        {
            return new();
        }
    }
}
