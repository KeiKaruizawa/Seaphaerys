using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Niezken.Data;
using Niezken.Models;
using System.Text.Json;

namespace Niezken.Controllers
{
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;

        public BookingController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> BookNow(string origin = null, string destination = null, string from = null)
        {
            var ships = await _context.Ships.ToListAsync();

            var ports = ships
                .SelectMany(s => s.Route.Split(" to ", StringSplitOptions.TrimEntries))
                .Distinct()
                .OrderBy(p => p)
                .ToList();

            ViewBag.Ports = ports;

            // Pass the "from" param to the view so it can be carried
            // forward in the "Select This Vessel" link to BookingForm
            ViewBag.From = from ?? "";

            var model = new BookSearchViewModel
            {
                Origin = origin ?? "",
                Destination = destination ?? ""
            };

            return View(model);
        }

        // STEP 1 — POST: Search for ships matching the route
        [HttpPost]
        public async Task<IActionResult> BookNow(BookSearchViewModel model)
        {
            // Reload ports for the dropdown in case we return the view
            var allShips = await _context.Ships.ToListAsync();
            var ports = allShips
                .SelectMany(s => s.Route.Split(" to ", StringSplitOptions.TrimEntries))
                .Distinct()
                .OrderBy(p => p)
                .ToList();
            ViewBag.Ports = ports;

            if (!ModelState.IsValid)
                return View(model);

            // Validate: origin and destination can't be the same
            if (model.Origin == model.Destination)
            {
                ModelState.AddModelError("", "Origin and destination cannot be the same.");
                return View(model);
            }

            // Validate: travel date must be at least 1 day ahead
            // Passengers cannot book for today — they need at least 1 day notice
            if (model.TravelDate.Date <= DateTime.Today)
            {
                ModelState.AddModelError("TravelDate", "Travel date must be at least 1 day in advance.");
                return View(model);
            }

            // Find ships whose route matches "Origin to Destination"
            string searchRoute = $"{model.Origin} to {model.Destination}";

            model.AvailableShips = await _context.Ships
                .Where(s => s.Route.ToLower() == searchRoute.ToLower())
                .ToListAsync();

            return View(model);
        }

        // STEP 2 — GET: Show the booking form for a chosen ship
        // Requires login — redirect to login if not authenticated
        [Authorize]
        [HttpGet]
        // from = "accommodation" if user came via AccommodationDetails → BookNow → here
        // Passed to the view so it can show the "Back to Fleet" button
        public async Task<IActionResult> BookingForm(int shipId, string route, string price, DateTime travelDate, string from = null)
        {
            var ship = await _context.Ships.FindAsync(shipId);

            if (ship == null)
                return NotFound();

            var model = new BookingFormViewModel
            {
                ShipId = ship.Id,
                ShipName = ship.Name,
                Route = route,
                Price = price,
                TravelDate = travelDate,
                DepartureTime = ship.DepartureTime ?? "",
                PassengerCount = 1,
                Passengers = new List<PassengerDetail> { new PassengerDetail() }
            };

            // Tell the view whether to show the Back to Fleet button
            ViewBag.From = from ?? "";

            return View(model);
        }

        // STEP 2 — POST: Save the booking
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> BookingForm(BookingFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Get the currently logged-in user
            var email = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return RedirectToAction("Login", "Home");

            // Serialize passenger details list to JSON for storage
            var passengersJson = JsonSerializer.Serialize(model.Passengers);

            // Parse price — strip ₱ symbol if present
            var rawPrice = model.Price?.Replace("₱", "").Replace(",", "").Trim() ?? "0";
            decimal.TryParse(rawPrice, out decimal parsedPrice);

            // Create and save the booking
            var booking = new Booking
            {
                ShipName = model.ShipName,
                Route = model.Route,
                Price = parsedPrice,
                TravelDate = model.TravelDate,
                Status = "Booked",
                UserId = user.Id,
                AccommodationType = model.AccommodationType,
                PaymentMethod = model.PaymentMethod,
                DepartureTime = model.DepartureTime,
                ContactNumber = model.ContactNumber,
                PassengerCount = model.PassengerCount,
                PassengersJson = passengersJson
            };

            _context.Bookings.Add(booking);

            // Log the activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                UserEmail = user.Email,
                Action = $"User booked trip: {model.ShipName} ({model.Route}) on {model.TravelDate:MMM dd yyyy} — {model.AccommodationType}, {model.PassengerCount} pax"
            });

            await _context.SaveChangesAsync();

            // Redirect to confirmation page with the new booking ID
            return RedirectToAction("Confirmation", new { id = booking.Id });
        }

        // STEP 3 — Booking Confirmation Page
        [Authorize]
        public async Task<IActionResult> Confirmation(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound();

            // Deserialize passenger details from JSON
            var passengers = new List<PassengerDetail>();
            if (!string.IsNullOrEmpty(booking.PassengersJson))
            {
                passengers = JsonSerializer.Deserialize<List<PassengerDetail>>(booking.PassengersJson)
                             ?? new List<PassengerDetail>();
            }

            ViewBag.Passengers = passengers;

            return View(booking);
        }

        // CANCEL — from passenger dashboard
        [Authorize]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking != null)
            {
                booking.Status = "Cancelled";

                _context.ActivityLogs.Add(new ActivityLog
                {
                    UserEmail = User.Identity.Name,
                    Action = $"User cancelled booking for {booking.ShipName} ({booking.Route})"
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Dashboard", "User");
        }
    }
}
