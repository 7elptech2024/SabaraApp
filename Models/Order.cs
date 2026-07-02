    using System.ComponentModel.DataAnnotations;

    namespace Sabara.Web.Models
    {
        public enum OrderStage
        {
            New = 0,        // جديد
            Contacted = 1,  // تم التواصل
            Proposal = 2,   // عرض سعر
            Won = 3,        // فوز
            Lost = 4,       // خسارة
        }

        public class Order
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "الاسم مطلوب")]
            [Display(Name = "اسم الزبون")]
            public string CustomerName { get; set; }

            [Required(ErrorMessage = "رقم الهاتف مطلوب")]
            [Phone(ErrorMessage = "رقم الهاتف غير صالح")]
            [Display(Name = "رقم الهاتف")]
            public string PhoneNumber { get; set; }

            [Display(Name = "اسم الخدمة أو الباقة")]
            public string? ServiceName { get; set; }

            [Display(Name = "تاريخ الطلب")]
            [DataType(DataType.DateTime)]
            public DateTime CreatedAt { get; set; } = DateTime.Now;

            [Display(Name = "تم الرد؟")]
            public bool IsReplied { get; set; } = false;

            [Display(Name = "ملاحظات")]
            public string? Notes { get; set; }

            [Display(Name = "المرحلة")]
            public OrderStage Stage { get; set; } = OrderStage.New;

            [Display(Name = "العميل")]
            public int? ClientId { get; set; }
            public Client? Client { get; set; }
        }
    }
