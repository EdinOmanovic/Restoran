using Microsoft.Maui.Controls;
using System;
using System.Text.RegularExpressions;
//using Windows.System;


namespace FoodApp.Pages;

public partial class Registracija : ContentPage
{
    // Lista korisnika (privremeno u memoriji dok ne dodamo bazu)
    private static readonly List<User> Users = new List<User>();

         public Registracija()
	       {
	        	InitializeComponent();
	       }

    //greske u error labelu
    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    



    private async void Button_Clicked(object sender, EventArgs e)
    {

        string username = UsernameEntry.Text;
        string password = PasswordEntry.Text;
        string confirmPassword = ConfirmPasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(username))
        {
            ShowError("Korisnicko ime je obavezno!");
        }

        if (password.Length < 6)
        {
            ShowError("Lozinka mora imati minimalno 6 karaktera! ");
        }

        if (password != confirmPassword)
        {
            ShowError("Lozinke se ne poklapaju.");
            return;
        }


        // Ako je validacija prosla, dodaj korisnika u memoriju
        var newUser = new User
        {
            Username = username,
            Password = password
        };

        Users.Add(newUser);


        await DisplayAlert("Uspješno", "Registracija je uspješna!", "OK");

        // cisti polja nakon validacije
        UsernameEntry.Text = string.Empty;
        PasswordEntry.Text = string.Empty;
        ConfirmPasswordEntry.Text = string.Empty;

        await Navigation.PopAsync();

        //test
        //DisplayAlert("Registracija", "Registracija uspjesna!", "OK"); 
    }

    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}