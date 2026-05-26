using System;
using System.ComponentModel.DataAnnotations;

namespace BankApp.Core.Models
{
    public class EWalletTransaction
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Description { get; set; }
        
        public decimal Amount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        
        public EWalletTransactionType Type { get; set; }
        public EWalletTransactionStatus Status { get; set; }
        
        [StringLength(100)]
        public string ReferenceNumber { get; set; }
        
        [StringLength(500)]
        public string Notes { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public int EWalletId { get; set; }
        public EWallet EWallet { get; set; }
        
        // For transfers
        public int? ToWalletId { get; set; }
        public EWallet ToWallet { get; set; }
        
        public int? FromWalletId { get; set; }
        public EWallet FromWallet { get; set; }
        
        // Link to bank transaction if applicable
        public int? BankTransactionId { get; set; }
        public Transaction BankTransaction { get; set; }
    }

    public enum EWalletTransactionType
    {
        Fund = 0,
        Withdraw = 1,
        Transfer = 2,
        Payment = 3,
        Refund = 4,
        Fee = 5,
        Reward = 6
    }

    public enum EWalletTransactionStatus
    {
        Pending = 0,
        Completed = 1,
        Failed = 2,
        Cancelled = 3
    }
}
