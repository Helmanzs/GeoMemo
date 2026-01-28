using GeoMemo.Services;
using GeoMemo.ViewModels;

namespace GeoMemo
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            bool granted = await PermissionHelper.RequestLocationPermissionAsync();

            if (granted)
            {
                await ((MainPageViewModel)BindingContext).LoadAsync();
                await ((MainPageViewModel)BindingContext).StartLocationAsync();
            }
        }
    }
}
