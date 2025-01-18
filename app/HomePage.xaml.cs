using SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace app;

public partial class HomePage : ContentPage
{
    private SQLiteConnection _database;
    public HomePage()
	{
		InitializeComponent();
		InitializeDatabase();
		LoadItems();
	}
    private void InitializeDatabase()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db3");
        _database = new SQLiteConnection(dbPath);
        _database.CreateTable<Item>();
    }

    private void LoadItems()
    {
        var items = _database.Table<Item>().ToList();

        
        ItemsCountLabel.Text = $"Broj stavki u bazi: {items.Count}";

        ItemsListView.ItemsSource = items;
    }



    private async void AddItemButton_Clicked(object sender, EventArgs e)
    {
        string itemName = ItemNameEntry.Text;
        string itemDescription = ItemDescriptionEntry.Text;

        if (string.IsNullOrEmpty(itemName) || string.IsNullOrEmpty(itemDescription))
        {
            await DisplayAlert("Greška", "Molimo unesite sve informacije", "OK");
            return;
        }

        var newItem = new Item
        {
            Name = itemName,
            Description = itemDescription
        };

        _database.Insert(newItem);
        LoadItems(); // Ponovno uèitaj listu

        ItemNameEntry.Text = string.Empty;
        ItemDescriptionEntry.Text = string.Empty;
    }

    private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem != null)
        {
            var selectedItem = e.SelectedItem as Item;
            if (selectedItem != null)
            {
                
                DisplayAlert("Item Selected", $"Selected item: {selectedItem.Name} - {selectedItem.Description}", "OK");
            }
            
            ((ListView)sender).SelectedItem = null;
        }
    }

    public class Item
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

}