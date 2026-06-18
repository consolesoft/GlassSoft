using System.ComponentModel.DataAnnotations;

namespace GlassSoft.Domain.Enums;

public enum UnitType
{
    [Display(Name = "Adet")]
    Adet = 0,

    [Display(Name = "Metre Kare (m²)")]
    MetreKare = 1,

    [Display(Name = "Metre")]
    Metre = 2,

    [Display(Name = "Kilogram")]
    Kilogram = 3,

    [Display(Name = "Litre")]
    Litre = 4
}
