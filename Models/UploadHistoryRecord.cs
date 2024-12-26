namespace MACS.Models
{
    public class UploadHistoryRecord
    {
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public string? FileName { get; set; }
        public string? FileSizeKB { get; set; }
        public string? FileCount { get; set; }
        public string? UploadTime { get; set; }
        public List<string>? FileList { get; set; }
    }

}
