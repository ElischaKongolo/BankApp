using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BankApp.Core.Models;
using BankApp.Core.Interfaces;
using BankApp.Infrastructure.Data;

namespace BankApp.Infrastructure.Services
{
    public class EWalletService : IEWalletService
    {
        private readonly BankDbContext _context;

        public EWalletService(BankDbContext context)
        {
            _context = context;
        }

        public async Task<EWallet> CreateWalletAsync(EWallet wallet)
        {
            // Generate unique wallet number
            wallet.WalletNumber = await GenerateWalletNumberAsync();
            wallet.CreatedAt = DateTime.UtcNow;
            wallet.UpdatedAt = DateTime.UtcNow;
            wallet.Status = EWalletStatus.Active;
            wallet.Balance = 0;
            wallet.AvailableBalance = 0;

            _context.EWallets.Add(wallet);
            await _context.SaveChangesAsync();
            return wallet;
        }

        public async Task<EWallet> UpdateWalletAsync(EWallet wallet)
        {
            wallet.UpdatedAt = DateTime.UtcNow;
            _context.EWallets.Update(wallet);
            await _context.SaveChangesAsync();
            return wallet;
        }

        public async Task<bool> DeleteWalletAsync(int walletId)
        {
            var wallet = await _context.EWallets.FindAsync(walletId);
            if (wallet == null) return false;

            wallet.Status = EWalletStatus.Closed;
            wallet.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<EWallet> GetWalletByIdAsync(int walletId)
        {
            return await _context.EWallets
                .Include(w => w.User)
                .Include(w => w.LinkedAccount)
                .FirstOrDefaultAsync(w => w.Id == walletId);
        }

        public async Task<IEnumerable<EWallet>> GetUserWalletsAsync(int userId)
        {
            return await _context.EWallets
                .Include(w => w.LinkedAccount)
                .Where(w => w.UserId == userId && w.Status == EWalletStatus.Active)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();
        }

        public async Task<EWalletTransaction> FundWalletAsync(int walletId, decimal amount, string description)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero");

            var wallet = await _context.EWallets.FindAsync(walletId);
            if (wallet == null)
                throw new ArgumentException("Wallet not found");

            var transaction = new EWalletTransaction
            {
                EWalletId = walletId,
                Description = description ?? "Wallet Funding",
                Amount = amount,
                BalanceBefore = wallet.Balance,
                Type = EWalletTransactionType.Fund,
                Status = EWalletTransactionStatus.Completed,
                ReferenceNumber = await GenerateReferenceNumberAsync(),
                CreatedAt = DateTime.UtcNow
            };

            transaction.BalanceAfter = transaction.BalanceBefore + amount;

            // Update wallet balance
            wallet.Balance = transaction.BalanceAfter;
            wallet.AvailableBalance = transaction.BalanceAfter;
            wallet.LastTransactionDate = DateTime.UtcNow;
            wallet.UpdatedAt = DateTime.UtcNow;

            _context.EWalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return transaction;
        }

        public async Task<EWalletTransaction> WithdrawFromWalletAsync(int walletId, decimal amount, string description)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero");

            var wallet = await _context.EWallets.FindAsync(walletId);
            if (wallet == null)
                throw new ArgumentException("Wallet not found");

            if (wallet.AvailableBalance < amount)
                throw new InvalidOperationException("Insufficient wallet balance");

            var transaction = new EWalletTransaction
            {
                EWalletId = walletId,
                Description = description ?? "Wallet Withdrawal",
                Amount = amount,
                BalanceBefore = wallet.Balance,
                Type = EWalletTransactionType.Withdraw,
                Status = EWalletTransactionStatus.Completed,
                ReferenceNumber = await GenerateReferenceNumberAsync(),
                CreatedAt = DateTime.UtcNow
            };

            transaction.BalanceAfter = transaction.BalanceBefore - amount;

            // Update wallet balance
            wallet.Balance = transaction.BalanceAfter;
            wallet.AvailableBalance = transaction.BalanceAfter;
            wallet.LastTransactionDate = DateTime.UtcNow;
            wallet.UpdatedAt = DateTime.UtcNow;

            _context.EWalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return transaction;
        }

        public async Task<EWalletTransaction> TransferBetweenWalletsAsync(int fromWalletId, int toWalletId, decimal amount, string description)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero");

            if (fromWalletId == toWalletId)
                throw new ArgumentException("Cannot transfer to the same wallet");

            var fromWallet = await _context.EWallets.FindAsync(fromWalletId);
            var toWallet = await _context.EWallets.FindAsync(toWalletId);

