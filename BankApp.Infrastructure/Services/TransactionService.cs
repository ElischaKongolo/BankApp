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
    public class TransactionService : ITransactionService
    {
        private readonly BankDbContext _context;
        private readonly INotificationService _notificationService;

        public TransactionService(BankDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<Transaction> DepositAsync(int accountId, decimal amount, string description)
        {
            if (amount <= 0)
                throw new InvalidOperationException("Amount must be positive");

            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null)
                throw new InvalidOperationException("Account not found");
            if (account.Status != AccountStatus.Active)
                throw new InvalidOperationException("Account is not active");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                account.Balance += amount;
                account.AvailableBalance += amount;

                var txn = new Transaction
                {
                    TransactionReference = GenerateTransactionReference(),
                    Type = TransactionType.Deposit,
                    Amount = amount,
                    BalanceAfter = account.Balance,
                    Description = description,
                    Status = TransactionStatus.Completed,
                    CreatedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow,
                    AccountId = accountId
                };

                _context.Transactions.Add(txn);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return txn;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Transaction> WithdrawAsync(int accountId, decimal amount, string description)
        {
            if (amount <= 0)
                throw new InvalidOperationException("Amount must be positive");

            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null)
                throw new InvalidOperationException("Account not found");
            if (account.Status != AccountStatus.Active)
                throw new InvalidOperationException("Account is not active");
            if (account.AvailableBalance < amount)
                throw new InvalidOperationException("Insufficient funds");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                account.Balance -= amount;
                account.AvailableBalance -= amount;

                var txn = new Transaction
                {
                    TransactionReference = GenerateTransactionReference(),
                    Type = TransactionType.Withdrawal,
                    Amount = amount,
                    BalanceAfter = account.Balance,
                    Description = description,
                    Status = TransactionStatus.Completed,
                    CreatedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow,
                    AccountId = accountId
                };

                _context.Transactions.Add(txn);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return txn;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Transaction> TransferAsync(int fromAccountId, string toAccountNumber, decimal amount, string description)
        {
            if (amount <= 0)
                throw new InvalidOperationException("Amount must be positive");

            var fromAccount = await _context.Accounts.FindAsync(fromAccountId);
            if (fromAccount == null)
                throw new InvalidOperationException("Source account not found");
            if (fromAccount.Status != AccountStatus.Active)
                throw new InvalidOperationException("Source account is not active");
            if (fromAccount.AvailableBalance < amount)
                throw new InvalidOperationException("Insufficient funds");

            var toAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == toAccountNumber);
            bool isInternalTransfer = toAccount != null;

            if (isInternalTransfer && toAccount.Status != AccountStatus.Active)
                throw new InvalidOperationException("Destination account is not active");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                fromAccount.Balance -= amount;
                fromAccount.AvailableBalance -= amount;

                if (isInternalTransfer)
                {
                    toAccount.Balance += amount;
                    toAccount.AvailableBalance += amount;
                }

                string counterpartyName = "External Account";
                if (isInternalTransfer)
                {
                    counterpartyName = toAccount.User?.FirstName + " " + toAccount.User?.LastName;
                }
                else
                {
                    var payee = await _context.Payees.FirstOrDefaultAsync(p => p.AccountNumber == toAccountNumber && p.UserId == fromAccount.UserId);
                    if (payee != null)
                    {
                        counterpartyName = payee.Name;
                    }
                }

                var outgoingTxn = new Transaction
                {
                    TransactionReference = GenerateTransactionReference(),
                    Type = TransactionType.Transfer,
                    Amount = amount,
                    BalanceAfter = fromAccount.Balance,
                    Description = description,
                    CounterpartyName = counterpartyName,
                    CounterpartyAccount = toAccountNumber,
                    Status = TransactionStatus.Completed,
                    CreatedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow,
                    AccountId = fromAccountId
                };

                _context.Transactions.Add(outgoingTxn);

                if (isInternalTransfer)
                {
                    var incomingTxn = new Transaction
                    {
                        TransactionReference = GenerateTransactionReference(),
                        Type = TransactionType.Transfer,
                        Amount = amount,
                        BalanceAfter = toAccount.Balance,
                        Description = description,
                        CounterpartyName = fromAccount.User?.FirstName + " " + fromAccount.User?.LastName,
                        CounterpartyAccount = fromAccount.AccountNumber,
                        Status = TransactionStatus.Completed,
                        CreatedAt = DateTime.UtcNow,
                        CompletedAt = DateTime.UtcNow,
                        AccountId = toAccount.Id,
                        RelatedTransaction = outgoingTxn
                    };
                    _context.Transactions.Add(incomingTxn);
                }

                await _context.SaveChangesAsync();

                // Create notification for sender
                var senderNotification = new Notification
                {
                    Title = "Money Sent",
                    Message = $"You have successfully sent {amount:C} to {counterpartyName} ({toAccountNumber}).",
                    Type = NotificationType.Transaction,
                    UserId = fromAccount.UserId,
                    AccountId = fromAccountId,
                    ActionUrl = $"/Transactions/Details/{outgoingTxn.Id}",
                    ActionText = "View Transaction",
                    IsImportant = amount > 1000
                };
                await _notificationService.CreateNotificationAsync(senderNotification);

                // Create notification for receiver (if internal transfer)
                if (isInternalTransfer)
                {
                    var receiverNotification = new Notification
                    {
                        Title = "Money Received",
                        Message = $"You have received {amount:C} from {fromAccount.User?.FirstName + " " + fromAccount.User?.LastName} ({fromAccount.AccountNumber}).",
                        Type = NotificationType.Transaction,
                        UserId = toAccount.UserId,
                        AccountId = toAccount.Id,
                        ActionUrl = $"/Transactions/Details/{outgoingTxn.RelatedTransaction?.Id ?? outgoingTxn.Id}",
                        ActionText = "View Transaction",
                        IsImportant = amount > 1000
                    };
                    await _notificationService.CreateNotificationAsync(receiverNotification);
                }

                await transaction.CommitAsync();

                return outgoingTxn;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Transaction> GetByIdAsync(int id)
        {
            return await _context.Transactions
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Transaction>> GetAccountTransactionsAsync(int accountId, int page = 1, int pageSize = 20)
        {
            return await _context.Transactions
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetUserTransactionsAsync(int userId, int page = 1, int pageSize = 20)
        {
            return await _context.Transactions
                .Include(t => t.Account)
                .Where(t => t.Account.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        private string GenerateTransactionReference()
        {
            return $"TXN{DateTime.UtcNow:yyyyMMddHHmmss}{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }
}
