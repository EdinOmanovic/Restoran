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
        public ICommand OrderCommand { get; }

        public HomePageReadOnly()
        {
            InitializeComponent();

            InitializeDatabase();
            LoadTables();

            OrderCommand = new Command<Table>(OnOrder);
            BindingContext = this;
        }

        private void InitializeDatabase()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db3");
            _database = new SQLiteConnection(dbPath);
            _database.CreateTable<Table>();

            // Dodavanje po?etnih podataka ako baza nema stolove
            if (!_database.Table<Table>().Any())
            {
                for (int i = 1; i <= 20; i++)
                {
                    _database.Insert(new Table
                    {
                        TableNumber = $"Stol {i}",
                        IsAvailable = true // Svi stolovi su slobodni na po?etku
                    });
                }
            }
        }

        private void LoadTables()
        {
            var tables = _database.Table<Table>().ToList();
            Tables = new ObservableCollection<Table>(tables);
        }

        private async void OnOrder(Table table)
        {
            if (table.IsAvailable)
            {
                bool confirmed = await DisplayAlert("Narudžba", $"Želite li naru?iti za stol {table.TableNumber}?", "Da", "Ne");
                if (confirmed)
                {
                    table.IsAvailable = false;
                    _database.Update(table);
                    LoadTables(); // Osvježi prikaz
                }
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
            public string ImageUrl { get; set; } = "table.png"; // Placeholder za slike stolova
        }
    }
}
