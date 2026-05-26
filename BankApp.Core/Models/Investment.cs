using System;

namespace BankApp.Core.Models
{
    public class Investment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public decimal InitialAmount { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal ReturnPercentage { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime? MaturityDate { get; set; }
        public InvestmentStatus Status { get; set; }
        public string Description { get; set; }
        public decimal RiskLevel { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; }
        
        public int? AccountId { get; set; }
        public Account Account { get; set; }
    }

    public enum InvestmentStatus
    {
        Active = 0,
        Matured = 1,
        Closed = 2,
        Pending = 3
    }
}
