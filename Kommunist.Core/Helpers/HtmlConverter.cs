using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using HtmlAgilityPack;

namespace Kommunist.Core.Helpers;

public static class HtmlConverter
{
    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
    public static string HtmlToPlainText(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        if (!html.Contains('<'))
            return HtmlEntity.DeEntitize(html.Trim());
        
        html = html.Replace("<br>", "\n", StringComparison.OrdinalIgnoreCase)
            .Replace("<br/>", "\n", StringComparison.OrdinalIgnoreCase)
            .Replace("<br />", "\n", StringComparison.OrdinalIgnoreCase);

        var doc = new HtmlDocument();
        if (!html.Contains("<html>", StringComparison.OrdinalIgnoreCase))
            html = $"<html><body>{html}</body></html>";
        doc.LoadHtml(html);
        var sb = new StringBuilder();

        var headers = doc.DocumentNode.SelectNodes("//h1 | //h2 | //h3 | //h4 | //h5 | //h6");
        if (headers is { Count: > 0 })
        {
            foreach (var header in headers)
            {
                AppendCentered(sb, HtmlEntity.DeEntitize(header.InnerText.Trim()));
                sb.AppendLine();
            }
        }

        var paragraphs = doc.DocumentNode.SelectNodes("/html/body/p");
        if (paragraphs is { Count: > 0 })
        {
            foreach (var p in paragraphs)
            {
                sb.AppendLine(HtmlEntity.DeEntitize(p.InnerText.Trim()));
            }
        }

        sb.AppendLine();

        var listItems = doc.DocumentNode.SelectNodes("/html/body/ul/li");
        var listParagraphs = doc.DocumentNode.SelectNodes("/html/body/ul/p");

        if (listItems == null || listParagraphs == null || listItems.Count != listParagraphs.Count)
            return sb.ToString().TrimEnd();

        for (var i = 0; i < listItems.Count; i++)
        {
            var bullet = $"•{HtmlEntity.DeEntitize(listItems[i].InnerText.Trim())}";
            var lines = HtmlEntity.DeEntitize(listParagraphs[i].InnerText.Trim())
                .Replace("\r", "")
                .Split('\n', StringSplitOptions.RemoveEmptyEntries);

            sb.AppendLine(CultureInfo.InvariantCulture, $"{bullet}");
            if (lines == null) continue;

            foreach (var line in lines)
            {
                const string prefix = "  ";
                sb.AppendLine(CultureInfo.InvariantCulture, $"{prefix}{line.Trim()}");
            }
        }

        return sb.ToString().TrimEnd();
    }

    private static void AppendCentered(StringBuilder sb, string text)
    {
        const int width = 100;
        var padding = Math.Max(0, (width - text.Length) / 2 - 4);
        sb.AppendLine(new string(' ', padding) + text);
    }
}