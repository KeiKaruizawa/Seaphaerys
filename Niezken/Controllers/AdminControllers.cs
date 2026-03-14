using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Niezken.Data;
using Niezken.Models;
namespace Niezken.Controllers
{
    [Authorize(Roles = "Admin")]

    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

            public async Task<IActionResult> AdminDashboard()
            {

            ViewBag.TotalUsers = await _context.Users
                 .Where(u => u.Role == "Passenger")
                 .CountAsync();

            ViewBag.ActiveUsers = await _context.Users
                .Where(u => u.Role == "Passenger" && u.IsActive)
                .CountAsync();

            ViewBag.InactiveUsers = await _context.Users
                .Where(u => u.Role == "Passenger" && !u.IsActive)
                .CountAsync();

            ViewBag.TotalBookings = await _context.Bookings.CountAsync();

            ViewBag.TotalShips = await _context.Ships.CountAsync();


            // BOOKINGS PER MONTH

            var bookings = await _context.Bookings.ToListAsync();

            ViewBag.MonthLabels = new[]{
            "Jan","Feb","Mar","Apr","May","Jun",
            "Jul","Aug","Sep","Oct","Nov","Dec"
            };

            ViewBag.MonthBookings = bookings
            .GroupBy(b => b.TravelDate.Month)
            .OrderBy(g => g.Key)
            .Select(g => g.Count())
            .ToArray();


            // USERS PER MONTH

            var users = await _context.Users.ToListAsync();

            ViewBag.UserMonthLabels = ViewBag.MonthLabels;

            ViewBag.UsersPerMonth = users
            .GroupBy(u => u.Id)
            .Select(g => g.Count())
            .ToArray();


            // POPULAR ROUTES

            ViewBag.PopularRoutes = await _context.Bookings
            .GroupBy(b => b.Route)
            .Select(g => new
            {
            Route = g.Key,
            Count = g.Count()
            })
            .OrderByDescending(r => r.Count)
            .Take(5)
            .ToListAsync();


            // RECENT BOOKINGS

            var recentBookings = await _context.Bookings
            .Include(b => b.User)
            .OrderByDescending(b => b.TravelDate)
            .Take(5)
            .ToListAsync();

            return View(recentBookings);

            }
        
