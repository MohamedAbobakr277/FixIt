using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FixIt.BLL.Interfaces;
using System.Security.Claims;

namespace FixIt.PL.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnread()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var count = await _notificationService.GetUnreadCountAsync(userId);
        var notifications = await _notificationService.GetUserNotificationsAsync(userId, 5);

        return Ok(new
        {
            Count = count,
            Notifications = notifications.Select(n => new
            {
                n.Id,
                n.Title,
                n.Message,
                Url = n.RelatedEntityUrl,
                n.IsRead,
                Date = n.CreatedAt.ToString("MMM dd, h:mm tt"),
                IsNew = !n.IsRead
            })
        });
    }

    [HttpPost("mark-read/{id}")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        await _notificationService.MarkAsReadAsync(id, userId);
        return Ok();
    }

    [HttpGet("admin/read-and-redirect/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminReadAndRedirect(int id, [FromQuery] string url)
    {
        await _notificationService.MarkAdminNotificationAsReadAsync(id);
        
        if (string.IsNullOrEmpty(url) || !Url.IsLocalUrl(url))
        {
            return RedirectToAction("Dashboard", "Admin");
        }
        return Redirect(url);
    }
}
