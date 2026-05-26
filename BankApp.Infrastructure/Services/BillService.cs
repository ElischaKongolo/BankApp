using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankApp.Core.Interfaces;
using BankApp.Core.Models;
using BankApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Infrastructure.Services
{
    public class BillService : IBillService
    {
        private readonly BankDbContext _context;

        public BillService(BankDbContext context)
        {
            _context = context;
        }

        public async Task<Bill> CreateBillAsync(Bill bill)
        {
            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();
            return bill;
        }

        public async Task<Bill> GetByIdAsync(int id)
        {
            return await _context.Bills.FindAsync(id);
        }

        public async Task<IEnumerable<Bill>> GetUserBillsAsync(int userId)
        {
            return await _context.Bills.Where(b => b.UserId == userId).ToListAsync();
        }

        public async Task<bool> PayBillAsync(int billId, int accountId)
        {
            var bill = await _context.Bills.FindAsync(billId);
            var account = await _context.Accounts.FindAsync(accountId);

            if (bill == null || account == null || bill.Status != BillStatus.Pending) return false;
            if (account.Balance < bill.Amount) return false;

            account.Balance -= bill.Amount;
            bill.Status = BillStatus.Paid;
            bill.PaidDate = DateTime.UtcNow;
            bill.AccountId = accountId;

            _context.Transactions.Add(new Transaction
            {
                AccountId = accountId,
                Amount = -bill.Amount,
                BalanceAfter = account.Balance,
                Description = $"Bill Payment: {bill.BillerName}",
                TransactionReference = Guid.NewGuid().ToString().Substring(0, 8),
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelBillAsync(int billId)
        {
            var bill = await _context.Bills.FindAsync(billId);
            if (bill == null || bill.Status == BillStatus.Paid) return false;
            
            bill.Status = BillStatus.Cancelled;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
