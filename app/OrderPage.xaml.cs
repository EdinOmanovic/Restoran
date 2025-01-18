using SQLite;
using System.Collections.ObjectModel;

namespace app;

public partial class OrderPage : ContentPage
{
    private SQLiteConnection _database;
    private int _tableNumber;
    private ObservableCollection<OrderItem> _orderItems;
    private decimal _totalAmount = 0;

    public OrderPage(int tableNumber)
    {
        InitializeComponent();
        _tableNumber = tableNumber;
        TableNumberLabel.Text = $"Table {_tableNumber}";

        InitializeDatabase();
        LoadMenuItems();

        _orderItems = new ObservableCollection<OrderItem>();
        OrderItemsListView.ItemsSource = _orderItems;
        UpdateTotal();
    }

    private void InitializeDatabase()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db3");
        _database = new SQLiteConnection(dbPath);
        _database.CreateTable<OrderItem>();
        _database.CreateTable<HomePageWithCRUD.Item>(); // For menu items
    }


    private void LoadMenuItems()
    {
        var menuItems = _database.Table<HomePageWithCRUD.Item>().ToList();
        MenuItemPicker.ItemsSource = menuItems;
        MenuItemPicker.ItemDisplayBinding = new Binding("Name");
    }

    private void UpdateTotal()
    {
        _totalAmount = _orderItems.Sum(item => item.Price * item.Quantity);
        TotalLabel.Text = $"Total: ${_totalAmount:F2}";
    }

    private async void OnAddToOrderClicked(object sender, EventArgs e)
    {
        var selectedItem = MenuItemPicker.SelectedItem as HomePageWithCRUD.Item;
        if (selectedItem == null)
        {
            await DisplayAlert("Error", "Please select a menu item", "OK");
            return;
        }

        if (!int.TryParse(QuantityEntry.Text, out int quantity) || quantity <= 0)
        {
            await DisplayAlert("Error", "Please enter a valid quantity", "OK");
            return;
        }

        var orderItem = new OrderItem
        {
            ItemId = selectedItem.Id,
            ItemName = selectedItem.Name,
            Quantity = quantity,
            Price = selectedItem.Price
        };

        _orderItems.Add(orderItem);
        UpdateTotal();

        // Clear inputs
        MenuItemPicker.SelectedItem = null;
        QuantityEntry.Text = string.Empty;
    }

    private void OnRemoveItemClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var itemToRemove = button?.CommandParameter as OrderItem;
        if (itemToRemove != null)
        {
            _orderItems.Remove(itemToRemove);
            UpdateTotal();
        }
    }

    private async void OnCompleteOrderClicked(object sender, EventArgs e)
    {
        if (!_orderItems.Any())
        {
            await DisplayAlert("Error", "Please add items to the order first", "OK");
            return;
        }

        bool confirm = await DisplayAlert("Confirm Order",
            $"Total amount: ${_totalAmount:F2}\nDo you want to complete this order?",
            "Yes", "No");

        if (confirm)
        {
            try
            {
                // Begin transaction to ensure all operations complete together
                _database.BeginTransaction();

                // Create main order record
                var order = new TableOrder
                {
                    TableNumber = _tableNumber,
                    OrderDate = DateTime.Now,
                    TotalAmount = _totalAmount,
                    Status = "Active"
                };

                // Save the main order
                _database.Insert(order);

                // Save order items with reference to main order
                foreach (var item in _orderItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ItemId = item.ItemId,
                        ItemName = item.ItemName,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        TableNumber = _tableNumber,
                        OrderTime = DateTime.Now
                    };
                    _database.Insert(orderItem);
                }

                // Update table status to occupied - only update existing table
                var table = _database.Table<HomePageReadOnly.Table>()
                    .FirstOrDefault(t => t.TableNumber == _tableNumber);

                if (table != null)
                {
                    table.IsAvailable = false;
                    _database.Update(table);

                    // Commit the transaction after all inserts and updates
                    _database.Commit();
                    await DisplayAlert("Success", "Order completed successfully! Table is now occupied.", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    _database.Rollback();
                    await DisplayAlert("Error", "Table not found in the database.", "OK");
                }
            }
            catch (Exception ex)
            {
                // Rollback in case of error
                _database.Rollback();
                await DisplayAlert("Error", $"Failed to complete order: {ex.Message}", "OK");
            }
        }
    }

    public class TableOrder
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
    }

    public class OrderItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public int TableNumber { get; set; }
        public DateTime OrderTime { get; set; }
    }

    public class HomePageReadOnly
    {
        public class Table
        {
            [PrimaryKey]
            public int TableNumber { get; set; }
            public bool IsAvailable { get; set; }
        }
    }
}
