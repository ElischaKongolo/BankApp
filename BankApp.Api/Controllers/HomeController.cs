using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BankApp.Core.Interfaces;
using BankApp.Core.Models;
using BankApp.Api.Models;
using BankApp.Infrastructure.Data;

namespace BankApp.Api.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAccountService _accountService;
        private readonly ITransactionService _transactionService;
        private readonly ILogger<HomeController> _logger;
        private readonly IInvestmentService _investmentService;
        private readonly IThemeService _themeService;
        private readonly IEWalletService _eWalletService;

        public HomeController(IUserService userService, IAccountService accountService, 
            ITransactionService transactionService, ILogger<HomeController> logger, 
            IInvestmentService investmentService, IThemeService themeService, IEWalletService eWalletService)
        {
            _userService = userService;
            _accountService = accountService;
            _transactionService = transactionService;
            _logger = logger;
            _investmentService = investmentService;
            _themeService = themeService;
            _eWalletService = eWalletService;
        }

        // Helper method to get user ID from session
        private int GetUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 1; // Default to 1 if not logged in
        }

        // GET: / or /Home/Index
        public async Task<IActionResult> Index()
        {
            // Seed investment news if empty
            await SeedInvestmentNews();
            return View();
        }

        // GET: /Home/SignUp
        public IActionResult SignUp()
        {
            return View();
        }

        // POST: /Home/SignUp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = new User
                {
                    FirstName = model.Name,
                    LastName = model.Surname,
                    Email = model.Email,
                    PhoneNumber = model.Number,
                    Address = model.Address,
                    DateOfBirth = DateTime.UtcNow.AddYears(-25) // Default value, can be updated later
                };

                var createdUser = await _userService.RegisterAsync(user, model.Password);
                
                _logger.LogInformation($"User registered successfully: {createdUser.Email}");
                
                // Create a checking account with R500 welcome bonus for new users
                try
                {
                    var welcomeAccount = await _accountService.CreateAccountAsync(createdUser.Id, AccountType.Checking);
                    
                    // Add R500 welcome bonus using database context directly
                    using (var scope = new ServiceCollection()
                        .AddDbContext<BankDbContext>(options => 
                            options.UseSqlite("Data Source=BankApp.db"))
                        .BuildServiceProvider()
                        .CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<BankDbContext>();
                        var account = await context.Accounts.FindAsync(welcomeAccount.Id);
                        if (account != null)
                        {
                            account.Balance = 500m; // R500 welcome bonus
                            account.AvailableBalance = 500m;
                            await context.SaveChangesAsync();
                        }
                    }
                    
                    _logger.LogInformation($"Welcome bonus of R500 added to account {welcomeAccount.AccountNumber} for user {createdUser.Email}");
                    
                    TempData["SuccessMessage"] = "Registration successful! We've created a Checking Account for you with a R500 welcome bonus. Please log in to access your account.";
                }
                catch (Exception bonusEx)
                {
                    _logger.LogError(bonusEx, "Error adding welcome bonus");
                    // Don't fail registration if bonus fails
                    TempData["SuccessMessage"] = "Registration successful! Please log in with your credentials.";
                }
                
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("Email", ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during user registration: {ex.Message}");
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View(model);
            }
        }

        // GET: /Home/Login
        public IActionResult Login()
        {
            // Check for success message from registration
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }
            return View();
        }

        // POST: /Home/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userService.AuthenticateAsync(model.Email, model.Password);
                
                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid email or password");
                    return View(model);
                }

                if (!user.IsActive)
                {
                    ModelState.AddModelError("", "Your account has been disabled");
                    return View(model);
                }

                _logger.LogInformation($"User logged in successfully: {user.Email}");
                
                // Store user info in session for the web UI
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserName", user.FirstName);

                // Sign in the user with cookie authentication for MVC protected pages
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.FirstName)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                
                // Show success message and redirect to dashboard
                TempData["SuccessMessage"] = $"Welcome back, {user.FirstName}! You have successfully logged in.";
                
                // Redirect to dashboard
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                ModelState.AddModelError("", "An error occurred during login. Please try again.");
                return View(model);
            }
        }

        // GET: /Home/Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        // GET: /Home/Error
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }

        // GET: /Home/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index");
        }

        // ==================== ACCOUNT CREATION ====================

        // GET: /Home/CreateAccount
        public IActionResult CreateAccount()
        {
            return View();
        }

        // POST: /Home/CreateCheckingAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCheckingAccount()
        {
            try
            {
                var userId = GetUserId();
                var account = await _accountService.CreateAccountAsync(userId, AccountType.Checking);
                TempData["SuccessMessage"] = $"Your Checking Account has been created successfully! Account Number: {account.AccountNumber}";
                return RedirectToAction("Accounts");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to create account: {ex.Message}";
                return RedirectToAction("Accounts");
            }
        }

        // POST: /Home/CreateSavingsAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSavingsAccount()
        {
            try
            {
                var userId = GetUserId();
                var account = await _accountService.CreateAccountAsync(userId, AccountType.Savings);
                TempData["SuccessMessage"] = $"Your Savings Account has been created successfully! Account Number: {account.AccountNumber}";
                return RedirectToAction("Savings");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to create account: {ex.Message}";
                return RedirectToAction("Savings");
            }
        }

        // ==================== ACCOUNT PAGES ====================

        // GET: /Home/Dashboard - Main Account Page
        public async Task<IActionResult> Dashboard()
        {
            var userId = GetUserId();
            var accounts = await _accountService.GetUserAccountsAsync(userId);
            var transactions = await _transactionService.GetUserTransactionsAsync(userId, 1, 5);
            var userTheme = await _themeService.GetUserThemeAsync(userId);
            var eWalletBalance = await _eWalletService.GetTotalWalletBalanceAsync(userId);
            
            ViewBag.TotalBalance = accounts.Sum(a => a.Balance);
            ViewBag.AccountCount = accounts.Count();
            ViewBag.Accounts = accounts;
            ViewBag.RecentTransactions = transactions;
            ViewBag.UserTheme = userTheme;
            ViewBag.EWalletBalance = eWalletBalance;
            
            return View();
        }

        // GET: /Home/DownloadReport - Download CSV report
        public async Task<IActionResult> DownloadReport()
        {
            var userId = GetUserId();
            var transactions = await _transactionService.GetUserTransactionsAsync(userId, 1, 1000);
            
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Date,Type,Description,Amount,Balance After");
            
            foreach (var txn in transactions)
            {
                builder.AppendLine($"{txn.CreatedAt:yyyy-MM-dd HH:mm},{txn.Type},\"{txn.Description}\",{txn.Amount},{txn.BalanceAfter}");
            }
            
            return File(System.Text.Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", $"BankReport_{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        // GET: /Home/Accounts - View all accounts
        public async Task<IActionResult> Accounts()
        {
            var userId = GetUserId();
            var accounts = await _accountService.GetUserAccountsAsync(userId);
            return View(accounts);
        }

        // GET: /Home/Savings - Savings Account Page
        public async Task<IActionResult> Savings()
        {
            var userId = GetUserId();
            var accounts = await _accountService.GetUserAccountsAsync(userId);
            var savingsAccounts = accounts.Where(a => a.AccountType == AccountType.Savings).ToList();
            
            // Calculate interest projection (example: 4% annual interest)
            foreach (var account in savingsAccounts)
            {
                ViewBag.ProjectedInterest = account.Balance * 0.04m;
                ViewBag.MonthlyInterest = account.Balance * 0.04m / 12;
            }
            
            return View(savingsAccounts);
        }

        // GET: /Home/Transactions - Transaction/Send Money Page
        public async Task<IActionResult> Transactions()
        {
            var userId = GetUserId();
            var accounts = await _accountService.GetUserAccountsAsync(userId);
            ViewBag.Accounts = accounts;
            return View();
        }

        // POST: /Home/SendMoney - Process money transfer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMoney(SendMoneyViewModel model)
        {
            var userId = GetUserId();
            
            if (!ModelState.IsValid)
            {
                var accounts = await _accountService.GetUserAccountsAsync(userId);
                ViewBag.Accounts = accounts;
                return View("Transactions", model);
            }

            try
            {
                var transaction = await _transactionService.TransferAsync(
                    model.FromAccountId,
                    model.ToAccountNumber,
                    model.Amount,
                    model.Description);

                TempData["SuccessMessage"] = $"Successfully sent {model.Amount:C} to account {model.ToAccountNumber}";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Transfer failed: {ex.Message}");
                var accounts = await _accountService.GetUserAccountsAsync(userId);
                ViewBag.Accounts = accounts;
                return View("Transactions", model);
            }
        }

        // GET: /Home/Analytics - Monthly Analysis with Charts
        public async Task<IActionResult> Analytics()
        {
            var userId = GetUserId();
            var transactions = await _transactionService.GetUserTransactionsAsync(userId, 1, 100);
            
            // Group by month for chart data
            var monthlyData = transactions
                .GroupBy(t => t.CreatedAt.ToString("yyyy-MM"))
                .Select(g => new {
                    Month = g.Key,
                    Deposits = g.Where(t => t.Type == TransactionType.Deposit || t.Type == TransactionType.Transfer).Sum(t => t.Amount),
                    Withdrawals = g.Where(t => t.Type == TransactionType.Withdrawal).Sum(t => t.Amount)
                })
                .OrderBy(x => x.Month)
                .Take(12)
                .ToList();

            ViewBag.Months = monthlyData.Select(x => x.Month).ToList();
            ViewBag.Deposits = monthlyData.Select(x => x.Deposits).ToList();
            ViewBag.Withdrawals = monthlyData.Select(x => x.Withdrawals).ToList();
            ViewBag.NetSavings = monthlyData.Select(x => x.Deposits - x.Withdrawals).ToList();
            
            return View();
        }

        // GET: /Home/Calculator - Transaction Calculator
        public IActionResult Calculator()
        {
            return View();
        }

        // POST: /Home/CalculateLoan - Loan calculator
        [HttpPost]
        public IActionResult CalculateLoan(CalculatorViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Simple loan calculation
                decimal monthlyRate = model.InterestRate / 100 / 12;
                int numberOfPayments = model.LoanTermYears * 12;
                
                if (monthlyRate > 0)
                {
                    model.MonthlyPayment = model.Principal * (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), numberOfPayments)) 
                        / (decimal)(Math.Pow((double)(1 + monthlyRate), numberOfPayments) - 1);
                }
                else
                {
                    model.MonthlyPayment = model.Principal / numberOfPayments;
                }
                
                model.TotalPayment = model.MonthlyPayment * numberOfPayments;
                model.TotalInterest = model.TotalPayment - model.Principal;
            }
            
            return View("Calculator", model);
        }

        // ==================== ACCOUNT DETAILS & PAYMENT METHODS ====================

        // GET: /Home/AccountDetails/5 - Detailed account view with payment methods
        public async Task<IActionResult> AccountDetails(int id)
        {
            var userId = GetUserId();
            var accounts = await _accountService.GetUserAccountsAsync(userId);
            var account = accounts.FirstOrDefault(a => a.Id == id);
            
            if (account == null)
            {
                TempData["ErrorMessage"] = "Account not found or access denied.";
                return RedirectToAction("Accounts");
            }
            
            // Load payment methods for this account
            var paymentMethods = await GetPaymentMethodsForAccount(id);
            ViewBag.PaymentMethods = paymentMethods;
            
            return View(account);
        }

        // POST: /Home/AddPaymentMethod - Add a new payment method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPaymentMethod(AddPaymentMethodViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill in all required fields.";
                return RedirectToAction("AccountDetails", new { id = model.AccountId });
            }

            try
            {
                var paymentMethod = new PaymentMethod
                {
                    Name = model.Name,
                    Type = model.Type,
                    CardNumber = model.CardNumber,
                    AccountNumber = model.AccountNumber,
                    SortCode = model.SortCode,
                    ExpiryDate = model.ExpiryDate,
                    CVV = model.CVV,
                    Status = PaymentMethodStatus.Active,
                    IsDefault = model.IsDefault,
                    DailyLimit = model.DailyLimit,
                    TransactionLimit = model.TransactionLimit,
                    CreatedAt = DateTime.UtcNow,
                    AccountId = model.AccountId
                };

                // Save to database
                using (var scope = new ServiceCollection()
                    .AddDbContext<BankDbContext>(options => 
                        options.UseSqlite("Data Source=BankApp.db"))
                    .BuildServiceProvider()
                    .CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<BankDbContext>();
                    context.PaymentMethods.Add(paymentMethod);
                    await context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = $"{model.Type} payment method added successfully!";
                return RedirectToAction("AccountDetails", new { id = model.AccountId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding payment method");
                TempData["ErrorMessage"] = $"Failed to add payment method: {ex.Message}";
                return RedirectToAction("AccountDetails", new { id = model.AccountId });
            }
        }

        // POST: /Home/RemovePaymentMethod - Remove a payment method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePaymentMethod(int paymentMethodId, int accountId)
        {
            try
            {
                using (var scope = new ServiceCollection()
                    .AddDbContext<BankDbContext>(options => 
                        options.UseSqlite("Data Source=BankApp.db"))
                    .BuildServiceProvider()
                    .CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<BankDbContext>();
                    var paymentMethod = await context.PaymentMethods.FindAsync(paymentMethodId);
                    
                    if (paymentMethod != null && paymentMethod.AccountId == accountId)
                    {
                        context.PaymentMethods.Remove(paymentMethod);
                        await context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Payment method removed successfully.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Payment method not found.";
                    }
                }
                
                return RedirectToAction("AccountDetails", new { id = accountId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing payment method");
                TempData["ErrorMessage"] = $"Failed to remove payment method: {ex.Message}";
                return RedirectToAction("AccountDetails", new { id = accountId });
            }
        }

        // POST: /Home/SetDefaultPaymentMethod
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDefaultPaymentMethod(int paymentMethodId, int accountId)
        {
            try
            {
                using (var scope = new ServiceCollection()
                    .AddDbContext<BankDbContext>(options => 
                        options.UseSqlite("Data Source=BankApp.db"))
                    .BuildServiceProvider()
                    .CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<BankDbContext>();
                    
                    // Remove default from all other payment methods for this account
                    var existingMethods = context.PaymentMethods.Where(pm => pm.AccountId == accountId);
                    foreach (var pm in existingMethods)
                    {
                        pm.IsDefault = false;
                    }
                    
                    // Set the selected one as default
                    var selectedMethod = await context.PaymentMethods.FindAsync(paymentMethodId);
                    if (selectedMethod != null)
                    {
                        selectedMethod.IsDefault = true;
                        await context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Default payment method updated.";
                    }
                }
                
                return RedirectToAction("AccountDetails", new { id = accountId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default payment method");
                TempData["ErrorMessage"] = "Failed to update default payment method.";
                return RedirectToAction("AccountDetails", new { id = accountId });
            }
        }

        private async Task<List<PaymentMethod>> GetPaymentMethodsForAccount(int accountId)
        {
            using (var scope = new ServiceCollection()
                .AddDbContext<BankDbContext>(options => 
                    options.UseSqlite("Data Source=BankApp.db"))
                .BuildServiceProvider()
                .CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<BankDbContext>();
                return await context.PaymentMethods
                    .Where(pm => pm.AccountId == accountId)
                    .OrderByDescending(pm => pm.IsDefault)
                    .ThenBy(pm => pm.CreatedAt)
                    .ToListAsync();
            }
        }

        // GET: /Home/Settings - User settings page
        public async Task<IActionResult> Settings()
        {
            var userId = GetUserId();
            var user = await _userService.GetByIdAsync(userId);
            var currentTheme = await _themeService.GetUserThemeAsync(userId);
            var availableThemes = await _themeService.GetAvailableThemesAsync();
            
            ViewBag.CurrentTheme = currentTheme;
            ViewBag.AvailableThemes = availableThemes;
            
            return View(user);
        }

        // POST: /Home/SetTheme - Set user theme
        [HttpPost]
        public async Task<IActionResult> SetTheme([FromBody] SetThemeViewModel model)
        {
            try
            {
                var userId = GetUserId();
                await _themeService.SetUserThemeAsync(userId, model.ThemeName);
                
                return Json(new { success = true, message = "Theme updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting theme");
                return Json(new { success = false, message = "Failed to update theme" });
            }
        }

        // GET: /Home/HelpCenter
        public IActionResult HelpCenter()
        {
            return View();
        }

        // GET: /Home/ContactSupport
        public IActionResult ContactSupport()
        {
            return View();
        }

        // POST: /Home/SubmitSupport
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitSupport(string Subject, string Message)
        {
            _logger.LogInformation($"Support ticket submitted: {Subject}");
            TempData["SupportSuccess"] = "Your message has been sent successfully. Our team will contact you shortly.";
            return RedirectToAction("ContactSupport");
        }

        // ==================== NOTIFICATIONS ====================

        // GET: /Home/Notifications - View all notifications
        public async Task<IActionResult> Notifications()
        {
            var userId = GetUserId();
            var notifications = await GetUserNotificationsAsync(userId);
            ViewBag.UnreadCount = notifications.Count(n => n.Status == NotificationStatus.Unread);
            return View(notifications);
        }

        // POST: /Home/MarkNotificationAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            try
            {
                var userId = GetUserId();
                using (var scope = new ServiceCollection()
                    .AddDbContext<BankDbContext>(options => 
                        options.UseSqlite("Data Source=BankApp.db"))
                    .BuildServiceProvider()
                    .CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<BankDbContext>();
                    var notification = await context.Notifications
                        .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
                    
                    if (notification != null)
                    {
                        notification.Status = NotificationStatus.Read;
                        notification.ReadAt = DateTime.UtcNow;
                        await context.SaveChangesAsync();
                    }
                }
                return RedirectToAction("Notifications");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return RedirectToAction("Notifications");
            }
        }

        // POST: /Home/MarkAllNotificationsAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllNotificationsAsRead()
        {
            try
            {
                var userId = GetUserId();
                using (var scope = new ServiceCollection()
                    .AddDbContext<BankDbContext>(options => 
                        options.UseSqlite("Data Source=BankApp.db"))
                    .BuildServiceProvider()
                    .CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<BankDbContext>();
                    var unreadNotifications = await context.Notifications
                        .Where(n => n.UserId == userId && n.Status == NotificationStatus.Unread)
                        .ToListAsync();
                    
                    foreach (var notification in unreadNotifications)
                    {
                        notification.Status = NotificationStatus.Read;
                        notification.ReadAt = DateTime.UtcNow;
                    }
                    
                    await context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "All notifications marked as read.";
                }
                return RedirectToAction("Notifications");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return RedirectToAction("Notifications");
            }
        }

        // POST: /Home/DeleteNotification
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNotification(int notificationId)
        {
            try
            {
                var userId = GetUserId();
                using (var scope = new ServiceCollection()
                    .AddDbContext<BankDbContext>(options => 
                        options.UseSqlite("Data Source=BankApp.db"))
                    .BuildServiceProvider()
                    .CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<BankDbContext>();
                    var notification = await context.Notifications
                        .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
                    
                    if (notification != null)
                    {
                        context.Notifications.Remove(notification);
                        await context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Notification deleted.";
                    }
                }
                return RedirectToAction("Notifications");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification");
                return RedirectToAction("Notifications");
            }
        }

        // ==================== ACCOUNT DELETION ====================

        // POST: /Home/DeleteAccount - Delete an account
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount(int accountId)
        {
            try
            {
                var userId = GetUserId();
                var accounts = await _accountService.GetUserAccountsAsync(userId);
                var account = accounts.FirstOrDefault(a => a.Id == accountId);
                
                if (account == null)
                {
                    TempData["ErrorMessage"] = "Account not found or access denied.";
                    return RedirectToAction("Accounts");
                }

                // Check if account has balance
                if (account.Balance > 0)
                {
                    TempData["ErrorMessage"] = "Cannot delete account with remaining balance. Please transfer or withdraw funds first.";
                    return RedirectToAction("AccountDetails", new { id = accountId });
                }

                // Delete the account
                using (var scope = new ServiceCollection()
                    .AddDbContext<BankDbContext>(options => 
                        options.UseSqlite("Data Source=BankApp.db"))
                    .BuildServiceProvider()
                    .CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<BankDbContext>();
                    var accountToDelete = await context.Accounts.FindAsync(accountId);
                    
                    if (accountToDelete != null)
                    {
                        context.Accounts.Remove(accountToDelete);
                        await context.SaveChangesAsync();
                        
                        // Create notification about account deletion
                        await CreateNotificationAsync(userId, new Notification
                        {
                            Title = "Account Deleted",
                            Message = $"Your {account.AccountType} account ({account.AccountNumber}) has been successfully deleted.",
                            Type = NotificationType.Account,
                            Status = NotificationStatus.Unread,
                            CreatedAt = DateTime.UtcNow,
                            IsImportant = true
                        });
                        
                        TempData["SuccessMessage"] = $"{account.AccountType} account deleted successfully.";
                        _logger.LogInformation($"Account {accountId} deleted by user {userId}");
                    }
                }
                
                return RedirectToAction("Accounts");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting account {accountId}");
                TempData["ErrorMessage"] = $"Failed to delete account: {ex.Message}";
                return RedirectToAction("Accounts");
            }
        }

        // Helper method to get user notifications
        private async Task<List<Notification>> GetUserNotificationsAsync(int userId)
        {
            using (var scope = new ServiceCollection()
                .AddDbContext<BankDbContext>(options => 
                    options.UseSqlite("Data Source=BankApp.db"))
                .BuildServiceProvider()
                .CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<BankDbContext>();
                return await context.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();
            }
        }

        // Helper method to create a notification
        private async Task CreateNotificationAsync(int? userId, Notification notification)
        {
            if (userId == null) return;
            
            using (var scope = new ServiceCollection()
                .AddDbContext<BankDbContext>(options => 
                    options.UseSqlite("Data Source=BankApp.db"))
                .BuildServiceProvider()
                .CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<BankDbContext>();
                notification.UserId = userId;
                context.Notifications.Add(notification);
                await context.SaveChangesAsync();
            }
        }

        private async Task SeedInvestmentNews()
        {
            using (var scope = new ServiceCollection()
                .AddDbContext<BankDbContext>(options => 
                    options.UseSqlite("Data Source=BankApp.db"))
                .BuildServiceProvider()
                .CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<BankDbContext>();
                
                if (!await context.InvestmentNews.AnyAsync())
                {
                    var newsItems = new List<InvestmentNews>
                    {
                        new InvestmentNews
                        {
                            Title = "Tech Stocks Rally on AI Optimism",
                            Content = "Major technology stocks saw significant gains today as investors expressed renewed optimism about artificial intelligence developments. Companies in AI sector reported better-than-expected earnings, driving the broader market higher.",
                            Summary = "Tech stocks surge as AI optimism drives market gains with better-than-expected earnings reports.",
                            Category = "StockUpdates",
                            Source = "Financial Times",
                            PublishedAt = DateTime.UtcNow.AddHours(-2),
                            IsFeatured = true,
                            ImageUrl = "https://picsum.photos/seed/tech-stocks/400/200.jpg",
                            Tags = "tech, AI, stocks, earnings"
                        },
                        new InvestmentNews
                        {
                            Title = "Federal Reserve Signals Rate Pause",
                            Content = "The Federal Reserve indicated it may pause interest rate hikes in the coming months, citing cooling inflation data. Markets responded positively to the news, with bond yields falling and equity markets rising.",
                            Summary = "Fed signals potential pause in rate hikes as inflation shows signs of cooling.",
                            Category = "EconomicIndicators",
                            Source = "Reuters",
                            PublishedAt = DateTime.UtcNow.AddHours(-4),
                            IsFeatured = true,
                            ImageUrl = "https://picsum.photos/seed/fed-reserve/400/200.jpg",
                            Tags = "federal reserve, interest rates, inflation"
                        },
                        new InvestmentNews
                        {
                            Title = "Green Energy Investments Surge",
                            Content = "Renewable energy companies are seeing unprecedented investment levels as governments worldwide push for carbon neutrality. Solar and wind energy stocks have outperformed the broader market by 15% this quarter.",
                            Summary = "Renewable energy sector attracts record investments as climate policies drive growth.",
                            Category = "InvestmentTips",
                            Source = "Bloomberg",
                            PublishedAt = DateTime.UtcNow.AddHours(-6),
                            IsFeatured = true,
                            ImageUrl = "https://picsum.photos/seed/green-energy/400/200.jpg",
                            Tags = "renewable energy, solar, wind, investments"
                        },
                        new InvestmentNews
                        {
                            Title = "Cryptocurrency Market Update",
                            Content = "Bitcoin and major cryptocurrencies showed mixed performance today. While Bitcoin remained relatively stable, altcoins experienced significant volatility following regulatory announcements from major economies.",
                            Summary = "Crypto markets show mixed performance amid regulatory developments and volatility.",
                            Category = "MarketNews",
                            Source = "CoinDesk",
                            PublishedAt = DateTime.UtcNow.AddHours(-8),
                            IsFeatured = false,
                            ImageUrl = "https://picsum.photos/seed/cryptocurrency/400/200.jpg",
                            Tags = "bitcoin, cryptocurrency, regulation, volatility"
                        },
                        new InvestmentNews
                        {
                            Title = "Real Estate Investment Trusts (REITs) Outlook",
                            Content = "REITs are showing signs of recovery as commercial real estate markets stabilize. Office REITs continue to face challenges, while industrial and residential REITs show strong performance.",
                            Summary = "REIT sector shows mixed recovery with industrial and residential properties leading.",
                            Category = "InvestmentTips",
                            Source = "Wall Street Journal",
                            PublishedAt = DateTime.UtcNow.AddHours(-12),
                            IsFeatured = false,
                            ImageUrl = "https://picsum.photos/seed/real-estate/400/200.jpg",
                            Tags = "REITs, real estate, commercial, residential"
                        }
                    };

                    await context.InvestmentNews.AddRangeAsync(newsItems);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
