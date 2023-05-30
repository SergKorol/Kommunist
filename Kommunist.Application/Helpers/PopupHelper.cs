﻿using CommunityToolkit.Maui.Views;
using XCalendar.Core.Enums;
using XCalendar.Core.Extensions;
using XCalendarMauiSample.Popups;

namespace Kommunist.Application.Helpers
{
    public static class PopupHelper
    {
        #region Properties
        public static List<SelectionType> AllSelectionTypes { get; set; } = Enum.GetValues(typeof(SelectionType)).Cast<SelectionType>().ToList();
        public static List<SelectionAction> AllSelectionActions { get; set; } = Enum.GetValues(typeof(SelectionAction)).Cast<SelectionAction>().ToList();
        public static List<PageStartMode> AllPageStartModes { get; set; } = Enum.GetValues(typeof(PageStartMode)).Cast<PageStartMode>().ToList();
        public static List<NavigationLoopMode> AllNavigationLoopModes { get; set; } = Enum.GetValues(typeof(NavigationLoopMode)).Cast<NavigationLoopMode>().ToList();
        public static List<DayOfWeek> AllDaysOfWeek { get; set; } = DayOfWeek.Monday.GetWeekAsFirst();
        #endregion

        #region Methods
        public static async Task<T> ShowSelectItemDialogAsync<T>(T initialValue, IEnumerable<T> itemsSource)
        {
            return (T)await Shell.Current.ShowPopupAsync(new SelectItemDialogPopup(initialValue, itemsSource));
        }
        public static async Task<IEnumerable<T>> ShowConstructListDialogPopupAsync<T>(IEnumerable<T> initialValue, IEnumerable<T> daysOfWeek)
        {
            return ((List<object>)await Shell.Current.ShowPopupAsync(new ConstructListDialogPopup(initialValue, daysOfWeek))).Cast<T>();
        }
        public static async Task<Color> ShowColorDialogAsync(Color color)
        {
            return (Color)await Shell.Current.ShowPopupAsync(new ColorDialogPopup(color));
        }
        #endregion
    }
}
