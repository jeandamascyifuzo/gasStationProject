using Escale.API.DTOs.Subscriptions;
using Escale.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Escale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionsController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpPost("topup")]
    [Authorize(Roles = "Admin,Manager,SuperAdmin")]
    public async Task<IActionResult> TopUp([FromBody] TopUpSubscriptionRequestDto request)
    {
        var result = await _subscriptionService.TopUpAsync(request);
        return Ok(new { Success = true, Message = "Subscription topped up successfully", Data = result });
    }

    [HttpGet("customer/{customerId}/active")]
    public async Task<IActionResult> GetActive(Guid customerId)
    {
        var result = await _subscriptionService.GetActiveSubscriptionAsync(customerId);
        if (result == null)
            return Ok(new { Success = true, Message = "No active subscription", Data = (object?)null });
        return Ok(new { Success = true, Message = "Active subscription found", Data = result });
    }

    [HttpGet("customer/{customerId}/history")]
    public async Task<IActionResult> GetHistory(Guid customerId)
    {
        var result = await _subscriptionService.GetSubscriptionHistoryAsync(customerId);
        return Ok(new { Success = true, Message = "Subscription history retrieved", Data = result });
    }

    [HttpPost("lookup")]
    public async Task<IActionResult> Lookup([FromBody] LookupCarRequestDto request)
    {
        var result = await _subscriptionService.LookupByCarAsync(request);
        return Ok(new { Success = true, Message = "Lookup completed", Data = result });
    }

    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Admin,Manager,SuperAdmin")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _subscriptionService.CancelSubscriptionAsync(id);
        return Ok(new { Success = true, Message = "Subscription cancelled", Data = result });
    }
}
