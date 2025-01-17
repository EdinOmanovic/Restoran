using System;
using System.Collections.Generic;
using System.IO;
using SQLite;
using app.Models;

namespace app.Helpers
{
    public static class DatabaseHelper
    {
        private static SQLiteConnection _database;

        public static SQLiteConnection Database
        {
            get
            {
                if (_database == null)
                {
                    var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "app.db3");
                    _database = new SQLiteConnection(dbPath);
                    _database.CreateTable<User>();
                    _database.CreateTable<FoodItem>(); // Dodajemo kreaciju tabele za FoodItem
                }
                return _database;
            }
        }

        public static int AddFoodItem(FoodItem item)
        {
            return Database.Insert(item);
        }

        public static int UpdateFoodItem(FoodItem item)
        {
            return Database.Update(item);
        }

        public static int DeleteFoodItem(int id)
        {
            return Database.Delete<FoodItem>(id);
        }

        public static FoodItem GetFoodItem(int id)
        {
            return Database.Table<FoodItem>().FirstOrDefault(f => f.Id == id);
        }

        public static List<FoodItem> GetAllFoodItems()
        {
            return Database.Table<FoodItem>().ToList();
        }
    }
}