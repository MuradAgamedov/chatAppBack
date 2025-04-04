using chatApp.Interfaces;
using chatApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace chatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        private readonly IStatusService _statusService;
        private readonly IWebHostEnvironment _env;

        public StatusController(IStatusService statusService, IWebHostEnvironment env)
        {
            _statusService = statusService;
            _env = env;
        }

        [HttpPost("add")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadStatus([FromForm] UploadStatusDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                var status = await _statusService.UploadStatusAsync(userId, model, _env);
                return Ok(status);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetMyStatuses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var statuses = await _statusService.GetUserStatusesAsync(userId);
            return Ok(statuses);
        }

        [Authorize]
        [HttpGet("by-email/{email}")]
        public async Task<IActionResult> GetUserStatusesByEmail(string email)
        {
            var statuses = await _statusService.GetUserStatusesByEmailAsync(email);
            if (!statuses.Any()) return NotFound("Пользователь не найден");
            return Ok(statuses);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteStatus(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _statusService.DeleteStatusAsync(id, userId, _env);

            if (!result)
                return Forbid("Bu status sizə aid deyil və ya mövcud deyil.");

            return Ok(new { message = "Статус удалён" });
        }
    }
}
