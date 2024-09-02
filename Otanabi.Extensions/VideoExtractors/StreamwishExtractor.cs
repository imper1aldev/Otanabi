using Otanabi.Extensions.Contracts.VideoExtractors;
using HtmlAgilityPack;
using JsUnpacker;
using System.Net.Http.Headers;
namespace Otanabi.Extensions.VideoExtractors;
public class StreamwishExtractor : IVideoExtractor
{
    public async Task<(string,HttpHeaders)> GetStreamAsync(string url)
    {
        var streamUrl = "";
        try
        {
            HtmlWeb oWeb = new HtmlWeb();
            HtmlDocument doc = await oWeb.LoadFromWebAsync(url);

            var packed = doc.DocumentNode.Descendants()
                         .FirstOrDefault(x => x.Name == "script" && x.InnerText?.Contains("eval") == true);
            var unpacked = "";
            if (Unpacker.IsPacked(packed?.InnerText))
            {
                unpacked = Unpacker.UnpackAndCombine(packed?.InnerText);
            }
            else
            {
                //not valid pack
                return (streamUrl = "",null);
            }

            streamUrl = unpacked.SubstringAfter("sources:[{file:\"").Split(new[] { "\"}" }, StringSplitOptions.None)[0];

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        return (streamUrl,null);
    }
}
