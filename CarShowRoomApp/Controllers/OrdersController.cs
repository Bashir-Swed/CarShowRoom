using CarShowRoom.DAL.Enums;
using CarShowRoom.DAL.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarShowRoom.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly OrderRepository _orderRepo;
        private readonly IWebHostEnvironment _env;

        public OrdersController(OrderRepository orderRepo, IWebHostEnvironment env)
        {
            _orderRepo = orderRepo;
            _env = env;
        }

        [HttpPost("rent")]
        public async Task<IActionResult> CreateRentOrder([FromForm] RentOrderCreateDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User not found or not authenticated." });
            }
            int userId = int.Parse(userIdClaim.Value);

            List<string> documentUrls = new List<string>();
            List<string> physicalFilePaths = new List<string>();

            try
            {
                if (dto.Documents != null && dto.Documents.Count > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "Uploads", "Orders");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    foreach (var file in dto.Documents)
                    {
                        if (file.Length > 0)
                        {
                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            physicalFilePaths.Add(filePath);

                            documentUrls.Add($"/Uploads/Orders/{uniqueFileName}");
                        }
                    }
                }

                int orderId = await _orderRepo.AddRentOrderAsync(dto, userId, documentUrls);

                if (orderId > 0)
                {
                    return Ok(new { message = "Rent order created successfully.", id = orderId });
                }

                return BadRequest(new { message = "Failed to create the rent order." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {

                foreach (var path in physicalFilePaths)
                {
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }

                return StatusCode(500, new { message = "An error occurred.", details = ex.Message });
            }
        }

        [HttpPut("review")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReviewOrder([FromBody] OrderReviewDto dto)
        {
            if (dto.Status != OrderStatus.Approved && dto.Status != OrderStatus.Rejected && dto.Status != OrderStatus.Completed)
            {
                return BadRequest(new { message = "Status must be either Approved (2), Rejected (3), or Completed (4)." });
            }

            bool isUpdated = await _orderRepo.ReviewOrderAsync(dto);

            if (!isUpdated)
            {
                return NotFound(new { message = "Order not found or has already been reviewed." });
            }

            return Ok(new { message = $"Order status updated successfully to {dto.Status}." });
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _orderRepo.GetOrderDetailsAsync(id);

            if (order == null)
            {
                return NotFound(new { message = "Order not found." });
            }

            return Ok(order);
        }

        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User not found or not authenticated." });
            }
            int userId = int.Parse(userIdClaim.Value);

            var orders = await _orderRepo.GetOrdersByUserIdAsync(userId);
            return Ok(orders);
        }

        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrdersForAdmin([FromQuery] OrderStatus? status, [FromQuery] OrderType? type)
        {
            var orders = await _orderRepo.GetOrdersForAdminAsync(status, type);
            return Ok(orders);
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User not found or not authenticated." });
            }
            int userId = int.Parse(userIdClaim.Value);

            bool isCanceled = await _orderRepo.CancelOrderAsync(id, userId);

            if (!isCanceled)
            {
                return BadRequest(new
                {
                    message = "Cannot cancel order. It either does not exist, does not belong to you, or is no longer in Pending status."
                });
            }

            return Ok(new { message = "Order has been canceled successfully." });
        }

        [HttpGet("{carId}/check-availability")]
        public async Task<IActionResult> CheckAvailability( int carId,[FromQuery] OrderType orderType,[FromQuery] DateTime? startDate,[FromQuery] DateTime? endDate)
        {
            if (orderType == OrderType.Rent)
            {
                if (!startDate.HasValue || !endDate.HasValue || startDate >= endDate || startDate < DateTime.UtcNow.Date)
                {
                    return BadRequest(new { message = "Invalid or missing date range for rent order." });
                }
            }

            bool isAvailable = await _orderRepo.IsCarAvailableAsync(carId, orderType, startDate, endDate);

            return Ok(new
            {
                carId,
                orderType,
                startDate,
                endDate,
                isAvailable
            });
        }

        [HttpPost("buy")]
        public async Task<IActionResult> CreateBuyOrder([FromForm] BuyOrderCreateDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User not found or not authenticated." });
            }
            int userId = int.Parse(userIdClaim.Value);

            List<string> documentUrls = new List<string>();
            List<string> physicalFilePaths = new List<string>();

            try
            {
                if (dto.Documents != null && dto.Documents.Count > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "Uploads", "Orders");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    foreach (var file in dto.Documents)
                    {
                        if (file.Length > 0)
                        {
                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            physicalFilePaths.Add(filePath);

                            documentUrls.Add($"/Uploads/Orders/{uniqueFileName}");
                        }
                    }
                }

                int orderId = await _orderRepo.AddBuyOrderAsync(dto, userId, documentUrls);

                if (orderId > 0)
                {
                    return Ok(new { message = "Buy order created successfully.", id = orderId });
                }

                return BadRequest(new { message = "Failed to create the buy order." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                foreach (var path in physicalFilePaths)
                {
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }

                return StatusCode(500, new { message = "An error occurred.", details = ex.Message });
            }
        }

        [HttpPost("installment")]
        public async Task<IActionResult> CreateInstallmentOrder([FromForm] InstallmentOrderCreateDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User not found or not authenticated." });
            }
            int userId = int.Parse(userIdClaim.Value);

            List<string> documentUrls = new List<string>();
            List<string> physicalFilePaths = new List<string>();

            try
            {
                if (dto.Documents != null && dto.Documents.Count > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "Uploads", "Orders");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    foreach (var file in dto.Documents)
                    {
                        if (file.Length > 0)
                        {
                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            physicalFilePaths.Add(filePath);
                            documentUrls.Add($"/Uploads/Orders/{uniqueFileName}");
                        }
                    }
                }

                int orderId = await _orderRepo.AddInstallmentOrderAsync(dto, userId, documentUrls);

                if (orderId > 0)
                {
                    return Ok(new { message = "Installment order created successfully.", id = orderId });
                }

                return BadRequest(new { message = "Failed to create the installment order." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                foreach (var path in physicalFilePaths)
                {
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }

                return StatusCode(500, new { message = "An error occurred.", details = ex.Message });
            }
        }
    }
}