using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System;

//namespace FastFoodApp2.Pages;


namespace FoodApp.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StartPage : ContentPage
    {
        public StartPage()
        {
            InitializeComponent();
        }
        private async void OnSwipe(object sender, SwipedEventArgs e)
        {
            Console.WriteLine("Swipe detected");
            await Navigation.PushAsync(new login());
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("Skip button clicked");
            await Navigation.PushAsync(new HomePage());
        }
    }
}