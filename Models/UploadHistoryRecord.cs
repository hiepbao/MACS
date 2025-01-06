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

    public class FileResultModel
    {
        public string FileName { get; set; }       // Tên file gốc
        public long FileSize { get; set; }        // Kích thước file (byte)
        public List<string> InnerFiles { get; set; } // Danh sách file bên trong zip
    }

}
