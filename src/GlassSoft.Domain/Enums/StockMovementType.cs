using System.ComponentModel.DataAnnotations;

namespace GlassSoft.Domain.Enums;

public enum StockMovementType
{
    [Display(Name = "Giriş")]
    Giris = 0,

    [Display(Name = "Çıkış")]
    Cikis = 1
}
