using System.Net.Http.Headers;
using HtmlAgilityPack;
using JsUnpacker;
using Otanabi.Extensions.Contracts.VideoExtractors;
namespace Otanabi.Extensions.VideoExtractors;
public class StreamwishExtractor : IVideoExtractor
{
    public async Task<(string, HttpHeaders)> GetStreamAsync(string url)
    {
        try
        {
            HtmlWeb oWeb = new();
            var doc = await oWeb.LoadFromWebAsync(url);
            var packedScript = doc.DocumentNode.Descendants()
                .FirstOrDefault(x => x.Name == "script" && x.InnerText?.Contains("eval") == true);

            if (packedScript != null && Unpacker.IsPacked(packedScript.InnerText))
            {
                var unpacked = Unpacker.UnpackAndCombine(packedScript.InnerText);
                var streamUrl = unpacked.SubstringAfter("sources:[{file:\"").Split(["\"}"], StringSplitOptions.None)[0];
                return (streamUrl, null);
            }
        }
        catch (Exception ex)
        {
            // Mejorar la gestión de errores para facilitar la depuración
            Console.WriteLine($"Error extracting video stream: {ex.Message}");
        }

        // Devolver valor por defecto si no se pudo obtener la URL
        return ("", null);
    }
}
