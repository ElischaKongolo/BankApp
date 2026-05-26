using System;

namespace BankApp.Core.Models
{
    public enum BillStatus
    {
        Pending,
        Paid,
        Overdue,
        Cancelled
    }

    public class Bill
    {
        public int Id { get; set; }
        public string BillerName { get; set; }
        public string ReferenceNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public BillStatus Status { get; set; }
        public DateTime? PaidDate { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; }
        
        public int? AccountId { get; set; }
        public Account Account { get; set; }
    }
}
