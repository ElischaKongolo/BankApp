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
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IUserService _userService;

        public AccountsController(IAccountService accountService, IUserService userService)
        {
            _accountService = accountService;
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount(CreateAccountRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var account = await _accountService.CreateAccountAsync(userId, request.AccountType);
            return Ok(MapToDto(account));
        }

        [HttpGet]
        public async Task<IActionResult> GetMyAccounts()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var accounts = await _accountService.GetUserAccountsAsync(userId);
            return Ok(accounts.Select(MapToDto));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccount(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var account = await _accountService.GetByIdAsync(id);

            if (account == null)
                return NotFound(new { message = "Account not found" });

            if (account.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            return Ok(MapToDto(account));
        }

        [HttpGet("number/{accountNumber}")]
        public async Task<IActionResult> GetByAccountNumber(string accountNumber)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var account = await _accountService.GetByAccountNumberAsync(accountNumber);

            if (account == null)
                return NotFound(new { message = "Account not found" });

            if (account.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            return Ok(MapToDto(account));
        }

        [HttpGet("{id}/balance")]
        public async Task<IActionResult> GetBalance(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var account = await _accountService.GetByIdAsync(id);

            if (account == null)
                return NotFound(new { message = "Account not found" });

            if (account.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            var balance = await _accountService.GetBalanceAsync(id);
            return Ok(new { balance, availableBalance = account.AvailableBalance });
        }

        private AccountDto MapToDto(Account account)
        {
            return new AccountDto
            {
                Id = account.Id,
                AccountNumber = account.AccountNumber,
                AccountType = account.AccountType,
                Balance = account.Balance,
                AvailableBalance = account.AvailableBalance,
                Status = account.Status,
                CreatedAt = account.CreatedAt
            };
        }
    }
}
