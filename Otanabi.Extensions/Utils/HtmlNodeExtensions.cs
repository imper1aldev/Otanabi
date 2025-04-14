using HtmlAgilityPack;

namespace Otanabi.Extensions.Utils;

public static class HtmlNodeExtensions
{
    public static string? GetImageUrl(this HtmlNode node, string basePath)
    {
        if (node.IsValidUrl("data-src"))
            return node.GetAbsoluteUrl("data-src", basePath);

        if (node.IsValidUrl("data-lazy-src"))
            return node.GetAbsoluteUrl("data-lazy-src", basePath);

        if (node.IsValidUrl("srcset"))
        {
            var srcset = node.GetAbsoluteUrl("srcset", basePath);
            return srcset.Split(' ')[0]; // Toma la primera URL del srcset
        }

        if (node.IsValidUrl("src"))
            return node.GetAbsoluteUrl("src", basePath);

        return "";
    }

    public static bool IsValidUrl(this HtmlNode node, string attrName)
    {
        if (!node.Attributes.Contains(attrName))
            return false;

        var attrValue = node.GetAttributeValue(attrName, "");
        return !attrValue.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetAbsoluteUrl(this HtmlNode node, string attrName, string basePath)
    {
        var value = node.GetAttributeValue(attrName, "");

        try
        {
            // Intenta convertir a URI absoluta, combinando con basePath si es necesario
            var uri = new Uri(value, UriKind.RelativeOrAbsolute);
            if (!uri.IsAbsoluteUri && Uri.TryCreate(new Uri(basePath), uri, out Uri? resolved))
                return resolved.ToString();

            return uri.ToString();
        }
        catch
        {
            // En caso de error de formato, regresamos el valor crudo
            return value;
        }
    }
}

