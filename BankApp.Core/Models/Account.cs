using System;
using System.Collections.Generic;

namespace BankApp.Core.Models
{
    public enum AccountType
    {
        Checking,
        Savings,
        Business
    }

    public enum AccountStatus
    {
        Active,
        Inactive,
        Frozen,
        Closed
    }

    public class Account
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; }
        public AccountType AccountType { get; set; }
        public decimal Balance { get; set; }
        public decimal AvailableBalance { get; set; }
        public AccountStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public decimal DailyTransferLimit { get; set; } = 10000m;
        public decimal MonthlyTransferLimit { get; set; } = 100000m;
        
        public int UserId { get; set; }
        public User User { get; set; }
        
        public ICollection<Transaction> Transactions { get; set; }
        public ICollection<PaymentMethod> PaymentMethods { get; set; }
    }
}
