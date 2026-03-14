using System.ComponentModel.DataAnnotations;

namespace Niezken.Models
{
    // Used to pass search parameters from BookNow search form to results
    public class BookSearchViewModel
    {
        [Required(ErrorMessage = "Please select an origin.")]
        public string Origin { get; set; }

        [Required(ErrorMessage = "Please select a destination.")]
        public string Destination { get; set; }

        [Required(ErrorMessage = "Please select a travel date.")]
        public DateTime TravelDate { get; set; }

        // Ships that match the searched route
        public List<Ship> AvailableShips { get; set; } = new();
    }

    // Used to fill in passenger details before confirming a booking
    public class BookingFormViewModel
    {
        // --- Ship Info (passed from search results) ---
        public int ShipId { get; set; }
        public string ShipName { get; set; }
        public string Route { get; set; }
        public string Price { get; set; }
        public DateTime TravelDate { get; set; }

        // --- Booking Details ---
        [Required(ErrorMessage = "Please select an accommodation type.")]
        public string AccommodationType { get; set; }   // Cabin / Balcony / Suite

        [Required(ErrorMessage = "Please enter a contact number.")]
        [Phone(ErrorMessage = "Invalid phone number.")]
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "Please enter number of passengers.")]
        [Range(1, 20, ErrorMessage = "Passengers must be between 1 and 20.")]
        public int PassengerCount { get; set; } = 1;

        // Each passenger's details (populated dynamically)
        public List<PassengerDetail> Passengers { get; set; } = new();
    }

    // Represents one individual passenger's info
    public class PassengerDetail
    {
        [Required(ErrorMessage = "Full name is required.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Age is required.")]
        [Range(1, 120, ErrorMessage = "Please enter a valid age.")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Sex is required.")]
        public string Sex { get; set; }   // Male / Female
    }
}
