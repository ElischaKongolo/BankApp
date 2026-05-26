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
    public class PayeeService : IPayeeService
    {
        private readonly BankDbContext _context;

        public PayeeService(BankDbContext context)
        {
            _context = context;
        }

        public async Task<Payee> AddPayeeAsync(Payee payee)
        {
            payee.CreatedAt = DateTime.UtcNow;
            _context.Payees.Add(payee);
            await _context.SaveChangesAsync();
            return payee;
        }

        public async Task<Payee> GetByIdAsync(int id)
        {
            return await _context.Payees.FindAsync(id);
        }

        public async Task<IEnumerable<Payee>> GetUserPayeesAsync(int userId)
        {
            return await _context.Payees.Where(p => p.UserId == userId).ToListAsync();
        }

        public async Task<bool> UpdatePayeeAsync(Payee payee)
        {
            var existing = await _context.Payees.FindAsync(payee.Id);
            if (existing == null) return false;

            existing.Name = payee.Name;
            existing.AccountNumber = payee.AccountNumber;
            existing.BankName = payee.BankName;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePayeeAsync(int id)
        {
            var payee = await _context.Payees.FindAsync(id);
            if (payee == null) return false;

            _context.Payees.Remove(payee);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
