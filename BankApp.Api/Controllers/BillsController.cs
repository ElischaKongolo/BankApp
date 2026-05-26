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
    public class BillsController : ControllerBase
    {
        private readonly IBillService _billService;

        public BillsController(IBillService billService)
        {
            _billService = billService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBill([FromBody] Bill bill)
        {
            var createdBill = await _billService.CreateBillAsync(bill);
            return Ok(createdBill);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var bill = await _billService.GetByIdAsync(id);
            if (bill == null) return NotFound();
            return Ok(bill);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserBills(int userId)
        {
            var bills = await _billService.GetUserBillsAsync(userId);
            return Ok(bills);
        }

        [HttpPost("{id}/pay")]
        public async Task<IActionResult> PayBill(int id, [FromBody] int accountId)
        {
            var success = await _billService.PayBillAsync(id, accountId);
            if (!success) return BadRequest(new { message = "Payment failed. Insufficient funds or invalid bill." });
            return Ok(new { message = "Bill paid successfully" });
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelBill(int id)
        {
            var success = await _billService.CancelBillAsync(id);
            if (!success) return BadRequest(new { message = "Cannot cancel this bill." });
            return NoContent();
        }
    }
}
