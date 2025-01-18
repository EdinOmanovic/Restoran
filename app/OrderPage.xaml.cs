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
        SeedMenuItems();
        LoadMenuItems();

        _orderItems = new ObservableCollection<OrderItem>();
        OrderItemsListView.ItemsSource = _orderItems;

        LoadExistingOrders();
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

    private void LoadExistingOrders()
    {
        try
        {
            // Get all orders for this table that haven't been completed
            var existingOrders = _database.Table<OrderItem>()
                .Where(o => o.TableNumber == _tableNumber)
                .ToList();

            foreach (var order in existingOrders)
            {
                _orderItems.Add(order);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading existing orders: {ex.Message}");
        }
    }
    private void SeedMenuItems()
    {
        // Check if we already have menu items
        if (_database.Table<HomePageWithCRUD.Item>().Count() == 0)
        {
            var menuItems = new List<HomePageWithCRUD.Item>
            {
                // Main Dishes
                new HomePageWithCRUD.Item
                {
                    Name = "Margherita Pizza",
                    Description = "Classic tomato sauce, mozzarella, and basil",
                    Price = 12.99M,
                    Category = "Pizza",
                    ImageUrl = "pizza.png"
                },
                new HomePageWithCRUD.Item
                {
                    Name = "Pepperoni Pizza",
                    Description = "Tomato sauce, mozzarella, and pepperoni",
                    Price = 14.99M,
                    Category = "Pizza",
                    ImageUrl = "pizza.png"
                },
                new HomePageWithCRUD.Item
                {
                    Name = "Classic Burger",
                    Description = "Beef patty, lettuce, tomato, cheese",
                    Price = 10.99M,
                    Category = "Burgers",
                    ImageUrl = "burger.png"
                },
                new HomePageWithCRUD.Item
                {
                    Name = "Chicken Burger",
                    Description = "Grilled chicken, avocado, bacon",
                    Price = 11.99M,
                    Category = "Burgers",
                    ImageUrl = "burger.png"
                },

                // Pasta
                new HomePageWithCRUD.Item
                {
                    Name = "Spaghetti Carbonara",
                    Description = "Creamy sauce with pancetta and parmesan",
                    Price = 13.99M,
                    Category = "Pasta",
                    ImageUrl = "pasta.png"
                },
                new HomePageWithCRUD.Item
                {
                    Name = "Penne Arrabbiata",
                    Description = "Spicy tomato sauce with garlic",
                    Price = 12.99M,
                    Category = "Pasta",
                    ImageUrl = "pasta.png"
                },

                // Salads
                new HomePageWithCRUD.Item
                {
                    Name = "Caesar Salad",
                    Description = "Romaine lettuce, croutons, parmesan",
                    Price = 8.99M,
                    Category = "Salads",
                    ImageUrl = "salad.png"
                },
                new HomePageWithCRUD.Item
                {
                    Name = "Greek Salad",
                    Description = "Mixed greens, feta, olives, cucumber",
                    Price = 9.99M,
                    Category = "Salads",
                    ImageUrl = "salad.png"
                },

                // Drinks
                new HomePageWithCRUD.Item
                {
                    Name = "Coca Cola",
                    Description = "330ml",
                    Price = 2.99M,
                    Category = "Drinks",
                    ImageUrl = "drink.png"
                },
                new HomePageWithCRUD.Item
                {
                    Name = "Fresh Orange Juice",
                    Description = "300ml",
                    Price = 3.99M,
                    Category = "Drinks",
                    ImageUrl = "drink.png"
                },

                // Desserts
                new HomePageWithCRUD.Item
                {
                    Name = "Chocolate Cake",
                    Description = "Rich chocolate cake with cream",
                    Price = 6.99M,
                    Category = "Desserts",
                    ImageUrl = "dessert.png"
                },
                new HomePageWithCRUD.Item
                {
                    Name = "Tiramisu",
                    Description = "Classic Italian coffee-flavored dessert",
                    Price = 7.99M,
                    Category = "Desserts",
                    ImageUrl = "dessert.png"
                }
            };

            // Insert all menu items
            foreach (var item in menuItems)
            {
                _database.Insert(item);
            }
        }
    }

    private void LoadMenuItems()
    {
        var items = _database.Table<HomePageWithCRUD.Item>().ToList();
        MenuItemPicker.ItemsSource = items;
        MenuItemPicker.ItemDisplayBinding = new Binding("Name");
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
                // Update existing item quantity
                existingItem.Quantity += quantity;
                _database.Update(existingItem);
                // Force refresh of the collection
                var index = _orderItems.IndexOf(existingItem);
                _orderItems[index] = existingItem;
            }
            else
            {
                // Add new item
                var orderItem = new OrderItem
                {
                    ItemId = selectedItem.Id,
                    ItemName = selectedItem.Name,
                    Quantity = quantity,
                    Price = selectedItem.Price,
                    TableNumber = _tableNumber,
                    OrderTime = DateTime.Now,
                    IsCompleted = false // Add this flag to track order status
                };
                _database.Insert(orderItem);
                _orderItems.Add(orderItem);
            }

            UpdateTotal();

            // Clear inputs
            MenuItemPicker.SelectedItem = null;
            QuantityEntry.Text = string.Empty;
        }
        else
        {
            DisplayAlert("Error", "Please select an item and enter a valid quantity", "OK");
        }
    }

    private void OnRemoveItemClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is OrderItem item)
        {
            _database.Delete<OrderItem>(item.Id);
            // Remove from collection
            _orderItems.Remove(item);
            UpdateTotal();
        }
    }

    private void UpdateTotal()
    {
        _totalAmount = _orderItems.Sum(item => item.Price * item.Quantity);
        TotalLabel.Text = $"Total: ${_totalAmount:F2}";
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
                // Mark all orders as completed
                foreach (var item in _orderItems)
                {
                    item.IsCompleted = true;
                    _database.Update(item);
                }

                // Update table status - Find the table and set it as unavailable
                var tableNumber = $"Stol {_tableNumber}";
                var table = _database.Table<HomePageReadOnly.Table>()
                    .FirstOrDefault(t => t.TableNumber == tableNumber);

                if (table != null)
                {
                    table.IsAvailable = false; // Set table as unavailable
                    _database.Update(table);
                }
                else
                {
                    // If table doesn't exist, create it
                    var newTable = new HomePageReadOnly.Table
                    {
                        TableNumber = tableNumber,
                        IsAvailable = false // Set as unavailable
                    };
                    _database.Insert(newTable);
                }

                await DisplayAlert("Success", "Order completed successfully!", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to complete order: {ex.Message}", "OK");
            }
        }
    }

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
    }
}