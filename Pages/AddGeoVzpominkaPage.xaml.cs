using GeoMemo.Services;
using GeoMemo.ViewModels;

namespace GeoMemo;

public partial class AddGeoVzpominkaPage : ContentPage
{
    public AddGeoVzpominkaPage(AddGeoVzpominkaViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AddGeoVzpominkaViewModel vm)
            await vm.OnAppearingAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is AddGeoVzpominkaViewModel vm)
            vm.OnDisappearing();
    }

}
