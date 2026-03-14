using System.ComponentModel.DataAnnotations;
namespace Niezken.Models
{
    public class ActivityLog
    {
       
            public int Id { get; set; }
            public string UserEmail { get; set; }
            public string Action { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

    }
}
