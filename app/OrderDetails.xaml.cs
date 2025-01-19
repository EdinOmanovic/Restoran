using SQLite;

namespace app;

public partial class OrderDetailsPage : ContentPage
{
    private SQLiteConnection _database;
    private int _tableNumber;
    private List<OrderPage.OrderItem> _orderItems;

    public OrderDetailsPage(int tableNumber)
    {
        InitializeComponent();
        _tableNumber = tableNumber;
        InitializeDatabase();
        LoadOrderDetails();
    }

    private void InitializeDatabase()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db3");
        _database = new SQLiteConnection(dbPath);
        _database.CreateTable<OrderPage.OrderItem>();
    }

    private void LoadOrderDetails()
    {
        try
        {
            // Get all orders for this table, including completed ones
            _orderItems = _database.Table<OrderPage.OrderItem>()
                .Where(o => o.TableNumber == _tableNumber)
                .OrderBy(o => o.OrderTime)
                .ToList();

            Console.WriteLine($"Orders found for table {_tableNumber}: {_orderItems.Count}");

            // Set header information
            TableNumberLabel.Text = $"Stol {_tableNumber}";

            if (_orderItems.Any())
            {
                var mostRecentOrder = _orderItems.OrderByDescending(o => o.OrderTime).First();
                OrderTimeLabel.Text = $"Vrijeme narudžbe: {mostRecentOrder.OrderTime:dd.MM.yyyy HH:mm}";

                // Update ListView with all items
                OrderItemsListView.ItemsSource = _orderItems;

                // Calculate and display total
                decimal total = _orderItems.Sum(item => item.Price * item.Quantity);
                TotalLabel.Text = $"Ukupno: {total:C}";

                // Debug output
                foreach (var item in _orderItems)
                {
                    Console.WriteLine($"Item: {item.ItemName}, Quantity: {item.Quantity}, Price: {item.Price}");
                }
            }
            else
            {
                OrderTimeLabel.Text = "Nema narudžbi";
                TotalLabel.Text = "Ukupno: 0.00 KM";
                Console.WriteLine("No orders found for this table");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading order details: {ex.Message}");
            DisplayAlert("Greška", "Došlo je do greške pri uèitavanju detalja narudžbe", "U redu");
        }
    }
}