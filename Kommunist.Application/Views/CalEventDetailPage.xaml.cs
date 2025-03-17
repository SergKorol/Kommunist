using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using Kommunist.Application.ViewModels;
using Microsoft.Maui.Controls;

namespace Kommunist.Application.Views;

public partial class CalEventDetailPage : ContentPage
{
    public string BgImg { get; set; }
    
    public CalEventDetailPage(EventCalendarDetailViewModel eventDetailViewModel)
    {
        BgImg = eventDetailViewModel.SelectedEventDetail.BgImageUrl;
        InitializeComponent();
        BindingContext = eventDetailViewModel;
    }

    private async void WebView_OnLoaded(object sender, EventArgs eventArgs)
    {
        await Task.Delay(500); // ✅ Ensures the content is fully rendered before executing JavaScript

        try
        {
            string heightStr = await DescriptionWebView.EvaluateJavaScriptAsync("document.documentElement.getBoundingClientRect().height");
            Debug.WriteLine($"WebView Height (JS): {heightStr}"); // ✅ Debug output

            if (double.TryParse(heightStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double height) && height > 0)
            {
                DescriptionWebView.HeightRequest = height;
                DescriptionWebView.InvalidateMeasure();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error evaluating JavaScript: {ex.Message}");
        }
    }

}

