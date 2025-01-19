using SQLite;
using System.Collections.ObjectModel;

namespace app;

public partial class OrderPage : ContentPage
{
    private SQLiteConnection _database;
    private int _tableNumber;
    private ObservableCollection<OrderItem> _orderItems;
    private decimal _totalAmount = 0;

    public class OrderItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public int TableNumber { get; set; }
        public DateTime OrderTime { get; set; } = DateTime.Now;
        public bool IsCompleted { get; set; }
        public string OrderGroupId { get; set; }
    }

    public OrderPage(int tableNumber)
    {
        InitializeComponent();
        _tableNumber = tableNumber;
        TableNumberLabel.Text = $"Table {_tableNumber}";

        InitializeDatabase();
        SeedMenuItems();
        LoadMenuItems();

        _orderItems = new ObservableCollection<OrderItem>();
        OrderItemsListView.ItemsSource = _orderItems;

        LoadCurrentOrderItems();
        UpdateTotal();
    }

    private void InitializeDatabase()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db3");
        _database = new SQLiteConnection(dbPath);
        _database.CreateTable<OrderItem>();
        _database.CreateTable<HomePageWithCRUD.Item>();
        _database.CreateTable<HomePageReadOnly.Table>();
    }

    private void LoadCurrentOrderItems()
    {
        try
        {
            var currentOrders = _database.Table<OrderItem>()
                .Where(o => o.TableNumber == _tableNumber && !o.IsCompleted)
                .ToList();

            _orderItems.Clear();
            foreach (var order in currentOrders)
            {
                _orderItems.Add(order);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading current order items: {ex.Message}");
        }
    }

    private void LoadMenuItems()
    {
        var items = _database.Table<HomePageWithCRUD.Item>().ToList();
        MenuItemPicker.ItemsSource = items;
        MenuItemPicker.ItemDisplayBinding = new Binding("Name");
    }

    private void SeedMenuItems()
    {
        if (_database.Table<HomePageWithCRUD.Item>().Count() == 0)
        {
            var menuItems = new List<HomePageWithCRUD.Item>
            {
                new HomePageWithCRUD.Item
                {
                    Name = "Margherita Pizza",
                    Description = "Classic tomato sauce, mozzarella, and basil",
                    Price = 12.99M,
                    Category = "Pizza",
                    ImageUrl = "pizza.png"
                },
                // Dodajte ostale stavke menija po potrebi
            };

            foreach (var item in menuItems)
            {
                _database.Insert(item);
            }
        }
    }

    private void UpdateTotal()
    {
        _totalAmount = _orderItems.Sum(item => item.Price * item.Quantity);
        TotalLabel.Text = $"Ukupno ${_totalAmount:F2}";
    }

    private void OnAddToOrderClicked(object sender, EventArgs e)
    {
        if (MenuItemPicker.SelectedItem is HomePageWithCRUD.Item selectedItem &&
            !string.IsNullOrEmpty(QuantityEntry.Text) &&
            int.TryParse(QuantityEntry.Text, out int quantity))
        {
            var existingItem = _orderItems.FirstOrDefault(i => i.ItemName == selectedItem.Name);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                _database.Update(existingItem);
                var index = _orderItems.IndexOf(existingItem);
                _orderItems[index] = existingItem;
            }
            else
            {
                var orderItem = new OrderItem
                {
                    ItemId = selectedItem.Id,
                    ItemName = selectedItem.Name,
                    Quantity = quantity,
                    Price = selectedItem.Price,
                    TableNumber = _tableNumber,
                    OrderTime = DateTime.Now,
                    IsCompleted = false
                };
                _database.Insert(orderItem);
                _orderItems.Add(orderItem);
            }

            UpdateTotal();

            MenuItemPicker.SelectedItem = null;
            QuantityEntry.Text = string.Empty;
        }
        else
        {
            DisplayAlert("Error", "Molimo izaberite validnu stavku i unesite koli?inu broj?ano!", "OK");
        }
    }

    private void OnRemoveItemClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is OrderItem item)
        {
            _database.Delete<OrderItem>(item.Id);
            _orderItems.Remove(item);
            UpdateTotal();
        }
    }

    private async void OnCompleteOrderClicked(object sender, EventArgs e)
    {
        if (!_orderItems.Any())
        {
            await DisplayAlert("Error", "Molimo prvo dodajte stavke u narudûbu", "OK");
            return;
        }

        bool confirm = await DisplayAlert("Potvrdi narudûbu",
            $"Ukupna cijena: ${_totalAmount:F2}\néelite li zavröiti narudûbu",
            "Da", "Ne");

        if (confirm)
        {
            try
            {
                string orderGroupId = Guid.NewGuid().ToString();

                foreach (var item in _orderItems)
                {
                    item.IsCompleted = true;
                    item.OrderGroupId = orderGroupId;
                    item.OrderTime = DateTime.Now;
                    _database.Update(item);
                }

                var tableNumber = $"Stol {_tableNumber}";
                var table = _database.Table<HomePageReadOnly.Table>()
                    .FirstOrDefault(t => t.TableNumber == tableNumber);

                if (table != null)
                {
                    table.IsAvailable = false;
                    _database.Update(table);
                }
                else
                {
                    var newTable = new HomePageReadOnly.Table
                    {
                        TableNumber = tableNumber,
                        IsAvailable = false
                    };
                    _database.Insert(newTable);
                }

                await DisplayAlert("Uspjeh", "Narudûba uspjeöno kreirana", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Greöka pri kreiranju narudûbe: {ex.Message}", "OK");
            }
        }
    }
}