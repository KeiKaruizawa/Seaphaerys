using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Niezken.Data;
using Niezken.Models;

namespace Niezken.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;

        public BookingController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Create(int id, string name, string route, string price)
        {
            var model = new ShipViewModel
            {
                Id = id,
                Name = name,
                Route = route,
                Price = price
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(int shipId, string shipName, string route, string price, DateTime travelDate)
        {
            var email = User.Identity.Name;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            var booking = new Booking
            {
                ShipName = shipName,
                Route = route,
                Price = decimal.Parse(price.Replace("₱", "")),
                TravelDate = travelDate,
                Status = "Booked",
                UserId = user.Id
            };

            _context.Bookings.Add(booking);

            _context.ActivityLogs.Add(new ActivityLog
            {
                UserEmail = user.Email,
                Action = $"User booked trip: {shipName} ({route}) on {travelDate:MMM dd yyyy}"
            });

            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard", "User");
        }

        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking != null)
            {
                booking.Status = "Cancelled";

                _context.ActivityLogs.Add(new ActivityLog
                {
                    UserEmail = User.Identity.Name,
                    Action = $"User cancelled booking for {booking.ShipName}"
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Dashboard", "User");
        }


    }

}