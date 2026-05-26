using System.Threading.Tasks;
using BankApp.Core.Interfaces;
using BankApp.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoansController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        [HttpPost("apply")]
        public async Task<IActionResult> ApplyForLoan([FromBody] Loan loan)
        {
            var createdLoan = await _loanService.ApplyForLoanAsync(loan);
            return Ok(createdLoan);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var loan = await _loanService.GetByIdAsync(id);
            if (loan == null) return NotFound();
            return Ok(loan);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserLoans(int userId)
        {
            var loans = await _loanService.GetUserLoansAsync(userId);
            return Ok(loans);
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveLoan(int id)
        {
            var success = await _loanService.ApproveLoanAsync(id);
            if (!success) return BadRequest(new { message = "Cannot approve this loan." });
            return Ok(new { message = "Loan approved successfully." });
        }

        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectLoan(int id)
        {
            var success = await _loanService.RejectLoanAsync(id);
            if (!success) return BadRequest(new { message = "Cannot reject this loan." });
            return NoContent();
        }

        [HttpPost("{id}/repay")]
        public async Task<IActionResult> ProcessRepayment(int id, [FromBody] decimal amount)
        {
            var repayment = await _loanService.ProcessRepaymentAsync(id, amount);
            if (repayment == null) return BadRequest(new { message = "Repayment failed. Insufficient funds or invalid loan." });
            return Ok(repayment);
        }

        [HttpGet("{id}/repayments")]
        public async Task<IActionResult> GetLoanRepayments(int id)
        {
            var repayments = await _loanService.GetLoanRepaymentsAsync(id);
            return Ok(repayments);
        }
    }
}
