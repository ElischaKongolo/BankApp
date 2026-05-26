using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BankApp.Core.Interfaces;
using BankApp.Core.Models;
using BankApp.Api.Models;

namespace BankApp.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly IAccountService _accountService;

        public TransactionsController(ITransactionService transactionService, IAccountService accountService)
        {
            _transactionService = transactionService;
            _accountService = accountService;
        }

        [HttpPost("deposit/{accountId}")]
        public async Task<IActionResult> Deposit(int accountId, DepositRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var account = await _accountService.GetByIdAsync(accountId);

            if (account == null)
                return NotFound(new { message = "Account not found" });

            if (account.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            try
            {
                var transaction = await _transactionService.DepositAsync(accountId, request.Amount, request.Description);
                return Ok(MapToDto(transaction));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("withdraw/{accountId}")]
        public async Task<IActionResult> Withdraw(int accountId, WithdrawRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var account = await _accountService.GetByIdAsync(accountId);

            if (account == null)
                return NotFound(new { message = "Account not found" });

            if (account.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            try
            {
                var transaction = await _transactionService.WithdrawAsync(accountId, request.Amount, request.Description);
                return Ok(MapToDto(transaction));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer(TransferRequestDto request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var fromAccount = await _accountService.GetByIdAsync(request.FromAccountId);

            if (fromAccount == null)
                return NotFound(new { message = "Source account not found" });

            if (fromAccount.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            try
            {
                var transaction = await _transactionService.TransferAsync(
                    request.FromAccountId,
                    request.ToAccountNumber,
                    request.Amount,
                    request.Description);
                return Ok(MapToDto(transaction));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("account/{accountId}")]
        public async Task<IActionResult> GetAccountTransactions(int accountId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var account = await _accountService.GetByIdAsync(accountId);

            if (account == null)
                return NotFound(new { message = "Account not found" });

            if (account.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            var transactions = await _transactionService.GetAccountTransactionsAsync(accountId, page, pageSize);
            return Ok(transactions.Select(MapToDto));
        }

        [HttpGet("my-transactions")]
        public async Task<IActionResult> GetMyTransactions([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var transactions = await _transactionService.GetUserTransactionsAsync(userId, page, pageSize);
            return Ok(transactions.Select(MapToDto));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransaction(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var transaction = await _transactionService.GetByIdAsync(id);

            if (transaction == null)
                return NotFound(new { message = "Transaction not found" });

            if (transaction.Account.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            return Ok(MapToDto(transaction));
        }

        private TransactionDto MapToDto(Transaction transaction)
        {
            return new TransactionDto
            {
                Id = transaction.Id,
                TransactionReference = transaction.TransactionReference,
                Type = transaction.Type,
                Amount = transaction.Amount,
                BalanceAfter = transaction.BalanceAfter,
                Description = transaction.Description,
                CounterpartyName = transaction.CounterpartyName,
                CounterpartyAccount = transaction.CounterpartyAccount,
                Status = transaction.Status,
                CreatedAt = transaction.CreatedAt
            };
        }
    }
}
