using System.ComponentModel.DataAnnotations;

namespace Sabara.Web.Models
{
    public enum ClientStatus
    {
        Lead = 0,       // محتمل
        Active = 1,     // نشط
        Inactive = 2,   // متوقف
        Lost = 3,       // مفقود
    }

    public enum ClientSource
    {
        Website = 0,    // الموقع
        Referral = 1,   // إحالة
        Social = 2,     // سوشيال ميديا
        Other = 99,
    }

    public class Client
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم مطلوب")]
        [Display(Name = "الاسم")]
        public string Name { get; set; } = "";

        [Display(Name = "اسم الشركة")]
        public string? CompanyName { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Phone(ErrorMessage = "رقم الهاتف غير صالح")]
        [Display(Name = "رقم الهاتف")]
        public string PhoneNumber { get; set; } = "";

        [EmailAddress(ErrorMessage = "بريد إلكتروني غير صالح")]
        [Display(Name = "البريد الإلكتروني")]
        public string? Email { get; set; }

        [Display(Name = "المدينة")]
        public string? City { get; set; }

        [Display(Name = "الحالة")]
        public ClientStatus Status { get; set; } = ClientStatus.Lead;

        [Display(Name = "المصدر")]
        public ClientSource Source { get; set; } = ClientSource.Website;

        [Display(Name = "الموظف المسؤول")]
        public string? AssignedToUserId { get; set; }

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastContactAt { get; set; }

        public List<Order> Orders { get; set; } = new();
    }
}
