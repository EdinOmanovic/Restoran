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
        public ICommand ViewDetailsCommand { get; private set; }

        public HomePageReadOnly()
        {
            InitializeComponent();
            InitializeDatabase();
            UpdateTableNames();
            LoadTables();

            OrderCommand = new Command<Table>(async (table) => await OnOrderClicked(table));
            ViewDetailsCommand = new Command<Table>(async (table) => await OnViewDetailsClicked(table));
            ClearTableCommand = new Command<Table>(async (table) => await OnClearTableClicked(table));
            BindingContext = this;

            this.NavigatedTo += HomePageReadOnly_NavigatedTo;
        }

        private void HomePageReadOnly_NavigatedTo(object sender, NavigatedToEventArgs e)
        {
            LoadTables();
        }

        private void InitializeDatabase()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db3");
            _database = new SQLiteConnection(dbPath);
            _database.CreateTable<Table>();
            _database.CreateTable<OrderPage.OrderItem>(); // Dodajemo i OrderItem tabelu

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
            OnPropertyChanged(nameof(Tables));
        }

        private async Task OnViewDetailsClicked(Table table)
        {
            if (!table.IsAvailable)
            {
                try
                {
                    string numberPart = table.TableNumber.Replace("Stol ", "");
                    if (int.TryParse(numberPart, out int tableNumber))
                    {
                        await Navigation.PushAsync(new OrderDetailsPage(tableNumber));
                    }
                    else
                    {
                        await DisplayAlert("Greška", "Nije mogu?e o?itati broj stola", "U redu");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error viewing details: {ex.Message}");
                    await DisplayAlert("Greška", "Došlo je do greške pri otvaranju detalja", "U redu");
                }
            }
        }

        private async Task OnOrderClicked(Table table)
        {
            if (table.IsAvailable)
            {
                if (int.TryParse(table.TableNumber.Split(' ')[1], out int tableNumber))
                {
                    await Navigation.PushAsync(new OrderPage(tableNumber));
                }
            }
            else
            {
                await DisplayAlert("Info", "Ovaj stol je ve? zauzet", "OK");
            }
        }

        private async Task OnClearTableClicked(Table table)
        {
            if (!table.IsAvailable)
            {
                bool confirm = await DisplayAlert("O?isti stol",
                    $"Da li sigurno želite o?istiti stol: {table.TableNumber}? Sve narudžbe za ovaj stol ?e biti obrisane.",
                    "Da", "Ne");

                if (confirm)
                {
                    try
                    {
                        // Dohvati broj stola
                        int tableNumber = int.Parse(table.TableNumber.Replace("Stol ", ""));

                        // Obriši sve narudžbe za ovaj stol
                        var orderItems = _database.Table<OrderPage.OrderItem>()
                            .Where(o => o.TableNumber == tableNumber)
                            .ToList();

                        foreach (var orderItem in orderItems)
                        {
                            _database.Delete<OrderPage.OrderItem>(orderItem.Id);
                        }

                        // Ažuriraj status stola
                        table.IsAvailable = true;
                        _database.Update(table);
                        LoadTables();

                        await DisplayAlert("Uspjeh",
                            $"{table.TableNumber} je o?iš?en i sve narudžbe su obrisane",
                            "OK");
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Greška",
                            $"Došlo je do greške pri ?iš?enju stola: {ex.Message}",
                            "OK");
                    }
                }
            }
            else
            {
                await DisplayAlert("Info", "Ovaj stol je ve? slobodan", "OK");
            }
        }

        private void UpdateTableNames()
        {
            try
            {
                var tablesToUpdate = _database.Table<Table>()
                    .Where(t => t.TableNumber.StartsWith("Table"))
                    .ToList();

                foreach (var table in tablesToUpdate)
                {
                    string newTableNumber = table.TableNumber.Replace("Table", "Stol");
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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadTables();
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