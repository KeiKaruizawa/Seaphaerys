using System.Diagnostics;
using System.Security.Cryptography;  
using System.Text;                  
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Niezken.Data;               
using Niezken.Models;               

namespace Niezken.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger; // For logging errors or info
        private readonly AppDbContext _context;           // Your database context

        // Controller constructor: inject logger and DbContext
        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

       
        // HOME PAGES
      
        public IActionResult Index()
        {
            // Returns Home/Index view
            return View();
        }

        public IActionResult Accommodation() => View();
        public IActionResult Outlets() => View();
        public IActionResult Contact() => View();
        public IActionResult faq() => View();
        public IActionResult Forgot() => View();

       
        // REGISTER (GET)
      
        [HttpGet]
        public IActionResult Register()
        {
            // When user navigates to /Home/Register, show the registration form
            // This does NOT process data yet
            return View();
        }

       
        // REGISTER (POST)
       
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // [HttpPost] handles form submissions from the registration page
            // The RegisterViewModel contains FirstName, LastName, Email, Password, ConfirmPassword

            if (!ModelState.IsValid)
            {
                // If validation fails (like missing fields or password mismatch), redisplay form with errors
                return View(model);
            }

            // Check if a user with the same email already exists
            bool emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email is already registered.");
                return View(model);
            }

            // Create a new User entity to save in the database
            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password) // Hash password before saving
            };

            _context.Users.Add(user);           // Add user to the DbSet
            await _context.SaveChangesAsync();  // Commit changes to the database

            TempData["SuccessMessage"] = "Registration successful! You can now log in.";

            // Redirect to Login page after successful registration
            return RedirectToAction("Login");
        }

       
        // LOGIN (GET)
        
        [HttpGet]
        public IActionResult Login()
        {
            // Show the login form when user navigates to /Home/Login
            return View();
        }

      
        // LOGIN (POST)
       
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Handles form submission from login page
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email and Password are required.";
                return View();
            }

            // Hash the input password to compare with database
            string hashedPassword = HashPassword(password);

            // Look for a user with matching email and password hash
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == hashedPassword);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            // Optional: you can create a session/cookie here for authentication

            TempData["SuccessMessage"] = "Login successful!";
            return RedirectToAction("Index");
        }

       
        // PASSWORD HASHING METHOD
        
        private string HashPassword(string password)
        {
            // Converts the plain password to SHA256 hash
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

      
        // ERROR HANDLING
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        public IActionResult AccommodationDetails(int id)
        {
            var ships = new List<ShipViewModel>
    {
        new ShipViewModel
        {
            Id = 1,
            Name = "MV St. Michael",
            Image = "ship1.jpg",
            Route = "Manila to Cebu",
            Price = "₱1,500",
            Description = "Modern passenger vessel with comfortable cabins and dining areas."
        },
        new ShipViewModel
        {
            Id = 2,
            Name = "MV St. Joseph",
            Image = "ship2.jpg",
            Route = "Cebu to Davao",
            Price = "₱1,800",
            Description = "Spacious vessel designed for long-distance travel."
        },
        new ShipViewModel
        {
            Id = 3,
            Name = "MV St. Augustine",
            Image = "ship3.jpg",
            Route = "Manila to Bacolod",
            Price = "₱1,600",
            Description = "Reliable and fast ferry with premium seating."
        },
        new ShipViewModel
        {
            Id = 4,
            Name = "MV St. Leo",
            Image = "ship4.jpg",
            Route = "Cebu to Iloilo",
            Price = "₱1,200",
            Description = "Affordable and comfortable sea travel experience."
        },
        new ShipViewModel
        {
            Id = 5,
            Name = "MV St. John Paul",
            Image = "ship5.jpg",
            Route = "Manila to Palawan",
            Price = "₱2,200",
            Description = "Luxury vessel with full onboard amenities."
        },
        new ShipViewModel
        {
            Id = 6,
            Name = "MV St. Francis",
            Image = "ship6.jpg",
            Route = "Davao to Cebu",
            Price = "₱1,700",
            Description = "Efficient vessel for inter-island transport."
        },
        new ShipViewModel
        {
            Id = 7,
            Name = "MV St. Peter",
            Image = "ship7.jpg",
            Route = "Iloilo to Manila",
            Price = "₱1,900",
            Description = "Premium ferry with modern navigation systems."
        },
        new ShipViewModel
        {
            Id = 8,
            Name = "MV St. Benedict",
            Image = "ship8.jpg",
            Route = "Cebu to Palawan",
            Price = "₱2,000",
            Description = "High-capacity ship for long sea journeys."
        }
    };

            var ship = ships.FirstOrDefault(s => s.Id == id);

            if (ship == null)
                return NotFound();

            return View(ship);
        }
    }
}
