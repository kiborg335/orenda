using System;

namespace SportsRental.Models
{
    public class Rental
    {
        public int Id { get; set; }
        public int EquipmentId { get; set; }
        public int ClientId { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string ClientName { get; set; }
        public string EquipmentName { get; set; }
        public decimal RentalCost { get; set; }
        public bool IsOverdue => DateTime.Now > DueDate && ReturnDate == null;
    }
}