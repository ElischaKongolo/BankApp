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
    public class PayeesController : ControllerBase
    {
        private readonly IPayeeService _payeeService;

        public PayeesController(IPayeeService payeeService)
        {
            _payeeService = payeeService;
        }

        [HttpPost]
        public async Task<IActionResult> AddPayee([FromBody] Payee payee)
        {
            var createdPayee = await _payeeService.AddPayeeAsync(payee);
            return Ok(createdPayee);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var payee = await _payeeService.GetByIdAsync(id);
            if (payee == null) return NotFound();
            return Ok(payee);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserPayees(int userId)
        {
            var payees = await _payeeService.GetUserPayeesAsync(userId);
            return Ok(payees);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayee(int id, [FromBody] Payee payee)
        {
            if (id != payee.Id) return BadRequest();
            var success = await _payeeService.UpdatePayeeAsync(payee);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayee(int id)
        {
            var success = await _payeeService.DeletePayeeAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
