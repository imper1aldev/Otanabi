using System;
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
}