        public async Task<IActionResult> Users(string query)  // Action method to display all passenger users with optional search
        {
            // Start a query to get users whose role is "Passenger"
            // This prevents admins from appearing in the user list
            var users = _context.Users
                .Where(u => u.Role == "Passenger");

            // Check if the admin entered something in the search box
            if (!string.IsNullOrEmpty(query))
            {
                // Convert the search text to lowercase
                // so the search becomes case-insensitive
                query = query.ToLower();

                // Filter the users based on email, first name, or last name
                users = users.Where(u =>
                    u.Email.ToLower().Contains(query) ||
                    u.FirstName.ToLower().Contains(query) ||
                    u.LastName.ToLower().Contains(query));
            }

            // Store the search query in ViewBag
            // This allows the view to keep the search text in the textbox
            // and highlight matching rows
            ViewBag.SearchQuery = query; //Stores the search text so the view can remember what the admin typed.    

            // Execute the query and convert results into a list
            // Then send the list of users to the View
            return View(await users.ToListAsync());
        }
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user != null && user.Role == "Passenger")
            {
                user.IsActive = !user.IsActive;

                _context.ActivityLogs.Add(new ActivityLog
                {
                    UserEmail = User.Identity.Name,
                    Action = $"Admin changed status of {user.Email}"
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Users");
        }


        // Action method for deleting a user
        public async Task<IActionResult> DeleteUser(int id)
        {
            // Find the user in the database using the given ID
            var user = await _context.Users.FindAsync(id);

            // Get the email/username of the currently logged-in admin
            var currentAdmin = User.Identity.Name;

            // Check conditions before deleting:
            // 1. User exists
            // 2. User role is Passenger (prevents deleting other admins)
            // 3. The admin is not deleting their own account
            if (user != null &&
                user.Role == "Passenger" &&
                user.Email != currentAdmin)
            {
                // Remove the user from the Users table
                _context.Users.Remove(user);

                // Add a log entry to track admin activity
                _context.ActivityLogs.Add(new ActivityLog
                {
                    // Store the admin who performed the action
                    UserEmail = currentAdmin,

                    // Record what action was done
                    Action = $"Admin deleted passenger: {user.Email}"
                });

                // Save the changes to the database
                await _context.SaveChangesAsync();
            }

            // After deletion, redirect back to the Users list page
            return RedirectToAction("Users");
        }
        public async Task<IActionResult> ActivityLogs(string search)
        {
            var logs = _context.ActivityLogs.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                logs = logs.Where(l =>
                    l.UserEmail.Contains(search) ||
                    l.Action.Contains(search));
            }

            return View(await logs
                .OrderByDescending(l => l.Date)
                .ToListAsync());
        }
        //public async Task<IActionResult> Reports()
        //{
        //    var users = await _context.Users.CountAsync();
        //    var bookings = await _context.Bookings.CountAsync();

        //    ViewBag.Users = users;
        //    ViewBag.Bookings = bookings;

        //    return View();
        //}
        // Controller
   

        public async Task<IActionResult> MyAccount()
        {
            var email = User.Identity.Name;

            var admin = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            return View(admin);
        }
        public async Task<IActionResult> Accommodation()
        {
            var ships = await _context.Ships.ToListAsync();
            return View(ships);
        }
        //public IActionResult CreateShip()
        //{
        //    return View();
        //}



        //[HttpPost]
        //public async Task<IActionResult> CreateShip(Ship ship)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Ships.Add(ship);

        //        _context.ActivityLogs.Add(new ActivityLog
        //        {
        //            UserEmail = User.Identity.Name,
        //            Action = $"Admin added new ship accommodation: {ship.Name}"
        //        });

        //        await _context.SaveChangesAsync();

        //        return RedirectToAction("Accommodations");
        //    }

        //    return View(ship);
        //}
        //public async Task<IActionResult> EditShip(int id)
        //{
        //    var ship = await _context.Ships.FindAsync(id);
        //    return View(ship);
        //}

        //[HttpPost]
        //public async Task<IActionResult> EditShip(Ship ship)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Ships.Update(ship);

        //        _context.ActivityLogs.Add(new ActivityLog
        //        {
        //            UserEmail = User.Identity.Name,
        //            Action = $"Admin updated ship: {ship.Name}"
        //        });

        //        await _context.SaveChangesAsync();

        //        return RedirectToAction("Accommodations");
        //    }

        //    return View(ship);
        //}
        //public async Task<IActionResult> DeleteShip(int id)
        //{
        //    var ship = await _context.Ships.FindAsync(id);

        //    if (ship != null)
        //    {
        //        _context.Ships.Remove(ship);

        //        _context.ActivityLogs.Add(new ActivityLog
        //        {
        //            UserEmail = User.Identity.Name,
        //            Action = $"Admin deleted ship: {ship.Name}"
        //        });

        //        await _context.SaveChangesAsync();
        //    }

        //    return RedirectToAction("Accommodations");
        //}
        //public async Task<IActionResult> SearchShip(string query)
        //{
        //    var ships = await _context.Ships
        //        .Where(s => s.Name.Contains(query) || s.Route.Contains(query))
        //        .ToListAsync();

        //    return View("Accommodations", ships);
        //}
        //public async Task<IActionResult> Bookings()
        //{
        //    var bookings = await _context.Bookings
        //        .Include(b => b.User)
        //        .OrderByDescending(b => b.TravelDate)
        //        .ToListAsync();

        //    return View(bookings);
        //}
        //public async Task<IActionResult> ApproveBooking(int id)
        //{
        //    var booking = await _context.Bookings.FindAsync(id);

        //    if (booking != null)
        //    {
        //        booking.Status = "Approved";
        //        await _context.SaveChangesAsync();
        //    }

        //    return RedirectToAction("Dashboard");
        //}

     
        public async Task<IActionResult> UserBookings(int id)    // Action method that shows all bookings of a specific user
        {
            // Query the Bookings table and get all bookings
            // where the UserId matches the provided id
            var bookings = await _context.Bookings
                .Where(b => b.UserId == id)   // Filter bookings for this specific user
                .Include(b => b.User)         // Load related User data (navigation property)
                .ToListAsync();               // Execute query and convert result to a list

            // Send the bookings list to the View
            return View(bookings);
        }

        //public IActionResult Create()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public async Task<IActionResult> Create(User user)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        user.DateCreated = DateTime.Now;

        //        _context.Users.Add(user);

        //        _context.ActivityLogs.Add(new ActivityLog
        //        {
        //            UserEmail = User.Identity.Name,
        //            Action = $"Admin created user: {user.Email}"
        //        });

        //        await _context.SaveChangesAsync();

        //        return RedirectToAction("Users");
        //    }

        //    return View(user);
        //}





        // GET: Admin/Edit/5
        // Displays the edit page for a specific user
        public async Task<IActionResult> Edit(int id)
        {
            // Find the user in the database using the provided ID
            var user = await _context.Users.FindAsync(id);

            // If the user does not exist, return a 404 error page
            if (user == null)
                return NotFound();

            // Send the user data to the Edit View
            return View(user);
        }


        
        [HttpPost]// POST: Admin/Edit
                  // Handles the form submission when the admin updates a user
        public async Task<IActionResult> Edit(User model)
        {
            // Retrieve the existing user from the database using the ID from the form
            var user = await _context.Users.FindAsync(model.Id);

            // Check if the user exists
            if (user != null)
            {
                // Update user information with the values from the form
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;

                // Update the user status (Active or Inactive)
                user.IsActive = model.IsActive;

                // Record this action in the ActivityLogs table
                _context.ActivityLogs.Add(new ActivityLog
                {
                    // Email of the admin performing the action
                    UserEmail = User.Identity.Name,

                    // Description of the action performed
                    Action = $"Admin updated user status: {user.Email} (Active: {user.IsActive})"
                });

                // Save all changes to the database
                await _context.SaveChangesAsync();
            }

            // Redirect back to the Users list after editing
            return RedirectToAction("Users");
        }

        // GET: Admin/Create
        // Displays the Add User form
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost] // Indicates this method handles POST requests
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            // Check if the submitted form data is valid based on model validation
            if (ModelState.IsValid)
            {
                // Password hasher used to securely hash the user's password
                var hasher = new PasswordHasher<User>();

                // Create a new User object and assign values from the form model
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,

                    // Default role assigned when admin creates a user
                    Role = "Passenger",

                    // User account is active by default
                    IsActive = true,

                    // Record the date when the user was created
                    DateCreated = DateTime.Now
                };

                // Hash the password before saving to the database
                user.PasswordHash = hasher.HashPassword(user, model.Password);

                // Add the new user to the Users table
                _context.Users.Add(user);

                // Log the admin activity in the ActivityLogs table
                _context.ActivityLogs.Add(new ActivityLog
                {
                    UserEmail = User.Identity.Name, // Admin who performed the action
                    Action = $"Admin created new user: {user.Email}"
                });

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Redirect back to the Users list page after successful creation
                return RedirectToAction("Users");
            }

            // If validation fails, return the same view with entered data
            return View(model);
        }

        public async Task<IActionResult> ClearLogs()
        {
            _context.ActivityLogs.RemoveRange(_context.ActivityLogs);

            await _context.SaveChangesAsync();

            return RedirectToAction("ActivityLogs");
        }


    }
}