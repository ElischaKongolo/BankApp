using System;
using System.ComponentModel.DataAnnotations;
using BankApp.Core.Models;

namespace BankApp.Api.Models
{
    public class RegisterRequest
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public DateTime DateOfBirth { get; set; }
    }

    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class CreateAccountRequest
    {
        [Required]
        public AccountType AccountType { get; set; }
    }

    public class DepositRequest
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        public string Description { get; set; }
    }

    public class WithdrawRequest
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        public string Description { get; set; }
    }

    public class TransferRequestDto
    {
        [Required]
        public int FromAccountId { get; set; }

        [Required]
        [StringLength(20)]
        public string ToAccountNumber { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        public string Description { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; }
    }

    public class AuthResponse
    {
        public string Token { get; set; }
        public UserDto User { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
    }

    public class AccountDto
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; }
        public AccountType AccountType { get; set; }
        public decimal Balance { get; set; }
        public decimal AvailableBalance { get; set; }
        public AccountStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TransactionDto
    {
        public int Id { get; set; }
        public string TransactionReference { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Description { get; set; }
        public string CounterpartyName { get; set; }
        public string CounterpartyAccount { get; set; }
        public TransactionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // View Models for Web UI
    public class SignUpViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Surname is required")]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "Surname cannot exceed 50 characters")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Display(Name = "Phone Number")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20)]
        public string Number { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public class SendMoneyViewModel
    {
        [Required(ErrorMessage = "Please select an account")]
        [Display(Name = "From Account")]
        public int FromAccountId { get; set; }

        [Required(ErrorMessage = "Please enter the recipient's account number")]
        [Display(Name = "To Account Number")]
        [StringLength(20, ErrorMessage = "Account number cannot exceed 20 characters")]
        public string ToAccountNumber { get; set; }

        [Required(ErrorMessage = "Please enter an amount")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Display(Name = "Description")]
        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
        public string Description { get; set; }
    }

    public class CalculatorViewModel
    {
        [Required(ErrorMessage = "Please enter the principal amount")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Principal must be greater than 0")]
        [Display(Name = "Principal Amount")]
        public decimal Principal { get; set; }

        [Required(ErrorMessage = "Please enter the interest rate")]
        [Range(0, 100, ErrorMessage = "Interest rate must be between 0 and 100")]
        [Display(Name = "Annual Interest Rate (%)")]
        public decimal InterestRate { get; set; } = 5.0m;

        [Required(ErrorMessage = "Please enter the loan term")]
        [Range(1, 50, ErrorMessage = "Loan term must be between 1 and 50 years")]
        [Display(Name = "Loan Term (Years)")]
        public int LoanTermYears { get; set; } = 5;

        // Calculated fields
        public decimal MonthlyPayment { get; set; }
        public decimal TotalPayment { get; set; }
        public decimal TotalInterest { get; set; }
    }

    public class AddPaymentMethodViewModel
    {
        [Required]
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Card/Method Name")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Payment method type is required")]
        [Display(Name = "Payment Method Type")]
        public PaymentMethodType Type { get; set; }

        [Display(Name = "Card Number")]
        [StringLength(20, ErrorMessage = "Card number cannot exceed 20 characters")]
        public string CardNumber { get; set; }

        [Display(Name = "Account Number")]
        [StringLength(20, ErrorMessage = "Account number cannot exceed 20 characters")]
        public string AccountNumber { get; set; }

        [Display(Name = "Sort Code")]
        [StringLength(10, ErrorMessage = "Sort code cannot exceed 10 characters")]
        public string SortCode { get; set; }

        [Display(Name = "Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "CVV")]
        [StringLength(4, ErrorMessage = "CVV cannot exceed 4 characters")]
        public string CVV { get; set; }

        [Display(Name = "Set as Default")]
        public bool IsDefault { get; set; }

        [Display(Name = "Daily Limit")]
        [Range(0, double.MaxValue, ErrorMessage = "Daily limit must be 0 or greater")]
        public decimal DailyLimit { get; set; } = 5000m;

        [Display(Name = "Transaction Limit")]
        [Range(0, double.MaxValue, ErrorMessage = "Transaction limit must be 0 or greater")]
        public decimal TransactionLimit { get; set; } = 2500m;
    }
}
