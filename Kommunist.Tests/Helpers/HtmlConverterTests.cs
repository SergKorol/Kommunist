using System.Text.RegularExpressions;
using Kommunist.Application.Helpers;

namespace Kommunist.Tests.Helpers
{
    public class HtmlConverterTests
    {
        [Fact]
        public void HtmlToPlainText_EmptyHtml_ReturnsEmptyString()
        {
            // Arrange
            var html = "";

            // Act
            var result = HtmlConverter.HtmlToPlainText(html);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void HtmlToPlainText_NullHtml_ThrowsArgumentNullException()
        {
            // Arrange
            string? html = null;

            // Act / Assert
            Assert.Throws<ArgumentNullException>(() => HtmlConverter.HtmlToPlainText(html!));
        }

        [Fact]
        public void HtmlToPlainText_NoHeadingNoParagraphNoList_ReturnsEmpty()
        {
            // Arrange
            var html = "<div><span>ignored</span></div>";

            // Act
            var result = HtmlConverter.HtmlToPlainText(html);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void HtmlToPlainText_OnlyH5_CenteredHeading_NoTrailingNewline()
        {
            // Arrange
            var text = "My Title";
            var html = $"<h5>{text}</h5>";

            // Act
            var result = HtmlConverter.HtmlToPlainText(html);

            // Assert
            // Centered to width 100 with padding = max(0, (100 - len)/2 - 4)
            var padding = Math.Max(0, (100 - text.Length) / 2 - 4);
            var expected = new string(' ', padding) + text;

            Assert.Equal(expected, result);
        }

        [Fact]
        public void HtmlToPlainText_OnlyH6_ParagraphDecoded_NoTrailingNewline()
        {
            // Arrange
            var html = "<h6>Some &amp; paragraph</h6>";

            // Act
            var result = HtmlConverter.HtmlToPlainText(html);

            // Assert
            Assert.Equal("Some & paragraph", result);
        }

        [Fact]
        public void HtmlToPlainText_H5AndH6_CenteredHeadingThenParagraph()
        {
            // Arrange
            var title = "Centered Title";
            var para = "Sub &amp; Title";
            var html = $"<h5>{title}</h5><h6>{para}</h6>";

            // Act
            var result = HtmlConverter.HtmlToPlainText(html);

            // Assert
            var padding = Math.Max(0, (100 - title.Length) / 2 - 4);
            var expectedHeading = new string(' ', padding) + title;

            // Expected is:
            // [centered h5]
            // [blank line]
            // [decoded h6]
            // (no trailing blank line)
            var expected = string.Join(Environment.NewLine, new[]
            {
                expectedHeading,
                "", // the extra line after heading that remains before paragraph
                "Sub & Title"
            });

            Assert.Equal(expected, result);
        }

        [Fact]
        public void HtmlToPlainText_ListItemsAndParagraphs_MatchingCounts_RendersBulletsWithIndentedLines()
        {
            // Arrange
            // Two list entries with corresponding paragraphs (equal counts)
            // Use <br> to make multi-line paragraph for the second item
            var html = @"
                <h5>Title</h5>
                <h6>Intro &amp; More</h6>
                <ul>
                    <li>First</li>
                    <p>Alpha</p>

                    <li>Second</li>
                    <p>Line1<br>Line2</p>
                </ul>";

            // Act
            var result = HtmlConverter.HtmlToPlainText(html);

            // Assert
            var headingPadding = Math.Max(0, (100 - "Title".Length) / 2 - 4);
            var expected = string.Join(Environment.NewLine, new[]
            {
                new string(' ', headingPadding) + "Title",
                "",                          // blank after heading
                "Intro & More",
                "",                          // blank before list begins
                "•First",
                "  Alpha",
                "•Second",
                "  Line1Line2"
            });

            Assert.Equal(expected, result);
        }

        [Fact]
        public void HtmlToPlainText_ListItemsAndParagraphs_EntitiesDecodedAndWhitespaceTrimmed()
        {
            // Arrange
            var html = @"
                <ul>
                    <li>  One &amp; Only  </li>
                    <p>  A &amp; B  </p>

                    <li>  Two  </li>
                    <p>  X &amp; Y  </p>
                </ul>";

            // Act
            var result = HtmlConverter.HtmlToPlainText(html);

            // Assert
            var expected = string.Join(Environment.NewLine, new[]
            {
                "\n•One &amp; Only",
                "  A & B",
                "•Two",
                "  X & Y"
            });

            Assert.Equal(expected, result);
        }

        [Fact]
        public void HtmlToPlainText_ListItemsWithWindowsNewlines_RemovesCarriageReturnsAndSplitsLines()
        {
            // Arrange
            var html = @"
                <ul>
                    <li>Item</li>
                    <p>Line1\r\nLine2\r\nLine3</p>
                </ul>";

            // Act
            var result = HtmlConverter.HtmlToPlainText(html);

            // Assert
            var expected = string.Join(Environment.NewLine, new[]
            {
                "\n•Item",
                "  Line1\\r\\nLine2\\r\\nLine3"
            });

            Assert.Equal(expected, result);
        }

        [Fact]
        public void HtmlToPlainText_ListItems_ParagraphsMissing_ReturnsHeadingAndOrParagraphsOnly_NoList()
        {
            // Arrange
            var html = @"
                <h5>Title</h5>
                <h6>Lead</h6>
                <ul>
                    <li>A</li>
                    <li>B</li>
                </ul>";

            // Act
            var result = HtmlConverter.HtmlToPlainText(html);

            // Assert
            // Because paragraphs == null, the method returns early (no list rendered)
            var pad = Math.Max(0, (100 - "Title".Length) / 2 - 4);
            var expected = string.Join(Environment.NewLine, new[]
            {
                new string(' ', pad) + "Title",
                "",
                "Lead"
            });

            Assert.Equal(expected, result);
        }

        [Fact]
        public void HtmlToPlainText_ListItemsCountDoesNotMatchParagraphsCount_ReturnsEarlyWithoutList()
        {
            // Arrange
            var html = @"
                <h6>Only lead</h6>
                <ul>
                    <li>First</li>
                    <p>Para1</p>
                    <li>Second</li>
                    <!-- Missing corresponding <p> -->
                </ul>";

            // Act
            var result = HtmlConverter.HtmlToPlainText(html);

            // Assert
            // counts differ (2 li, 1 p), so list is not rendered
            Assert.Equal("Only lead", result);
        }

        [Fact]
        public void HtmlToPlainText_HeadingWithEntities_AreHandledViaInnerText()
        {
            // Arrange
            // HtmlAgilityPack InnerText decodes entities for h5/h6
            var html = "<h5>Tom & Jerry</h5>";

            // Act
            var result = HtmlConverter.HtmlToPlainText(html);

            // Assert
            var pad = Math.Max(0, (100 - "Tom & Jerry".Length) / 2 - 4);
            var expected = new string(' ', pad) + "Tom & Jerry";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void HtmlToPlainText_TrimsTrailingNewlines()
        {
            // Arrange
            // Ensure that the function does not end with a trailing newline
            var html = "<h6>EndNoNewline</h6>";

            // Act
            var result = HtmlConverter.HtmlToPlainText(html);

            // Assert
            Assert.DoesNotMatch(new Regex(@"\r?\n$"), result);
            Assert.Equal("EndNoNewline", result);
        }
    }
}