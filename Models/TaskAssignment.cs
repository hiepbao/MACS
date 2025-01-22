using System.ComponentModel.DataAnnotations;

namespace MACS.Models
{
    public class TaskAssignment
    {
        [Key]
        public Guid TaskId { get; set; }
        [Display(Name = "Nhiệm vụ")]
        public string TaskName { get; set; }
        [Display(Name = "Mô tả")]
        public string Description { get; set; }
        [Display(Name = "Hạn chót")]
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "Đã giao";
        [Display(Name = "Mức độ")]
        public int Priority { get; set; }
    }
    public class TaskAssignmentGroup
    {
        [Key]
        public Guid GroupTaskId { get; set; }
        public int AccountId { get; set; }
        public string AssignedTo { get; set; }
        public string AssignedBy { get; set; } 
        public DateTime AssignedDate { get; set; }
        public string UpdateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        [Display(Name = "Công việc")]
        public string GroupName { get; set; }
        [Display(Name = "Id user")]
        public List<int>? UserIds { get; set; }

        [Display(Name = "Id Group")]
        public int GroupId { get; set; }
        public List<TaskAssignment> Tasks { get; set; } 

        public TaskAssignmentGroup()
        {
            Tasks = new List<TaskAssignment>();
        }
    }

    public class TaskAssignmentIndexViewModel
    {
        public List<TaskAssignmentGroup> AllTaskGroups { get; set; } = new();
        public List<TaskAssignment>? UserTaskGroup { get; set; }
    }
}