            if (fromWallet == null || toWallet == null)
                throw new ArgumentException("One or both wallets not found");

            if (fromWallet.AvailableBalance < amount)
                throw new InvalidOperationException("Insufficient balance in source wallet");

            var referenceNumber = await GenerateReferenceNumberAsync();

            // Create withdrawal transaction
            var withdrawalTransaction = new EWalletTransaction
            {
                EWalletId = fromWalletId,
                ToWalletId = toWalletId,
                Description = description ?? $"Transfer to {toWallet.WalletName}",
                Amount = amount,
                BalanceBefore = fromWallet.Balance,
                Type = EWalletTransactionType.Transfer,
                Status = EWalletTransactionStatus.Completed,
                ReferenceNumber = referenceNumber,
                CreatedAt = DateTime.UtcNow
            };
            withdrawalTransaction.BalanceAfter = withdrawalTransaction.BalanceBefore - amount;

            // Create deposit transaction
            var depositTransaction = new EWalletTransaction
            {
                EWalletId = toWalletId,
                FromWalletId = fromWalletId,
                Description = description ?? $"Transfer from {fromWallet.WalletName}",
                Amount = amount,
                BalanceBefore = toWallet.Balance,
                Type = EWalletTransactionType.Transfer,
                Status = EWalletTransactionStatus.Completed,
                ReferenceNumber = referenceNumber,
                CreatedAt = DateTime.UtcNow
            };
            depositTransaction.BalanceAfter = depositTransaction.BalanceBefore + amount;

            // Update wallet balances
            fromWallet.Balance = withdrawalTransaction.BalanceAfter;
            fromWallet.AvailableBalance = withdrawalTransaction.BalanceAfter;
            fromWallet.LastTransactionDate = DateTime.UtcNow;
            fromWallet.UpdatedAt = DateTime.UtcNow;

            toWallet.Balance = depositTransaction.BalanceAfter;
            toWallet.AvailableBalance = depositTransaction.BalanceAfter;
            toWallet.LastTransactionDate = DateTime.UtcNow;
            toWallet.UpdatedAt = DateTime.UtcNow;

            _context.EWalletTransactions.AddRange(new[] { withdrawalTransaction, depositTransaction });
            await _context.SaveChangesAsync();

            return withdrawalTransaction;
        }

        public async Task<IEnumerable<EWalletTransaction>> GetWalletTransactionsAsync(int walletId, int page = 1, int pageSize = 20)
        {
            return await _context.EWalletTransactions
                .Include(t => t.ToWallet)
                .Include(t => t.FromWallet)
                .Where(t => t.EWalletId == walletId)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<EWallet> LinkWalletToAccountAsync(int walletId, int accountId)
        {
            var wallet = await _context.EWallets.FindAsync(walletId);
            var account = await _context.Accounts.FindAsync(accountId);

            if (wallet == null || account == null)
                throw new ArgumentException("Wallet or account not found");

            wallet.LinkedAccountId = accountId;
            wallet.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return wallet;
        }

        public async Task<EWallet> UnlinkWalletFromAccountAsync(int walletId)
        {
            var wallet = await _context.EWallets.FindAsync(walletId);
            if (wallet == null)
                throw new ArgumentException("Wallet not found");

            wallet.LinkedAccountId = null;
            wallet.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return wallet;
        }

        public async Task<decimal> GetTotalWalletBalanceAsync(int userId)
        {
            var balances = await _context.EWallets
                .Where(w => w.UserId == userId && w.Status == EWalletStatus.Active)
                .Select(w => w.Balance)
                .ToListAsync();

            return balances.Sum();
        }

        public async Task<bool> ValidateWalletTransactionAsync(int walletId, decimal amount)
        {
            var wallet = await _context.EWallets.FindAsync(walletId);
            if (wallet == null || wallet.Status != EWalletStatus.Active)
                return false;

            return wallet.AvailableBalance >= amount && amount > 0;
        }

        private async Task<string> GenerateWalletNumberAsync()
        {
            string walletNumber;
            var random = new Random();
            
            do
            {
                walletNumber = "EW" + random.Next(100000000, 999999999).ToString();
            } 
            while (await _context.EWallets.AnyAsync(w => w.WalletNumber == walletNumber));

            return walletNumber;
        }

        private async Task<string> GenerateReferenceNumberAsync()
        {
            string referenceNumber;
            var random = new Random();
            
            do
            {
                referenceNumber = "EWT" + random.Next(100000000, 999999999).ToString();
            } 
            while (await _context.EWalletTransactions.AnyAsync(t => t.ReferenceNumber == referenceNumber));

            return referenceNumber;
        }
    }
}
