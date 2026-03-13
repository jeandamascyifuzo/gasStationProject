using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAssertions;
using Moq;
using System.Collections.ObjectModel;
using Xunit;

namespace Escale.Mobile.Tests.ViewModels;

/// <summary>
/// Tests for the DashboardViewModel.
/// Since MAUI projects can't be directly referenced from standard test projects,
/// we replicate the ViewModel and model types here for isolated testing.
/// This validates the business logic, property updates, and command behavior.
/// </summary>
public class DashboardViewModelTests
{
    private readonly Mock<IApiService> _apiServiceMock;
    private readonly Mock<ISignalRService> _signalRServiceMock;
    private readonly TestDashboardViewModel _viewModel;

    public DashboardViewModelTests()
    {
        _apiServiceMock = new Mock<IApiService>();
        _signalRServiceMock = new Mock<ISignalRService>();
        _viewModel = new TestDashboardViewModel(
            _apiServiceMock.Object,
            _signalRServiceMock.Object);
    }

    #region Initial State

    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        _viewModel.UserName.Should().BeEmpty();
        _viewModel.StationName.Should().BeEmpty();
        _viewModel.TodaysSales.Should().Be(0);
        _viewModel.TransactionCount.Should().Be(0);
        _viewModel.AverageSale.Should().Be(0);
        _viewModel.LowStockAlerts.Should().BeEmpty();
        _viewModel.RecentTransactions.Should().BeEmpty();
        _viewModel.HasLowStock.Should().BeFalse();
        _viewModel.IsRefreshing.Should().BeFalse();
        _viewModel.IsLoading.Should().BeTrue();
        _viewModel.HasData.Should().BeFalse();
    }

    #endregion

    #region LoadDashboard

    [Fact]
    public async Task LoadDashboard_PopulatesAllProperties_FromApiResponse()
    {
        // Arrange
        var summary = new DashboardSummaryResponse
        {
            TodaysSales = 500000,
            TransactionCount = 42,
            AverageSale = 11905,
            LowStockAlerts = new List<StockAlertResponse>
            {
                new() { FuelType = "Diesel", CurrentLevel = 1000, Capacity = 20000 }
            },
            RecentTransactions = new List<RecentTransactionResponse>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    FuelType = "Petrol",
                    TransactionDate = DateTime.UtcNow,
                    Total = 15000,
                    Liters = 10,
                    EBMSent = true,
                    EBMReceiptUrl = "https://ebm.example.com/receipt/1"
                }
            }
        };
        _apiServiceMock.Setup(a => a.GetDashboardSummaryAsync(It.IsAny<Guid?>()))
            .ReturnsAsync(summary);

        _viewModel.SetTestUser("John Doe", "Main Station", Guid.NewGuid());

        // Act
        await _viewModel.LoadFromApiPublic();

        // Assert
        _viewModel.TodaysSales.Should().Be(500000);
        _viewModel.TransactionCount.Should().Be(42);
        _viewModel.AverageSale.Should().Be(11905);
        _viewModel.HasLowStock.Should().BeTrue();
        _viewModel.LowStockAlerts.Should().HaveCount(1);
        _viewModel.LowStockAlerts[0].FuelType.Should().Be("Diesel");
        _viewModel.RecentTransactions.Should().HaveCount(1);
        _viewModel.RecentTransactions[0].FuelType.Should().Be("Petrol");
        _viewModel.HasData.Should().BeTrue();
        _viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoadDashboard_SetsHasDataFalse_WhenApiThrows()
    {
        // Arrange
        _apiServiceMock.Setup(a => a.GetDashboardSummaryAsync(It.IsAny<Guid?>()))
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        // Act
        await _viewModel.LoadFromApiPublic();

        // Assert
        _viewModel.HasData.Should().BeFalse();
        _viewModel.TodaysSales.Should().Be(0);
        _viewModel.TransactionCount.Should().Be(0);
        _viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoadDashboard_SetsLoadingTrue_ThenFalse()
    {
        // Arrange
        var loadingStates = new List<bool>();
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.IsLoading))
                loadingStates.Add(_viewModel.IsLoading);
        };

        _apiServiceMock.Setup(a => a.GetDashboardSummaryAsync(It.IsAny<Guid?>()))
            .ReturnsAsync(new DashboardSummaryResponse());

        // Act
        await _viewModel.LoadFromApiPublic();

        // Assert
        loadingStates.Should().Contain(false); // at minimum, IsLoading should be set to false at the end
    }

    [Fact]
    public async Task LoadDashboard_HasLowStock_IsFalse_WhenNoAlerts()
    {
        // Arrange
        var summary = new DashboardSummaryResponse
        {
            LowStockAlerts = new List<StockAlertResponse>(),
            RecentTransactions = new List<RecentTransactionResponse>()
        };
        _apiServiceMock.Setup(a => a.GetDashboardSummaryAsync(It.IsAny<Guid?>()))
            .ReturnsAsync(summary);

        // Act
        await _viewModel.LoadFromApiPublic();

        // Assert
        _viewModel.HasLowStock.Should().BeFalse();
        _viewModel.LowStockAlerts.Should().BeEmpty();
    }

    #endregion

    #region Refresh Command

    [Fact]
    public async Task RefreshCommand_SetsIsRefreshing_TrueThenFalse()
    {
        // Arrange
        var refreshingStates = new List<bool>();
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.IsRefreshing))
                refreshingStates.Add(_viewModel.IsRefreshing);
        };

        _apiServiceMock.Setup(a => a.GetDashboardSummaryAsync(It.IsAny<Guid?>()))
            .ReturnsAsync(new DashboardSummaryResponse
            {
                LowStockAlerts = new(),
                RecentTransactions = new()
            });

        _viewModel.SetTestUser("User", "Station", Guid.NewGuid());

        // Act
        await _viewModel.RefreshPublic();

        // Assert
        refreshingStates.Should().Contain(true);
        refreshingStates.Last().Should().BeFalse();
    }

    [Fact]
    public async Task RefreshCommand_UpdatesUserAndStationNames()
    {
        // Arrange
        _apiServiceMock.Setup(a => a.GetDashboardSummaryAsync(It.IsAny<Guid?>()))
            .ReturnsAsync(new DashboardSummaryResponse
            {
                LowStockAlerts = new(),
                RecentTransactions = new()
            });

        _viewModel.SetTestUser("Jane Doe", "South Station", Guid.NewGuid());

        // Act
        await _viewModel.RefreshPublic();

        // Assert
        _viewModel.UserName.Should().Be("Jane Doe");
        _viewModel.StationName.Should().Be("South Station");
    }

    [Fact]
    public async Task RefreshCommand_CallsApiService()
    {
        // Arrange
        var stationId = Guid.NewGuid();
        _apiServiceMock.Setup(a => a.GetDashboardSummaryAsync(stationId))
            .ReturnsAsync(new DashboardSummaryResponse
            {
                LowStockAlerts = new(),
                RecentTransactions = new()
            });

        _viewModel.SetTestUser("User", "Station", stationId);

        // Act
        await _viewModel.RefreshPublic();

        // Assert
        _apiServiceMock.Verify(a => a.GetDashboardSummaryAsync(stationId), Times.Once);
    }

    #endregion

    #region Property Change Notifications

    [Fact]
    public async Task PropertyChanged_IsFiredForTodaysSales()
    {
        // Arrange
        var changedProperties = new List<string>();
        _viewModel.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName!);

        _apiServiceMock.Setup(a => a.GetDashboardSummaryAsync(It.IsAny<Guid?>()))
            .ReturnsAsync(new DashboardSummaryResponse
            {
                TodaysSales = 100000,
                LowStockAlerts = new(),
                RecentTransactions = new()
            });

        // Act
        await _viewModel.LoadFromApiPublic();

        // Assert
        changedProperties.Should().Contain(nameof(_viewModel.TodaysSales));
    }

    [Fact]
    public void SettingIsLoading_RaisesPropertyChanged()
    {
        // Arrange
        var raised = false;
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.IsLoading))
                raised = true;
        };

        // Act
        _viewModel.IsLoading = false;

        // Assert
        raised.Should().BeTrue();
    }

    #endregion

    #region Data Transformation

    [Fact]
    public async Task LoadDashboard_MapsStockAlerts_Correctly()
    {
        // Arrange
        var summary = new DashboardSummaryResponse
        {
            LowStockAlerts = new List<StockAlertResponse>
            {
                new() { FuelType = "Diesel", CurrentLevel = 2000, Capacity = 20000 },
                new() { FuelType = "Petrol", CurrentLevel = 500, Capacity = 15000 }
            },
            RecentTransactions = new()
        };
        _apiServiceMock.Setup(a => a.GetDashboardSummaryAsync(It.IsAny<Guid?>()))
            .ReturnsAsync(summary);

        // Act
        await _viewModel.LoadFromApiPublic();

        // Assert
        _viewModel.LowStockAlerts.Should().HaveCount(2);
        _viewModel.LowStockAlerts[0].FuelType.Should().Be("Diesel");
        _viewModel.LowStockAlerts[0].CurrentLevel.Should().Be(2000);
        _viewModel.LowStockAlerts[0].Capacity.Should().Be(20000);
        _viewModel.LowStockAlerts[1].FuelType.Should().Be("Petrol");
    }

    [Fact]
    public async Task LoadDashboard_ClearsOldTransactions_BeforeAddingNew()
    {
        // Arrange - first load with 2 transactions
        var summary1 = new DashboardSummaryResponse
        {
            LowStockAlerts = new(),
            RecentTransactions = new List<RecentTransactionResponse>
            {
                new() { Id = Guid.NewGuid(), FuelType = "A", Total = 100, Liters = 1 },
                new() { Id = Guid.NewGuid(), FuelType = "B", Total = 200, Liters = 2 }
            }
        };
        _apiServiceMock.Setup(a => a.GetDashboardSummaryAsync(It.IsAny<Guid?>()))
            .ReturnsAsync(summary1);
        await _viewModel.LoadFromApiPublic();
        _viewModel.RecentTransactions.Should().HaveCount(2);

        // Arrange - second load with 1 transaction
        var summary2 = new DashboardSummaryResponse
        {
            LowStockAlerts = new(),
            RecentTransactions = new List<RecentTransactionResponse>
            {
                new() { Id = Guid.NewGuid(), FuelType = "C", Total = 300, Liters = 3 }
            }
        };
        _apiServiceMock.Setup(a => a.GetDashboardSummaryAsync(It.IsAny<Guid?>()))
            .ReturnsAsync(summary2);

        // Act
        await _viewModel.LoadFromApiPublic();

        // Assert - should only have the new transaction, not old ones
        _viewModel.RecentTransactions.Should().HaveCount(1);
        _viewModel.RecentTransactions[0].FuelType.Should().Be("C");
    }

    #endregion

    #region Notification Subscription

    [Fact]
    public void SubscribeToNotifications_AttachesHandler()
    {
        // Act
        _viewModel.SubscribeToNotifications();

        // Assert - we can verify indirectly by checking unsubscribe works
        _viewModel.HasSubscription.Should().BeTrue();
    }

    [Fact]
    public void UnsubscribeFromNotifications_DetachesHandler()
    {
        // Arrange
        _viewModel.SubscribeToNotifications();

        // Act
        _viewModel.UnsubscribeFromNotifications();

        // Assert
        _viewModel.HasSubscription.Should().BeFalse();
    }

    [Fact]
    public void SubscribeToNotifications_ReplacesExistingHandler()
    {
        // Arrange
        _viewModel.SubscribeToNotifications();

        // Act - subscribe again should not create double subscription
        _viewModel.SubscribeToNotifications();

        // Assert
        _viewModel.HasSubscription.Should().BeTrue();
        _viewModel.SubscriptionCount.Should().Be(1);
    }

    #endregion
}

