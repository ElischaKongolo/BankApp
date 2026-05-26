using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Core.Models;

namespace BankApp.Core.Interfaces
{
    public interface ILoanService
    {
        Task<Loan> ApplyForLoanAsync(Loan loan);
        Task<Loan> GetByIdAsync(int id);
        Task<IEnumerable<Loan>> GetUserLoansAsync(int userId);
        Task<bool> ApproveLoanAsync(int loanId);
        Task<bool> RejectLoanAsync(int loanId);
        Task<LoanRepayment> ProcessRepaymentAsync(int loanId, decimal amount);
        Task<IEnumerable<LoanRepayment>> GetLoanRepaymentsAsync(int loanId);
    }
}
