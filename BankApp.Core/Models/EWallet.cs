using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BankApp.Core.Models
{
    public class EWallet
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string WalletName { get; set; }
        
        [Required]
        [StringLength(20)]
        public string WalletNumber { get; set; }
        
        public decimal Balance { get; set; }
        public decimal AvailableBalance { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        public EWalletType WalletType { get; set; }
        public EWalletStatus Status { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastTransactionDate { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; }
        
        // For linking to bank account
        public int? LinkedAccountId { get; set; }
        public Account LinkedAccount { get; set; }
        public ICollection<EWalletTransaction> Transactions { get; set; }
    }

    public enum EWalletType
    {
        DigitalWallet = 0,
        MobileMoney = 1,
        CryptoWallet = 2,
        PrepaidCard = 3,
        GiftCard = 4
    }

    public enum EWalletStatus
    {
        Active = 0,
        Inactive = 1,
        Suspended = 2,
        Closed = 3,
        Pending = 4
    }
}
