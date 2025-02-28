using System.Diagnostics;
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
}

