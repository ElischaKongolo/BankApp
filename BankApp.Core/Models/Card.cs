using System;

namespace BankApp.Core.Models
{
    public enum CardType
    {
        Debit,
        Credit,
        Virtual
    }

    public enum CardStatus
    {
        Active,
        Frozen,
        Blocked,
        Expired
    }

    public class Card
    {
        public int Id { get; set; }
        public string CardNumber { get; set; }
        public string CardHolderName { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Cvv { get; set; }
        public string Pin { get; set; }
        public CardType Type { get; set; }
        public CardStatus Status { get; set; }
        public decimal DailySpendingLimit { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public int AccountId { get; set; }
        public Account Account { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
