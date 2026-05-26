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
    public class AccountService : IAccountService
    {
        private readonly BankDbContext _context;

        public AccountService(BankDbContext context)
        {
            _context = context;
        }

        public async Task<Account> CreateAccountAsync(int userId, AccountType type)
        {
            var accountNumber = GenerateAccountNumber();
            
            while (await _context.Accounts.AnyAsync(a => a.AccountNumber == accountNumber))
            {
                accountNumber = GenerateAccountNumber();
            }

            var account = new Account
            {
                AccountNumber = accountNumber,
                AccountType = type,
                Balance = 0,
                AvailableBalance = 0,
                Status = AccountStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                DailyTransferLimit = 10000m,
                MonthlyTransferLimit = 100000m
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            return account;
        }

        public async Task<Account> GetByIdAsync(int id)
        {
            return await _context.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Account> GetByAccountNumberAsync(string accountNumber)
        {
            return await _context.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
        }

        public async Task<IEnumerable<Account>> GetUserAccountsAsync(int userId)
        {
            return await _context.Accounts
                .Where(a => a.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> UpdateStatusAsync(int accountId, AccountStatus status)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null) return false;

            account.Status = status;
            if (status == AccountStatus.Closed)
                account.ClosedAt = DateTime.UtcNow;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<decimal> GetBalanceAsync(int accountId)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            return account?.Balance ?? 0;
        }

        private string GenerateAccountNumber()
        {
            var random = new Random();
            return $"ACC{random.Next(10000000, 99999999):D8}{random.Next(1000, 9999):D4}";
        }
    }
}
