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
                        TableNumber = $"Table {i}",
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
                await DisplayAlert("Info", "This table is already occupied", "OK");
            }
        }

        private async Task OnClearTableClicked(Table table)
        {
            if (!table.IsAvailable)
            {
                bool confirm = await DisplayAlert("Clear Table",
                    $"Are you sure you want to clear {table.TableNumber}?",
                    "Yes", "No");

                if (confirm)
                {
                    // Update table status in database
                    table.IsAvailable = true;
                    _database.Update(table);

                    // Refresh the tables list
                    LoadTables();

                    await DisplayAlert("Success", $"{table.TableNumber} has been cleared", "OK");
                }
            }
            else
            {
                await DisplayAlert("Info", "This table is already available", "OK");
            }
        }

        public class Table
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public string TableNumber { get; set; }
            public bool IsAvailable { get; set; }
            public string Status => IsAvailable ? "Available" : "Occupied";
            public string StatusColor => IsAvailable ? "Green" : "Red";
            public string ImageUrl { get; set; } = "table.png";
        }
    }
}