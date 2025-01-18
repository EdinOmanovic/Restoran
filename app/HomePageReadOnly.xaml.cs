using SQLite;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace app
{
    public partial class HomePageReadOnly : ContentPage
    {
        private SQLiteConnection _database;
        public ObservableCollection<Table> Tables { get; set; }
        public ICommand OrderCommand { get; private set; }

        public HomePageReadOnly()
        {
            InitializeComponent();
            InitializeDatabase();
            LoadTables();

            // Initialize the OrderCommand
            OrderCommand = new Command<Table>(async (table) => await OnOrderClicked(table));
            BindingContext = this;
        }

        private void InitializeDatabase()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db3");
            _database = new SQLiteConnection(dbPath);
            _database.CreateTable<Table>();

            // Check if tables exist and seed if necessary
            if (!_database.Table<Table>().Any())
            {
                for (int i = 1; i <= 20; i++)
                {
                    _database.Insert(new Table
                    {
                        TableNumber = $"Stol {i}",
                        IsAvailable = true
                    });
                }
            }
        }

        private void LoadTables()
        {
            try
            {
                var tables = _database.Table<Table>().ToList();
                if (tables != null && tables.Any())
                {
                    Tables = new ObservableCollection<Table>(tables);
                }
                else
                {
                    // Re-initialize tables if none found
                    InitializeDatabase();
                    Tables = new ObservableCollection<Table>(_database.Table<Table>().ToList());
                }
            }
            catch (SQLiteException ex)
            {
                // Handle database errors
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Database Error",
                        "Error loading tables. Please restart the application.",
                        "OK");
                });
            }
        }

        private async Task OnOrderClicked(Table table)
        {
            if (table.IsAvailable)
            {
                bool confirmed = await DisplayAlert("Narudžba",
                    $"Želite li naruèiti za {table.TableNumber}?", "Da", "Ne");

                if (confirmed)
                {
                    // Extract table number from the string (e.g., "Stol 1" -> 1)
                    string numberStr = table.TableNumber.Split(' ')[1];
                    if (int.TryParse(numberStr, out int tableNumber))
                    {
                        // Navigate to OrderPage
                        await Navigation.PushAsync(new OrderPage(tableNumber));
                    }
                }
            }
            else
            {
                await DisplayAlert("Info", "Ovaj stol je veæ zauzet.", "OK");
            }
        }

        public class Table
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public string TableNumber { get; set; }
            public bool IsAvailable { get; set; }
            public string Status => IsAvailable ? "Slobodan" : "Zauzet";
            public string StatusColor => IsAvailable ? "Green" : "Red";
            public string ImageUrl { get; set; } = "table.png";
        }
    }
}