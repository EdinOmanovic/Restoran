using SQLite;
using app.Helpers;
using app.Models;
using System.Collections.Generic;

namespace app
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent(); // Ispravljen naziv metode
            LoadUsers(); // Ispravljen naziv metode
        }

        private void LoadUsers()
        {
            // Učitaj sve korisnike iz baze
            List<User> users = DatabaseHelper.Database.Table<User>().ToList();

            // Provjera da li lista nije prazna
            if (users.Count > 0)
            {
                UsersListView.ItemsSource = users;
            }
            else
            {
                DisplayAlert("Informacija", "Nema korisnika u bazi.", "OK");
            }
        }
    }
}