namespace GlassSoft.Web.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public int StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorDetail { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public string StatusTitle => StatusCode switch
    {
        400 => "Geçersiz İstek",
        401 => "Yetkisiz Erişim",
        403 => "Erişim Engellendi",
        404 => "Sayfa Bulunamadı",
        500 => "Sunucu Hatası",
        _ => "Bir Hata Oluştu"
    };

    public string StatusIcon => StatusCode switch
    {
        404 => "fas fa-search",
        401 or 403 => "fas fa-lock",
        _ => "fas fa-exclamation-triangle"
    };
}
