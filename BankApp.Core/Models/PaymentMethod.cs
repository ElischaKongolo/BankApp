using System;

namespace BankApp.Core.Models
{
    public enum PaymentMethodType
    {
        DebitCard,
        CreditCard,
        DirectDebit,
        StandingOrder,
        BankTransfer,
        MobilePayment
    }

    public enum PaymentMethodStatus
    {
        Active,
        Inactive,
        Expired,
        Blocked
    }

    public class PaymentMethod
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public PaymentMethodType Type { get; set; }
        public string CardNumber { get; set; }
        public string AccountNumber { get; set; }
        public string SortCode { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string CVV { get; set; }
        public PaymentMethodStatus Status { get; set; }
        public bool IsDefault { get; set; }
        public decimal DailyLimit { get; set; }
        public decimal TransactionLimit { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public int AccountId { get; set; }
        public Account Account { get; set; }
        
        public string MaskedCardNumber 
        { 
            get 
            { 
                if (string.IsNullOrEmpty(CardNumber) || CardNumber.Length < 4)
                    return CardNumber;
                return "****-****-****-" + CardNumber.Substring(CardNumber.Length - 4);
            }
        }
    }
}
