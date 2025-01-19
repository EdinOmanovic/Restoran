using SQLite;
using System.Collections.ObjectModel;

namespace app;

public partial class DailySummaryPage : ContentPage
{
    private SQLiteConnection _database;
    private DateTime _currentDate = DateTime.Today;
    private ObservableCollection<OrderSummary> _orders;

    public DailySummaryPage()
    {
        InitializeComponent();
        InitializeDatabase();
        _orders = new ObservableCollection<OrderSummary>();
        ordersCollectionViewSummary.ItemsSource = _orders;
        LoadSummaryForDate(_currentDate);
    }

    private void InitializeDatabase()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db3");
        _database = new SQLiteConnection(dbPath);
    }

    private void LoadSummaryForDate(DateTime date)
    {
        dateLabelSummary.Text = date.ToString("dd MMMM yyyy");
        var startDate = date.Date;
        var endDate = startDate.AddDays(1);

        // Get all orders for the selected date, including completed ones
        var orders = _database.Table<OrderPage.OrderItem>()
            .Where(o => o.OrderTime >= startDate && o.OrderTime < endDate)
            .ToList();

        // Group orders by table and order group ID to get unique orders
        var orderSummaries = orders
            .GroupBy(o => new { o.TableNumber, o.OrderGroupId })
            .Select(g => new OrderSummary
            {
                TableNumber = g.Key.TableNumber,
                OrderTime = g.First().OrderTime,
                TotalAmount = g.Sum(o => o.Price * o.Quantity)
            })
            .OrderByDescending(o => o.OrderTime)
            .ToList();

        _orders.Clear();
        foreach (var summary in orderSummaries)
        {
            _orders.Add(summary);
        }

        // Update summary statistics
        UpdateStatistics(orderSummaries);
    }

    private void UpdateStatistics(List<OrderSummary> summaries)
    {
        int totalOrders = summaries.Count;
        decimal totalRevenue = summaries.Sum(s => s.TotalAmount);
        decimal averageOrder = totalOrders > 0 ? totalRevenue / totalOrders : 0;

        totalOrdersLabelSummary.Text = totalOrders.ToString();
        totalRevenueLabelSummary.Text = totalRevenue.ToString("C");
        averageOrderLabelSummary.Text = averageOrder.ToString("C");
    }

    private void OnPreviousDayClicked(object sender, EventArgs e)
    {
        _currentDate = _currentDate.AddDays(-1);
        LoadSummaryForDate(_currentDate);
    }

    private void OnNextDayClicked(object sender, EventArgs e)
    {
        _currentDate = _currentDate.AddDays(1);
        LoadSummaryForDate(_currentDate);
    }

    public class OrderSummary
    {
        public int TableNumber { get; set; }
        public DateTime OrderTime { get; set; }
        public decimal TotalAmount { get; set; }
    }
}