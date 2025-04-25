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

        if (listItems != null && paragraphs != null && listItems.Count == paragraphs.Count)
        {
            for (int i = 0; i < listItems.Count; i++)
            {
                string bullet = $"•{listItems[i].InnerText.Trim()}";
                string[] lines = HtmlEntity.DeEntitize(paragraphs[i].InnerText.Trim())
                    ?.Replace("\r", "")
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries);

                sb.AppendLine(bullet);
                for (int j = 0; j < lines.Length; j++)
                {
                    string prefix = "  ";
                    sb.AppendLine($"{prefix}{lines[j].Trim()}");
                }
            }
        }

        return sb.ToString().TrimEnd();
    }

    private static void AppendCentered(StringBuilder sb, string text)
    {
        const int width = 100;
        int padding = Math.Max(0, (width - text.Length) / 2 - 4);
        sb.AppendLine(new string(' ', padding) + text);
    }
}