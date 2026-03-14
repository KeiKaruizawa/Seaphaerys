// ============================================================
// HomeController.cs
// Handles all PUBLIC pages that anyone can see:
//   - Home, Accommodation, Outlets, Contact, FAQ
//   - Login, Register, Logout, Forgot Password
//
// Also handles the LOGIN REDIRECT LOGIC:
//   - Admin  → goes to Admin Dashboard
//   - Passenger → stays on the public Home page
// ============================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Niezken.Data;
using Niezken.Models;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Niezken.Controllers
{
    public class HomeController : Controller
    {
        // _logger is used to write debug/error messages to the console
        private readonly ILogger<HomeController> _logger;

        // _context is the database connection — used to read/write Users, Bookings, etc.
        private readonly AppDbContext _context;

        // Constructor: ASP.NET automatically provides (injects) these when the controller is created
        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // PUBLIC HOME PAGES
        // These pages are visible to EVERYONE — no login needed

        // GET: /Home/Index  (or just "/" — this is the homepage)
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Accommodation() => View();

        public IActionResult Outlets() => View();

        public IActionResult Contact() => View();

        public IActionResult faq() => View();

        public IActionResult Forgot() => View();

        // REGISTER
        // GET: /Home/Register
        // Shows the registration form
        [HttpGet]
        public IActionResult Register()
        {
            // If user is already logged in, redirect them away from the register page
            if (User.Identity.IsAuthenticated)
            {
                // Redirect Admin to Admin Dashboard
                if (User.IsInRole("Admin"))
                    return RedirectToAction("AdminDashboard", "Admin");

                // Redirect Passenger to Home
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: /Home/Register
        // Processes the registration form when submitted
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Check if all form fields are valid (e.g. email format, password rules)
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if someone already registered with this email
            bool emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);

            if (emailExists)
            {
                // Add an error message under the Email field
                ModelState.AddModelError("Email", "Email is already registered.");
                return View(model);
            }

            // Create a new User record for the database
            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Role = "Passenger",             // All self-registered users are Passengers
                IsActive = true,                // Account is active by default
                PasswordHash = HashPassword(model.Password)
            };

            // Add the user to the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // After successful registration, go to Login page
            return RedirectToAction("Login");
        }

        // LOGIN
        // GET: /Home/Login
        // Shows the login form
        [HttpGet]
        public IActionResult Login()
        {
            // If user is already logged in, redirect them — no need to login again
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("AdminDashboard", "Admin");

                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: /Home/Login
        // Processes the login form when submitted
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Make sure both fields are filled in
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email and Password are required.";
                return View();
            }

            string hashedPassword = HashPassword(password);

            // Look for a user in the database with matching email AND password hash
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == hashedPassword);

            // If no user found, credentials are wrong
            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            // If the admin disabled this account, don't allow login
            if (!user.IsActive)
            {
                ViewBag.Error = "Your account has been disabled. Please contact support.";
                return View();
            }

            // CREATE THE LOGIN COOKIE (CLAIMS)
            // Claims are pieces of info stored in the cookie:
            //   - ClaimTypes.Name  = the user's email (used as their identity)
            //   - ClaimTypes.Role  = "Admin" or "Passenger"
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),  // This becomes User.Identity.Name
                new Claim(ClaimTypes.Role, user.Role),   // This enables User.IsInRole("Admin")
                new Claim("FirstName", user.FirstName),  // Bonus: store first name in cookie
            };

            // Wrap claims in an identity using Cookie authentication
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Sign in — this creates the browser cookie
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity)
            );

            // LOG THIS ACTION
            // Records in the database that this user logged in
            _context.ActivityLogs.Add(new ActivityLog
            {
                UserEmail = user.Email,
                Action = "User logged in"
            });
            await _context.SaveChangesAsync();

            // REDIRECT BASED ON ROLE
            // Admin  → Admin Dashboard
            // Passenger → Public Home page (they can browse & book)
            if (user.Role == "Admin")
            {
                return RedirectToAction("AdminDashboard", "Admin");
            }
            else
            {
                // Passenger stays on the public site
                return RedirectToAction("Index", "Home");
            }
        }

        // LOGOUT
        // GET: /Home/Logout
        public async Task<IActionResult> Logout()
        {
            // Log the logout action before clearing the session
            _context.ActivityLogs.Add(new ActivityLog
            {
                UserEmail = User.Identity?.Name ?? "Unknown",
                Action = "User logged out"
            });
            await _context.SaveChangesAsync();

            // Delete the authentication cookie — user is now logged out
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Send them back to the public home page
            return RedirectToAction("Index");
        }

        // ACCOMMODATION DETAILS
        // Shows detailed info for a specific ship (by ID)
        public IActionResult AccommodationDetails(int id)
        {
            // These 8 ships are hardcoded for now
            // In the future, this should be pulled from the Ships table in the database
            var ships = new List<ShipViewModel>
            {
                new ShipViewModel { Id = 1, Name = "MV St. Michael",    Image = "ship1.jpg", Route = "Manila to Cebu",    Price = "₱1,500", Description = "Modern passenger vessel with comfortable cabins and dining areas." },
                new ShipViewModel { Id = 2, Name = "MV St. Joseph",     Image = "ship2.jpg", Route = "Cebu to Davao",     Price = "₱1,800", Description = "Spacious vessel designed for long-distance travel." },
                new ShipViewModel { Id = 3, Name = "MV St. Augustine",  Image = "ship3.jpg", Route = "Manila to Bacolod", Price = "₱1,600", Description = "Reliable and fast ferry with premium seating." },
                new ShipViewModel { Id = 4, Name = "MV St. Leo",        Image = "ship4.jpg", Route = "Cebu to Iloilo",    Price = "₱1,200", Description = "Affordable and comfortable sea travel experience." },
                new ShipViewModel { Id = 5, Name = "MV St. John Paul",  Image = "ship5.jpg", Route = "Manila to Palawan", Price = "₱2,200", Description = "Luxury vessel with full onboard amenities." },
                new ShipViewModel { Id = 6, Name = "MV St. Francis",    Image = "ship6.jpg", Route = "Davao to Cebu",     Price = "₱1,700", Description = "Efficient vessel for inter-island transport." },
                new ShipViewModel { Id = 7, Name = "MV St. Peter",      Image = "ship7.jpg", Route = "Iloilo to Manila",  Price = "₱1,900", Description = "Premium ferry with modern navigation systems." },
                new ShipViewModel { Id = 8, Name = "MV St. Benedict",   Image = "ship8.jpg", Route = "Cebu to Palawan",   Price = "₱2,000", Description = "High-capacity ship for long sea journeys." }
            };

            // Find the ship that matches the given ID
            var ship = ships.FirstOrDefault(s => s.Id == id);

            // If no ship found with that ID, return 404
            if (ship == null)
                return NotFound();

            return View(ship);
        }

        // ERROR PAGE
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        // PRIVATE HELPER: PASSWORD HASHING
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
