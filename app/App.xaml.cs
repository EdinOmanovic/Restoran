using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using app.Helpers;
using app.Models;

namespace app
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Osiguraj inicijalizaciju baze i kreaciju potrebnih tabela
            var db = DatabaseHelper.Database; // Ovo automatski kreira bazu i tabele

            // Dodaj početne podatke ako baza nije već popunjena
            SeedData();

            // Postavljanje početne stranice aplikacije
            MainPage = new NavigationPage(new LoginPage());
        }

        // Metoda za dodavanje početnih podataka u bazu (npr. stavke hrane)
        private void SeedData()
        {
            var foodItems = DatabaseHelper.GetAllFoodItems();
            if (foodItems.Count == 0)
            {
                // Dodajte početne stavke hrane
                var items = new List<FoodItem>
                {
                    new FoodItem { Name = "Pepperoni Pizza", Description = "Delicious pizza with pepperoni", Price = 8.99M, ImageUrl = "pizza.jpg" },
                    new FoodItem { Name = "Sushi Deluxe", Description = "Fresh sushi rolls", Price = 15.99M, ImageUrl = "sushi.jpg" },
                    new FoodItem { Name = "Burger Express", Description = "Juicy burger with cheese", Price = 6.99M, ImageUrl = "burger.jpg" }
                };

                foreach (var item in items)
                {
                    DatabaseHelper.AddFoodItem(item);
                }
            }
        }
    }
}
