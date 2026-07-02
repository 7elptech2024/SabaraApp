using Sabara.Web.Models;

namespace Sabara.Web.ViewModel
{
    public static class ClientHelpers
    {
        public static string Label(this ClientStatus s) => s switch
        {
            ClientStatus.Lead => "محتمل",
            ClientStatus.Active => "نشط",
            ClientStatus.Inactive => "متوقف",
            ClientStatus.Lost => "مفقود",
            _ => s.ToString(),
        };

        public static (string bg, string text) Colors(this ClientStatus s) => s switch
        {
            ClientStatus.Lead     => ("bg-amber-100", "text-amber-700"),
            ClientStatus.Active   => ("bg-emerald-100", "text-emerald-700"),
            ClientStatus.Inactive => ("bg-slate-100", "text-slate-600"),
            ClientStatus.Lost     => ("bg-red-100", "text-red-700"),
            _ => ("bg-brand-surface", "text-brand-ink/70"),
        };

        public static string Label(this ClientSource s) => s switch
        {
            ClientSource.Website => "الموقع",
            ClientSource.Referral => "إحالة",
            ClientSource.Social => "سوشيال ميديا",
            ClientSource.Other => "أخرى",
            _ => s.ToString(),
        };

        public static string Label(this OrderStage s) => s switch
        {
            OrderStage.New => "جديد",
            OrderStage.Contacted => "تم التواصل",
            OrderStage.Proposal => "عرض سعر",
            OrderStage.Won => "فوز",
            OrderStage.Lost => "خسارة",
            _ => s.ToString(),
        };

        public static (string bg, string text, string ring) StageColors(this OrderStage s) => s switch
        {
            OrderStage.New       => ("bg-blue-50", "text-blue-700", "ring-blue-300"),
            OrderStage.Contacted => ("bg-amber-50", "text-amber-700", "ring-amber-300"),
            OrderStage.Proposal  => ("bg-purple-50", "text-purple-700", "ring-purple-300"),
            OrderStage.Won       => ("bg-emerald-50", "text-emerald-700", "ring-emerald-300"),
            OrderStage.Lost      => ("bg-red-50", "text-red-700", "ring-red-300"),
            _ => ("bg-brand-surface", "text-brand-ink/70", "ring-brand-surface"),
        };

        public static string Icon(this OrderStage s) => s switch
        {
            OrderStage.New => "bi-stars",
            OrderStage.Contacted => "bi-telephone",
            OrderStage.Proposal => "bi-file-earmark-text",
            OrderStage.Won => "bi-trophy-fill",
            OrderStage.Lost => "bi-x-circle",
            _ => "bi-dot",
        };
    }
}
