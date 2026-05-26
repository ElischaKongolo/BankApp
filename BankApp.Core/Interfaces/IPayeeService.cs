using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Core.Models;

namespace BankApp.Core.Interfaces
{
    public interface IPayeeService
    {
        Task<Payee> AddPayeeAsync(Payee payee);
        Task<Payee> GetByIdAsync(int id);
        Task<IEnumerable<Payee>> GetUserPayeesAsync(int userId);
        Task<bool> UpdatePayeeAsync(Payee payee);
        Task<bool> DeletePayeeAsync(int id);
    }
}
