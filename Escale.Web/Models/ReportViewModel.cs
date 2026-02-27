namespace Escale.Web.Models
{
    public class ReportViewModel
    {
        public string ReportType { get; set; } = "Sales";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? StationId { get; set; }
        public Guid? FuelTypeId { get; set; }

        public List<Station> Stations { get; set; } = new();
        public List<FuelType> FuelTypes { get; set; } = new();
    }
}
