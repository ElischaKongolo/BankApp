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
    public class LoanService : ILoanService
    {
        private readonly BankDbContext _context;

        public LoanService(BankDbContext context)
        {
            _context = context;
        }

        public async Task<Loan> ApplyForLoanAsync(Loan loan)
        {
            loan.AppliedAt = DateTime.UtcNow;
            loan.Status = LoanStatus.Pending;
            loan.RemainingBalance = loan.Amount;
            
            decimal monthlyRate = loan.InterestRate / 100 / 12;
            if (monthlyRate > 0)
            {
                loan.MonthlyPayment = loan.Amount * (monthlyRate * (decimal)Math.Pow(1 + (double)monthlyRate, loan.TermMonths)) / (decimal)(Math.Pow(1 + (double)monthlyRate, loan.TermMonths) - 1);
            }
            else
            {
                loan.MonthlyPayment = loan.Amount / loan.TermMonths;
            }

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();
            return loan;
        }

        public async Task<Loan> GetByIdAsync(int id)
        {
            return await _context.Loans.FindAsync(id);
        }

        public async Task<IEnumerable<Loan>> GetUserLoansAsync(int userId)
        {
            return await _context.Loans.Where(l => l.UserId == userId).ToListAsync();
        }

        public async Task<bool> ApproveLoanAsync(int loanId)
        {
            var loan = await _context.Loans.FindAsync(loanId);
            if (loan == null || loan.Status != LoanStatus.Pending) return false;

            loan.Status = LoanStatus.Approved;
            loan.ApprovedAt = DateTime.UtcNow;

            var account = await _context.Accounts.FindAsync(loan.AccountId);
            if (account != null)
            {
                account.Balance += loan.Amount;
                _context.Transactions.Add(new Transaction
                {
                    AccountId = account.Id,
                    Amount = loan.Amount,
                    BalanceAfter = account.Balance,
                    Description = $"Loan Disbursement",
                    TransactionReference = Guid.NewGuid().ToString().Substring(0, 8),
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectLoanAsync(int loanId)
        {
            var loan = await _context.Loans.FindAsync(loanId);
            if (loan == null || loan.Status != LoanStatus.Pending) return false;

            loan.Status = LoanStatus.Rejected;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<LoanRepayment> ProcessRepaymentAsync(int loanId, decimal amount)
        {
            var loan = await _context.Loans.FindAsync(loanId);
            var account = await _context.Accounts.FindAsync(loan.AccountId);

            if (loan == null || account == null || account.Balance < amount) return null;

            account.Balance -= amount;
            loan.RemainingBalance -= amount; // simplified math for demo

            var repayment = new LoanRepayment
            {
                LoanId = loan.Id,
                Amount = amount,
                PaymentDate = DateTime.UtcNow,
                PrincipalAmount = amount * 0.8m,
                InterestAmount = amount * 0.2m,
                TransactionReference = Guid.NewGuid().ToString().Substring(0, 8)
            };

            _context.LoanRepayments.Add(repayment);
            
            _context.Transactions.Add(new Transaction
            {
                AccountId = account.Id,
                Amount = -amount,
                BalanceAfter = account.Balance,
                Description = $"Loan Repayment",
                TransactionReference = repayment.TransactionReference,
                CreatedAt = DateTime.UtcNow
            });

            if (loan.RemainingBalance <= 0)
            {
                loan.Status = LoanStatus.PaidOff;
            }

            await _context.SaveChangesAsync();
            return repayment;
        }

        public async Task<IEnumerable<LoanRepayment>> GetLoanRepaymentsAsync(int loanId)
        {
            return await _context.LoanRepayments.Where(r => r.LoanId == loanId).ToListAsync();
        }
    }
}
