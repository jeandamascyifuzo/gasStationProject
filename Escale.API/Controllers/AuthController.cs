using Escale.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Escale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation($"Login attempt for user: {request.Username}");

            // TODO: Implement real authentication with database
            // This is a temporary implementation for testing
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Ok(new LoginResponse
                {
                    Success = false,
                    Message = "Username and password are required"
                });
            }

            // Temporary demo user - replace with real authentication
            if (request.Username.Equals("admin", StringComparison.OrdinalIgnoreCase) && 
                request.Password == "admin123")
            {
                var response = new LoginResponse
                {
                    Success = true,
                    Token = GenerateToken(),
                    Message = "Login successful",
                    User = new UserInfo
                    {
                        Id = 1,
                        Username = request.Username,
                        FullName = "Admin User",
                        Role = "Cashier",
                        AssignedStations = new List<StationInfo>
                        {
                            new StationInfo
                            {
                                Id = 1,
                                Name = "Main Station",
                                Location = "Downtown",
                                Address = "123 Main St, Kigali"
                            }
                        }
                    }
                };

                return Ok(response);
            }

            return Ok(new LoginResponse
            {
                Success = false,
                Message = "Invalid username or password"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return Ok(new LoginResponse
            {
                Success = false,
                Message = "An error occurred during login"
            });
        }
    }

    private string GenerateToken()
    {
        // TODO: Implement proper JWT token generation
        // This is a temporary implementation
        var guid = Guid.NewGuid().ToString();
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"token_{guid}"));
    }
}
