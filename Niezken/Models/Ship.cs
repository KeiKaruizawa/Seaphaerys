namespace Niezken.Models
{
    public class Ship
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Route { get; set; }

        public string? Price { get; set; }

        public string? Description { get; set; }

        public string? Image { get; set; }

        public int RouteId { get; set; }

        public string? DepartureTime { get; set; }
    }
}