#region Test Doubles (Interfaces and Models mirroring the MAUI project)

/// <summary>
/// Interface mirroring the ApiService's dashboard method signature.
/// </summary>
public interface IApiService
{
    Task<DashboardSummaryResponse> GetDashboardSummaryAsync(Guid? stationId);
}

/// <summary>
/// Interface mirroring the SignalRService's event pattern.
/// </summary>
public interface ISignalRService
{
    event Action<string>? OnDataChanged;
}

// Models mirroring Escale.mobile.Models
public class DashboardSummaryResponse
{
    public decimal TodaysSales { get; set; }
    public int TransactionCount { get; set; }
    public decimal AverageSale { get; set; }
    public List<StockAlertResponse> LowStockAlerts { get; set; } = new();
    public List<RecentTransactionResponse> RecentTransactions { get; set; } = new();
}

public class StockAlertResponse
{
    public string FuelType { get; set; } = string.Empty;
    public decimal CurrentLevel { get; set; }
    public decimal Capacity { get; set; }
    public decimal PercentageRemaining => Capacity > 0 ? (CurrentLevel / Capacity) * 100 : 0;
}

public class RecentTransactionResponse
{
    public Guid Id { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public decimal Total { get; set; }
    public decimal Liters { get; set; }
    public bool EBMSent { get; set; }
    public string? EBMReceiptUrl { get; set; }
}

public class StockAlert
{
    public string FuelType { get; set; } = string.Empty;
    public decimal CurrentLevel { get; set; }
    public decimal Capacity { get; set; }
}

public class RecentTransaction
{
    public Guid Id { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public decimal Total { get; set; }
    public decimal Liters { get; set; }
    public bool EBMSent { get; set; }
    public string? EBMReceiptUrl { get; set; }
}

/// <summary>
/// Test-friendly DashboardViewModel that mirrors the real one's business logic
/// but removes MAUI-specific dependencies (Shell, MainThread, AppState singleton).
/// This is the pattern for testing MAUI ViewModels without the MAUI platform.
/// </summary>
public partial class TestDashboardViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly ISignalRService _signalRService;
    private Action<string>? _dataChangedHandler;
    private int _subscriptionCount;

