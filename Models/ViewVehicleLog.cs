using System.ComponentModel.DataAnnotations;

namespace MACS.Models
{
    public class ViewVehicleLog : VehicleLog
    {
        [MaxLength(11)]
        [RegularExpression(@"^[A-Z]{4}\d{7}$", ErrorMessage = "Vui lòng nhập đúng định dạng số cont")]
        [DisplayFormat(ConvertEmptyStringToNull = true, DataFormatString = "{0:upper}")]
        [Display(Name = "Số Cont")]
        public string ContainerNo { get; set; } // varchar(50), null

        [MaxLength(50)]
        [Display(Name = "Loại Cont")]
        public string ContainerType { get; set; } // varchar(50), null
    }
}
