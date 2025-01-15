namespace FoodApp.Pages;

public partial class HomePage : TabbedPage
{
	public HomePage()
	{
		InitializeComponent();
	}

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        string action = await DisplayActionSheet(
              "Opcije",         // Naslov pop-upa
              "Odustani",       // Dugme za otkazivanje
              null,             // Uništavanje opcije (može biti null)
              "Korpa",
              "O nama");

        // Obradi izabranu opciju
        switch (action)
        {
            case "Kategorije":
                await DisplayAlert("Odabrano", "Kategorije", "OK");
                break;
            case "Popularno":
                await DisplayAlert("Odabrano", "Popularno", "OK");
                break;
            case "Korpa":
                await DisplayAlert("Odabrano", "Korpa", "OK");
                break;
            case "O nama":
                await DisplayAlert("Odabrano", "O nama", "OK");
                break;
        }
    }
}