using Escale.API.DTOs.Subscriptions;

namespace Escale.API.Services.Interfaces;

public interface ISubscriptionService
{
    Task<SubscriptionResponseDto> TopUpAsync(TopUpSubscriptionRequestDto request);
    Task<SubscriptionResponseDto?> GetActiveSubscriptionAsync(Guid customerId);
    Task<List<SubscriptionResponseDto>> GetSubscriptionHistoryAsync(Guid customerId);
    Task<SubscriptionCustomerLookupResponseDto> LookupByCarAsync(LookupCarRequestDto request);
    Task<SubscriptionResponseDto> CancelSubscriptionAsync(Guid subscriptionId);
}
