using System.Windows.Input;
using Kommunist.Application.Models;
using Kommunist.Application.Views;
using XCalendar.Core.Collections;
using XCalendar.Maui.Views;
using PropertyChanged;

namespace Kommunist.Application.ViewModels
{
    public class ExamplesViewModel : BaseViewModel
    {
        #region Properties
        public ObservableRangeCollection<Example> Examples { get; } = new ObservableRangeCollection<Example>()
        {
            new Example()
            {
                Page = new EventCalendarPage(),
                Title = "Event Calendar",
                Description = "Uses indicators to show events for a certain day.",
                Tags = new List<Tag>()
                {
                    new Tag() { Title = "Event" },
                    new Tag() { Title = "Events" },
                    new Tag() { Title = "Appointmnts" },
                    new Tag() { Title = "Special" },
                    new Tag() { Title = "Indicator" }
                }
            }
        };
        public ObservableRangeCollection<Example> DisplayedExamples { get; } = new ObservableRangeCollection<Example>();
        [OnChangedMethod(nameof(OnSearchTextChanged))]
        public string SearchText { get; set; }
        #endregion

        #region Commands
        public ICommand SearchExamplesCommand { get; set; }
        public ICommand ShowPageCommand { get; set; }
        #endregion

        #region Constructors
        public ExamplesViewModel()
        {
            SearchExamplesCommand = new Command(SearchExamples);
            ShowPageCommand = new Command<Page>(async (Page page) => await ShowPage(page));
            SearchExamples();
        }
        #endregion

        #region Methods
        private void OnSearchTextChanged()
        {
            SearchExamples();
        }
        public void SearchExamples()
        {
            bool searchTags = true;

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                DisplayedExamples.ReplaceRange(Examples);
            }
            else
            {
                DisplayedExamples.ReplaceRange(
                    Examples.Where(x =>
                    x.Title.ToLower().Contains(SearchText.ToLower()) ||
                    (searchTags && x.Tags.Any(tag =>
                        tag.Title.ToLower().Contains(SearchText.ToLower())))));
            }
        }
        public async Task ShowPage(Page page)
        {
            await Shell.Current.Navigation.PushAsync(page);
        }
        #endregion
    }
}
