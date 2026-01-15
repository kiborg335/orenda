namespace SportsRental.Models
{
    public class Equipment
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int Year { get; set; }
        public string Brand { get; set; }
        public int TotalQuantity { get; set; }
        public int AvailableQuantity { get; set; }
    }
}