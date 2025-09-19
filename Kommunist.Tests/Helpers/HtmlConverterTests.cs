using Kommunist.Core.Helpers;

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
        public void HtmlToPlainText_NullHtml_ReturnEmptyString()
        {
            // Arrange
            string? html = null;

            // Act
            var result = HtmlConverter.HtmlToPlainText(html!);
            // Assert
            Assert.Equal(string.Empty, result);       
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
        public void HtmlToPlainText_H5AndH6_CenteredHeadingThenParagraph()
        {
            // Arrange
            var title = "Centered Title";
            var para = "Sub &amp; Title";
            var html = $"<h5>{title}</h5><h6>{para}</h6>";

            // Act
            var result = HtmlConverter.HtmlToPlainText(html);

            // Assert
            var h5 = Math.Max(0, (100 - title.Length) / 2 - 4);
            var h6 = Math.Max(0, (100 - para.Length) / 2 - 4);
            var expectedH5 = new string(' ', h5) + title;
            var expectedH6 = new string(' ', h6) + para;

            var expected = string.Join(Environment.NewLine, new[]
            {
                expectedH5,
                "",
                "  " + expectedH6.Replace("&amp;", "&")
            });

            Assert.Equal(expected, result);
        }

        [Fact]
        public void HtmlToPlainText_ListItemsAndParagraphs_MatchingCounts_RendersBulletsWithIndentedLines()
        {
            // Arrange
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
            var h5 = Math.Max(0, (100 - "Title".Length) / 2 - 4);
            var h6 = Math.Max(0, (100 - "Intro & More".Length) / 2 - 4);
            var expected = string.Join(Environment.NewLine, new[]
            {
                new string(' ', h5) + "Title",
                "",
                new string(' ', h6) + "Intro & More",
                "",
                "",
                "•First",
                "  Alpha",
                "•Second",
                "  Line1",
                "  Line2"
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
                "\n•One & Only",
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
            var h5 = Math.Max(0, (100 - "Title".Length) / 2 - 4);
            var h6 = Math.Max(0, (100 - "Lead".Length) / 2 - 4);
            var expected = string.Join(Environment.NewLine, new[]
            {
                new string(' ', h5) + "Title",
                "",
                new string(' ', h6) + "Lead"
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
            var pad = Math.Max(0, (100 - "Only lead".Length) / 2 - 4);
            var expected = new string(' ', pad) + "Only lead";
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("h1")]
        [InlineData("h2")]
        [InlineData("h3")]
        [InlineData("h4")]
        [InlineData("h5")]
        [InlineData("h6")]
        public void HtmlToPlainText_HeadingWithEntities_AreHandledViaInnerText(string tag)
        {
            // Arrange
            // HtmlAgilityPack InnerText decodes entities for h5/h6
            var html = $"<{tag}>Tom &amp; Jerry</{tag}>";

            // Act
            var result = HtmlConverter.HtmlToPlainText(html);

            // Assert
            var pad = Math.Max(0, (100 - "Tom & Jerry".Length) / 2 - 4);
            var expected = new string(' ', pad) + "Tom & Jerry";
            Assert.Equal(expected, result);
        }
        
        [Fact]
        public void HtmlToPlainText_TextWith_Paragraph_ReturnsParagraph()
        {
            var html = "<p>Hello ICS</p>";
            var result = HtmlConverter.HtmlToPlainText(html);
            Assert.Equal("Hello ICS", result);
        }
        
        [Fact]
        public void HtmlToPlainText_PlainText_ReturnsText()
        {
            var html = "Hello ICS";
            var result = HtmlConverter.HtmlToPlainText(html);
            Assert.Equal("Hello ICS", result);
        }
    }
}