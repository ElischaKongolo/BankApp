using System;

namespace BankApp.Core.Models
{
    public enum TransactionType
    {
        Deposit,
        Withdrawal,
        Transfer,
        Payment,
        Refund,
        Fee
    }

    public enum TransactionStatus
    {
        Pending,
        Completed,
        Failed,
        Cancelled,
        Reversed
    }

    public class Transaction
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
        public DateTime? CompletedAt { get; set; }
        
        public int AccountId { get; set; }
        public Account Account { get; set; }
        
        public int? RelatedTransactionId { get; set; }
        public Transaction RelatedTransaction { get; set; }
    }
}
