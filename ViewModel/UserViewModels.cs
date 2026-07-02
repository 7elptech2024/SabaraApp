using System.ComponentModel.DataAnnotations;

namespace Sabara.Web.ViewModel
{
    public class UserListItem
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public bool IsCurrentUser { get; set; }
    }

    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "بريد إلكتروني غير صالح")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [MinLength(6, ErrorMessage = "كلمة المرور يجب أن تكون 6 خانات على الأقل")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "الدور مطلوب")]
        [Display(Name = "الدور")]
        public string Role { get; set; } = "Employee";
    }

    public class EditUserViewModel
    {
        public string Id { get; set; } = "";

        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "الدور مطلوب")]
        [Display(Name = "الدور")]
        public string Role { get; set; } = "Employee";

        [DataType(DataType.Password)]
        [Display(Name = "كلمة مرور جديدة (اختياري)")]
        public string? NewPassword { get; set; }
    }
}
