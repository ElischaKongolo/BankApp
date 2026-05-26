using System;
using System.Collections.Generic;

namespace BankApp.Core.Models
{
    public enum LoanType
    {
        Personal,
        Mortgage,
        Auto,
        Student
    }

    public enum LoanStatus
    {
        Pending,
        Approved,
        Rejected,
        Active,
        PaidOff,
        Defaulted
    }

    public class Loan
    {
        public int Id { get; set; }
        public LoanType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal InterestRate { get; set; }
        public int TermMonths { get; set; }
        public decimal MonthlyPayment { get; set; }
        public decimal RemainingBalance { get; set; }
        public LoanStatus Status { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; }
        
        public int AccountId { get; set; }
        public Account Account { get; set; }

        public ICollection<LoanRepayment> Repayments { get; set; }
    }
}
