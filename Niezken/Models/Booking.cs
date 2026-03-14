using System.ComponentModel.DataAnnotations;

namespace Niezken.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public string ShipName { get; set; }

        public string Route { get; set; }

        public decimal Price { get; set; }

        public DateTime TravelDate { get; set; }

        public string Status { get; set; } = "Booked";

        public int UserId { get; set; }

        public User User { get; set; }
    }
}