    // Test helpers for user/station context
    private string? _testUserName;
    private string? _testStationName;
    private Guid? _testStationId;

    [ObservableProperty]
    private string userName = string.Empty;

    [ObservableProperty]
    private string stationName = string.Empty;

    [ObservableProperty]
    private decimal todaysSales;

    [ObservableProperty]
    private int transactionCount;

    [ObservableProperty]
    private decimal averageSale;

    [ObservableProperty]
    private List<StockAlert> lowStockAlerts = new();

    [ObservableProperty]
    private ObservableCollection<RecentTransaction> recentTransactions = new();

    [ObservableProperty]
    private bool hasLowStock;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private bool isLoading = true;

    [ObservableProperty]
    private bool hasData;

    public bool HasSubscription => _dataChangedHandler != null;
    public int SubscriptionCount => _subscriptionCount;

    public TestDashboardViewModel(IApiService apiService, ISignalRService signalRService)
    {
        _apiService = apiService;
        _signalRService = signalRService;
    }

    public void SetTestUser(string name, string station, Guid stationId)
    {
        _testUserName = name;
        _testStationName = station;
        _testStationId = stationId;
    }

    public void SubscribeToNotifications()
    {
        UnsubscribeFromNotifications();
        _dataChangedHandler = changeType =>
        {
            if (changeType == "sale_completed" ||
                changeType == "inventory_changed" ||
                changeType == "fuel_types_changed" ||
                changeType == "station_changed")
            {
                // In real code: MainThread.BeginInvokeOnMainThread(() => RefreshCommand.Execute(null));
                // In tests we just track that this would trigger a refresh
            }
        };
        _signalRService.OnDataChanged += _dataChangedHandler;
        _subscriptionCount++;
    }

