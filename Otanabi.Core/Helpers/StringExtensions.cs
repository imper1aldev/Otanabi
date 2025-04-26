using System.Text;
using System.Text.RegularExpressions;
using System.Web;

public static class StringExtensions
{
    public static string SubstringBetween(this string str, string start, string end)
    {
        var startIndex = str.IndexOf(start);
        if (startIndex == -1)
            return "";

        startIndex += start.Length;

        var endIndex = str.IndexOf(end, startIndex);
        if (endIndex == -1)
            return "";

        return str.Substring(startIndex, endIndex - startIndex);
    }
    public static int? ToIntOrNull(this string? value) => int.TryParse(value, out var i) ? i : null;

    public static int? ToIntOrNull(this string? value, int radix)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        var length = value?.Length;
        if (length == 0)
            return null;

        return Convert.ToByte(value, radix);
    }

    public static string Reverse(this string value)
    {
        var charArray = value.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    public static string SubstringAfter(this string str, string delimiter)
    {
        if (string.IsNullOrEmpty(str) || delimiter == null)
            return str;

        int index = str.IndexOf(delimiter);
        if (index == -1)
            return str;

        return str.Substring(index + delimiter.Length);
    }

    public static string SubstringBefore(this string str, string delimiter)
    {
        if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(delimiter))
            return str;

        int index = str.IndexOf(delimiter);
        if (index == -1)
            return str;

        return str.Substring(0, index);
    }

    public static string DecodeBase64(this string value) =>
        Encoding.UTF8.GetString(Convert.FromBase64String(value));

    public static byte[] DecodeBase64ToBytes(this string value) => Convert.FromBase64String(value);

    private static readonly Regex _whitespace = new(@"\s+");

    public static string ReplaceWhitespaces(this string input, string replacement) =>
        _whitespace.Replace(input, replacement);

    public static string RemoveWhitespaces(this string input) =>
        input.ReplaceWhitespaces(string.Empty);

    public static string RemovePrefix(this string input, string prefix)
    {
        if (input.StartsWith(prefix))
            return input.Remove(0, prefix.Length);

        return input;
    }

    public static string RemovePrefix(this string input, int prefixLen)
    {
        if (input.Length < prefixLen)
            return string.Empty;

        return input.Remove(0, prefixLen);
    }

    public static string TrimAll(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        return Regex.Replace(input, @"[\s\u200B\u200C\u200D\uFEFF]+", " ").Trim();
    }

    public static string GetParameter(this string url, string key)
    {
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(key))
        {
            return null;
        }
        try
        {
            var uri = new Uri(url);
            var query = HttpUtility.ParseQueryString(uri.Query);
            return query[key];
        }
        catch
        {
            return null;
        }
    }

    public static string RemoveParameter(this string url, string key)
    {
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(key))
        {
            return url;
        }

        try
        {
            var uri = new Uri(url);
            var query = HttpUtility.ParseQueryString(uri.Query);

            query.Remove(key);

            var baseUrl = url.Split('?')[0];
            var newQuery = string.Join("&", query.AllKeys
                .Where(k => !string.IsNullOrEmpty(k))
                .Select(k => $"{HttpUtility.UrlEncode(k)}={HttpUtility.UrlEncode(query[k])}"));

            return string.IsNullOrEmpty(newQuery) ? baseUrl : $"{baseUrl}?{newQuery}";
        }
        catch
        {
            return url;
        }
    }

    public static string AddOrUpdateParameter(this string url, string key, string value)
    {
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(key))
        {
            return url;
        }

        try
        {
            var uri = new Uri(url);
            var query = HttpUtility.ParseQueryString(uri.Query);
            query[key] = value;

            var baseUrl = url.Split('?')[0];
            var newQuery = string.Join("&", query.AllKeys
                .Where(k => !string.IsNullOrEmpty(k))
                .Select(k => $"{HttpUtility.UrlEncode(k)}={HttpUtility.UrlEncode(query[k])}"));

            return string.IsNullOrEmpty(newQuery) ? baseUrl : $"{baseUrl}?{newQuery}";
        }
        catch
        {
            return url;
        }
    }

}