using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Niezken.Data;
using Niezken.Models;
using System.Security.Claims;

namespace Niezken.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var email = User.Identity.Name;

            var user = await _context.Users
                .Include(u => u.Bookings)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }

            return View(user);
        }

        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking != null)
            {
                booking.Status = "Cancelled";
                await _context.SaveChangesAsync();
                _context.ActivityLogs.Add(new ActivityLog
                {
                    UserEmail = User.Identity.Name,
                    Action = "User Cancelled Booking"
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> Reschedule(int id, DateTime newDate)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking != null)
            {
                booking.TravelDate = newDate;
                booking.Status = "Rescheduled";
                await _context.SaveChangesAsync();
                _context.ActivityLogs.Add(new ActivityLog
                {
                    UserEmail = User.Identity.Name,
                    Action = "User Rescheduled Booking"
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Dashboard");
        }
        public async Task<IActionResult> EditProfile()
        {
            var email = User.Identity.Name;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(User model)
        {
            var user = await _context.Users.FindAsync(model.Id);

            if (user != null)
            {
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;

                await _context.SaveChangesAsync();
                _context.ActivityLogs.Add(new ActivityLog
                {
                    UserEmail = User.Identity.Name,
                    Action = "User Updated Profile"
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Dashboard");
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(int id, string newPassword)
        {
            var user = await _context.Users.FindAsync(id);

            if (user != null)
            {
                user.PasswordHash = HashPassword(newPassword);
                await _context.SaveChangesAsync();
                _context.ActivityLogs.Add(new ActivityLog
                {
                    UserEmail = User.Identity.Name,
                    Action = "User updated password"
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Dashboard");
        }
      
     
        private string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}