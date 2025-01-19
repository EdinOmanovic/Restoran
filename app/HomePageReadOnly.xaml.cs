using SQLite;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace app
{
    public partial class HomePageReadOnly : ContentPage
    {
        private SQLiteConnection _database;
        public ObservableCollection<Table> Tables { get; set; }
        public ICommand OrderCommand { get; private set; }
        public ICommand ClearTableCommand { get; private set; }

        public HomePageReadOnly()
        {
            InitializeComponent();
            InitializeDatabase();
            UpdateTableNames();
            LoadTables();

            // Initialize commands
            OrderCommand = new Command<Table>(async (table) => await OnOrderClicked(table));
            ClearTableCommand = new Command<Table>(async (table) => await OnClearTableClicked(table));
            BindingContext = this;
        }

        private void InitializeDatabase()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db3");
            _database = new SQLiteConnection(dbPath);
            _database.CreateTable<Table>();

            // Add sample tables if none exist
            if (!_database.Table<Table>().Any())
            {
                for (int i = 1; i <= 6; i++)
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
            var tablesList = _database.Table<Table>().ToList();
            Tables = new ObservableCollection<Table>(tablesList);
        }

        private async Task OnOrderClicked(Table table)
        {
            if (table.IsAvailable)
            {
                // Extract table number from string (e.g., "Table 1" -> 1)
                if (int.TryParse(table.TableNumber.Split(' ')[1], out int tableNumber))
                {
                    // Navigate to OrderPage
                    await Navigation.PushAsync(new OrderPage(tableNumber));
                }
            }
            else
            {
                await DisplayAlert("Info", "Ovaj stol je veæ zauzet", "OK");
            }
        }

        private async Task OnClearTableClicked(Table table)
        {
            if (!table.IsAvailable)
            {
                bool confirm = await DisplayAlert("Oèisti stol",
                    $"Da li sigurno želite oèistiti narudžbu : {table.TableNumber}?",
                    "Da", "Ne");

                if (confirm)
                {
                    // Update table status in database
                    table.IsAvailable = true;
                    _database.Update(table);

                    // Refresh the tables list
                    LoadTables();

                    await DisplayAlert("Uspjeh", $"{table.TableNumber} je oèisæena", "OK");
                }
            }
            else
            {
                await DisplayAlert("Info", "Ovaj stol je veæ slobodan", "OK");
            }
        }
        private void UpdateTableNames()
        {
            try
            {
                // Get all tables with "Table" prefix
                var tablesToUpdate = _database.Table<Table>()
                    .Where(t => t.TableNumber.StartsWith("Table"))
                    .ToList();

                foreach (var table in tablesToUpdate)
                {
                    // Replace "Table" with "Stol" in the table number
                    string newTableNumber = table.TableNumber.Replace("Table", "Stol");

                    // Update the table number
                    _database.Execute(
                        "UPDATE Table SET TableNumber = ? WHERE Id = ?",
                        newTableNumber, table.Id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating table names: {ex.Message}");
            }
        }
        public class Table
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public string TableNumber { get; set; }
            public bool IsAvailable { get; set; }
            public string Status => IsAvailable ? "Slobodno" : "Zauzeto";
            public string StatusColor => IsAvailable ? "Green" : "Red";
            public string ImageUrl { get; set; } = "table.png";
        }
    }
}