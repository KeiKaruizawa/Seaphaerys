namespace Niezken.Models
{
    public class Route
    {
        public int Id { get; set; }

        public string Origin { get; set; }

        public string Destination { get; set; }

        public int Distance { get; set; }

        public int Duration { get; set; }
    }
}
