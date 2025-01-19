using SQLite;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace app
{
    public partial class HomePageWithCRUD : ContentPage
    {
        private SQLiteConnection _database;
        private Item _itemBeingEdited;
        private ObservableCollection<Item> _items;

        public HomePageWithCRUD()
        {
            InitializeComponent();
            InitializeDatabase();
            InitializeCollections();
            LoadItems();
        }

        private void InitializeDatabase()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db3");
            _database = new SQLiteConnection(dbPath);
            _database.CreateTable<Item>();
        }

        private async void ViewDailyRevenue_Clicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new DailySummaryPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Unable to open daily revenue page: {ex.Message}", "OK");
            }
        }

        private void InitializeCollections()
        {
            _items = new ObservableCollection<Item>();
            ItemsCollectionView.ItemsSource = _items;

            // Set up CategoryPicker if not already populated
            if (CategoryPicker.ItemsSource == null)
            {
                CategoryPicker.ItemsSource = new List<string>
                {
                    "Pizza",
                    "Burgers",
                    "Pasta",
                    "Salads",
                    "Drinks",
                    "Desserts"
                };
            }
        }

        private void LoadItems()
        {
            try
            {
                var items = _database.Table<Item>()
                                   .OrderBy(i => i.Category)
                                   .ThenBy(i => i.Name)
                                   .ToList();

                _items.Clear();
                foreach (var item in items)
                {
                    _items.Add(item);
                }

                // Refresh the CollectionView
                ItemsCollectionView.ItemsSource = null;
                ItemsCollectionView.ItemsSource = _items;
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Error", $"Error loading items: {ex.Message}", "OK");
                });
            }
        }

        private async void AddOrUpdateItemButton_Clicked(object sender, EventArgs e)
        {
            if (!ValidateInputs())
            {
                await DisplayAlert("Error", "Please fill in all required fields", "OK");
                return;
            }

            try
            {
                if (_itemBeingEdited == null)
                {
                    // Add new item
                    var newItem = CreateItemFromInputs();
                    _database.Insert(newItem);
                    _items.Add(newItem);
                    await DisplayAlert("Success", "Menu item added successfully", "OK");
                }
                else
                {
                    // Update existing item
                    UpdateItemFromInputs(_itemBeingEdited);
                    _database.Update(_itemBeingEdited);

                    // Update the item in the ObservableCollection
                    var index = _items.IndexOf(_itemBeingEdited);
                    if (index != -1)
                    {
                        _items[index] = _itemBeingEdited;
                    }

                    _itemBeingEdited = null;
                    await DisplayAlert("Success", "Menu item updated successfully", "OK");
                }

                ClearInputs();
                LoadItems(); // Refresh the list
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(ItemNameEntry.Text) ||
                string.IsNullOrWhiteSpace(ItemDescriptionEntry.Text) ||
                string.IsNullOrWhiteSpace(ItemPriceEntry.Text) ||
                CategoryPicker.SelectedItem == null)
            {
                return false;
            }

            return decimal.TryParse(ItemPriceEntry.Text, out _);
        }

        private Item CreateItemFromInputs()
        {
            return new Item
            {
                Name = ItemNameEntry.Text.Trim(),
                Description = ItemDescriptionEntry.Text.Trim(),
                Price = decimal.Parse(ItemPriceEntry.Text),
                Category = CategoryPicker.SelectedItem.ToString(),
                ImageUrl = GetDefaultImageForCategory(CategoryPicker.SelectedItem.ToString())
            };
        }

        private void UpdateItemFromInputs(Item item)
        {
            item.Name = ItemNameEntry.Text.Trim();
            item.Description = ItemDescriptionEntry.Text.Trim();
            item.Price = decimal.Parse(ItemPriceEntry.Text);
            item.Category = CategoryPicker.SelectedItem.ToString();
            item.ImageUrl = GetDefaultImageForCategory(CategoryPicker.SelectedItem.ToString());
        }

        private string GetDefaultImageForCategory(string category)
        {
            return category.ToLower() switch
            {
                "pizza" => "pizza.png",
                "burgers" => "burger.png",
                "pasta" => "pasta.png",
                "salads" => "salad.png",
                "drinks" => "drink.png",
                "desserts" => "dessert.png",
                _ => "food.png"
            };
        }

        private void ClearInputs()
        {
            ItemNameEntry.Text = string.Empty;
            ItemDescriptionEntry.Text = string.Empty;
            ItemPriceEntry.Text = string.Empty;
            CategoryPicker.SelectedItem = null;
        }

        private void UpdateItemButton_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Item itemToUpdate)
            {
                _itemBeingEdited = itemToUpdate;
                ItemNameEntry.Text = itemToUpdate.Name;
                ItemDescriptionEntry.Text = itemToUpdate.Description;
                ItemPriceEntry.Text = itemToUpdate.Price.ToString();
                CategoryPicker.SelectedItem = itemToUpdate.Category;
            }
        }

        private async void DeleteItemButton_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int itemId)
            {
                bool confirm = await DisplayAlert("Confirm Delete",
                    "Are you sure you want to delete this menu item?",
                    "Yes", "No");

                if (confirm)
                {
                    var itemToDelete = _items.FirstOrDefault(i => i.Id == itemId);
                    if (itemToDelete != null)
                    {
                        _database.Delete<Item>(itemId);
                        _items.Remove(itemToDelete);
                        await DisplayAlert("Success", "Menu item deleted successfully", "OK");
                    }
                }
            }
        }

        public class Item
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public string ImageUrl { get; set; }
            public string Category { get; set; }
        }
    }
}