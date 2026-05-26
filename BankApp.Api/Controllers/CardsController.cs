using System;
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
    public class CardsController : ControllerBase
    {
        private readonly ICardService _cardService;

        public CardsController(ICardService cardService)
        {
            _cardService = cardService;
        }

        [HttpPost("issue")]
        public async Task<IActionResult> IssueCard([FromBody] IssueCardRequest request)
        {
            var card = await _cardService.IssueCardAsync(request.AccountId, request.UserId, request.Type, request.CardHolderName);
            return Ok(card);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var card = await _cardService.GetByIdAsync(id);
            if (card == null) return NotFound();
            return Ok(card);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserCards(int userId)
        {
            var cards = await _cardService.GetUserCardsAsync(userId);
            return Ok(cards);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateCardStatusRequest request)
        {
            var success = await _cardService.UpdateStatusAsync(id, request.Status);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpPut("{id}/limit")]
        public async Task<IActionResult> SetDailyLimit(int id, [FromBody] decimal limit)
        {
            var success = await _cardService.SetDailyLimitAsync(id, limit);
            if (!success) return NotFound();
            return NoContent();
        }
    }

    public class IssueCardRequest
    {
        public int AccountId { get; set; }
        public int UserId { get; set; }
        public CardType Type { get; set; }
        public string CardHolderName { get; set; }
    }

    public class UpdateCardStatusRequest
    {
        public CardStatus Status { get; set; }
    }
}
