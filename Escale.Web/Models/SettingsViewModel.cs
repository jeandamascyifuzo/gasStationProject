using System.ComponentModel.DataAnnotations;

namespace Escale.Web.Models
{
    public class SettingsViewModel
    {
        public CompanySettings CompanySettings { get; set; } = new();
        public SystemSettings SystemSettings { get; set; } = new();
        public NotificationSettings NotificationSettings { get; set; } = new();
        public List<FuelPrice> FuelPrices { get; set; } = new();
    }

    public class CompanySettings
    {
        [Required]
        public string CompanyName { get; set; } = string.Empty;
        
        [Required]
        public string TIN { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Phone]
        public string Phone { get; set; } = string.Empty;
        
        public string Address { get; set; } = string.Empty;
        
        public string? Logo { get; set; }
        
        public string Currency { get; set; } = "RWF";
        
        public string TimeZone { get; set; } = "Africa/Kigali";
    }

    public class SystemSettings
    {
        public bool MaintenanceMode { get; set; }
        
        public bool AutoBackup { get; set; }
        
        public string BackupFrequency { get; set; } = "Daily";
        
        public int SessionTimeout { get; set; } = 30;
        
        public bool EnableEBM { get; set; } = true;
        
        public string EBMApiUrl { get; set; } = string.Empty;
        
        public string? EBMApiKey { get; set; }
        
        public bool AllowNegativeStock { get; set; }
        
        public int LowStockThreshold { get; set; } = 20;
    }

    public class NotificationSettings
    {
        public bool EmailNotifications { get; set; } = true;
        
        public bool SMSNotifications { get; set; }
        
        public bool LowStockAlerts { get; set; } = true;
        
        public bool DailyReports { get; set; } = true;
        
        public bool TransactionAlerts { get; set; }
        
        public string NotificationEmail { get; set; } = string.Empty;
        
        public string? NotificationPhone { get; set; }
    }

    public class FuelPrice
    {
        public int Id { get; set; }
        
        [Required]
        public string FuelType { get; set; } = string.Empty;
        
        [Required]
        public decimal Price { get; set; }
        
        public DateTime EffectiveDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public string? Notes { get; set; }
    }
}
