using System.ComponentModel.DataAnnotations;

namespace UserApp.Models.ViewModels
{
    public class ResetPasswordViewModel
    {
        //public string UserId { get; set; }

        //[Required]
        //[DataType(DataType.Password)]
        //public string OldPassword { get; set; }

        //[Required]
        //[DataType(DataType.Password)]
        //public string NewPassword { get; set; }

        //[Required]
        //[DataType(DataType.Password)]
        //[Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        //public string ConfirmPassword { get; set; }

        public string UserId { get; set; }

        public string NewPassword { get; set; }

        public string ConfirmPassword { get; set; }
    }
}
