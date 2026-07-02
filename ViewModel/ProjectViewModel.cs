using System.ComponentModel.DataAnnotations;

namespace Sabara.Web.ViewModel
{
    public class ProjectViewModel
    {
        public int Id { get; set; }

        [Display(Name = "صورة المشروع")]
        public string? ImagePath { get; set; }


        [Required]
        [Display(Name = "اسم المشروع")]
        public string Name { get; set; }

        [Display(Name = "وصف المشروع")]
        public string Description { get; set; }

        [Display(Name = "الصنف")]
        public string Category { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
