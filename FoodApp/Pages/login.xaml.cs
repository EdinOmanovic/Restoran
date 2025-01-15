namespace FoodApp.Pages;

public partial class login : ContentPage
{
	public login()
	{
		InitializeComponent();
	}

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
		await Navigation.PushAsync(new Registracija());
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        Application.Current.MainPage = new HomePage();
    }
}