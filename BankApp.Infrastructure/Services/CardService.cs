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
    public class CardService : ICardService
    {
        private readonly BankDbContext _context;

        public CardService(BankDbContext context)
        {
            _context = context;
        }

        public async Task<Card> IssueCardAsync(int accountId, int userId, CardType type, string cardHolderName)
        {
            var card = new Card
            {
                AccountId = accountId,
                UserId = userId,
                Type = type,
                CardHolderName = cardHolderName,
                CardNumber = GenerateCardNumber(),
                Cvv = new Random().Next(100, 999).ToString(),
                Pin = "0000",
                ExpiryDate = DateTime.UtcNow.AddYears(3),
                Status = CardStatus.Active,
                DailySpendingLimit = 1000m,
                CreatedAt = DateTime.UtcNow
            };

            _context.Cards.Add(card);
            await _context.SaveChangesAsync();
            return card;
        }

        public async Task<Card> GetByIdAsync(int id)
        {
            return await _context.Cards.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Card>> GetUserCardsAsync(int userId)
        {
            return await _context.Cards.Where(c => c.UserId == userId).ToListAsync();
        }

        public async Task<IEnumerable<Card>> GetAccountCardsAsync(int accountId)
        {
            return await _context.Cards.Where(c => c.AccountId == accountId).ToListAsync();
        }

        public async Task<bool> UpdateStatusAsync(int cardId, CardStatus status)
        {
            var card = await _context.Cards.FindAsync(cardId);
            if (card == null) return false;
            card.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePinAsync(int cardId, string newPin)
        {
            var card = await _context.Cards.FindAsync(cardId);
            if (card == null) return false;
            card.Pin = newPin;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetDailyLimitAsync(int cardId, decimal limit)
        {
            var card = await _context.Cards.FindAsync(cardId);
            if (card == null) return false;
            card.DailySpendingLimit = limit;
            await _context.SaveChangesAsync();
            return true;
        }

        private string GenerateCardNumber()
        {
            var random = new Random();
            return $"4532{random.Next(1000, 9999)}{random.Next(1000, 9999)}{random.Next(1000, 9999)}";
        }
    }
}
