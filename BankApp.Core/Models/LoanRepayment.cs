using System;

namespace BankApp.Core.Models
{
    public class LoanRepayment
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string TransactionReference { get; set; }
        
        public int LoanId { get; set; }
        public Loan Loan { get; set; }
    }
}
