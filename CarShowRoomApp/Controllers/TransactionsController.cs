using CarShowRoom.DAL.Repositories;
using CarShowRoomApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarShowRoomApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private const int MaxContractImages = 20;
        private const long MaxImageSize =
            10 * 1024 * 1024; // 10 MB

        private static readonly string[] AllowedExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        private static readonly string[] AllowedContentTypes =
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        private readonly TransactionRepository _repo;
        private readonly ImageService _imageService;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(
            TransactionRepository repo,
            ImageService imageService,
            ILogger<TransactionsController> logger)
        {
            _repo = repo;
            _imageService = imageService;
            _logger = logger;
        }

        // إضافة سجل جديد مع صور العقد
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateTransaction(
            [FromForm] TransactionCreateDto dto)
        {
            int? adminId = GetCurrentUserId();

            if (!adminId.HasValue)
            {
                return Unauthorized(new
                {
                    message = "User identity was not found."
                });
            }

            string? validationError =
                ValidateContractImages(dto.ContractImages);

            if (validationError != null)
            {
                return BadRequest(new
                {
                    message = validationError
                });
            }

            var savedImageUrls = new List<string>();

            try
            {
                savedImageUrls =
                    await SaveContractImagesAsync(
                        dto.ContractImages
                    );

                int transactionId =
                    await _repo.CreateTransactionAsync(
                        dto,
                        adminId.Value,
                        savedImageUrls
                    );

                return CreatedAtAction(
                    nameof(GetTransactionById),
                    new { id = transactionId },
                    new
                    {
                        message =
                            "Transaction created successfully.",
                        transactionId,
                        contractImages = savedImageUrls
                    }
                );
            }
            catch (InvalidOperationException ex)
            {
                DeleteSavedImages(savedImageUrls);

                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                DeleteSavedImages(savedImageUrls);

                _logger.LogError(
                    ex,
                    "Failed to create transaction."
                );

                return StatusCode(500, new
                {
                    message =
                        "An error occurred while creating the transaction."
                });
            }
        }

        // عرض جميع السجلات للأدمن
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllTransactions()
        {
            try
            {
                var transactions =
                    await _repo.GetAllTransactionsAsync();

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to retrieve transactions."
                );

                return StatusCode(500, new
                {
                    message =
                        "An error occurred while retrieving transactions."
                });
            }
        }

        // عرض سجل محدد
        // الأدمن أو المشتري أو البائع فقط
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetTransactionById(
            int id)
        {
            int? currentUserId = GetCurrentUserId();

            if (!currentUserId.HasValue)
            {
                return Unauthorized();
            }

            var transaction =
                await _repo.GetTransactionByIdAsync(id);

            if (transaction == null)
            {
                return NotFound(new
                {
                    message = "Transaction was not found."
                });
            }

            bool canView =
                User.IsInRole("Admin") ||
                transaction.BuyerId ==
                    currentUserId.Value ||
                transaction.SellerId ==
                    currentUserId.Value;

            if (!canView)
            {
                return Forbid();
            }

            return Ok(transaction);
        }

        // سجلات المستخدم الحالي كمشتري أو بائع
        [HttpGet("my-transactions")]
        public async Task<IActionResult> GetMyTransactions()
        {
            int? userId = GetCurrentUserId();

            if (!userId.HasValue)
            {
                return Unauthorized(new
                {
                    message = "User identity was not found."
                });
            }

            try
            {
                var transactions =
                    await _repo.GetTransactionsForUserAsync(
                        userId.Value
                    );

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to retrieve user transactions."
                );

                return StatusCode(500, new
                {
                    message =
                        "An error occurred while retrieving your transactions."
                });
            }
        }

        // عرض سجلات مستخدم معين للأدمن
        [HttpGet("user/{userId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult>
            GetTransactionsForUser(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid user ID."
                });
            }

            var transactions =
                await _repo.GetTransactionsForUserAsync(
                    userId
                );

            return Ok(transactions);
        }

        // عرض سجلات طلب معين
        [HttpGet("order/{orderId:int}")]
        public async Task<IActionResult>
            GetTransactionsByOrderId(int orderId)
        {
            int? currentUserId = GetCurrentUserId();

            if (!currentUserId.HasValue)
            {
                return Unauthorized();
            }

            var transactions =
                await _repo.GetTransactionsByOrderIdAsync(
                    orderId
                );

            if (transactions.Count == 0)
            {
                return Ok(transactions);
            }

            var firstTransaction = transactions[0];

            bool canView =
                User.IsInRole("Admin") ||
                firstTransaction.BuyerId ==
                    currentUserId.Value ||
                firstTransaction.SellerId ==
                    currentUserId.Value;

            if (!canView)
            {
                return Forbid();
            }

            return Ok(transactions);
        }

        // تعديل سجل وإضافة أو حذف صور
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateTransaction(
            int id,
            [FromForm] TransactionUpdateDto dto)
        {
            int? adminId = GetCurrentUserId();

            if (!adminId.HasValue)
            {
                return Unauthorized();
            }

            string? validationError =
                ValidateContractImages(
                    dto.NewContractImages
                );

            if (validationError != null)
            {
                return BadRequest(new
                {
                    message = validationError
                });
            }

            var newSavedImageUrls =
                new List<string>();

            try
            {
                newSavedImageUrls =
                    await SaveContractImagesAsync(
                        dto.NewContractImages
                    );

                TransactionUpdateResult result =
                    await _repo.UpdateTransactionAsync(
                        id,
                        dto,
                        adminId.Value,
                        newSavedImageUrls
                    );

                if (!result.Success)
                {
                    DeleteSavedImages(
                        newSavedImageUrls
                    );

                    return NotFound(new
                    {
                        message =
                            "Transaction was not found."
                    });
                }

                // حذف الملفات التي اختار الأدمن إزالتها
                DeleteSavedImages(
                    result.DeletedImageUrls
                );

                return Ok(new
                {
                    message =
                        "Transaction updated successfully.",
                    transactionId = id
                });
            }
            catch (InvalidOperationException ex)
            {
                DeleteSavedImages(
                    newSavedImageUrls
                );

                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                DeleteSavedImages(
                    newSavedImageUrls
                );

                _logger.LogError(
                    ex,
                    "Failed to update transaction {Id}.",
                    id
                );

                return StatusCode(500, new
                {
                    message =
                        "An error occurred while updating the transaction."
                });
            }
        }

        // حذف منطقي للسجل
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTransaction(
            int id)
        {
            int? adminId = GetCurrentUserId();

            if (!adminId.HasValue)
            {
                return Unauthorized();
            }

            try
            {
                bool deleted =
                    await _repo.SoftDeleteTransactionAsync(
                        id,
                        adminId.Value
                    );

                if (!deleted)
                {
                    return NotFound(new
                    {
                        message =
                            "Transaction was not found or was already deleted."
                    });
                }

                return Ok(new
                {
                    message =
                        "Transaction deleted successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to delete transaction {Id}.",
                    id
                );

                return StatusCode(500, new
                {
                    message =
                        "An error occurred while deleting the transaction."
                });
            }
        }

        private int? GetCurrentUserId()
        {
            string? value =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            return int.TryParse(value, out int userId)
                ? userId
                : null;
        }

        private static string? ValidateContractImages(
            IReadOnlyCollection<IFormFile>? images)
        {
            if (images == null || images.Count == 0)
            {
                return null;
            }

            if (images.Count > MaxContractImages)
            {
                return
                    $"You can upload a maximum of {MaxContractImages} contract images.";
            }

            foreach (var image in images)
            {
                if (image.Length == 0)
                {
                    return
                        $"File '{image.FileName}' is empty.";
                }

                if (image.Length > MaxImageSize)
                {
                    return
                        $"File '{image.FileName}' exceeds the 10 MB limit.";
                }

                string extension =
                    Path.GetExtension(image.FileName)
                        .ToLowerInvariant();

                if (!AllowedExtensions.Contains(extension))
                {
                    return
                        $"File '{image.FileName}' has an unsupported extension.";
                }

                if (!AllowedContentTypes.Contains(
                    image.ContentType.ToLowerInvariant()))
                {
                    return
                        $"File '{image.FileName}' is not a supported image.";
                }
            }

            return null;
        }

        private async Task<List<string>>
            SaveContractImagesAsync(
                IEnumerable<IFormFile>? images)
        {
            var urls = new List<string>();

            if (images == null)
            {
                return urls;
            }

            foreach (var image in images)
            {
                string url =
                    await _imageService.SaveImageAsync(
                        image,
                        "contracts"
                    );

                if (!string.IsNullOrWhiteSpace(url))
                {
                    urls.Add(url);
                }
            }

            return urls;
        }

        private void DeleteSavedImages(
            IEnumerable<string>? imageUrls)
        {
            if (imageUrls == null)
            {
                return;
            }

            foreach (string url in imageUrls)
            {
                _imageService.DeleteImage(url);
            }
        }
    }
}