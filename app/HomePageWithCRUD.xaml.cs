using SQLite;
using System.IO;
using System.Linq;

namespace app
{
    public partial class HomePageWithCRUD : ContentPage
    {
        private SQLiteConnection _database;
        private Item _itemBeingEdited;

        public HomePageWithCRUD()
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

            // Dodavanje testnih podataka
            SeedData();
        }

        private void SeedData()
        {
            // Provjera da li veæ postoji item "Pizza"
            var existingItem = _database.Table<Item>().FirstOrDefault(i => i.Name == "Pizza");
            if (existingItem == null)
            {
                var pizzaItem = new Item
                {
                    Name = "Stol 1",
                    Description = "Srenji",
                    Price = 0,
                    ImageUrl = "stp.png",
                    Category = "Pizza"
                };
                _database.Insert(pizzaItem);
            }

            // Dodavanje testnih podataka za druge stavke
            var existingChicken = _database.Table<Item>().FirstOrDefault(i => i.Name == "Chicken");
            if (existingChicken == null)
            {
                var chickenItem = new Item
                {
                    Name = "Stol 2",
                    Description = "Srednji",
                    Price = 0,
                    ImageUrl = "sto.png",
                    Category = "Chicken"
                };
                _database.Insert(chickenItem);
            }

            var existingBurger = _database.Table<Item>().FirstOrDefault(i => i.Name == "Burger");
            if (existingBurger == null)
            {
                var burgerItem = new Item
                {
                    Name = "Cheese Burger",
                    Description = "Classic cheeseburger with lettuce and tomato",
                    Price = 6.50M,
                    ImageUrl = "burger.jpg",
                    Category = "Burger"
                };
                _database.Insert(burgerItem);
            }

            var existingPasta = _database.Table<Item>().FirstOrDefault(i => i.Name == "Pasta");
            if (existingPasta == null)
            {
                var pastaItem = new Item
                {
                    Name = "Spaghetti Bolognese",
                    Description = "Traditional spaghetti with meat sauce",
                    Price = 12.00M,
                    ImageUrl = "pasta.jpg",
                    Category = "Pasta"
                };
                _database.Insert(pastaItem);
            }

            var existingSushi = _database.Table<Item>().FirstOrDefault(i => i.Name == "Sushi");
            if (existingSushi == null)
            {
                var sushiItem = new Item
                {
                    Name = "California Roll",
                    Description = "Sushi rolls with crab and avocado",
                    Price = 15.00M,
                    ImageUrl = "sushi.jpg",
                    Category = "Sushi"
                };
                _database.Insert(sushiItem);
            }

            var existingBreakfast = _database.Table<Item>().FirstOrDefault(i => i.Name == "Breakfast");
            if (existingBreakfast == null)
            {
                var breakfastItem = new Item
                {
                    Name = "Pancakes",
                    Description = "Fluffy pancakes with syrup",
                    Price = 7.00M,
                    ImageUrl = "dorucak.jpg",
                    Category = "Breakfast"
                };
                _database.Insert(breakfastItem);
            }

            var existingDesert = _database.Table<Item>().FirstOrDefault(i => i.Name == "Desert");
            if (existingDesert == null)
            {
                var desertItem = new Item
                {
                    Name = "Cake",
                    Description = "Rich chocolate cake with cream",
                    Price = 5.00M,
                    ImageUrl = "desert.jpg",
                    Category = "Desert"
                };
                _database.Insert(desertItem);
            }
        }

        private void LoadItems()
        {
            var items = _database.Table<Item>().ToList();
            ItemsListView.ItemsSource = items;
        }

        private async void AddOrUpdateItemButton_Clicked(object sender, EventArgs e)
        {
            string itemName = ItemNameEntry.Text;
            string itemDescription = ItemDescriptionEntry.Text;
            string itemPriceText = ItemPriceEntry.Text;
            string itemImageUrl = ItemImageUrlEntry.Text;
            string itemCategory = ItemCategoryEntry.Text; // Nova kategorija

            if (string.IsNullOrEmpty(itemName) || string.IsNullOrEmpty(itemDescription) || string.IsNullOrEmpty(itemPriceText) || string.IsNullOrEmpty(itemImageUrl) || string.IsNullOrEmpty(itemCategory))
            {
                await DisplayAlert("Greška", "Molimo unesite sve informacije", "OK");
                return;
            }

            decimal itemPrice = decimal.Parse(itemPriceText);

            if (_itemBeingEdited == null)
            {
                var newItem = new Item
                {
                    Name = itemName,
                    Description = itemDescription,
                    Price = itemPrice,
                    ImageUrl = itemImageUrl,
                    Category = itemCategory // Postavljanje kategorije
                };

                _database.Insert(newItem);
            }
            else
            {
                _itemBeingEdited.Name = itemName;
                _itemBeingEdited.Description = itemDescription;
                _itemBeingEdited.Price = itemPrice;
                _itemBeingEdited.ImageUrl = itemImageUrl;
                _itemBeingEdited.Category = itemCategory; // Ažuriranje kategorije

                _database.Update(_itemBeingEdited);
                _itemBeingEdited = null;
            }

            LoadItems();

            ItemNameEntry.Text = string.Empty;
            ItemDescriptionEntry.Text = string.Empty;
            ItemPriceEntry.Text = string.Empty;
            ItemImageUrlEntry.Text = string.Empty;
            ItemCategoryEntry.Text = string.Empty; // Praznjenje polja za kategoriju
        }

        private void UpdateItemButton_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var itemToUpdate = button?.CommandParameter as Item;

            if (itemToUpdate == null)
                return;

            // Popuni polja s odabranim itemom
            ItemNameEntry.Text = itemToUpdate.Name;
            ItemDescriptionEntry.Text = itemToUpdate.Description;
            ItemPriceEntry.Text = itemToUpdate.Price.ToString();
            ItemImageUrlEntry.Text = itemToUpdate.ImageUrl;
            ItemCategoryEntry.Text = itemToUpdate.Category; // Popunjavanje polja za kategoriju

            _itemBeingEdited = itemToUpdate; // Pohrani item koji se ureðuje
        }

        private async void DeleteItemButton_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var itemId = (int)button?.CommandParameter;

            var itemToDelete = _database.Table<Item>().FirstOrDefault(i => i.Id == itemId);
            if (itemToDelete != null)
            {
                _database.Delete(itemToDelete);
                LoadItems();
                await DisplayAlert("Uspješno", "Stavka je obrisana.", "OK");
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
            public string Category { get; set; } // Novo polje za kategoriju
        }
    }
}