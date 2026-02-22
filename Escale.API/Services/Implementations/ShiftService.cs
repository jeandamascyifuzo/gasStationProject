using AutoMapper;
using Escale.API.Data.Repositories;
using Escale.API.Domain.Entities;
using Escale.API.Domain.Enums;
using Escale.API.DTOs.Shifts;
using Escale.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Escale.API.Services.Implementations;

public class ShiftService : IShiftService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public ShiftService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<ShiftResponseDto?> GetCurrentShiftAsync(Guid userId, Guid stationId)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var shift = await _unitOfWork.Shifts.Query()
            .Include(s => s.Transactions)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.StationId == stationId && s.IsActive && s.OrganizationId == orgId);

        if (shift == null) return null;

        var dto = _mapper.Map<ShiftResponseDto>(shift);
        dto.TransactionCount = shift.Transactions.Count;
        dto.TotalSales = shift.Transactions.Sum(t => t.Total);
        return dto;
    }

    public async Task<ClockResponseDto> ClockAsync(ClockRequestDto request)
    {
        var orgId = _currentUser.OrganizationId!.Value;

        if (request.IsClockIn)
        {
            // Check for existing active shift
            var existing = await _unitOfWork.Shifts.Query()
                .FirstOrDefaultAsync(s => s.UserId == request.UserId && s.StationId == request.StationId && s.IsActive && s.OrganizationId == orgId);

            if (existing != null)
                return new ClockResponseDto { Success = false, Message = "Already clocked in at this station" };

            var shift = new Shift
            {
                OrganizationId = orgId,
                UserId = request.UserId,
                StationId = request.StationId,
                StartTime = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.Shifts.AddAsync(shift);
            await _unitOfWork.SaveChangesAsync();

            return new ClockResponseDto
            {
                Success = true,
                Message = "Clocked in successfully",
                Shift = _mapper.Map<ShiftResponseDto>(shift)
            };
        }
        else
        {
            var shift = await _unitOfWork.Shifts.Query()
                .Include(s => s.Transactions)
                .FirstOrDefaultAsync(s => s.UserId == request.UserId && s.StationId == request.StationId && s.IsActive && s.OrganizationId == orgId);

            if (shift == null)
                return new ClockResponseDto { Success = false, Message = "No active shift found" };

            shift.EndTime = DateTime.UtcNow;
            shift.IsActive = false;
            _unitOfWork.Shifts.Update(shift);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<ShiftResponseDto>(shift);
            dto.TransactionCount = shift.Transactions.Count;
            dto.TotalSales = shift.Transactions.Sum(t => t.Total);

            return new ClockResponseDto
            {
                Success = true,
                Message = "Clocked out successfully",
                Shift = dto
            };
        }
    }

    public async Task<ShiftSummaryDto?> GetShiftSummaryAsync(Guid userId, Guid stationId)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var shift = await _unitOfWork.Shifts.Query()
            .Include(s => s.Transactions)
            .Where(s => s.UserId == userId && s.StationId == stationId && s.OrganizationId == orgId)
            .OrderByDescending(s => s.StartTime)
            .FirstOrDefaultAsync();

        if (shift == null) return null;

        var duration = (shift.EndTime ?? DateTime.UtcNow) - shift.StartTime;

        return new ShiftSummaryDto
        {
            ShiftId = shift.Id,
            StartTime = shift.StartTime,
            EndTime = shift.EndTime,
            Duration = $"{(int)duration.TotalHours}h {duration.Minutes}m",
            TransactionCount = shift.Transactions.Count,
            TotalSales = shift.Transactions.Sum(t => t.Total),
            CashSales = shift.Transactions.Where(t => t.PaymentMethod == PaymentMethod.Cash).Sum(t => t.Total),
            MobileMoneySales = shift.Transactions.Where(t => t.PaymentMethod == PaymentMethod.MobileMoney).Sum(t => t.Total),
            CardSales = shift.Transactions.Where(t => t.PaymentMethod == PaymentMethod.Card).Sum(t => t.Total),
            CreditSales = shift.Transactions.Where(t => t.PaymentMethod == PaymentMethod.Credit).Sum(t => t.Total)
        };
    }
}
