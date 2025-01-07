
using MACSAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MACS.Models
{
    public class HistoryCar
    {
        [Key]
        [Display(Name = "Vehicle Log Id")]
        public Guid VehicleLogId { get; set; } // uniqueidentifier, not null

        [MaxLength(50)]
        [Display(Name = "Số thẻ")]
        public string CardNo { get; set; } // nvarchar(50), not null

        [MaxLength]
        [Display(Name = "Tên TX")]
        public string DriverName { get; set; } // nvarchar(max), not null

        [MaxLength(50)]
        [Display(Name = "CCCD")]
        public string PersonalId { get; set; } // nvarchar(50), not null

        [MaxLength(50)]
        [Display(Name = "Số ĐT")]
        public string PhoneNo { get; set; } // nvarchar(50), not null

        [MaxLength(50)]
        [Display(Name = "B.Số xe")]
        public string LicensePlate { get; set; } // varchar(50), not null

        [MaxLength(50)]
        [Display(Name = "Loại xe")]
        public string VehicleType { get; set; } // varchar(50), not null

        [Display(Name = "Mã loại")]
        public string TypeCode { get; set; }
        [MaxLength(50)]
        [Display(Name = "Mục đích")]
        public string Purpose { get; set; } // varchar(50), not null

        [MaxLength]
        [Display(Name = "Ghi chú")]
        public string Remark { get; set; } = "Không hút thuốc - ";// nvarchar(max), null

        [Display(Name = "Có HH")]
        public bool IsCarried { get; set; } // nvarchar(20), null

        [Display(Name = "Có Cont vào")]
        public bool IsCarriedIn { get; set; } // bit, not null

        [Display(Name = "Cho vào")]
        public bool IsGetIn { get; set; } // bit, not null

        [Display(Name = "Vào lúc")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? GetInDate { get; set; } // datetime, null

        [MaxLength(50)]
        [Display(Name = "Cho vào bởi")]
        public string? GetInBy { get; set; } // varchar(50), null

        [Display(Name = "Có Cont ra")]
        public bool IsCarriedOut { get; set; } // bit, not null

        [Display(Name = "Cho ra")]
        public bool IsGetOut { get; set; } // bit, not null

        [Display(Name = "Ra lúc")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? GetOutDate { get; set; } // datetime, null

        [MaxLength(50)]
        [Display(Name = "Cho ra bởi")]
        public string? GetOutBy { get; set; } // varchar(50), null

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }// datetime, not null

        [MaxLength(50)]
        [Display(Name = "Tạo bởi")]
        public string? CreatedBy { get; set; } // varchar(50), not null


        [Display(Name = "Ngày sửa")]
        public DateTime ModifiedDate { get; set; } // datetime, not null

        [MaxLength(50)]
        [Display(Name = "Sửa bởi")]
        public string? ModifiedBy { get; set; } // varchar(50), not null


        [Display(Name = "Id user")]
        public int? UserId { get; set; }

    }
}
