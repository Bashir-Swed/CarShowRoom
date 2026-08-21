using CarShowRoom.DAL.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarShowRoomApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly TransactionRepository _repo;

        public TransactionsController(TransactionRepository repo)
        {
            _repo = repo;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] TransactionCreateDto dto)
        {
            int id = await _repo.CreateTransactionAsync(dto);
            if (id > 0)
            {
                return Ok(new { message = "Transaction recorded successfully.", transactionId = id });
            }
            return BadRequest(new { message = "Failed to record transaction." });
        }

        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetByOrderId(int orderId)
        {
            var result = await _repo.GetTransactionsByOrderIdAsync(orderId);
            return Ok(result);
        }
    }
}
