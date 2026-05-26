using System;

namespace BankApp.Core.Models
{
    public class Payee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AccountNumber { get; set; }
        public string BankName { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
