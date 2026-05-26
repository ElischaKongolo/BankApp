namespace BankApp.Core.Models
{
    public class TransferRequest
    {
        public int FromAccountId { get; set; }
        public string ToAccountNumber { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }
}
