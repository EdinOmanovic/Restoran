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

            // Add initial data if the database is empty
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
            var tables = _database.Table<Table>().ToList();
            Tables = new ObservableCollection<Table>(tables);
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