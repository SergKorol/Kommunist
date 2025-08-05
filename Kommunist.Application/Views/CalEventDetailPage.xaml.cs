using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Maui.Alerts;
using Kommunist.Application.ViewModels;

namespace Kommunist.Application.Views;

public partial class CalEventDetailPage
{
    public string BgImg { get; set; }
    public bool HasParticipants { get; set; }
    
    public CalEventDetailPage(EventCalendarDetailViewModel eventDetailViewModel)
    {
        BgImg = eventDetailViewModel.SelectedEventDetail.BgImageUrl;
        InitializeComponent();
        HasParticipants = eventDetailViewModel.HasParticipants;
        BindingContext = eventDetailViewModel;
    }

    private async void WebView_OnLoaded(object sender, EventArgs eventArgs)
    {
        try
        {
            await Task.Delay(500);

            try
            {
                if (!DescriptionWebView.IsVisible)
                {
                    ArgumentNullException.ThrowIfNull("The Description shouldn't be NULL");
                }
            
                var heightStr = await DescriptionWebView.EvaluateJavaScriptAsync("document.documentElement.getBoundingClientRect().height");
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
        }
        catch (Exception e)
        {
            await Toast.Make($"Error loading WebView: {e}").Show();
        }
    }
}

