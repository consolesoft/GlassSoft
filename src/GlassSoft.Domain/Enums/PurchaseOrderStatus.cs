using System.ComponentModel.DataAnnotations;

namespace GlassSoft.Domain.Enums;

public enum PurchaseOrderStatus
{
    [Display(Name = "Taslak")]
    Taslak = 0,

    [Display(Name = "Onaylandı")]
    Onaylandi = 1,

    [Display(Name = "Kısmi Teslim")]
    KismiTeslim = 2,

    [Display(Name = "Tamamlandı")]
    Tamamlandi = 3,

    [Display(Name = "İptal Edildi")]
    IptalEdildi = 4
}
