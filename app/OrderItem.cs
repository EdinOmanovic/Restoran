using SQLite;
using System;

namespace app
{
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
        public Guid OrderGroupId { get; set; } = Guid.NewGuid();
    }
}