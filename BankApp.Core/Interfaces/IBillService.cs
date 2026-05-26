using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Core.Models;

namespace BankApp.Core.Interfaces
{
    public interface IBillService
    {
        Task<Bill> CreateBillAsync(Bill bill);
        Task<Bill> GetByIdAsync(int id);
        Task<IEnumerable<Bill>> GetUserBillsAsync(int userId);
        Task<bool> PayBillAsync(int billId, int accountId);
        Task<bool> CancelBillAsync(int billId);
    }
}
