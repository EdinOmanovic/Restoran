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
        SeedMenuItems(); // Add sample menu items
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
                    TableNumber = _tableNumber
                };
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
                // Save order items to database
                foreach (var item in _orderItems)
                {
                    _database.Insert(item);
                }

                // Update table status
                var tables = _database.Table<HomePageReadOnly.Table>().ToList();
                var table = tables.FirstOrDefault(t => t.TableNumber == $"Table {_tableNumber}");
                if (table != null)
                {
                    table.IsAvailable = false;
                    _database.Update(table);
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
    }
}