using Microsoft.Maui.Controls;
using SQLite;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace app
{
    public partial class RegisterPage : ContentPage
    {
        private SQLiteConnection _database;

        public RegisterPage()
        {
            InitializeComponent();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db3");
            _database = new SQLiteConnection(dbPath);
            _database.CreateTable<User>();  // kreiraj tabelu ako fali
        }

        private void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            string username = UsernameEntry.Text;
            string password = PasswordEntry.Text;
            string confirmPassword = ConfirmPasswordEntry.Text;

            // provjeri prazna polja
            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("Korisnièko ime je obavezno!");
                return;
            }

            if (password.Length < 6)
            {
                ShowError("Lozinka mora imati minimalno 6 karaktera!");
                return;
            }

            if (password != confirmPassword)
            {
                ShowError("Lozinke se ne poklapaju.");
                return;
            }

            // da li user postoji
            var existingUser = _database.Table<User>().FirstOrDefault(u => u.Username == username);
            if (existingUser != null)
            {
                ShowError("Korisnièko ime veæ postoji.");
                return;
            }

            
            var newUser = new User { Username = username, Password = password };
            _database.Insert(newUser);

            
            DisplayAlert("Uspješno", "Registracija je uspješna!", "OK");
            ClearFields();
        }

        
        private void OnShowUsersButtonClicked(object sender, EventArgs e)
        {
            List<User> users = _database.Table<User>().ToList();
            UsersListView.ItemsSource = users;

            if (!users.Any())
            {
                DisplayAlert("Informacija", "Nema korisnika u bazi.", "OK");
            }
        }
         

        private void ShowError(string message)
        {
            ErrorLabel.Text = message;
            ErrorLabel.IsVisible = true;
        }

        private void ClearFields()
        {
            UsernameEntry.Text = string.Empty;
            PasswordEntry.Text = string.Empty;
            ConfirmPasswordEntry.Text = string.Empty;
            ErrorLabel.IsVisible = false;
        }

        public class User
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Role { get; set; }  
        }
    }
}