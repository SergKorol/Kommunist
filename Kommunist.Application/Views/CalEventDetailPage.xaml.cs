using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Maui.Alerts;
using Kommunist.Application.ViewModels;

namespace Kommunist.Application.Views;

public partial class CalEventDetailPage
{
    public static readonly BindableProperty BgImgProperty =
        BindableProperty.Create(
            nameof(BgImg),
            typeof(string),
            typeof(CalEventDetailPage));

    public string? BgImg
    {
        get => (string?)GetValue(BgImgProperty);
        set => SetValue(BgImgProperty, value);
    }

    public CalEventDetailPage(EventCalendarDetailViewModel eventDetailViewModel)
    {
        InitializeComponent();
        BindingContext = eventDetailViewModel;
    }

    private async void DescriptionWebView_Navigating(object sender, WebNavigatingEventArgs e)
    {
        try
        {
            if (BindingContext is not EventCalendarDetailViewModel vm)
                return;

            try
            {
                await Task.Delay(100);
                BgImg = vm.SelectedEventDetail?.BgImageUrl ?? string.Empty;
                var heightStr = await DescriptionWebView.EvaluateJavaScriptAsync("document.documentElement.scrollHeight");
                Debug.WriteLine($"WebView Height (JS): {heightStr}");

                if (!double.TryParse(heightStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var height) ||
                    !(height > 0)) return;
                DescriptionWebView.HeightRequest = height;
                DescriptionWebView.InvalidateMeasure();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error evaluating JavaScript: {ex.Message}");
            }
            finally
            {
                vm.IsWebViewLoading = false;
            }
        }
        catch (Exception ex)
        {
            await Toast.Make($"Error evaluating JavaScript: {ex.Message}").Show();
        }
    }
}