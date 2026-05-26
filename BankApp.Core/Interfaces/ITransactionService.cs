using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Core.Models;

namespace BankApp.Core.Interfaces
{
    public interface ITransactionService
    {
        Task<Transaction> DepositAsync(int accountId, decimal amount, string description);
        Task<Transaction> WithdrawAsync(int accountId, decimal amount, string description);
        Task<Transaction> TransferAsync(int fromAccountId, string toAccountNumber, decimal amount, string description);
        Task<Transaction> GetByIdAsync(int id);
        Task<IEnumerable<Transaction>> GetAccountTransactionsAsync(int accountId, int page = 1, int pageSize = 20);
        Task<IEnumerable<Transaction>> GetUserTransactionsAsync(int userId, int page = 1, int pageSize = 20);
    }
}
