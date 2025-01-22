namespace MACS.Models
{
    public class FileModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string FileName { get; set; }
        public string FileSizeInKB { get; set; }
        public int CountFile { get; set; }
        public DateTime Date { get; set; }
        public List<string> SavedFileList { get; set; }

        public FileModel()
        {
            SavedFileList = new List<string>();
        }
    }

    public class FileResultModel
    {
        public string FileName { get; set; }       // Tên file gốc
        public long FileSize { get; set; }        // Kích thước file (byte)
        public List<string> InnerFiles { get; set; } // Danh sách file bên trong zip
    }

}
