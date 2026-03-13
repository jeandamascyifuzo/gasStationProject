using AutoMapper;
using Escale.API.Data.Repositories;
using Escale.API.Domain.Entities;
using Escale.API.DTOs.EBM;
using Escale.API.DTOs.FuelTypes;
using Escale.API.Services.Implementations;
using Escale.API.Services.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace Escale.API.Tests.Services;

public class FuelTypeServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IEBMService> _ebmServiceMock;
    private readonly Mock<INotificationService> _notificationMock;
    private readonly Mock<ILogger<FuelTypeService>> _loggerMock;
    private readonly FuelTypeService _service;

    private readonly Guid _orgId = Guid.NewGuid();
    private readonly Guid _fuelTypeId = Guid.NewGuid();

    public FuelTypeServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _mapperMock = new Mock<IMapper>();
        _ebmServiceMock = new Mock<IEBMService>();
        _notificationMock = new Mock<INotificationService>();
        _loggerMock = new Mock<ILogger<FuelTypeService>>();

        _currentUserMock.Setup(c => c.OrganizationId).Returns(_orgId);

        _service = new FuelTypeService(
            _unitOfWorkMock.Object,
            _currentUserMock.Object,
            _mapperMock.Object,
            _ebmServiceMock.Object,
            _notificationMock.Object,
            _loggerMock.Object);
    }

    // Helper: set up common mocks for update tests
    private FuelType SetupFuelTypeForUpdate(decimal currentPrice = 1200, string? ebmVariantId = null, decimal? ebmSupplyPrice = null)
    {
        var fuelType = new FuelType
        {
            Id = _fuelTypeId,
            OrganizationId = _orgId,
            Name = "Diesel",
            CurrentPrice = currentPrice,
            IsActive = true,
            EBMVariantId = ebmVariantId,
            EBMSupplyPrice = ebmSupplyPrice
        };
        var fuelTypes = new List<FuelType> { fuelType }.AsQueryable().BuildMock();
        _unitOfWorkMock.Setup(u => u.FuelTypes.Query()).Returns(fuelTypes);
        return fuelType;
    }

    private void SetupOrgSettings(bool ebmEnabled)
    {
        var orgSettings = new List<OrganizationSettings>
        {
            new() { OrganizationId = _orgId, EBMEnabled = ebmEnabled }
        }.AsQueryable().BuildMock();
        _unitOfWorkMock.Setup(u => u.OrganizationSettings.Query()).Returns(orgSettings);
    }

    private void SetupSaveAndNotify()
    {
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _notificationMock.Setup(n => n.NotifyDataChangedAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
    }

    private void SetupMapperForUpdate()
    {
        _mapperMock.Setup(m => m.Map<FuelTypeResponseDto>(It.IsAny<FuelType>()))
            .Returns((FuelType ft) => new FuelTypeResponseDto
            {
                Id = ft.Id,
                Name = ft.Name,
                PricePerLiter = ft.CurrentPrice,
                IsActive = ft.IsActive,
                EBMProductId = ft.EBMProductId,
                EBMVariantId = ft.EBMVariantId,
                EBMSupplyPrice = ft.EBMSupplyPrice,
                EBMRegistered = ft.EBMProductId != null,
                IsDeleted = ft.IsDeleted,
                DeletedAt = ft.DeletedAt,
                CreatedAt = ft.CreatedAt
            });
    }

    #region GetFuelTypesAsync

    [Fact]
    public async Task GetFuelTypesAsync_ReturnsFilteredByOrg_OrderedByName()
    {
        // Arrange
        var fuelTypes = new List<FuelType>
        {
            new() { Id = Guid.NewGuid(), Name = "Petrol", OrganizationId = _orgId, CurrentPrice = 1350 },
            new() { Id = Guid.NewGuid(), Name = "Diesel", OrganizationId = _orgId, CurrentPrice = 1200 },
            new() { Id = Guid.NewGuid(), Name = "Kerosene", OrganizationId = Guid.NewGuid(), CurrentPrice = 900 }
        };
        var queryable = fuelTypes.AsQueryable().BuildMock();
        _unitOfWorkMock.Setup(u => u.FuelTypes.Query()).Returns(queryable);

        var expectedDtos = new List<FuelTypeResponseDto>
        {
            new() { Name = "Diesel", PricePerLiter = 1200 },
            new() { Name = "Petrol", PricePerLiter = 1350 }
        };
        _mapperMock.Setup(m => m.Map<List<FuelTypeResponseDto>>(It.IsAny<List<FuelType>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _service.GetFuelTypesAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Diesel");
        result[1].Name.Should().Be("Petrol");
    }

    #endregion

    #region GetFuelTypeByIdAsync

    [Fact]
    public async Task GetFuelTypeByIdAsync_ReturnsFuelType_WhenExistsInOrg()
    {
        var fuelType = new FuelType { Id = _fuelTypeId, Name = "Diesel", OrganizationId = _orgId, CurrentPrice = 1200 };
        var queryable = new List<FuelType> { fuelType }.AsQueryable().BuildMock();
        _unitOfWorkMock.Setup(u => u.FuelTypes.Query()).Returns(queryable);

        var expectedDto = new FuelTypeResponseDto { Id = _fuelTypeId, Name = "Diesel", PricePerLiter = 1200 };
        _mapperMock.Setup(m => m.Map<FuelTypeResponseDto>(fuelType)).Returns(expectedDto);

        var result = await _service.GetFuelTypeByIdAsync(_fuelTypeId);

        result.Should().NotBeNull();
        result.Id.Should().Be(_fuelTypeId);
        result.Name.Should().Be("Diesel");
    }

    [Fact]
    public async Task GetFuelTypeByIdAsync_ThrowsKeyNotFoundException_WhenNotFound()
    {
        var fuelTypes = new List<FuelType>().AsQueryable().BuildMock();
        _unitOfWorkMock.Setup(u => u.FuelTypes.Query()).Returns(fuelTypes);

        Func<Task> act = async () => await _service.GetFuelTypeByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Fuel type not found");
    }

    [Fact]
    public async Task GetFuelTypeByIdAsync_ThrowsKeyNotFoundException_WhenWrongOrg()
    {
        var fuelType = new FuelType { Id = _fuelTypeId, Name = "Diesel", OrganizationId = Guid.NewGuid(), CurrentPrice = 1200 };
        var fuelTypes = new List<FuelType> { fuelType }.AsQueryable().BuildMock();
        _unitOfWorkMock.Setup(u => u.FuelTypes.Query()).Returns(fuelTypes);

        Func<Task> act = async () => await _service.GetFuelTypeByIdAsync(_fuelTypeId);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region CreateFuelTypeAsync

    [Fact]
    public async Task CreateFuelTypeAsync_ThrowsInvalidOperation_WhenDuplicateName()
    {
        var request = new CreateFuelTypeRequestDto { Name = "Diesel", PricePerLiter = 1200 };
        _unitOfWorkMock.Setup(u => u.FuelTypes.ExistsAsync(It.IsAny<Expression<Func<FuelType, bool>>>()))
            .ReturnsAsync(true);

        Func<Task> act = async () => await _service.CreateFuelTypeAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Fuel type name already exists");
    }

    [Fact]
    public async Task CreateFuelTypeAsync_WithEBMDisabled_CreatesWithoutEBM()
    {
        var request = new CreateFuelTypeRequestDto { Name = "Petrol", PricePerLiter = 1350 };
        _unitOfWorkMock.Setup(u => u.FuelTypes.ExistsAsync(It.IsAny<Expression<Func<FuelType, bool>>>()))
            .ReturnsAsync(false);
        SetupOrgSettings(false);

        var stations = new List<Station>().AsQueryable().BuildMock();
        _unitOfWorkMock.Setup(u => u.Stations.Query()).Returns(stations);
        _unitOfWorkMock.Setup(u => u.FuelTypes.AddAsync(It.IsAny<FuelType>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.FuelPrices.AddAsync(It.IsAny<FuelPrice>())).Returns(Task.CompletedTask);
        SetupSaveAndNotify();

        var expectedDto = new FuelTypeResponseDto { Name = "Petrol", PricePerLiter = 1350, EBMRegistered = false };
        _mapperMock.Setup(m => m.Map<FuelTypeResponseDto>(It.IsAny<FuelType>())).Returns(expectedDto);

        var result = await _service.CreateFuelTypeAsync(request);

        result.Name.Should().Be("Petrol");
        result.EBMRegistered.Should().BeFalse();
        _ebmServiceMock.Verify(e => e.CreateProductAsync(It.IsAny<Guid>(), It.IsAny<string>(),
            It.IsAny<decimal>(), It.IsAny<decimal>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateFuelTypeAsync_WithNoOrgSettings_SkipsEBM()
    {
        // When org settings don't exist at all, EBM is treated as disabled
        var request = new CreateFuelTypeRequestDto { Name = "Petrol", PricePerLiter = 1350, EBMSupplyPrice = 1100 };
        _unitOfWorkMock.Setup(u => u.FuelTypes.ExistsAsync(It.IsAny<Expression<Func<FuelType, bool>>>()))
            .ReturnsAsync(false);

        // No org settings record exists
        var orgSettings = new List<OrganizationSettings>().AsQueryable().BuildMock();
        _unitOfWorkMock.Setup(u => u.OrganizationSettings.Query()).Returns(orgSettings);

        var stations = new List<Station>().AsQueryable().BuildMock();
        _unitOfWorkMock.Setup(u => u.Stations.Query()).Returns(stations);
        _unitOfWorkMock.Setup(u => u.FuelTypes.AddAsync(It.IsAny<FuelType>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.FuelPrices.AddAsync(It.IsAny<FuelPrice>())).Returns(Task.CompletedTask);
        SetupSaveAndNotify();

        _mapperMock.Setup(m => m.Map<FuelTypeResponseDto>(It.IsAny<FuelType>()))
            .Returns(new FuelTypeResponseDto { Name = "Petrol", EBMRegistered = false });

        var result = await _service.CreateFuelTypeAsync(request);

        result.EBMRegistered.Should().BeFalse();
        _ebmServiceMock.Verify(e => e.CreateProductAsync(It.IsAny<Guid>(), It.IsAny<string>(),
            It.IsAny<decimal>(), It.IsAny<decimal>()), Times.Never);
    }

    [Fact]
    public async Task CreateFuelTypeAsync_WithEBMEnabled_CallsEBMAndCreatesFuelType()
    {
        var request = new CreateFuelTypeRequestDto { Name = "Petrol", PricePerLiter = 1350, EBMSupplyPrice = 1100 };
        _unitOfWorkMock.Setup(u => u.FuelTypes.ExistsAsync(It.IsAny<Expression<Func<FuelType, bool>>>()))
            .ReturnsAsync(false);
        SetupOrgSettings(true);

        _ebmServiceMock.Setup(e => e.CreateProductAsync(_orgId, "Petrol", 1350, 1100))
            .ReturnsAsync(new EBMCreateProductResult
            {
                Success = true, ProductId = "ebm-prod-1", VariantId = "ebm-var-1", StockId = "ebm-stk-1"
            });

        var stations = new List<Station>
        {
            new() { Id = Guid.NewGuid(), OrganizationId = _orgId, IsActive = true, Name = "Station 1" }
        }.AsQueryable().BuildMock();
        _unitOfWorkMock.Setup(u => u.Stations.Query()).Returns(stations);
        _unitOfWorkMock.Setup(u => u.FuelTypes.AddAsync(It.IsAny<FuelType>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.FuelPrices.AddAsync(It.IsAny<FuelPrice>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.InventoryItems.AddAsync(It.IsAny<InventoryItem>())).Returns(Task.CompletedTask);
        SetupSaveAndNotify();

        var expectedDto = new FuelTypeResponseDto
        {
            Name = "Petrol", PricePerLiter = 1350, EBMRegistered = true, EBMProductId = "ebm-prod-1"
        };
        _mapperMock.Setup(m => m.Map<FuelTypeResponseDto>(It.IsAny<FuelType>())).Returns(expectedDto);

        var result = await _service.CreateFuelTypeAsync(request);

        result.EBMRegistered.Should().BeTrue();
        _ebmServiceMock.Verify(e => e.CreateProductAsync(_orgId, "Petrol", 1350, 1100), Times.Once);
        _unitOfWorkMock.Verify(u => u.InventoryItems.AddAsync(It.IsAny<InventoryItem>()), Times.Once);
    }

    [Fact]
    public async Task CreateFuelTypeAsync_WithEBMEnabled_NoSupplyPrice_DefaultsToZero()
    {
        var request = new CreateFuelTypeRequestDto { Name = "Petrol", PricePerLiter = 1350 };
        _unitOfWorkMock.Setup(u => u.FuelTypes.ExistsAsync(It.IsAny<Expression<Func<FuelType, bool>>>()))
            .ReturnsAsync(false);
        SetupOrgSettings(true);

        _ebmServiceMock.Setup(e => e.CreateProductAsync(_orgId, "Petrol", 1350, 0))
            .ReturnsAsync(new EBMCreateProductResult
            {
                Success = true, ProductId = "p1", VariantId = "v1", StockId = "s1"
            });

        var stations = new List<Station>().AsQueryable().BuildMock();
        _unitOfWorkMock.Setup(u => u.Stations.Query()).Returns(stations);
        _unitOfWorkMock.Setup(u => u.FuelTypes.AddAsync(It.IsAny<FuelType>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.FuelPrices.AddAsync(It.IsAny<FuelPrice>())).Returns(Task.CompletedTask);
        SetupSaveAndNotify();
        _mapperMock.Setup(m => m.Map<FuelTypeResponseDto>(It.IsAny<FuelType>()))
            .Returns(new FuelTypeResponseDto());

        await _service.CreateFuelTypeAsync(request);

        // Supply price defaults to 0 when not provided
        _ebmServiceMock.Verify(e => e.CreateProductAsync(_orgId, "Petrol", 1350, 0), Times.Once);
    }

    [Fact]
    public async Task CreateFuelTypeAsync_ThrowsInvalidOperation_WhenEBMFails()
    {
        var request = new CreateFuelTypeRequestDto { Name = "Petrol", PricePerLiter = 1350, EBMSupplyPrice = 1100 };
        _unitOfWorkMock.Setup(u => u.FuelTypes.ExistsAsync(It.IsAny<Expression<Func<FuelType, bool>>>()))
            .ReturnsAsync(false);
        SetupOrgSettings(true);

        _ebmServiceMock.Setup(e => e.CreateProductAsync(_orgId, "Petrol", 1350, 1100))
            .ReturnsAsync(new EBMCreateProductResult { Success = false, ErrorMessage = "EBM unavailable" });

        Func<Task> act = async () => await _service.CreateFuelTypeAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*EBM*");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateFuelTypeAsync_RevertsEBM_WhenDBSaveFails()
    {
        var request = new CreateFuelTypeRequestDto { Name = "Petrol", PricePerLiter = 1350, EBMSupplyPrice = 1100 };
        _unitOfWorkMock.Setup(u => u.FuelTypes.ExistsAsync(It.IsAny<Expression<Func<FuelType, bool>>>()))
            .ReturnsAsync(false);
        SetupOrgSettings(true);

        _ebmServiceMock.Setup(e => e.CreateProductAsync(_orgId, "Petrol", 1350, 1100))
            .ReturnsAsync(new EBMCreateProductResult
            {
                Success = true, ProductId = "ebm-prod-1", VariantId = "ebm-var-1", StockId = "ebm-stk-1"
            });

        var stations = new List<Station>().AsQueryable().BuildMock();
        _unitOfWorkMock.Setup(u => u.Stations.Query()).Returns(stations);
        _unitOfWorkMock.Setup(u => u.FuelTypes.AddAsync(It.IsAny<FuelType>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.FuelPrices.AddAsync(It.IsAny<FuelPrice>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ThrowsAsync(new Exception("DB connection lost"));
        _ebmServiceMock.Setup(e => e.DeleteProductAsync(_orgId, "ebm-prod-1")).ReturnsAsync(true);

        Func<Task> act = async () => await _service.CreateFuelTypeAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*EBM product has been reverted*");
        _ebmServiceMock.Verify(e => e.DeleteProductAsync(_orgId, "ebm-prod-1"), Times.Once);
    }

    [Fact]
    public async Task CreateFuelTypeAsync_LogsCritical_WhenDBFailsAndEBMRevertFails()
    {
        var request = new CreateFuelTypeRequestDto { Name = "Petrol", PricePerLiter = 1350, EBMSupplyPrice = 1100 };
        _unitOfWorkMock.Setup(u => u.FuelTypes.ExistsAsync(It.IsAny<Expression<Func<FuelType, bool>>>()))
            .ReturnsAsync(false);
        SetupOrgSettings(true);

        _ebmServiceMock.Setup(e => e.CreateProductAsync(_orgId, "Petrol", 1350, 1100))
            .ReturnsAsync(new EBMCreateProductResult
            {
                Success = true, ProductId = "ebm-prod-1", VariantId = "ebm-var-1", StockId = "ebm-stk-1"
            });

        var stations = new List<Station>().AsQueryable().BuildMock();
        _unitOfWorkMock.Setup(u => u.Stations.Query()).Returns(stations);
        _unitOfWorkMock.Setup(u => u.FuelTypes.AddAsync(It.IsAny<FuelType>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.FuelPrices.AddAsync(It.IsAny<FuelPrice>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ThrowsAsync(new Exception("DB failure"));
        // EBM revert also fails
        _ebmServiceMock.Setup(e => e.DeleteProductAsync(_orgId, "ebm-prod-1")).ReturnsAsync(false);

        Func<Task> act = async () => await _service.CreateFuelTypeAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _ebmServiceMock.Verify(e => e.DeleteProductAsync(_orgId, "ebm-prod-1"), Times.Once);
    }

    [Fact]
    public async Task CreateFuelTypeAsync_WithEBMEnabled_WithEBMVariantIdInRequest()
    {
        var request = new CreateFuelTypeRequestDto
        {
            Name = "Petrol", PricePerLiter = 1350, EBMSupplyPrice = 1100,
            EBMVariantId = "custom-variant"
        };
        _unitOfWorkMock.Setup(u => u.FuelTypes.ExistsAsync(It.IsAny<Expression<Func<FuelType, bool>>>()))
            .ReturnsAsync(false);
        SetupOrgSettings(true);

        _ebmServiceMock.Setup(e => e.CreateProductAsync(_orgId, "Petrol", 1350, 1100))
            .ReturnsAsync(new EBMCreateProductResult
            {
                Success = true, ProductId = "p1", VariantId = "ebm-returned-variant", StockId = "s1"
            });

        var stations = new List<Station>().AsQueryable().BuildMock();
        _unitOfWorkMock.Setup(u => u.Stations.Query()).Returns(stations);
        _unitOfWorkMock.Setup(u => u.FuelTypes.AddAsync(It.IsAny<FuelType>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.FuelPrices.AddAsync(It.IsAny<FuelPrice>())).Returns(Task.CompletedTask);
        SetupSaveAndNotify();
        _mapperMock.Setup(m => m.Map<FuelTypeResponseDto>(It.IsAny<FuelType>()))
            .Returns(new FuelTypeResponseDto());

        await _service.CreateFuelTypeAsync(request);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    #endregion

    #region DeleteFuelTypeAsync

    [Fact]
    public async Task DeleteFuelTypeAsync_RemovesFuelType_WhenExists()
    {
        var fuelType = new FuelType { Id = _fuelTypeId, OrganizationId = _orgId, Name = "Diesel" };
        var fuelTypes = new List<FuelType> { fuelType }.AsQueryable().BuildMock();
        _unitOfWorkMock.Setup(u => u.FuelTypes.Query()).Returns(fuelTypes);
        SetupSaveAndNotify();

        await _service.DeleteFuelTypeAsync(_fuelTypeId);

        _unitOfWorkMock.Verify(u => u.FuelTypes.Remove(fuelType), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteFuelTypeAsync_ThrowsKeyNotFoundException_WhenNotFound()
    {
        var fuelTypes = new List<FuelType>().AsQueryable().BuildMock();
        _unitOfWorkMock.Setup(u => u.FuelTypes.Query()).Returns(fuelTypes);

        Func<Task> act = async () => await _service.DeleteFuelTypeAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteFuelTypeAsync_SendsNotification_AfterDelete()
    {
        var fuelType = new FuelType { Id = _fuelTypeId, OrganizationId = _orgId, Name = "Diesel" };
        var fuelTypes = new List<FuelType> { fuelType }.AsQueryable().BuildMock();
        _unitOfWorkMock.Setup(u => u.FuelTypes.Query()).Returns(fuelTypes);
        SetupSaveAndNotify();

        await _service.DeleteFuelTypeAsync(_fuelTypeId);

        _notificationMock.Verify(
            n => n.NotifyDataChangedAsync(_orgId, Hubs.NotificationConstants.FuelTypesChanged), Times.Once);
    }

    #endregion

    #region UpdateFuelTypeAsync

    [Fact]
    public async Task UpdateFuelTypeAsync_ThrowsKeyNotFoundException_WhenNotFound()
    {
        var fuelTypes = new List<FuelType>().AsQueryable().BuildMock();
        _unitOfWorkMock.Setup(u => u.FuelTypes.Query()).Returns(fuelTypes);
        var request = new UpdateFuelTypeRequestDto { Name = "Updated", PricePerLiter = 1500, IsActive = true };

        Func<Task> act = async () => await _service.UpdateFuelTypeAsync(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateFuelTypeAsync_UpdatesNameAndStatus_WhenPriceUnchanged()
    {
        SetupFuelTypeForUpdate(1200);
        SetupOrgSettings(false);
        SetupSaveAndNotify();
        SetupMapperForUpdate();

        var request = new UpdateFuelTypeRequestDto { Name = "Diesel Pro", PricePerLiter = 1200, IsActive = true };

        var result = await _service.UpdateFuelTypeAsync(_fuelTypeId, request);

        result.Name.Should().Be("Diesel Pro");
        _unitOfWorkMock.Verify(u => u.FuelPrices.AddAsync(It.IsAny<FuelPrice>()), Times.Never);
    }

    [Fact]
    public async Task UpdateFuelTypeAsync_WithPriceChange_EBMDisabled_AddsFuelPriceRecord()
    {
        SetupFuelTypeForUpdate(1200);
        SetupOrgSettings(false);

        // Setup last price exists
        var lastPrice = new FuelPrice { FuelTypeId = _fuelTypeId, Price = 1200, EffectiveFrom = DateTime.UtcNow.AddDays(-10) };
        var fuelPrices = new List<FuelPrice> { lastPrice }.AsQueryable().BuildMock();
        _unitOfWorkMock.Setup(u => u.FuelPrices.Query()).Returns(fuelPrices);
        _unitOfWorkMock.Setup(u => u.FuelPrices.AddAsync(It.IsAny<FuelPrice>())).Returns(Task.CompletedTask);
        SetupSaveAndNotify();
        SetupMapperForUpdate();

        var request = new UpdateFuelTypeRequestDto { Name = "Diesel", PricePerLiter = 1500, IsActive = true };

        var result = await _service.UpdateFuelTypeAsync(_fuelTypeId, request);

        result.PricePerLiter.Should().Be(1500);
        _unitOfWorkMock.Verify(u => u.FuelPrices.AddAsync(It.IsAny<FuelPrice>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.FuelPrices.Update(lastPrice), Times.Once);
        lastPrice.EffectiveTo.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateFuelTypeAsync_WithPriceChange_NoLastPrice_SkipsUpdate()
    {
        SetupFuelTypeForUpdate(1200);
        SetupOrgSettings(false);

        // No existing FuelPrice record
        var fuelPrices = new List<FuelPrice>().AsQueryable().BuildMock();
        _unitOfWorkMock.Setup(u => u.FuelPrices.Query()).Returns(fuelPrices);
        _unitOfWorkMock.Setup(u => u.FuelPrices.AddAsync(It.IsAny<FuelPrice>())).Returns(Task.CompletedTask);
        SetupSaveAndNotify();
        SetupMapperForUpdate();

        var request = new UpdateFuelTypeRequestDto { Name = "Diesel", PricePerLiter = 1500, IsActive = true };

        await _service.UpdateFuelTypeAsync(_fuelTypeId, request);

        _unitOfWorkMock.Verify(u => u.FuelPrices.AddAsync(It.IsAny<FuelPrice>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.FuelPrices.Update(It.IsAny<FuelPrice>()), Times.Never);
    }

    [Fact]
    public async Task UpdateFuelTypeAsync_WithPriceChange_EBMEnabled_Success()
    {
        SetupFuelTypeForUpdate(1200, ebmVariantId: "var-1", ebmSupplyPrice: 1000);
        SetupOrgSettings(true);

        _ebmServiceMock.Setup(e => e.UpdatePriceAsync(_orgId, "var-1", 1500, 1100)).ReturnsAsync(true);

        var fuelPrices = new List<FuelPrice>().AsQueryable().BuildMock();
        _unitOfWorkMock.Setup(u => u.FuelPrices.Query()).Returns(fuelPrices);
        _unitOfWorkMock.Setup(u => u.FuelPrices.AddAsync(It.IsAny<FuelPrice>())).Returns(Task.CompletedTask);
        SetupSaveAndNotify();
        SetupMapperForUpdate();

        var request = new UpdateFuelTypeRequestDto
        {
            Name = "Diesel", PricePerLiter = 1500, IsActive = true, EBMSupplyPrice = 1100
        };

        var result = await _service.UpdateFuelTypeAsync(_fuelTypeId, request);

        result.PricePerLiter.Should().Be(1500);
        _ebmServiceMock.Verify(e => e.UpdatePriceAsync(_orgId, "var-1", 1500, 1100), Times.Once);
    }

    [Fact]
    public async Task UpdateFuelTypeAsync_WithPriceChange_EBMEnabled_NoVariantId_Throws()
    {
        SetupFuelTypeForUpdate(1200, ebmVariantId: null, ebmSupplyPrice: null);
        SetupOrgSettings(true);

        var request = new UpdateFuelTypeRequestDto { Name = "Diesel", PricePerLiter = 1500, IsActive = true };

        Func<Task> act = async () => await _service.UpdateFuelTypeAsync(_fuelTypeId, request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*EBM is enabled but*no EBM Variant ID*");
    }

    [Fact]
    public async Task UpdateFuelTypeAsync_WithPriceChange_EBMEnabled_EBMFails_Throws()
    {
        SetupFuelTypeForUpdate(1200, ebmVariantId: "var-1", ebmSupplyPrice: 1000);
        SetupOrgSettings(true);

        _ebmServiceMock.Setup(e => e.UpdatePriceAsync(_orgId, "var-1", 1500, It.IsAny<decimal>()))
            .ReturnsAsync(false);

        var request = new UpdateFuelTypeRequestDto
        {
            Name = "Diesel", PricePerLiter = 1500, IsActive = true, EBMSupplyPrice = 1100
        };

        Func<Task> act = async () => await _service.UpdateFuelTypeAsync(_fuelTypeId, request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Failed to update price in EBM*");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateFuelTypeAsync_DBFails_RevertsEBMPrice()
    {
        SetupFuelTypeForUpdate(1200, ebmVariantId: "var-1", ebmSupplyPrice: 1000);
        SetupOrgSettings(true);

        _ebmServiceMock.Setup(e => e.UpdatePriceAsync(_orgId, "var-1", 1500, 1100)).ReturnsAsync(true);

        var fuelPrices = new List<FuelPrice>().AsQueryable().BuildMock();
        _unitOfWorkMock.Setup(u => u.FuelPrices.Query()).Returns(fuelPrices);
        _unitOfWorkMock.Setup(u => u.FuelPrices.AddAsync(It.IsAny<FuelPrice>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ThrowsAsync(new Exception("DB timeout"));
        // EBM revert succeeds
        _ebmServiceMock.Setup(e => e.UpdatePriceAsync(_orgId, "var-1", 1200, 1000)).ReturnsAsync(true);

        var request = new UpdateFuelTypeRequestDto
        {
            Name = "Diesel", PricePerLiter = 1500, IsActive = true, EBMSupplyPrice = 1100
        };

        Func<Task> act = async () => await _service.UpdateFuelTypeAsync(_fuelTypeId, request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*EBM has been reverted*");
        // Should revert to old price
        _ebmServiceMock.Verify(e => e.UpdatePriceAsync(_orgId, "var-1", 1200, 1000), Times.Once);
    }

    [Fact]
    public async Task UpdateFuelTypeAsync_DBFails_EBMRevertFails_LogsCritical()
    {
        SetupFuelTypeForUpdate(1200, ebmVariantId: "var-1", ebmSupplyPrice: 1000);
        SetupOrgSettings(true);

        _ebmServiceMock.Setup(e => e.UpdatePriceAsync(_orgId, "var-1", 1500, 1100)).ReturnsAsync(true);

        var fuelPrices = new List<FuelPrice>().AsQueryable().BuildMock();
        _unitOfWorkMock.Setup(u => u.FuelPrices.Query()).Returns(fuelPrices);
        _unitOfWorkMock.Setup(u => u.FuelPrices.AddAsync(It.IsAny<FuelPrice>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ThrowsAsync(new Exception("DB failure"));
        // EBM revert also fails
        _ebmServiceMock.Setup(e => e.UpdatePriceAsync(_orgId, "var-1", 1200, 1000)).ReturnsAsync(false);

        var request = new UpdateFuelTypeRequestDto
        {
            Name = "Diesel", PricePerLiter = 1500, IsActive = true, EBMSupplyPrice = 1100
        };

        Func<Task> act = async () => await _service.UpdateFuelTypeAsync(_fuelTypeId, request);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _ebmServiceMock.Verify(e => e.UpdatePriceAsync(_orgId, "var-1", 1200, 1000), Times.Once);
    }

    [Fact]
    public async Task UpdateFuelTypeAsync_SupplyPriceOnly_EBMEnabled_UpdatesEBM()
    {
        // Only supply price changes, not retail price
        SetupFuelTypeForUpdate(1200, ebmVariantId: "var-1", ebmSupplyPrice: 1000);
        SetupOrgSettings(true);

        _ebmServiceMock.Setup(e => e.UpdatePriceAsync(_orgId, "var-1", 1200, 1100)).ReturnsAsync(true);
        SetupSaveAndNotify();
        SetupMapperForUpdate();

        var request = new UpdateFuelTypeRequestDto
        {
            Name = "Diesel", PricePerLiter = 1200, IsActive = true, EBMSupplyPrice = 1100 // only supply price changed
        };

        await _service.UpdateFuelTypeAsync(_fuelTypeId, request);

        _ebmServiceMock.Verify(e => e.UpdatePriceAsync(_orgId, "var-1", 1200, 1100), Times.Once);
        // No new FuelPrice record since retail price didn't change
        _unitOfWorkMock.Verify(u => u.FuelPrices.AddAsync(It.IsAny<FuelPrice>()), Times.Never);
    }

    [Fact]
    public async Task UpdateFuelTypeAsync_PreservesExistingEBMVariantId_WhenNotInRequest()
    {
        SetupFuelTypeForUpdate(1200, ebmVariantId: "existing-var", ebmSupplyPrice: 1000);
        SetupOrgSettings(false);
        SetupSaveAndNotify();
        SetupMapperForUpdate();

        var request = new UpdateFuelTypeRequestDto
        {
            Name = "Diesel Updated", PricePerLiter = 1200, IsActive = false
            // No EBMVariantId or EBMSupplyPrice in request
        };

        var result = await _service.UpdateFuelTypeAsync(_fuelTypeId, request);

        result.EBMVariantId.Should().Be("existing-var");
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateFuelTypeAsync_OverridesEBMVariantId_WhenInRequest()
    {
        SetupFuelTypeForUpdate(1200, ebmVariantId: "old-var", ebmSupplyPrice: 1000);
        SetupOrgSettings(false);
        SetupSaveAndNotify();
        SetupMapperForUpdate();

        var request = new UpdateFuelTypeRequestDto
        {
            Name = "Diesel", PricePerLiter = 1200, IsActive = true,
            EBMVariantId = "new-var", EBMSupplyPrice = 1100
        };

        var result = await _service.UpdateFuelTypeAsync(_fuelTypeId, request);

        result.EBMVariantId.Should().Be("new-var");
        result.EBMSupplyPrice.Should().Be(1100);
    }

    #endregion
}
