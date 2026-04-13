using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TicketsSystem.Core.Interfaces;

namespace TicketsSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ApiBaseController
    {
        private readonly INotificationService _notificationService;
        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("getusernotifications/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserNotificationes(string userId)
            => ProcessResult(await _notificationService.GetUserNotificationsAsync(userId));

        [HttpPut("tooglenotificationreadstatus/{notificationId}")]
        [Authorize]
        public async Task<IActionResult> ToogleNotificationReadStatus(string notificationId)
            => ProcessResult(await _notificationService.ToggleNotificationReadStatusAsync(notificationId));
    }
}
