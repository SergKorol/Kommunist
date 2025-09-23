using System.Collections.ObjectModel;
using FluentAssertions;
using Kommunist.Application.Helpers;

namespace Kommunist.Tests.Helpers;

public static class CountryExtensionsTests
{
    public class WithoutFlags
    {
        [Fact]
        public void Returns_names_without_prefix_when_item_contains_space()
        {
            // Arrange
            var input = new List<string> { "En English", "Ua Ukrainian", "Ru Russian" };

            // Act
            var result = input.WithoutFlags();

            // Assert
            result.Should().BeEquivalentTo(["English", "Ukrainian", "Russian"], options => options.WithStrictOrdering());
        }

        [Fact]
        public void Returns_original_when_item_has_no_space()
        {
            // Arrange
            var input = new List<string> { "English", "Ukrainian" };

            // Act
            var result = input.WithoutFlags();

            // Assert
            result.Should().BeEquivalentTo(input, options => options.WithStrictOrdering());
        }

        [Fact]
        public void Preserves_text_after_first_space_for_multi_word_names()
        {
            // Arrange
            var input = new List<string> { "En English Language", "Ua Ukrainian Language" };

            // Act
            var result = input.WithoutFlags();

            // Assert
            result.Should().Equal("English Language", "Ukrainian Language");
        }

        [Fact]
        public void Handles_empty_string_and_leading_space()
        {
            // Arrange
            var input = new List<string> { "", " English", "En English" };

            // Act
            var result = input.WithoutFlags();

            // Assert
            result.Should().Equal("", "English", "English");
        }

        [Fact]
        public void Does_not_mutate_original_list()
        {
            // Arrange
            var input = new List<string> { "En English", "Ukrainian" };
            var snapshot = input.ToList();

            // Act
            var result = input.WithoutFlags();

            // Assert
            input.Should().Equal(snapshot); 
            result.Should().NotBeSameAs(input);
        }
    }

    public class FindWithFlag
    {
        [Fact]
        public void Returns_original_entry_with_prefix_when_name_matches()
        {
            // Arrange
            var countries = new ObservableCollection<string> { "En English", "Ua Ukrainian" };

            // Act
            var result = countries.FindWithFlag("Ukrainian");

            // Assert
            result.Should().Be("Ua Ukrainian");
        }

        [Fact]
        public void Returns_entry_when_no_prefix_exists()
        {
            // Arrange
            var countries = new ObservableCollection<string> { "English", "Ukrainian" };

            // Act
            var result = countries.FindWithFlag("english");

            // Assert
            result.Should().Be("English");
        }

        [Fact]
        public void Returns_null_when_no_match_found()
        {
            // Arrange
            var countries = new ObservableCollection<string> { "En English", "Ua Ukrainian" };

            // Act
            var result = countries.FindWithFlag("German");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Comparison_is_case_insensitive()
        {
            // Arrange
            var countries = new ObservableCollection<string> { "En English" };

            // Act
            var result = countries.FindWithFlag("eNGLisH");

            // Assert
            result.Should().Be("En English");
        }

        [Fact]
        public void Supports_multi_word_names()
        {
            // Arrange
            var countries = new ObservableCollection<string> { "En English Language", "Ua Ukrainian Language" };

            // Act
            var result = countries.FindWithFlag("Ukrainian Language");

            // Assert
            result.Should().Be("Ua Ukrainian Language");
        }

        [Fact]
        public void Handles_leading_space_prefix_as_separator()
        {
            // Arrange
            var countries = new ObservableCollection<string> { " English", "Ua Ukrainian" };

            // Act
            var result = countries.FindWithFlag("English");

            // Assert
            result.Should().Be(" English");
        }

        [Fact]
        public void Returns_first_match_when_multiple_entries_match()
        {
            // Arrange
            var countries = new ObservableCollection<string> { "En English", "Ua English" };

            // Act
            var result = countries.FindWithFlag("English");

            // Assert
            result.Should().Be("En English");
        }
    }

    public class ReplaceCodesWithFlags
    {
        [Fact]
        public void Replaces_known_codes_with_names()
        {
            // Arrange
            var codes = new List<string> { "En", "Esp", "Slk" };

            // Act
            var result = codes.ReplaceCodesWithFlags();

            // Assert
            result.Should().Equal("English", "Spanish", "Slovak");
        }

        [Fact]
        public void Leaves_unknown_codes_unchanged()
        {
            // Arrange
            var codes = new List<string> { "De", "Fr" };

            // Act
            var result = codes.ReplaceCodesWithFlags();

            // Assert
            result.Should().Equal("De", "Fr");
        }

        [Fact]
        public void Keys_are_case_sensitive_unknown_lowercase_remains()
        {
            // Arrange
            var codes = new List<string> { "en", "esp", "ua" };

            // Act
            var result = codes.ReplaceCodesWithFlags();

            // Assert
            result.Should().Equal("en", "esp", "ua");
        }

        [Fact]
        public void Returns_empty_list_when_input_is_empty()
        {
            // Arrange
            var codes = new List<string>();

            // Act
            var result = codes.ReplaceCodesWithFlags();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void Does_not_mutate_original_list()
        {
            // Arrange
            var codes = new List<string> { "En", "De" };
            var snapshot = codes.ToList();

            // Act
            var result = codes.ReplaceCodesWithFlags();

            // Assert
            codes.Should().Equal(snapshot);
            result.Should().NotBeSameAs(codes);
        }
    }
}
