using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Core.Models;

namespace BankApp.Core.Interfaces
{
    public interface IEWalletService
    {
        Task<EWallet> CreateWalletAsync(EWallet wallet);
        Task<EWallet> UpdateWalletAsync(EWallet wallet);
        Task<bool> DeleteWalletAsync(int walletId);
        Task<EWallet> GetWalletByIdAsync(int walletId);
        Task<IEnumerable<EWallet>> GetUserWalletsAsync(int userId);
        Task<EWalletTransaction> FundWalletAsync(int walletId, decimal amount, string description);
        Task<EWalletTransaction> WithdrawFromWalletAsync(int walletId, decimal amount, string description);
        Task<EWalletTransaction> TransferBetweenWalletsAsync(int fromWalletId, int toWalletId, decimal amount, string description);
        Task<IEnumerable<EWalletTransaction>> GetWalletTransactionsAsync(int walletId, int page = 1, int pageSize = 20);
        Task<EWallet> LinkWalletToAccountAsync(int walletId, int accountId);
        Task<EWallet> UnlinkWalletFromAccountAsync(int walletId);
        Task<decimal> GetTotalWalletBalanceAsync(int userId);
        Task<bool> ValidateWalletTransactionAsync(int walletId, decimal amount);
    }
}
