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
            await ((MainPageViewModel)BindingContext).LoadAsync();
        }
    }
}
