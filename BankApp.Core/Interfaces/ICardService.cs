using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Core.Models;

namespace BankApp.Core.Interfaces
{
    public interface ICardService
    {
        Task<Card> IssueCardAsync(int accountId, int userId, CardType type, string cardHolderName);
        Task<Card> GetByIdAsync(int id);
        Task<IEnumerable<Card>> GetUserCardsAsync(int userId);
        Task<IEnumerable<Card>> GetAccountCardsAsync(int accountId);
        Task<bool> UpdateStatusAsync(int cardId, CardStatus status);
        Task<bool> UpdatePinAsync(int cardId, string newPin);
        Task<bool> SetDailyLimitAsync(int cardId, decimal limit);
    }
}