    public void UnsubscribeFromNotifications()
    {
        if (_dataChangedHandler != null)
        {
            _signalRService.OnDataChanged -= _dataChangedHandler;
            _dataChangedHandler = null;
            _subscriptionCount = Math.Max(0, _subscriptionCount - 1);
        }
    }

    /// <summary>
    /// Public wrapper for testing the API loading logic.
    /// Mirrors the private LoadFromApi() method in the real ViewModel.
    /// </summary>
    public async Task LoadFromApiPublic()
    {
        try
        {
            if (!IsRefreshing)
                IsLoading = true;

            var summary = await _apiService.GetDashboardSummaryAsync(_testStationId);

            TodaysSales = summary.TodaysSales;
            TransactionCount = summary.TransactionCount;
            AverageSale = summary.AverageSale;

            LowStockAlerts = summary.LowStockAlerts.Select(a => new StockAlert
            {
                FuelType = a.FuelType,
                CurrentLevel = a.CurrentLevel,
                Capacity = a.Capacity
            }).ToList();

            HasLowStock = LowStockAlerts.Count > 0;

            RecentTransactions.Clear();
            foreach (var t in summary.RecentTransactions)
            {
                RecentTransactions.Add(new RecentTransaction
                {
                    Id = t.Id,
                    FuelType = t.FuelType,
                    TransactionDate = t.TransactionDate,
                    Total = t.Total,
                    Liters = t.Liters,
                    EBMSent = t.EBMSent,
                    EBMReceiptUrl = t.EBMReceiptUrl
                });
            }

            HasData = true;
        }
        catch (Exception)
        {
            TodaysSales = 0;
            TransactionCount = 0;
            AverageSale = 0;
            LowStockAlerts = new List<StockAlert>();
            HasLowStock = false;
            RecentTransactions.Clear();
            HasData = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Public wrapper for testing the Refresh command logic.
    /// Mirrors the private Refresh() method in the real ViewModel.
    /// </summary>
    public async Task RefreshPublic()
    {
        IsRefreshing = true;
        try
        {
            UserName = _testUserName ?? "User";
            StationName = _testStationName ?? "Station";
            await LoadFromApiPublic();
        }
        finally
        {
            IsRefreshing = false;
        }
    }
}

#endregion
