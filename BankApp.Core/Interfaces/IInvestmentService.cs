using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Core.Models;

namespace BankApp.Core.Interfaces
{
    public interface IInvestmentService
    {
        Task<Investment> CreateInvestmentAsync(Investment investment);
        Task<Investment> UpdateInvestmentAsync(Investment investment);
        Task<bool> DeleteInvestmentAsync(int id);
        Task<Investment> GetInvestmentByIdAsync(int id);
        Task<IEnumerable<Investment>> GetUserInvestmentsAsync(int userId);
        Task<IEnumerable<InvestmentNews>> GetInvestmentNewsAsync(int page = 1, int pageSize = 20);
        Task<IEnumerable<InvestmentNews>> GetFeaturedNewsAsync(int count = 5);
        Task<InvestmentNews> GetNewsByIdAsync(int id);
        Task UpdateInvestmentValuesAsync();
    }
}
