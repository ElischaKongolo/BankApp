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
    public class InvestmentService : IInvestmentService
    {
        private readonly BankDbContext _context;

        public InvestmentService(BankDbContext context)
        {
            _context = context;
        }

        public async Task<Investment> CreateInvestmentAsync(Investment investment)
        {
            investment.PurchaseDate = DateTime.UtcNow;
            investment.Status = InvestmentStatus.Active;
            investment.ReturnPercentage = 0; // Will be calculated periodically
            
            _context.Investments.Add(investment);
            await _context.SaveChangesAsync();
            return investment;
        }

        public async Task<Investment> UpdateInvestmentAsync(Investment investment)
        {
            _context.Investments.Update(investment);
            await _context.SaveChangesAsync();
            return investment;
        }

        public async Task<bool> DeleteInvestmentAsync(int id)
        {
            var investment = await _context.Investments.FindAsync(id);
            if (investment == null) return false;

            investment.Status = InvestmentStatus.Closed;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Investment> GetInvestmentByIdAsync(int id)
        {
            return await _context.Investments
                .Include(i => i.User)
                .Include(i => i.Account)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<Investment>> GetUserInvestmentsAsync(int userId)
        {
            return await _context.Investments
                .Include(i => i.Account)
                .Where(i => i.UserId == userId && i.Status == InvestmentStatus.Active)
                .OrderByDescending(i => i.PurchaseDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<InvestmentNews>> GetInvestmentNewsAsync(int page = 1, int pageSize = 20)
        {
            return await _context.InvestmentNews
                .OrderByDescending(n => n.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<InvestmentNews>> GetFeaturedNewsAsync(int count = 5)
        {
            return await _context.InvestmentNews
                .Where(n => n.IsFeatured)
                .OrderByDescending(n => n.PublishedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<InvestmentNews> GetNewsByIdAsync(int id)
        {
            var news = await _context.InvestmentNews.FindAsync(id);
            if (news != null)
            {
                news.ViewCount++;
                await _context.SaveChangesAsync();
            }
            return news;
        }

        public async Task UpdateInvestmentValuesAsync()
        {
            var investments = await _context.Investments
                .Where(i => i.Status == InvestmentStatus.Active)
                .ToListAsync();

            foreach (var investment in investments)
            {
                // Simulate market fluctuations (real implementation would use real market data)
                var random = new Random();
                var marketChange = (random.NextDouble() - 0.5) * 0.1; // -5% to +5%
                
                investment.CurrentValue = investment.InitialAmount * (1 + (decimal)marketChange);
                investment.ReturnPercentage = ((investment.CurrentValue - investment.InitialAmount) / investment.InitialAmount) * 100;
            }

            await _context.SaveChangesAsync();
        }
    }
}
