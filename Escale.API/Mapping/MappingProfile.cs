using AutoMapper;
using Escale.API.Domain.Entities;
using Escale.API.Domain.Enums;
using Escale.API.DTOs.Auth;
using Escale.API.DTOs.Customers;
using Escale.API.DTOs.Dashboard;
using Escale.API.DTOs.FuelTypes;
using Escale.API.DTOs.Inventory;
using Escale.API.DTOs.Settings;
using Escale.API.DTOs.Shifts;
using Escale.API.DTOs.Stations;
using Escale.API.DTOs.Stock;
using Escale.API.DTOs.Subscriptions;
using Escale.API.DTOs.Transactions;
using Escale.API.DTOs.Organizations;
using Escale.API.DTOs.Users;

namespace Escale.API.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Station
        CreateMap<Station, StationResponseDto>()
            .ForMember(d => d.Manager, o => o.MapFrom(s => s.Manager != null ? s.Manager.FullName : null));
        CreateMap<Station, StationInfoDto>();
        CreateMap<Station, StationDetailResponseDto>()
            .ForMember(d => d.Manager, o => o.MapFrom(s => s.Manager != null ? s.Manager.FullName : null));
        CreateMap<CreateStationRequestDto, Station>();
        CreateMap<UpdateStationRequestDto, Station>();

        // User
        CreateMap<User, UserResponseDto>()
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()))
            .ForMember(d => d.AssignedStations, o => o.MapFrom(s =>
                s.UserStations.Select(us => us.Station).ToList()));
        CreateMap<User, UserInfoDto>()
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()))
            .ForMember(d => d.AssignedStations, o => o.MapFrom(s =>
                s.UserStations.Select(us => us.Station).ToList()));
        CreateMap<CreateUserRequestDto, User>();

        // FuelType
        CreateMap<FuelType, FuelTypeResponseDto>()
            .ForMember(d => d.PricePerLiter, o => o.MapFrom(s => s.CurrentPrice));
        CreateMap<CreateFuelTypeRequestDto, FuelType>()
            .ForMember(d => d.CurrentPrice, o => o.MapFrom(s => s.PricePerLiter));

        // Customer
        CreateMap<Customer, CustomerResponseDto>()
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.ActiveSubscription, o => o.MapFrom(s =>
                s.Subscriptions
                    .Where(sub => sub.Status == SubscriptionStatus.Active && !sub.IsDeleted)
                    .OrderByDescending(sub => sub.StartDate)
                    .FirstOrDefault()));
        CreateMap<CreateCustomerRequestDto, Customer>();
        CreateMap<UpdateCustomerRequestDto, Customer>();

        // Car
        CreateMap<Car, CarResponseDto>();
        CreateMap<CarDto, Car>()
            .ForMember(d => d.PINHash, o => o.Ignore());

        // Subscription
        CreateMap<Subscription, SubscriptionResponseDto>()
            .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer != null ? s.Customer.Name : string.Empty))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        // Transaction
        CreateMap<Transaction, TransactionResponseDto>()
            .ForMember(d => d.FuelType, o => o.MapFrom(s => s.FuelType.Name))
            .ForMember(d => d.VAT, o => o.MapFrom(s => s.VATAmount))
            .ForMember(d => d.PaymentMethod, o => o.MapFrom(s => s.PaymentMethod.ToString()))
            .ForMember(d => d.CashierName, o => o.MapFrom(s => s.Cashier.FullName))
            .ForMember(d => d.StationName, o => o.MapFrom(s => s.Station.Name))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
        CreateMap<Transaction, RecentTransactionDto>()
            .ForMember(d => d.FuelType, o => o.MapFrom(s => s.FuelType.Name));

        // InventoryItem
        CreateMap<InventoryItem, InventoryItemResponseDto>()
            .ForMember(d => d.StationName, o => o.MapFrom(s => s.Station.Name))
            .ForMember(d => d.FuelType, o => o.MapFrom(s => s.FuelType.Name))
            .ForMember(d => d.PercentageFull, o => o.MapFrom(s => s.Capacity > 0 ? Math.Round(s.CurrentLevel / s.Capacity * 100, 1) : 0))
            .ForMember(d => d.LastRefill, o => o.MapFrom(s => s.LastRefillDate));
        CreateMap<InventoryItem, StockLevelDto>()
            .ForMember(d => d.FuelType, o => o.MapFrom(s => s.FuelType.Name))
            .ForMember(d => d.PercentageFull, o => o.MapFrom(s => s.Capacity > 0 ? Math.Round(s.CurrentLevel / s.Capacity * 100, 1) : 0))
            .ForMember(d => d.LastUpdated, o => o.MapFrom(s => s.UpdatedAt ?? s.CreatedAt));

        // RefillRecord
        CreateMap<RefillRecord, RefillRecordResponseDto>()
            .ForMember(d => d.StationName, o => o.MapFrom(s => s.InventoryItem.Station.Name))
            .ForMember(d => d.FuelType, o => o.MapFrom(s => s.InventoryItem.FuelType.Name))
            .ForMember(d => d.RecordedBy, o => o.MapFrom(s => s.RecordedBy.FullName));

        // Shift
        CreateMap<Shift, ShiftResponseDto>();

        // OrganizationSettings
        CreateMap<OrganizationSettings, AppSettingsResponseDto>();
        CreateMap<UpdateSettingsRequestDto, OrganizationSettings>();

        // Organization
        CreateMap<Organization, OrganizationResponseDto>()
            .ForMember(d => d.StationCount, o => o.MapFrom(s => s.Stations.Count))
            .ForMember(d => d.UserCount, o => o.MapFrom(s => s.Users.Count));
        CreateMap<CreateOrganizationRequestDto, Organization>();
    }
}
