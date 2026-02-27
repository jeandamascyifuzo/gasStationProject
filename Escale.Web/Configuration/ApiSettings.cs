namespace Escale.Web.Configuration;

public class ApiSettings
{
    public string BaseUrl { get; set; } = "https://localhost:7015/api";
    public int TimeoutSeconds { get; set; } = 30;
}
