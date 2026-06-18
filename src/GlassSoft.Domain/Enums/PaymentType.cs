using System.ComponentModel.DataAnnotations;

namespace GlassSoft.Domain.Enums;

public enum PaymentType
{
    [Display(Name = "Nakit")]
    Nakit = 0,

    [Display(Name = "Havale/EFT")]
    Havale = 1,

    [Display(Name = "Çek")]
    Cek = 2,

    [Display(Name = "Senet")]
    Senet = 3,

    [Display(Name = "Kredi Kartı")]
    KrediKarti = 4,

    [Display(Name = "Açık Hesap")]
    AcikHesap = 5
}
