using MACS.Models;

namespace MACS.Services
{
    public class TaskAssignmentService
    {
        private readonly HttpClient _httpClient;
        //private const string ApiBaseUrl = "https://localhost:7279";
        private const string ApiBaseUrl = "https://macsapi.onrender.com";
        public TaskAssignmentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<TaskAssignmentGroup>> GetAllTaskGroupsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{ApiBaseUrl}/api/TaskAssignment");

                if (response.IsSuccessStatusCode)
                {
                    var taskGroups = await response.Content.ReadFromJsonAsync<List<TaskAssignmentGroup>>();
                    return taskGroups ?? new List<TaskAssignmentGroup>();
                }

                Console.WriteLine($"Failed to get task groups. Status Code: {response.StatusCode}");
                return new List<TaskAssignmentGroup>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetAllTaskGroupsAsync: {ex.Message}");
                return new List<TaskAssignmentGroup>();
            }
        }

        public async Task<List<TaskAssignment>> GetAllTasksAsync()
        {
            try
            {
                var taskGroups = await GetAllTaskGroupsAsync();
                return taskGroups.SelectMany(g => g.Tasks).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetAllTasksAsync: {ex.Message}");
                return new List<TaskAssignment>();
            }
        }

        public async Task<List<TaskAssignment>> GetTaskByAccountIdAsync(int accountId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{ApiBaseUrl}/api/TaskAssignment/GetTaskByAccountId/{accountId}");
                if (response.IsSuccessStatusCode)
                {
                    var tasks = await response.Content.ReadFromJsonAsync<List<TaskAssignment>>();
                    return tasks;
                }

                Console.WriteLine($"Failed to get task group for AccountId {accountId}. Status Code: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetTaskByAccountIdAsync: {ex.Message}");
                return null;
            }
        }


        public async Task<TaskAssignment?> GetTaskGroupByTaskIdAsync(Guid taskId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{ApiBaseUrl}/api/TaskAssignment/GetByTaskId/{taskId}");

                if (response.IsSuccessStatusCode)
                {
                    var taskGroup = await response.Content.ReadFromJsonAsync<TaskAssignment>();
                    return taskGroup;
                }

                Console.WriteLine($"Failed to get task group for AccountId {taskId}. Status Code: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetTaskGroupByTaskIdAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> AddTaskGroupAsync(TaskAssignmentGroup taskGroup)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{ApiBaseUrl}/api/TaskAssignment", taskGroup);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Successfully added TaskAssignmentGroup.");
                    return true;
                }

                Console.WriteLine($"Failed to add TaskAssignmentGroup. Status Code: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in AddTaskGroupAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateTaskStatusAsync(int accountId, Guid taskId, string newStatus, string updatedBy)
        {
            try
            {
                var url = $"{ApiBaseUrl}/api/TaskAssignment/{accountId}/tasks/{taskId}";

                var payload = new
                {
                    NewStatus = newStatus,
                    UpdatedBy = updatedBy
                };

                var response = await _httpClient.PutAsJsonAsync(url, payload);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Successfully updated task status for TaskId {taskId}.");
                    return true;
                }

                Console.WriteLine($"Failed to update task status for TaskId {taskId}. Status Code: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in UpdateTaskStatusAsync: {ex.Message}");
                return false;
            }
        }

    }
}
