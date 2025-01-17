using SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace app;

public partial class LoginPage : ContentPage
{
    private SQLiteConnection _database;

    public LoginPage()
    {
        InitializeComponent();
        InitializeDatabase();
        // CreateTestUser();  
    }

    private void InitializeDatabase()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db3");
        _database = new SQLiteConnection(dbPath);
        _database.CreateTable<User>();  // ako missa tabelu, kreirat ce ju

        //ADMIN
        var testUser = _database.Table<User>().FirstOrDefault(u => u.Username == "testuser");
        if (testUser == null)
        {
            _database.Insert(new User
            {
                Username = "testuser",
                Password = "password123"  // admin podaci
            });
        }
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text?.Trim();
        string password = PasswordEntry.Text?.Trim();

        // provjera user -pass
        var user = _database.Table<User>().FirstOrDefault(u => u.Username == username && u.Password == password);

        if (user != null)
        {
            // provjera da li je admin 
            if (user.Username == "testuser")
            {
                await DisplayAlert("Uspešno", "Prijava uspešna! Dobrodošli Admine.", "OK");
                await Navigation.PushAsync(new HomePageWithCRUD());  
            }
            else
            {
                //await DisplayAlert("Uspešno", "Prijava uspešna! Dobrodošli.", "OK");
                await Navigation.PushAsync(new HomePageReadOnly());  
            }
        }
        else
        {
            // Neuspešna prijava
            await DisplayAlert("Greška", "Pogrešno korisnièko ime ili lozinka.", "OK");
        }
    }

    
    private async void OnRegisterTapped(object sender, TappedEventArgs e)
    {
        
        await Navigation.PushAsync(new RegisterPage());  
    }

    // Definicija usera
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public string Role { get; set; }
    }
}