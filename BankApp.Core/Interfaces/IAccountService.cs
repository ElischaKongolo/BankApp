using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Core.Models;

namespace BankApp.Core.Interfaces
{
    public interface IAccountService
    {
        Task<Account> CreateAccountAsync(int userId, AccountType type);
        Task<Account> GetByIdAsync(int id);
        Task<Account> GetByAccountNumberAsync(string accountNumber);
        Task<IEnumerable<Account>> GetUserAccountsAsync(int userId);
        Task<bool> UpdateStatusAsync(int accountId, AccountStatus status);
        Task<decimal> GetBalanceAsync(int accountId);
    }
}
