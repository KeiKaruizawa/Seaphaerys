using System.ComponentModel.DataAnnotations;

namespace Niezken.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public string? ShipName { get; set; }

        public string? Route { get; set; }

        public decimal Price { get; set; }

        public DateTime TravelDate { get; set; }

        public string Status { get; set; } = "Booked";

        public int UserId { get; set; }

        public User? User { get; set; }

        public string? AccommodationType { get; set; }

        public string? ContactNumber { get; set; }

        public int PassengerCount { get; set; } = 1;

        public string? PaymentMethod { get; set; }

        public string? DepartureTime { get; set; }

        // JSON string storing each passenger's name, age, sex
        // e.g. [{"FullName":"Juan Dela Cruz","Age":25,"Sex":"Male"}, ...]
        public string? PassengersJson { get; set; }
    }
}
