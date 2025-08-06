using System.Globalization;
using System.Text;
using HtmlAgilityPack;

namespace Kommunist.Application.Helpers;

public static class HtmlConverter
{
    public static string HtmlToPlainText(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var sb = new StringBuilder();

        var heading = doc.DocumentNode.SelectSingleNode("//h5");
        if (heading != null)
        {
            AppendCentered(sb, heading.InnerText.Trim());
            sb.AppendLine();
        }

        var paragraph = doc.DocumentNode.SelectSingleNode("//h6");
        if (paragraph != null)
        {
            sb.AppendLine(HtmlEntity.DeEntitize(paragraph.InnerText.Trim()));
        }

        sb.AppendLine();

        var listItems = doc.DocumentNode.SelectNodes("//ul/li");
        var paragraphs = doc.DocumentNode.SelectNodes("//ul/p");

        if (listItems == null || paragraphs == null || listItems.Count != paragraphs.Count)
            return sb.ToString().TrimEnd();

        for (var i = 0; i < listItems.Count; i++)
        {
            var bullet = $"•{listItems[i].InnerText.Trim()}";
            var lines = HtmlEntity.DeEntitize(paragraphs[i].InnerText.Trim())
                ?.Replace("\r", "")
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