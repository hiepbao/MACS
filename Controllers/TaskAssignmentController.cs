using MACS.Models;
using MACS.Services;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MACS.Controllers
{
    public class TaskAssignmentController : Controller
    {
        private readonly TaskAssignmentService _taskAssignmentService;
        private readonly HistoryCarService _historyCarService;
        private readonly NotificationService _notificationService;

        public TaskAssignmentController(TaskAssignmentService taskAssignmentService, HistoryCarService historyCarService, NotificationService notificationService)
        {
            _taskAssignmentService = taskAssignmentService;
            _historyCarService = historyCarService;
            _notificationService = notificationService;
        }

        private UserInfo GetUserInfoFromToken()
        {
            var jwtToken = HttpContext.Request.Cookies["UserToken"];
            if (string.IsNullOrEmpty(jwtToken))
                return new UserInfo { FullName = "Unknown User" };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwtToken);

            return new UserInfo
            {
                AccountId = int.TryParse(token.Claims.FirstOrDefault(c => c.Type == "AccountId")?.Value, out var accountId) ? accountId : 0,
                FullName = token.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? "Unknown User",
                Role = token.Claims.FirstOrDefault(c => c.Type == "role")?.Value ?? "User"
            };

        }

        private async Task LoadUserAndGroupListsAsync()
        {
            var allUsers = await _historyCarService.GetAllUsersAsync();
            var userGroups = await _historyCarService.GetGroupsUsersAsync();

            ViewBag.UserList = allUsers?.Select(u => new SelectListItem
            {
                Value = u.AccountId.ToString(),
                Text = u.FullName
            }).ToList() ?? new List<SelectListItem>();

            ViewBag.GroupList = userGroups?.Select(g => new SelectListItem
            {
                Value = g.GroupId.ToString(),
                Text = g.GroupName
            }).ToList() ?? new List<SelectListItem>();
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var allTaskGroups = await _taskAssignmentService.GetAllTaskGroupsAsync();
                foreach (var group in allTaskGroups)
                {
                    var assignedByUser = await _historyCarService.GetUserByIdAsync(group.AccountId);
                    group.AssignedTo = assignedByUser?.FullName ?? "Không xác định";
  
                }
                var userInfo = GetUserInfoFromToken();
                var userTaskGroup = await _taskAssignmentService.GetTaskByAccountIdAsync(userInfo.AccountId);

                var viewModel = new TaskAssignmentIndexViewModel
                {
                    AllTaskGroups = allTaskGroups,
                    UserTaskGroup = userTaskGroup
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Index: {ex.Message}");
                return View(new TaskAssignmentIndexViewModel());
            }
        }

        

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var allUsers = await _historyCarService.GetAllUsersAsync();
                var userGroups = await _historyCarService.GetGroupsUsersAsync();

                ViewBag.UserList = allUsers.Select(u => new SelectListItem
                {
                    Value = u.AccountId.ToString(),
                    Text = u.FullName
                }).ToList() ?? new List<SelectListItem>();

                ViewBag.GroupList = userGroups.Select(g => new SelectListItem
                {
                    Value = g.GroupId.ToString(),
                    Text = g.GroupName
                }).ToList() ?? new List<SelectListItem>();

                return View(new TaskAssignmentGroup
                {
                    AssignedDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi khi tải dữ liệu: {ex.Message}");
                return View(new TaskAssignmentGroup());
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskAssignmentGroup model)
        {
            var userInfo = GetUserInfoFromToken();
            var results = new List<string>(); 

            try
            {
                var groupUserIds = new List<int>();
                if (model.GroupId > 0)
                {
                    var selectedGroup = (await _historyCarService.GetGroupsUsersAsync())
                        .FirstOrDefault(g => g.GroupId == model.GroupId);

                    if (selectedGroup != null)
                    {
                        groupUserIds = selectedGroup.Members.Select(m => m.AccountId).ToList();
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Nhóm được chọn không tồn tại hoặc không có thành viên.";
                        await LoadUserAndGroupListsAsync();
                        return View(model);
                    }
                }

                model.UserIds = model.UserIds?.Union(groupUserIds).Distinct().ToList() ?? groupUserIds;

                if (model.UserIds == null || !model.UserIds.Any())
                {
                    TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một nhóm hoặc một user.";
                    await LoadUserAndGroupListsAsync();
                    return View(model);
                }

                if (model.Tasks == null || !model.Tasks.Any() || model.Tasks.Any(task => string.IsNullOrWhiteSpace(task.TaskName)))
                {
                    TempData["ErrorMessage"] = "Vui lòng nhập ít nhất một công việc với tên công việc hợp lệ.";
                    await LoadUserAndGroupListsAsync();
                    return View(model);
                }

                foreach (var userId in model.UserIds)
                {
                    var assignedTo = await _historyCarService.GetUserByIdAsync(userId);
                    if (assignedTo == null)
                    {
                        results.Add($"Không tìm thấy user với ID: {userId}");
                        continue;
                    }

                    var validatedTasks = model.Tasks.Select(task =>
                    {
                        if (task.DueDate < DateTime.Now)
                        {
                            throw new ArgumentException($"Ngày hết hạn của nhiệm vụ '{task.TaskName}' không hợp lệ. Vui lòng nhập ngày hợp lệ.");
                        }

                        return new TaskAssignment
                        {
                            TaskName = task.TaskName,
                            Description = task.Description,
                            DueDate = task.DueDate,
                            Status = "Đã giao",
                            Priority = task.Priority
                        };
                    }).ToList();

                    var taskAssignmentGroup = new TaskAssignmentGroup
                    {
                        AccountId = userId,
                        GroupName = model.GroupName, 
                        AssignedTo = assignedTo.FullName,
                        AssignedBy = userInfo?.FullName ?? "Unknown",
                        UpdateBy = userInfo?.FullName ?? "Unknown",
                        AssignedDate = DateTime.UtcNow,
                        UpdateDate = DateTime.UtcNow,
                        Tasks = validatedTasks 
                    };


                    var isSuccess = await _taskAssignmentService.AddTaskGroupAsync(taskAssignmentGroup);
                    if (isSuccess)
                    {
                        results.Add($"Phân việc cho: {assignedTo.FullName}");
                        await _notificationService.NotifyUserTokenAsync(userId, "Công việc mới", $"Bạn đã được giao công việc mới {model.GroupName}.", "/TaskAssignment");
                    }
                    else
                    {
                        results.Add($"Thất bại: {assignedTo.FullName}");
                    }
                }

                TempData["SuccessMessage"] = string.Join(",", results);
                return RedirectToAction("Index", "TaskAssignment");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi: {ex.Message}";
                Console.WriteLine($"Error: {ex}");
            }

            await LoadUserAndGroupListsAsync();
            return View(model);
        }

        public async Task<IActionResult> Details(Guid taskId)
        {
            var taskGroup = await _taskAssignmentService.GetTaskGroupByTaskIdAsync(taskId);
            if (taskGroup == null)
            {
                return NotFound();
            }
            return View(taskGroup);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid taskId, string newStatus)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userInfo = GetUserInfoFromToken();
                    var task = await _taskAssignmentService.GetTaskGroupByTaskIdAsync(taskId);

                    if (task == null)
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy nhiệm vụ.";
                        return RedirectToAction(nameof(Index));
                    }

                    switch (task.Status)
                    {
                        case "Đã giao":
                            newStatus = "Đã nhận";
                            break;
                        case "Đã nhận":
                            newStatus = "Đang hoàn thành";
                            break;
                        case "Đang hoàn thành":
                            newStatus = "Đã hoàn thành";
                            break;
                        case "Đã hoàn thành":
                            TempData["ErrorMessage"] = "Nhiệm vụ đã hoàn thành, không thể cập nhật thêm.";
                            return RedirectToAction(nameof(Index));
                        default:
                            TempData["ErrorMessage"] = "Trạng thái không hợp lệ.";
                            return RedirectToAction(nameof(Index));
                    }

                    var success = await _taskAssignmentService.UpdateTaskStatusAsync(userInfo.AccountId, taskId, newStatus, userInfo.FullName);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Trạng thái nhiệm vụ đã được cập nhật thành công!";
                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError("", "Không thể cập nhật trạng thái nhiệm vụ.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
                }
            }

            TempData["ErrorMessage"] = "Cập nhật trạng thái thất bại. Vui lòng kiểm tra lại thông tin!";
            return RedirectToAction(nameof(Index));
        }


    }
}
