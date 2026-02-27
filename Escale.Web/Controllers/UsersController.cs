using Escale.Web.Models;
using Escale.Web.Models.Api;
using Escale.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Escale.Web.Controllers
{
    public class UsersController : Controller
    {
        private readonly IApiUserService _userService;
        private readonly IApiStationService _stationService;

        public UsersController(IApiUserService userService, IApiStationService stationService)
        {
            _userService = userService;
            _stationService = stationService;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            var usersTask = _userService.GetAllAsync(page, 20, search);
            var stationsTask = _stationService.GetAllAsync();

            await Task.WhenAll(usersTask, stationsTask);

            var usersResult = usersTask.Result;
            var stationsResult = stationsTask.Result;

            var model = new UserViewModel
            {
                Users = usersResult.Data?.Items?.Select(u => new User
                {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    LastLoginAt = u.LastLoginAt,
                    CreatedAt = u.CreatedAt,
                    AssignedStations = u.AssignedStations.Select(s => new StationInfo
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Location = s.Location
                    }).ToList()
                }).ToList() ?? new(),
                Stations = stationsResult.Data?.Select(s => new Station
                {
                    Id = s.Id,
                    Name = s.Name,
                    Location = s.Location
                }).ToList() ?? new(),
                TotalCount = usersResult.Data?.TotalCount ?? 0,
                Page = usersResult.Data?.Page ?? 1,
                PageSize = usersResult.Data?.PageSize ?? 20,
                TotalPages = usersResult.Data?.TotalPages ?? 0
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            var request = new CreateUserRequestDto
            {
                Username = user.Username,
                Password = user.Password ?? "TempPass123!",
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                StationIds = user.StationIds
            };

            var result = await _userService.CreateAsync(request);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "User created successfully!" : result.Message;

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(User user)
        {
            var request = new UpdateUserRequestDto
            {
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                StationIds = user.StationIds
            };

            var result = await _userService.UpdateAsync(user.Id, request);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "User updated successfully!" : result.Message;

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(Guid id, string newPassword, string currentPassword)
        {
            var request = new ChangePasswordRequestDto
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            };

            var result = await _userService.ChangePasswordAsync(id, request);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Password changed successfully!" : result.Message;

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _userService.DeleteAsync(id);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "User deleted successfully!" : result.Message;

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            var result = await _userService.ToggleStatusAsync(id);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "User status updated!" : result.Message;

            return RedirectToAction("Index");
        }
    }
}
