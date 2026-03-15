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

        public IActionResult AccessDenied()
        {
            return View();
        }

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
                // If not valid, show the form again with error messages
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
        // returnUrl = the page the user tried to visit before being sent to Login
        // Example: clicking "Select This Vessel" while not logged in sets
        // returnUrl = /Booking/BookingForm?shipId=1...
        // After successful login, we redirect them back there so the flow continues
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email and Password are required.";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            string hashedPassword = HashPassword(password);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == hashedPassword);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            if (!user.IsActive)
            {
                ViewBag.Error = "Your account has been disabled. Please contact support.";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            // Build the login cookie with the user's email and role
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("FirstName", user.FirstName),
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity)
            );

            _context.ActivityLogs.Add(new ActivityLog
            {
                UserEmail = user.Email,
                Action = "User logged in"
            });
            await _context.SaveChangesAsync();

            // REDIRECT LOGIC:
            // 1. Admin always goes to Admin Dashboard
            // 2. If returnUrl exists (came from BookingForm etc.), go back there
            // 3. Otherwise go to Home
            if (user.Role == "Admin")
            {
                return RedirectToAction("AdminDashboard", "Admin");
            }
            else if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                // Url.IsLocalUrl() prevents open redirect attacks
                return Redirect(returnUrl);
            }
            else
            {
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
        // Also gets all routes for that ship name so the view
        // can show direction options (e.g. Manila→Cebu / Cebu→Manila)
        // --------------------------------------------------------
        public async Task<IActionResult> AccommodationDetails(int id)
        {
            // Static ship info (images, descriptions) — keyed by RouteId 1-8
            var shipInfo = new Dictionary<int, (string Name, string Image, string Description, string Price)>
            {
                { 1, ("MV St. Nicholas",  "ship1.jpg", "A modern and spacious passenger vessel offering premium cabins, a full dining area, and entertainment facilities. Ideal for overnight inter-island voyages between Manila and Cebu.", "\u20b11,500") },
                { 2, ("MV St. Joseph",    "ship2.jpg", "Designed for long-distance sea travel, MV St. Joseph features wide deck spaces, air-conditioned cabins, and a fully equipped galley serving hot meals throughout the journey.", "\u20b11,800") },
                { 3, ("MV St. Augustine", "ship3.jpg", "A reliable and fast ferry with ergonomic premium seating, onboard Wi-Fi, and a dedicated family lounge. Perfect for the Manila-Bacolod overnight route.", "\u20b11,600") },
                { 4, ("MV St. Leo",       "ship4.jpg", "An affordable yet comfortable vessel connecting Cebu and Iloilo. Offers clean economy cabins, a snack bar, and open-air deck seating with panoramic sea views.", "\u20b11,200") },
                { 5, ("MV St. John Paul", "ship5.jpg", "Our flagship luxury vessel serving the Manila-Palawan corridor. Boasts premium suite cabins, a full-service restaurant, spa facilities, and a children\'s play area.", "\u20b12,200") },
                { 6, ("MV St. Francis",   "ship6.jpg", "An efficient and well-maintained inter-island ferry covering the Cebu-Bacolod route. Features comfortable reclining seats, a cafeteria, and a spacious cargo deck.", "\u20b11,100") },
                { 7, ("MV St. Peter",     "ship7.jpg", "Equipped with modern navigation systems and a full passenger manifest capacity, MV St. Peter is the go-to vessel for the Iloilo-Manila route with overnight cabin options.", "\u20b11,900") },
                { 8, ("MV St. Benedict",  "ship8.jpg", "A high-capacity ship purpose-built for the Cebu-Palawan sea corridor. Offers multiple cabin classes, a sun deck, and onboard retail shops for a pleasant long-haul journey.", "\u20b12,000") },
            };

            // id here is the RouteId (1-8), not the DB ship ID
            if (!shipInfo.ContainsKey(id))
                return NotFound();

            var info = shipInfo[id];

            // Build the base ShipViewModel for the view
            var ship = new ShipViewModel
            {
                Id = id,
                Name = info.Name,
                Image = info.Image,
                Description = info.Description,
                Price = info.Price,
                Route = "" // Route is handled separately below
            };

            // Get all routes for this ship name from the database
            // This gives us both directions e.g. ["Manila to Cebu", "Cebu to Manila"]
            var routes = await _context.Ships
                .Where(s => s.Name == info.Name)
                .Select(s => s.Route)
                .Distinct()
                .ToListAsync();

            // Pass routes to the view so it can show direction radio buttons
            ViewBag.Routes = routes;

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

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
