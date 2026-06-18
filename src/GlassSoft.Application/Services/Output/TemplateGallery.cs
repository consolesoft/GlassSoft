using GlassSoft.Domain.Enums;

namespace GlassSoft.Application.Services.Output;

/// <summary>
/// Hazır şablon galerisi - 3 stil × 5 çıktı türü = 15 profesyonel tasarım
/// </summary>
public static class TemplateGallery
{
    public static readonly string[] Styles = ["Modern", "Klasik", "Minimal"];

    public static string GetTemplate(OutputType type, string style) => (type, style) switch
    {
        (OutputType.Siparis, "Modern") => ModernSiparis(),
        (OutputType.Siparis, "Klasik") => KlasikSiparis(),
        (OutputType.Siparis, "Minimal") => MinimalSiparis(),

        (OutputType.Teslimat, "Modern") => ModernTeslimat(),
        (OutputType.Teslimat, "Klasik") => KlasikTeslimat(),
        (OutputType.Teslimat, "Minimal") => MinimalTeslimat(),

        (OutputType.Uretim, "Modern") => ModernUretim(),
        (OutputType.Uretim, "Klasik") => KlasikUretim(),
        (OutputType.Uretim, "Minimal") => MinimalUretim(),

        (OutputType.CariHareketleri, "Modern") => ModernCariHareket(),
        (OutputType.CariHareketleri, "Klasik") => KlasikCariHareket(),
        (OutputType.CariHareketleri, "Minimal") => MinimalCariHareket(),

        (OutputType.CariEkstre, "Modern") => ModernCariEkstre(),
        (OutputType.CariEkstre, "Klasik") => KlasikCariEkstre(),
        (OutputType.CariEkstre, "Minimal") => MinimalCariEkstre(),

        (OutputType.CitaRaporu, "Modern") => ModernCitaRaporu(),
        (OutputType.CitaRaporu, "Klasik") => KlasikCitaRaporu(),
        (OutputType.CitaRaporu, "Minimal") => MinimalCitaRaporu(),

        (OutputType.FiyatsizSiparis, "Modern") => ModernFiyatsizSiparis(),
        (OutputType.FiyatsizSiparis, "Klasik") => KlasikFiyatsizSiparis(),
        (OutputType.FiyatsizSiparis, "Minimal") => MinimalFiyatsizSiparis(),

        (OutputType.Etiket, "Modern") => ModernEtiket(),
        (OutputType.Etiket, "Klasik") => KlasikEtiket(),
        (OutputType.Etiket, "Minimal") => MinimalEtiket(),

        (OutputType.UrunSatisRaporu, "Modern") => ModernUrunSatisRaporu(),
        (OutputType.UrunSatisRaporu, "Klasik") => ModernUrunSatisRaporu(),
        (OutputType.UrunSatisRaporu, "Minimal") => ModernUrunSatisRaporu(),

        (OutputType.CariBakiyeListesi, "Modern") => ModernCariBakiyeListesi(),
        (OutputType.CariBakiyeListesi, "Klasik") => ModernCariBakiyeListesi(),
        (OutputType.CariBakiyeListesi, "Minimal") => ModernCariBakiyeListesi(),

        _ => ModernSiparis()
    };

    public static List<object> GetGalleryItems(OutputType type)
    {
        return Styles.Select(s => (object)new
        {
            Style = s,
            Name = $"{s} {TypeDisplayName(type)}",
            Description = s switch
            {
                "Modern" => "Gradient başlık, renkli vurgular, geniş boşluklar",
                "Klasik" => "Geleneksel çizgili tasarım, sade ve resmi",
                "Minimal" => "Sade, temiz, minimum süsleme",
                _ => ""
            }
        }).ToList();
    }

    private static string TypeDisplayName(OutputType type) => type switch
    {
        OutputType.Siparis => "Sipariş Fişi",
        OutputType.Teslimat => "Teslimat İrsaliyesi",
        OutputType.Uretim => "Üretim Emri",
        OutputType.CariHareketleri => "Cari Hareket Raporu",
        OutputType.CariEkstre => "Hesap Ekstresi",
        OutputType.CitaRaporu => "Çıta Raporu",
        OutputType.FiyatsizSiparis => "Fiyatsız Sipariş Fişi",
        OutputType.Etiket => "Etiket",
        OutputType.UrunSatisRaporu => "Ürün Satış Raporu",
        OutputType.CariBakiyeListesi => "Cari Bakiye Listesi",
        _ => ""
    };

    private static string ModernUrunSatisRaporu() => $@"{ModernCss()}
<div class=""document"">
    <div class=""header"">
        <div>
            {{{{FirmaLogo}}}}
            <div class=""company"">{{{{FirmaAdi}}}}</div>
            <div class=""company-sub"">{{{{FirmaSlogan}}}}</div>
        </div>
        <div>
            <div class=""doc-title"">ÜRÜN SATIŞ RAPORU</div>
            <div class=""doc-no"">{{{{BaslangicTarihi}}}} - {{{{BitisTarihi}}}}</div>
            <div class=""doc-date"">{{{{Tarih}}}} {{{{Saat}}}}</div>
        </div>
    </div>

    <div class=""info-row"">
        <div class=""info-box"">
            <div class=""box-title"">Rapor Bilgileri</div>
            <p>Başlangıç: <strong>{{{{BaslangicTarihi}}}}</strong></p>
            <p>Bitiş: <strong>{{{{BitisTarihi}}}}</strong></p>
            <p>Sipariş Durumu: <strong>Üretimde / Tamamlanmış</strong></p>
        </div>
        <div class=""info-box"">
            <div class=""box-title"">Genel Özet</div>
            <p>Toplam Ürün Çeşidi: <strong>{{{{ToplamUrunCesidi}}}}</strong></p>
            <p>Toplam Sipariş: <strong>{{{{ToplamSiparis}}}}</strong></p>
            <p>Toplam m²: <strong>{{{{GenelToplamM2}}}} m²</strong></p>
            <p>Toplam Adet: <strong>{{{{GenelToplamAdet}}}}</strong></p>
        </div>
    </div>

    <table>
        <thead>
            <tr>
                <th style=""width:35px; text-align:center"">#</th>
                <th>Ürün / Reçete</th>
                <th style=""text-align:right"">Toplam Adet</th>
                <th style=""text-align:right"">Toplam m²</th>
                <th style=""text-align:right"">Sipariş Sayısı</th>
            </tr>
        </thead>
        <tbody>
            {{{{Urunler}}}}
        </tbody>
        <tfoot>
            <tr>
                <td colspan=""2"" style=""text-align:right"">GENEL TOPLAM</td>
                <td style=""text-align:right"">{{{{GenelToplamAdet}}}}</td>
                <td style=""text-align:right"">{{{{GenelToplamM2}}}} m²</td>
                <td style=""text-align:right"">{{{{ToplamSiparis}}}}</td>
            </tr>
        </tfoot>
    </table>

    <p class=""footer-note"">Bu rapor {{{{Tarih}}}} {{{{Saat}}}} tarihinde {{{{FirmaAdi}}}} sistemi tarafından oluşturulmuştur.</p>
</div>";

    // ═══════════════════════════════════════════════════════════
    //  MODERN STİL - Gradient başlık, renkli, geniş
    // ═══════════════════════════════════════════════════════════

    private static string ModernCss() => @"
<style>
    * { margin: 0; padding: 0; box-sizing: border-box; }
    body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; color: #2d3436; font-size: 13px; padding: 20px; background: #fff; }
    .document { max-width: 210mm; margin: 0 auto; }

    /* Başlık */
    .header { background: linear-gradient(135deg, #2c3e50 0%, #3498db 100%); color: #fff; padding: 25px 30px; border-radius: 8px; margin-bottom: 25px; display: flex; justify-content: space-between; align-items: center; }
    .header .company { font-size: 28px; font-weight: 700; letter-spacing: 1px; }
    .header .company-sub { font-size: 13px; opacity: 0.8; margin-top: 4px; }
    .header .doc-title { font-size: 20px; font-weight: 600; text-align: right; }
    .header .doc-no { font-size: 15px; opacity: 0.9; margin-top: 5px; }
    .header .doc-date { font-size: 12px; opacity: 0.7; margin-top: 3px; }

    /* Bilgi Kutuları */
    .info-row { display: flex; gap: 20px; margin-bottom: 20px; }
    .info-box { flex: 1; border: 1px solid #e0e0e0; border-radius: 8px; padding: 18px; background: #fafbfc; }
    .info-box .box-title { font-size: 11px; font-weight: 700; text-transform: uppercase; color: #3498db; letter-spacing: 1px; border-bottom: 2px solid #3498db; padding-bottom: 8px; margin-bottom: 12px; }
    .info-box p { margin: 5px 0; font-size: 13px; line-height: 1.5; }
    .info-box strong { color: #2c3e50; }

    /* Tablo */
    table { width: 100%; border-collapse: collapse; margin: 20px 0; border-radius: 8px; overflow: hidden; }
    table thead th { background: #2c3e50; color: #fff; padding: 10px 12px; font-size: 11px; text-transform: uppercase; letter-spacing: 0.5px; font-weight: 600; text-align: left; }
    table tbody td { padding: 9px 12px; border-bottom: 1px solid #ecf0f1; font-size: 12px; }
    table tbody tr:nth-child(even) { background: #f8f9fa; }
    table tbody tr:hover { background: #eaf2f8; }
    table tfoot td { background: #2c3e50; color: #fff; padding: 10px 12px; font-weight: 700; }

    /* Toplamlar */
    .totals { display: flex; justify-content: flex-end; margin: 15px 0; }
    .totals table { width: 300px; }
    .totals td { padding: 6px 12px; font-size: 13px; }
    .totals tr:last-child { font-size: 16px; font-weight: 700; color: #2c3e50; border-top: 2px solid #3498db; }

    /* Notlar */
    .notes { background: #eaf2f8; border-left: 4px solid #3498db; padding: 12px 16px; border-radius: 0 6px 6px 0; margin: 20px 0; font-size: 12px; }

    /* İmza */
    .signatures { display: flex; justify-content: space-between; margin-top: 50px; padding-top: 10px; }
    .sign-block { text-align: center; width: 180px; }
    .sign-line { border-top: 1px solid #2c3e50; margin-top: 70px; padding-top: 8px; font-size: 12px; font-weight: 600; }

    /* Özet Kutuları */
    .summary-boxes { display: flex; gap: 15px; margin-bottom: 20px; }
    .summary-box { flex: 1; text-align: center; padding: 15px; border-radius: 8px; }
    .summary-box.red { background: #ffeef0; border: 1px solid #e74c3c; }
    .summary-box.green { background: #eafaf1; border: 1px solid #27ae60; }
    .summary-box.blue { background: #eaf2f8; border: 1px solid #3498db; }
    .summary-box .amount { font-size: 20px; font-weight: 700; }
    .summary-box .label { font-size: 11px; text-transform: uppercase; color: #666; margin-top: 4px; }

    /* Alt Bilgi */
    .footer-note { text-align: center; margin-top: 30px; color: #aaa; font-size: 10px; padding-top: 15px; border-top: 1px solid #eee; }

    @page { size: A4; margin: 12mm; }
    @media print { body { padding: 0; } .header { -webkit-print-color-adjust: exact; print-color-adjust: exact; } table thead th, table tfoot td { -webkit-print-color-adjust: exact; print-color-adjust: exact; } .summary-box { -webkit-print-color-adjust: exact; print-color-adjust: exact; } }
</style>";

    private static string ModernSiparis() => $@"{ModernCss()}
<div class=""document"">
    <div class=""header"">
        <div>
            {{{{FirmaLogo}}}}
            <div class=""company"">{{{{FirmaAdi}}}}</div>
            <div class=""company-sub"">{{{{FirmaSlogan}}}}</div>
        </div>
        <div>
            <div class=""doc-title"">SİPARİŞ FİŞİ</div>
            <div class=""doc-no"">{{{{SiparisNo}}}}</div>
            <div class=""doc-date"">{{{{SiparisTarihi}}}}</div>
        </div>
    </div>

    <div class=""info-row"">
        <div class=""info-box"">
            <div class=""box-title"">Müşteri Bilgileri</div>
            <p><strong>{{{{MusteriAdi}}}}</strong></p>
            <p>{{{{MusteriAdres}}}}</p>
            <p>{{{{MusteriSehir}}}}</p>
            <p>Tel: {{{{MusteriTelefon}}}} &bull; E-posta: {{{{MusteriEmail}}}}</p>
            <p>Vergi No: {{{{MusteriVergiNo}}}} &bull; V.D.: {{{{MusteriVergiDairesi}}}}</p>
        </div>
        <div class=""info-box"">
            <div class=""box-title"">Sipariş Bilgileri</div>
            <p>Sipariş No: <strong>{{{{SiparisNo}}}}</strong></p>
            <p>Sipariş Tarihi: <strong>{{{{SiparisTarihi}}}}</strong></p>
            <p>Teslim Tarihi: <strong>{{{{TeslimTarihi}}}}</strong></p>
            <p>Sipariş Müşterisi: <strong>{{{{SiparisMusterisi}}}}</strong></p>
            <p>Durum: <strong>{{{{Durum}}}}</strong></p>
            <p>Para Birimi: <strong>{{{{ParaBirimi}}}}</strong></p>
        </div>
    </div>

    <table>
        <thead>
            <tr>
                <th style=""width:35px; text-align:center"">#</th>
                <th>Ürün / Reçete</th>
                <th style=""text-align:right"">En (mm)</th>
                <th style=""text-align:right"">Boy (mm)</th>
                <th style=""text-align:center"">Adet</th>
                <th style=""text-align:right"">Satır m²</th>
                <th style=""text-align:right"">Birim Fiyat</th>
                <th style=""text-align:right"">Toplam</th>
                <th>Özellikler</th>
                <th>Not</th>
                <th>Poz No</th>
            </tr>
        </thead>
        <tbody>
            {{{{Kalemler}}}}
        </tbody>
    </table>

    <div class=""totals"">
        <table>
            <tr><td>Toplam Adet:</td><td style=""text-align:right"">{{{{ToplamAdet}}}}</td></tr>
            <tr><td>Toplam Alan:</td><td style=""text-align:right"">{{{{ToplamAlan}}}} m²</td></tr>
                        <tr><td>Ara Toplam:</td><td style=""text-align:right"">{{{{ToplamTutar}}}} {{{{ParaBirimi}}}}</td></tr>
            <tr><td>KDV (%{{{{KdvOrani}}}}):</td><td style=""text-align:right"">{{{{KdvTutari}}}} {{{{ParaBirimi}}}}</td></tr>
            <tr><td style=""border-top:2px solid #3498db; padding-top:8px""><strong>Genel Toplam:</strong></td><td style=""text-align:right; border-top:2px solid #3498db; padding-top:8px""><strong>{{{{GenelToplam}}}} {{{{ParaBirimi}}}}</strong></td></tr>
            <tr><td>Cari Bakiye:</td><td style=""text-align:right"">{{{{CariBakiye}}}} TL ({{{{CariBakiyeYon}}}})</td></tr>
        </table>
    </div>

    <div class=""notes""><strong>Notlar:</strong> {{{{Notlar}}}}</div>

    <div class=""signatures"">
        <div class=""sign-block""><div class=""sign-line"">Hazırlayan</div></div>
        <div class=""sign-block""><div class=""sign-line"">Onaylayan</div></div>
        <div class=""sign-block""><div class=""sign-line"">Müşteri</div></div>
    </div>

    <p class=""footer-note"">Bu belge {{{{Tarih}}}} {{{{Saat}}}} tarihinde {{{{FirmaAdi}}}} sistemi tarafından oluşturulmuştur.</p>
</div>";

    private static string ModernTeslimat() => $@"{ModernCss()}
<div class=""document"">
    <div class=""header"">
        <div>
            {{{{FirmaLogo}}}}
            <div class=""company"">{{{{FirmaAdi}}}}</div>
            <div class=""company-sub"">{{{{FirmaSlogan}}}}</div>
        </div>
        <div>
            <div class=""doc-title"">TESLİMAT İRSALİYESİ</div>
            <div class=""doc-no"">{{{{SiparisNo}}}}</div>
            <div class=""doc-date"">Teslimat: {{{{TeslimatTarihi}}}}</div>
        </div>
    </div>

    <div class=""info-row"">
        <div class=""info-box"">
            <div class=""box-title"">Teslim Edilen</div>
            <p><strong>{{{{MusteriAdi}}}}</strong></p>
            <p>{{{{MusteriAdres}}}}</p>
            <p>{{{{MusteriSehir}}}}</p>
            <p>Tel: {{{{MusteriTelefon}}}}</p>
        </div>
        <div class=""info-box"">
            <div class=""box-title"">Sevkiyat Bilgileri</div>
            <p>Sipariş No: <strong>{{{{SiparisNo}}}}</strong></p>
            <p>Sipariş Tarihi: <strong>{{{{SiparisTarihi}}}}</strong></p>
            <p>Teslimat Tarihi: <strong>{{{{TeslimatTarihi}}}}</strong></p>
            <p>Sipariş Müşterisi: <strong>{{{{SiparisMusterisi}}}}</strong></p>
        </div>
    </div>

    <table>
        <thead>
            <tr>
                <th style=""width:35px; text-align:center"">#</th>
                <th>Ürün / Reçete</th>
                <th style=""text-align:right"">En (mm)</th>
                <th style=""text-align:right"">Boy (mm)</th>
                <th style=""text-align:center"">Adet</th>
                <th style=""text-align:right"">Satır m²</th>
                <!--PRICECOL--><th style=""text-align:right"">Birim Fiyat</th><!--/PRICECOL-->
                <!--PRICECOL--><th style=""text-align:right"">Toplam</th><!--/PRICECOL-->
                <th>Özellikler</th>
                <th>Not</th>
                <th>Poz No</th>
            </tr>
        </thead>
        <tbody>
            {{{{Kalemler}}}}
        </tbody>
    </table>

    <div class=""totals"">
        <table>
            <tr><td>Toplam Adet:</td><td style=""text-align:right"">{{{{ToplamAdet}}}}</td></tr>
            <tr><td>Toplam Alan:</td><td style=""text-align:right"">{{{{ToplamAlan}}}} m²</td></tr>
            <!--PRICEROW--><tr><td>Ara Toplam:</td><td style=""text-align:right"">{{{{ToplamTutar}}}} {{{{ParaBirimi}}}}</td></tr><!--/PRICEROW-->
            <!--PRICEROW--><tr><td>KDV (%{{{{KdvOrani}}}}):</td><td style=""text-align:right"">{{{{KdvTutari}}}} {{{{ParaBirimi}}}}</td></tr><!--/PRICEROW-->
            <!--PRICEROW--><tr><td style=""border-top:2px solid #3498db; padding-top:8px""><strong>Genel Toplam:</strong></td><td style=""text-align:right; border-top:2px solid #3498db; padding-top:8px""><strong>{{{{GenelToplam}}}} {{{{ParaBirimi}}}}</strong></td></tr><!--/PRICEROW-->
        </table>
    </div>

    <div class=""signatures"">
        <div class=""sign-block""><div class=""sign-line"">Teslim Eden</div></div>
        <div class=""sign-block""><div class=""sign-line"">Taşıyan</div></div>
        <div class=""sign-block""><div class=""sign-line"">Teslim Alan</div></div>
    </div>

    <p class=""footer-note"">Bu belge {{{{Tarih}}}} {{{{Saat}}}} tarihinde {{{{FirmaAdi}}}} sistemi tarafından oluşturulmuştur.</p>
</div>";

    private static string ModernUretim() => $@"{ModernCss()}
<div class=""document"">
    <div class=""header"">
        <div>
            {{{{FirmaLogo}}}}
            <div class=""company"">{{{{FirmaAdi}}}}</div>
            <div class=""company-sub"">{{{{FirmaSlogan}}}}</div>
        </div>
        <div>
            <div class=""doc-title"">ÜRETİM EMRİ</div>
            <div class=""doc-no"">{{{{IsEmriNo}}}}</div>
            <div class=""doc-date"">{{{{IsEmriTarihi}}}}</div>
        </div>
    </div>

    <div class=""info-row"">
        <div class=""info-box"">
            <div class=""box-title"">İş Emri Bilgileri</div>
            <p>İş Emri No: <strong>{{{{IsEmriNo}}}}</strong></p>
            <p>Planlanan Tarih: <strong>{{{{IsEmriTarihi}}}}</strong></p>
            <p>Durum: <strong>{{{{Durum}}}}</strong></p>
        </div>
        <div class=""info-box"">
            <div class=""box-title"">Sipariş Bilgileri</div>
            <p>Müşteri: <strong>{{{{MusteriAdi}}}}</strong></p>
            <p>Sipariş No: <strong>{{{{SiparisNolari}}}}</strong></p>
        </div>
    </div>

    <table>
        <thead>
            <tr>
                <th style=""width:35px; text-align:center"">#</th>
                <th>Malzeme</th>
                <th>Tür</th>
                <th style=""text-align:right"">En (mm)</th>
                <th style=""text-align:right"">Boy (mm)</th>
                <th style=""text-align:center"">Adet</th>
                <th style=""text-align:right"">Satır m²</th>
            </tr>
        </thead>
        <tbody>
            {{{{Kalemler}}}}
        </tbody>
    </table>

    <div class=""totals"">
        <table>
            <tr><td>Toplam Adet:</td><td style=""text-align:right"">{{{{ToplamAdet}}}}</td></tr>
            <tr><td>Toplam Alan:</td><td style=""text-align:right"">{{{{ToplamAlan}}}} m²</td></tr>
        </table>
    </div>

    <div class=""notes""><strong>Notlar:</strong> {{{{Notlar}}}}</div>

    <div class=""signatures"">
        <div class=""sign-block""><div class=""sign-line"">Hazırlayan</div></div>
        <div class=""sign-block""><div class=""sign-line"">Üretim Sorumlusu</div></div>
    </div>

    <p class=""footer-note"">Bu belge {{{{Tarih}}}} {{{{Saat}}}} tarihinde {{{{FirmaAdi}}}} sistemi tarafından oluşturulmuştur.</p>
</div>";

    private static string ModernCariHareket() => $@"{ModernCss()}
<div class=""document"">
    <div class=""header"">
        <div>
            {{{{FirmaLogo}}}}
            <div class=""company"">{{{{FirmaAdi}}}}</div>
            <div class=""company-sub"">{{{{FirmaSlogan}}}}</div>
        </div>
        <div>
            <div class=""doc-title"">CARİ HAREKET RAPORU</div>
            <div class=""doc-date"">{{{{BaslangicTarihi}}}} - {{{{BitisTarihi}}}}</div>
        </div>
    </div>

    <div class=""info-row"">
        <div class=""info-box"">
            <div class=""box-title"">Müşteri Bilgileri</div>
            <p><strong>{{{{MusteriAdi}}}}</strong></p>
            <p>Müşteri Kodu: {{{{MusteriKodu}}}}</p>
            <p>{{{{MusteriAdres}}}}, {{{{MusteriSehir}}}}</p>
            <p>Tel: {{{{MusteriTelefon}}}}</p>
        </div>
    </div>

    <div class=""summary-boxes"">
        <div class=""summary-box red"">
            <div class=""amount"" style=""color:#e74c3c"">{{{{ToplamBorc}}}} TL</div>
            <div class=""label"">Toplam Borç</div>
        </div>
        <div class=""summary-box green"">
            <div class=""amount"" style=""color:#27ae60"">{{{{ToplamAlacak}}}} TL</div>
            <div class=""label"">Toplam Alacak</div>
        </div>
        <div class=""summary-box blue"">
            <div class=""amount"" style=""color:#2c3e50"">{{{{Bakiye}}}} TL</div>
            <div class=""label"">Bakiye ({{{{BakiyeYon}}}})</div>
        </div>
    </div>

    <table>
        <thead>
            <tr>
                <th style=""width:35px; text-align:center"">#</th>
                <th>Tarih</th>
                <th>İşlem</th>
                <th>Ödeme Türü</th>
                <th>Açıklama</th>
                <th style=""text-align:right"">Borç</th>
                <th style=""text-align:right"">Alacak</th>
                <th style=""text-align:right"">Bakiye</th>
                <th style=""text-align:center"">B/A</th>
            </tr>
        </thead>
        <tbody>
            {{{{Hareketler}}}}
        </tbody>
        <tfoot>
            <tr>
                <td colspan=""5"" style=""text-align:right"">TOPLAM</td>
                <td style=""text-align:right"">{{{{ToplamBorc}}}}</td>
                <td style=""text-align:right"">{{{{ToplamAlacak}}}}</td>
                <td style=""text-align:right"">{{{{Bakiye}}}}</td>
                <td style=""text-align:center"">{{{{BakiyeYon}}}}</td>
            </tr>
        </tfoot>
    </table>

    <p class=""footer-note"">Bu belge {{{{Tarih}}}} {{{{Saat}}}} tarihinde {{{{FirmaAdi}}}} sistemi tarafından oluşturulmuştur.</p>
</div>";

    private static string ModernCariEkstre() => $@"{ModernCss()}
<div class=""document"">
    <div class=""header"">
        <div>
            {{{{FirmaLogo}}}}
            <div class=""company"">{{{{FirmaAdi}}}}</div>
            <div class=""company-sub"">{{{{FirmaSlogan}}}}</div>
        </div>
        <div>
            <div class=""doc-title"">HESAP EKSTRESİ</div>
            <div class=""doc-no"">{{{{EkstreTarihi}}}}</div>
            <div class=""doc-date"">{{{{BaslangicTarihi}}}} - {{{{BitisTarihi}}}}</div>
        </div>
    </div>

    <div class=""info-row"">
        <div class=""info-box"">
            <div class=""box-title"">Müşteri Bilgileri</div>
            <p><strong>{{{{MusteriAdi}}}}</strong></p>
            <p>Müşteri Kodu: {{{{MusteriKodu}}}}</p>
            <p>{{{{MusteriAdres}}}}, {{{{MusteriSehir}}}}</p>
            <p>Tel: {{{{MusteriTelefon}}}}</p>
            <p>Vergi No: {{{{MusteriVergiNo}}}} &bull; V.D.: {{{{MusteriVergiDairesi}}}}</p>
        </div>
        <div class=""info-box"">
            <div class=""box-title"">Hesap Özeti</div>
            <p>Devreden Bakiye: <strong>{{{{DevredenBakiye}}}} TL ({{{{DevredenYon}}}})</strong></p>
            <p>Dönem Borç: <strong style=""color:#e74c3c"">{{{{ToplamBorc}}}} TL</strong></p>
            <p>Dönem Alacak: <strong style=""color:#27ae60"">{{{{ToplamAlacak}}}} TL</strong></p>
            <p style=""font-size:16px; margin-top:10px; padding-top:10px; border-top:2px solid #3498db"">
                Güncel Bakiye: <strong>{{{{Bakiye}}}} TL ({{{{BakiyeYon}}}})</strong>
            </p>
        </div>
    </div>

    <table>
        <thead>
            <tr>
                <th style=""width:35px; text-align:center"">#</th>
                <th>Tarih</th>
                <th>İşlem</th>
                <th>Ödeme Türü</th>
                <th>Açıklama</th>
                <th style=""text-align:right"">Borç</th>
                <th style=""text-align:right"">Alacak</th>
                <th style=""text-align:right"">Bakiye</th>
                <th style=""text-align:center"">B/A</th>
            </tr>
        </thead>
        <tbody>
            {{{{Hareketler}}}}
        </tbody>
        <tfoot>
            <tr>
                <td colspan=""5"" style=""text-align:right"">TOPLAM</td>
                <td style=""text-align:right"">{{{{ToplamBorc}}}}</td>
                <td style=""text-align:right"">{{{{ToplamAlacak}}}}</td>
                <td style=""text-align:right"">{{{{Bakiye}}}}</td>
                <td style=""text-align:center"">{{{{BakiyeYon}}}}</td>
            </tr>
        </tfoot>
    </table>

    <div class=""signatures"">
        <div class=""sign-block""><div class=""sign-line"">Yetkili</div></div>
        <div class=""sign-block""><div class=""sign-line"">Müşteri</div></div>
    </div>

    <p class=""footer-note"">Bu belge {{{{Tarih}}}} {{{{Saat}}}} tarihinde {{{{FirmaAdi}}}} sistemi tarafından oluşturulmuştur.</p>
</div>";

    // ═══════════════════════════════════════════════════════════
    //  KLASİK STİL - Geleneksel çizgili, resmi
    // ═══════════════════════════════════════════════════════════

    private static string KlasikCss() => @"
<style>
    * { margin: 0; padding: 0; box-sizing: border-box; }
    body { font-family: 'Times New Roman', Georgia, serif; color: #000; font-size: 13px; padding: 20px; background: #fff; }
    .document { max-width: 210mm; margin: 0 auto; }

    /* Başlık */
    .header { border-bottom: 3px double #000; padding-bottom: 15px; margin-bottom: 20px; display: flex; justify-content: space-between; align-items: flex-end; }
    .header .company { font-size: 26px; font-weight: 700; letter-spacing: 2px; }
    .header .company-sub { font-size: 12px; color: #555; margin-top: 3px; font-style: italic; }
    .header .doc-title { font-size: 18px; font-weight: 700; text-align: right; text-transform: uppercase; letter-spacing: 1px; }
    .header .doc-no { font-size: 14px; text-align: right; margin-top: 5px; }
    .header .doc-date { font-size: 12px; text-align: right; color: #555; margin-top: 3px; }

    /* Bilgi Kutuları */
    .info-row { display: flex; gap: 20px; margin-bottom: 20px; }
    .info-box { flex: 1; border: 1px solid #999; padding: 15px; }
    .info-box .box-title { font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 1px; border-bottom: 1px solid #999; padding-bottom: 6px; margin-bottom: 10px; }
    .info-box p { margin: 4px 0; font-size: 13px; line-height: 1.5; }

    /* Tablo */
    table { width: 100%; border-collapse: collapse; margin: 20px 0; }
    table thead th { background: #000; color: #fff; padding: 8px 10px; font-size: 11px; text-transform: uppercase; letter-spacing: 0.5px; text-align: left; border: 1px solid #000; }
    table tbody td { padding: 7px 10px; border: 1px solid #ccc; font-size: 12px; }
    table tbody tr:nth-child(even) { background: #f5f5f5; }
    table tfoot td { background: #000; color: #fff; padding: 8px 10px; font-weight: 700; border: 1px solid #000; }

    /* Toplamlar */
    .totals { display: flex; justify-content: flex-end; margin: 15px 0; }
    .totals table { width: 280px; }
    .totals td { padding: 5px 10px; font-size: 13px; border: 1px solid #ccc; }
    .totals tr:last-child { font-weight: 700; font-size: 14px; }
    .totals tr:last-child td { border-top: 2px solid #000; }

    /* Notlar */
    .notes { border: 1px solid #999; padding: 12px; margin: 20px 0; font-size: 12px; }

    /* İmza */
    .signatures { display: flex; justify-content: space-between; margin-top: 50px; }
    .sign-block { text-align: center; width: 180px; }
    .sign-line { border-top: 1px solid #000; margin-top: 70px; padding-top: 8px; font-size: 12px; font-weight: 700; }

    /* Özet */
    .summary-boxes { display: flex; gap: 15px; margin-bottom: 20px; }
    .summary-box { flex: 1; text-align: center; padding: 12px; border: 2px solid #000; }
    .summary-box .amount { font-size: 18px; font-weight: 700; }
    .summary-box .label { font-size: 11px; text-transform: uppercase; margin-top: 4px; }

    .footer-note { text-align: center; margin-top: 30px; color: #999; font-size: 10px; border-top: 1px solid #ccc; padding-top: 10px; }

    @page { size: A4; margin: 15mm; }
    @media print { body { padding: 0; } table thead th, table tfoot td { -webkit-print-color-adjust: exact; print-color-adjust: exact; } }
</style>";

    private static string KlasikSiparis() => $@"{KlasikCss()}
<div class=""document"">
    <div class=""header"">
        <div>
            {{{{FirmaLogo}}}}
            <div class=""company"">{{{{FirmaAdi}}}}</div>
            <div class=""company-sub"">{{{{FirmaSlogan}}}}</div>
        </div>
        <div>
            <div class=""doc-title"">Sipariş Fişi</div>
            <div class=""doc-no"">{{{{SiparisNo}}}}</div>
            <div class=""doc-date"">{{{{SiparisTarihi}}}}</div>
        </div>
    </div>

    <div class=""info-row"">
        <div class=""info-box"">
            <div class=""box-title"">Müşteri Bilgileri</div>
            <p><strong>{{{{MusteriAdi}}}}</strong></p>
            <p>{{{{MusteriAdres}}}}, {{{{MusteriSehir}}}}</p>
            <p>Tel: {{{{MusteriTelefon}}}} | E-posta: {{{{MusteriEmail}}}}</p>
            <p>Vergi No: {{{{MusteriVergiNo}}}} | V.D.: {{{{MusteriVergiDairesi}}}}</p>
        </div>
        <div class=""info-box"">
            <div class=""box-title"">Sipariş Bilgileri</div>
            <p>Sipariş No: <strong>{{{{SiparisNo}}}}</strong></p>
            <p>Tarih: <strong>{{{{SiparisTarihi}}}}</strong></p>
            <p>Teslim: <strong>{{{{TeslimTarihi}}}}</strong></p>
            <p>Sipariş Müşterisi: <strong>{{{{SiparisMusterisi}}}}</strong></p>
            <p>Durum: <strong>{{{{Durum}}}}</strong> | Para Birimi: <strong>{{{{ParaBirimi}}}}</strong></p>
        </div>
    </div>

    <table>
        <thead><tr><th style=""width:35px;text-align:center"">#</th><th>Ürün / Reçete</th><th style=""text-align:right"">En (mm)</th><th style=""text-align:right"">Boy (mm)</th><th style=""text-align:center"">Adet</th><th style=""text-align:right"">Satır m²</th><th style=""text-align:right"">Birim Fiyat</th><th style=""text-align:right"">Toplam</th><th>Özellikler</th><th>Not</th><th>Poz</th></tr></thead>
        <tbody>{{{{Kalemler}}}}</tbody>
    </table>

    <div class=""totals""><table><tr><td>Toplam Adet:</td><td style=""text-align:right"">{{{{ToplamAdet}}}}</td></tr><tr><td>Toplam Alan:</td><td style=""text-align:right"">{{{{ToplamAlan}}}} m²</td></tr><tr><td>Genel Toplam:</td><td style=""text-align:right"">{{{{ToplamTutar}}}} {{{{ParaBirimi}}}}</td></tr><tr><td style=""border-top:1px solid #333;padding-top:6px"">Cari Bakiye:</td><td style=""text-align:right;border-top:1px solid #333;padding-top:6px"">{{{{CariBakiye}}}} TL ({{{{CariBakiyeYon}}}})</td></tr></table></div>

    <div class=""notes""><strong>Notlar:</strong> {{{{Notlar}}}}</div>

    <div class=""signatures"">
        <div class=""sign-block""><div class=""sign-line"">Hazırlayan</div></div>
        <div class=""sign-block""><div class=""sign-line"">Onaylayan</div></div>
        <div class=""sign-block""><div class=""sign-line"">Müşteri</div></div>
    </div>
    <p class=""footer-note"">{{{{Tarih}}}} {{{{Saat}}}} — {{{{FirmaAdi}}}}</p>
</div>";

    private static string KlasikTeslimat() => $@"{KlasikCss()}
<div class=""document"">
    <div class=""header"">
        <div>{{{{FirmaLogo}}}}<div class=""company"">{{{{FirmaAdi}}}}</div><div class=""company-sub"">{{{{FirmaSlogan}}}}</div></div>
        <div><div class=""doc-title"">Teslimat İrsaliyesi</div><div class=""doc-no"">{{{{SiparisNo}}}}</div><div class=""doc-date"">Teslimat: {{{{TeslimatTarihi}}}}</div></div>
    </div>
    <div class=""info-row"">
        <div class=""info-box""><div class=""box-title"">Teslim Edilen</div><p><strong>{{{{MusteriAdi}}}}</strong></p><p>{{{{MusteriAdres}}}}, {{{{MusteriSehir}}}}</p><p>Tel: {{{{MusteriTelefon}}}}</p></div>
        <div class=""info-box""><div class=""box-title"">Sevkiyat</div><p>Sipariş No: <strong>{{{{SiparisNo}}}}</strong></p><p>Sipariş Tarihi: <strong>{{{{SiparisTarihi}}}}</strong></p><p>Teslimat Tarihi: <strong>{{{{TeslimatTarihi}}}}</strong></p><p>Sipariş Müşterisi: <strong>{{{{SiparisMusterisi}}}}</strong></p></div>
    </div>
    <table><thead><tr><th style=""width:35px;text-align:center"">#</th><th>Ürün / Reçete</th><th style=""text-align:right"">En (mm)</th><th style=""text-align:right"">Boy (mm)</th><th style=""text-align:center"">Adet</th><th style=""text-align:right"">Satır m²</th><!--PRICECOL--><th style=""text-align:right"">Birim Fiyat</th><!--/PRICECOL--><!--PRICECOL--><th style=""text-align:right"">Toplam</th><!--/PRICECOL--><th>Özellikler</th><th>Not</th><th>Poz</th></tr></thead><tbody>{{{{Kalemler}}}}</tbody></table>
    <div class=""totals""><table><tr><td>Toplam Adet:</td><td style=""text-align:right"">{{{{ToplamAdet}}}}</td></tr><tr><td>Toplam Alan:</td><td style=""text-align:right"">{{{{ToplamAlan}}}} m²</td></tr><!--PRICEROW--><tr><td>Genel Toplam:</td><td style=""text-align:right"">{{{{ToplamTutar}}}} {{{{ParaBirimi}}}}</td></tr><!--/PRICEROW--></table></div>
    <div class=""signatures""><div class=""sign-block""><div class=""sign-line"">Teslim Eden</div></div><div class=""sign-block""><div class=""sign-line"">Taşıyan</div></div><div class=""sign-block""><div class=""sign-line"">Teslim Alan</div></div></div>
    <p class=""footer-note"">{{{{Tarih}}}} {{{{Saat}}}} — {{{{FirmaAdi}}}}</p>
</div>";

    private static string KlasikUretim() => $@"{KlasikCss()}
<div class=""document"">
    <div class=""header"">
        <div>{{{{FirmaLogo}}}}<div class=""company"">{{{{FirmaAdi}}}}</div><div class=""company-sub"">{{{{FirmaSlogan}}}}</div></div>
        <div><div class=""doc-title"">Üretim Emri</div><div class=""doc-no"">{{{{IsEmriNo}}}}</div><div class=""doc-date"">{{{{IsEmriTarihi}}}}</div></div>
    </div>
    <div class=""info-row"">
        <div class=""info-box""><div class=""box-title"">İş Emri</div><p>İş Emri No: <strong>{{{{IsEmriNo}}}}</strong></p><p>Tarih: <strong>{{{{IsEmriTarihi}}}}</strong></p><p>Durum: <strong>{{{{Durum}}}}</strong></p></div>
        <div class=""info-box""><div class=""box-title"">Sipariş</div><p>Müşteri: <strong>{{{{MusteriAdi}}}}</strong></p><p>Sipariş No: <strong>{{{{SiparisNolari}}}}</strong></p></div>
    </div>
    <table><thead><tr><th style=""width:35px;text-align:center"">#</th><th>Malzeme</th><th>Tür</th><th style=""text-align:right"">En (mm)</th><th style=""text-align:right"">Boy (mm)</th><th style=""text-align:center"">Adet</th><th style=""text-align:right"">Satır m²</th></tr></thead><tbody>{{{{Kalemler}}}}</tbody></table>
    <div class=""totals""><table><tr><td>Toplam Adet:</td><td style=""text-align:right"">{{{{ToplamAdet}}}}</td></tr><tr><td>Toplam Alan:</td><td style=""text-align:right"">{{{{ToplamAlan}}}} m²</td></tr></table></div>
    <div class=""notes""><strong>Notlar:</strong> {{{{Notlar}}}}</div>
    <div class=""signatures""><div class=""sign-block""><div class=""sign-line"">Hazırlayan</div></div><div class=""sign-block""><div class=""sign-line"">Üretim Sorumlusu</div></div></div>
    <p class=""footer-note"">{{{{Tarih}}}} {{{{Saat}}}} — {{{{FirmaAdi}}}}</p>
</div>";

    private static string KlasikCariHareket() => $@"{KlasikCss()}
<div class=""document"">
    <div class=""header"">
        <div>{{{{FirmaLogo}}}}<div class=""company"">{{{{FirmaAdi}}}}</div><div class=""company-sub"">{{{{FirmaSlogan}}}}</div></div>
        <div><div class=""doc-title"">Cari Hareket Raporu</div><div class=""doc-date"">{{{{BaslangicTarihi}}}} - {{{{BitisTarihi}}}}</div></div>
    </div>
    <div class=""info-row"">
        <div class=""info-box""><div class=""box-title"">Müşteri</div><p><strong>{{{{MusteriAdi}}}}</strong> ({{{{MusteriKodu}}}})</p><p>{{{{MusteriAdres}}}}, {{{{MusteriSehir}}}}</p><p>Tel: {{{{MusteriTelefon}}}}</p></div>
    </div>
    <div class=""summary-boxes"">
        <div class=""summary-box""><div class=""amount"" style=""color:#c0392b"">{{{{ToplamBorc}}}} TL</div><div class=""label"">Borç</div></div>
        <div class=""summary-box""><div class=""amount"" style=""color:#27ae60"">{{{{ToplamAlacak}}}} TL</div><div class=""label"">Alacak</div></div>
        <div class=""summary-box""><div class=""amount"">{{{{Bakiye}}}} TL</div><div class=""label"">Bakiye ({{{{BakiyeYon}}}})</div></div>
    </div>
    <table><thead><tr><th style=""width:35px;text-align:center"">#</th><th>Tarih</th><th>İşlem</th><th>Ödeme Türü</th><th>Açıklama</th><th style=""text-align:right"">Borç</th><th style=""text-align:right"">Alacak</th><th style=""text-align:right"">Bakiye</th><th style=""text-align:center"">B/A</th></tr></thead>
    <tbody>{{{{Hareketler}}}}</tbody>
    <tfoot><tr><td colspan=""5"" style=""text-align:right"">TOPLAM</td><td style=""text-align:right"">{{{{ToplamBorc}}}}</td><td style=""text-align:right"">{{{{ToplamAlacak}}}}</td><td style=""text-align:right"">{{{{Bakiye}}}}</td><td style=""text-align:center"">{{{{BakiyeYon}}}}</td></tr></tfoot></table>
    <p class=""footer-note"">{{{{Tarih}}}} {{{{Saat}}}} — {{{{FirmaAdi}}}}</p>
</div>";

    private static string KlasikCariEkstre() => $@"{KlasikCss()}
<div class=""document"">
    <div class=""header"">
        <div>{{{{FirmaLogo}}}}<div class=""company"">{{{{FirmaAdi}}}}</div><div class=""company-sub"">{{{{FirmaSlogan}}}}</div></div>
        <div><div class=""doc-title"">Hesap Ekstresi</div><div class=""doc-no"">{{{{EkstreTarihi}}}}</div><div class=""doc-date"">{{{{BaslangicTarihi}}}} - {{{{BitisTarihi}}}}</div></div>
    </div>
    <div class=""info-row"">
        <div class=""info-box""><div class=""box-title"">Müşteri</div><p><strong>{{{{MusteriAdi}}}}</strong> ({{{{MusteriKodu}}}})</p><p>{{{{MusteriAdres}}}}, {{{{MusteriSehir}}}}</p><p>Tel: {{{{MusteriTelefon}}}}</p><p>Vergi No: {{{{MusteriVergiNo}}}} | V.D.: {{{{MusteriVergiDairesi}}}}</p></div>
        <div class=""info-box""><div class=""box-title"">Hesap Özeti</div><p>Devreden: <strong>{{{{DevredenBakiye}}}} TL ({{{{DevredenYon}}}})</strong></p><p>Borç: <strong style=""color:#c0392b"">{{{{ToplamBorc}}}} TL</strong></p><p>Alacak: <strong style=""color:#27ae60"">{{{{ToplamAlacak}}}} TL</strong></p><p style=""font-size:15px;margin-top:8px;padding-top:8px;border-top:2px double #000"">Bakiye: <strong>{{{{Bakiye}}}} TL ({{{{BakiyeYon}}}})</strong></p></div>
    </div>
    <table><thead><tr><th style=""width:35px;text-align:center"">#</th><th>Tarih</th><th>İşlem</th><th>Ödeme Türü</th><th>Açıklama</th><th style=""text-align:right"">Borç</th><th style=""text-align:right"">Alacak</th><th style=""text-align:right"">Bakiye</th><th style=""text-align:center"">B/A</th></tr></thead>
    <tbody>{{{{Hareketler}}}}</tbody>
    <tfoot><tr><td colspan=""5"" style=""text-align:right"">TOPLAM</td><td style=""text-align:right"">{{{{ToplamBorc}}}}</td><td style=""text-align:right"">{{{{ToplamAlacak}}}}</td><td style=""text-align:right"">{{{{Bakiye}}}}</td><td style=""text-align:center"">{{{{BakiyeYon}}}}</td></tr></tfoot></table>
    <div class=""signatures""><div class=""sign-block""><div class=""sign-line"">Yetkili</div></div><div class=""sign-block""><div class=""sign-line"">Müşteri</div></div></div>
    <p class=""footer-note"">{{{{Tarih}}}} {{{{Saat}}}} — {{{{FirmaAdi}}}}</p>
</div>";

    // ═══════════════════════════════════════════════════════════
    //  MİNİMAL STİL - Sade, temiz, minimum süsleme
    // ═══════════════════════════════════════════════════════════

    private static string MinimalCss() => @"
<style>
    * { margin: 0; padding: 0; box-sizing: border-box; }
    body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; color: #333; font-size: 13px; padding: 25px; background: #fff; }
    .document { max-width: 210mm; margin: 0 auto; }

    .header { padding-bottom: 20px; margin-bottom: 25px; border-bottom: 1px solid #e0e0e0; display: flex; justify-content: space-between; align-items: flex-start; }
    .header .company { font-size: 22px; font-weight: 300; letter-spacing: 3px; color: #555; }
    .header .doc-title { font-size: 16px; font-weight: 600; text-align: right; color: #333; }
    .header .doc-no { font-size: 13px; text-align: right; color: #888; margin-top: 4px; }
    .header .doc-date { font-size: 12px; text-align: right; color: #aaa; margin-top: 2px; }

    .info-row { display: flex; gap: 30px; margin-bottom: 25px; }
    .info-box { flex: 1; }
    .info-box .box-title { font-size: 10px; font-weight: 600; text-transform: uppercase; color: #aaa; letter-spacing: 2px; margin-bottom: 10px; }
    .info-box p { margin: 3px 0; font-size: 13px; line-height: 1.6; color: #555; }
    .info-box strong { color: #333; }

    table { width: 100%; border-collapse: collapse; margin: 20px 0; }
    table thead th { padding: 10px 12px; font-size: 10px; text-transform: uppercase; letter-spacing: 1px; color: #999; font-weight: 600; text-align: left; border-bottom: 2px solid #333; }
    table tbody td { padding: 10px 12px; border-bottom: 1px solid #f0f0f0; font-size: 12px; color: #555; }
    table tbody tr:hover { background: #fafafa; }
    table tfoot td { padding: 10px 12px; font-weight: 600; color: #333; border-top: 2px solid #333; }

    .totals { display: flex; justify-content: flex-end; margin: 20px 0; }
    .totals table { width: 260px; }
    .totals table thead th { display: none; }
    .totals td { padding: 6px 0; font-size: 13px; color: #555; border-bottom: none; }
    .totals tr:last-child td { font-size: 15px; font-weight: 600; color: #333; padding-top: 10px; border-top: 1px solid #333; }

    .notes { color: #888; font-size: 12px; margin: 20px 0; padding-top: 15px; border-top: 1px solid #f0f0f0; }

    .signatures { display: flex; justify-content: space-between; margin-top: 60px; }
    .sign-block { text-align: center; width: 160px; }
    .sign-line { border-top: 1px solid #ccc; margin-top: 70px; padding-top: 8px; font-size: 11px; color: #999; }

    .summary-boxes { display: flex; gap: 20px; margin-bottom: 25px; }
    .summary-box { flex: 1; text-align: center; padding: 15px; }
    .summary-box .amount { font-size: 22px; font-weight: 300; }
    .summary-box .label { font-size: 10px; text-transform: uppercase; letter-spacing: 2px; color: #aaa; margin-top: 5px; }

    .footer-note { text-align: center; margin-top: 40px; color: #ccc; font-size: 10px; }

    @page { size: A4; margin: 15mm; }
    @media print { body { padding: 0; } }
</style>";

    private static string MinimalSiparis() => $@"{MinimalCss()}
<div class=""document"">
    <div class=""header"">
        <div>{{{{FirmaLogo}}}}<div class=""company"">{{{{FirmaAdi}}}}</div></div>
        <div><div class=""doc-title"">Sipariş</div><div class=""doc-no"">{{{{SiparisNo}}}}</div><div class=""doc-date"">{{{{SiparisTarihi}}}}</div></div>
    </div>
    <div class=""info-row"">
        <div class=""info-box""><div class=""box-title"">Müşteri</div><p><strong>{{{{MusteriAdi}}}}</strong></p><p>{{{{MusteriAdres}}}}, {{{{MusteriSehir}}}}</p><p>{{{{MusteriTelefon}}}} · {{{{MusteriEmail}}}}</p><p>VKN: {{{{MusteriVergiNo}}}} · {{{{MusteriVergiDairesi}}}}</p></div>
        <div class=""info-box""><div class=""box-title"">Detaylar</div><p>Teslim: <strong>{{{{TeslimTarihi}}}}</strong></p><p>Sipariş Müşterisi: <strong>{{{{SiparisMusterisi}}}}</strong></p><p>Durum: <strong>{{{{Durum}}}}</strong></p><p>Para Birimi: <strong>{{{{ParaBirimi}}}}</strong></p></div>
    </div>
    <table><thead><tr><th style=""width:30px;text-align:center"">#</th><th>Ürün</th><th style=""text-align:right"">En</th><th style=""text-align:right"">Boy</th><th style=""text-align:center"">Adet</th><th style=""text-align:right"">Satır m²</th><th style=""text-align:right"">Fiyat</th><th style=""text-align:right"">Toplam</th><th>Özellik</th><th>Not</th><th>Poz</th></tr></thead><tbody>{{{{Kalemler}}}}</tbody></table>
    <div class=""totals""><table><tr><td>Adet</td><td style=""text-align:right"">{{{{ToplamAdet}}}}</td></tr><tr><td>Alan</td><td style=""text-align:right"">{{{{ToplamAlan}}}} m²</td></tr><tr><td>Toplam</td><td style=""text-align:right"">{{{{ToplamTutar}}}} {{{{ParaBirimi}}}}</td></tr><tr><td style=""border-top:1px solid #e0e0e0;padding-top:6px"">Cari Bakiye</td><td style=""text-align:right;border-top:1px solid #e0e0e0;padding-top:6px"">{{{{CariBakiye}}}} TL ({{{{CariBakiyeYon}}}})</td></tr></table></div>
    <div class=""notes"">{{{{Notlar}}}}</div>
    <div class=""signatures""><div class=""sign-block""><div class=""sign-line"">Hazırlayan</div></div><div class=""sign-block""><div class=""sign-line"">Onaylayan</div></div><div class=""sign-block""><div class=""sign-line"">Müşteri</div></div></div>
    <p class=""footer-note"">{{{{Tarih}}}} · {{{{Saat}}}} · {{{{FirmaAdi}}}}</p>
</div>";

    private static string MinimalTeslimat() => $@"{MinimalCss()}
<div class=""document"">
    <div class=""header"">
        <div>{{{{FirmaLogo}}}}<div class=""company"">{{{{FirmaAdi}}}}</div></div>
        <div><div class=""doc-title"">Teslimat</div><div class=""doc-no"">{{{{SiparisNo}}}}</div><div class=""doc-date"">{{{{TeslimatTarihi}}}}</div></div>
    </div>
    <div class=""info-row"">
        <div class=""info-box""><div class=""box-title"">Alıcı</div><p><strong>{{{{MusteriAdi}}}}</strong></p><p>{{{{MusteriAdres}}}}, {{{{MusteriSehir}}}}</p><p>{{{{MusteriTelefon}}}}</p></div>
        <div class=""info-box""><div class=""box-title"">Sevkiyat</div><p>Sipariş: <strong>{{{{SiparisNo}}}}</strong></p><p>Sipariş Tarihi: <strong>{{{{SiparisTarihi}}}}</strong></p><p>Teslimat: <strong>{{{{TeslimatTarihi}}}}</strong></p><p>Sipariş Müşterisi: <strong>{{{{SiparisMusterisi}}}}</strong></p></div>
    </div>
    <table><thead><tr><th style=""width:30px;text-align:center"">#</th><th>Ürün</th><th style=""text-align:right"">En</th><th style=""text-align:right"">Boy</th><th style=""text-align:center"">Adet</th><th style=""text-align:right"">Satır m²</th><!--PRICECOL--><th style=""text-align:right"">Fiyat</th><!--/PRICECOL--><!--PRICECOL--><th style=""text-align:right"">Toplam</th><!--/PRICECOL--><th>Özellik</th><th>Not</th><th>Poz</th></tr></thead><tbody>{{{{Kalemler}}}}</tbody></table>
    <div class=""totals""><table><tr><td>Adet</td><td style=""text-align:right"">{{{{ToplamAdet}}}}</td></tr><tr><td>Alan</td><td style=""text-align:right"">{{{{ToplamAlan}}}} m²</td></tr><!--PRICEROW--><tr><td>Toplam</td><td style=""text-align:right"">{{{{ToplamTutar}}}} {{{{ParaBirimi}}}}</td></tr><!--/PRICEROW--></table></div>
    <div class=""signatures""><div class=""sign-block""><div class=""sign-line"">Teslim Eden</div></div><div class=""sign-block""><div class=""sign-line"">Taşıyan</div></div><div class=""sign-block""><div class=""sign-line"">Teslim Alan</div></div></div>
    <p class=""footer-note"">{{{{Tarih}}}} · {{{{Saat}}}} · {{{{FirmaAdi}}}}</p>
</div>";

    private static string MinimalUretim() => $@"{MinimalCss()}
<div class=""document"">
    <div class=""header"">
        <div>{{{{FirmaLogo}}}}<div class=""company"">{{{{FirmaAdi}}}}</div></div>
        <div><div class=""doc-title"">Üretim Emri</div><div class=""doc-no"">{{{{IsEmriNo}}}}</div><div class=""doc-date"">{{{{IsEmriTarihi}}}}</div></div>
    </div>
    <div class=""info-row"">
        <div class=""info-box""><div class=""box-title"">İş Emri</div><p>No: <strong>{{{{IsEmriNo}}}}</strong></p><p>Tarih: <strong>{{{{IsEmriTarihi}}}}</strong></p><p>Durum: <strong>{{{{Durum}}}}</strong></p></div>
        <div class=""info-box""><div class=""box-title"">Sipariş</div><p>Müşteri: <strong>{{{{MusteriAdi}}}}</strong></p><p>Sipariş: <strong>{{{{SiparisNolari}}}}</strong></p></div>
    </div>
    <table><thead><tr><th style=""width:30px;text-align:center"">#</th><th>Malzeme</th><th>Tür</th><th style=""text-align:right"">En</th><th style=""text-align:right"">Boy</th><th style=""text-align:center"">Adet</th><th style=""text-align:right"">Satır m²</th></tr></thead><tbody>{{{{Kalemler}}}}</tbody></table>
    <div class=""totals""><table><tr><td>Adet</td><td style=""text-align:right"">{{{{ToplamAdet}}}}</td></tr><tr><td>Alan</td><td style=""text-align:right"">{{{{ToplamAlan}}}} m²</td></tr></table></div>
    <div class=""notes"">{{{{Notlar}}}}</div>
    <div class=""signatures""><div class=""sign-block""><div class=""sign-line"">Hazırlayan</div></div><div class=""sign-block""><div class=""sign-line"">Üretim Sorumlusu</div></div></div>
    <p class=""footer-note"">{{{{Tarih}}}} · {{{{Saat}}}} · {{{{FirmaAdi}}}}</p>
</div>";

    private static string MinimalCariHareket() => $@"{MinimalCss()}
<div class=""document"">
    <div class=""header"">
        <div>{{{{FirmaLogo}}}}<div class=""company"">{{{{FirmaAdi}}}}</div></div>
        <div><div class=""doc-title"">Cari Hareketler</div><div class=""doc-date"">{{{{BaslangicTarihi}}}} — {{{{BitisTarihi}}}}</div></div>
    </div>
    <div class=""info-row"">
        <div class=""info-box""><div class=""box-title"">Müşteri</div><p><strong>{{{{MusteriAdi}}}}</strong> · {{{{MusteriKodu}}}}</p><p>{{{{MusteriAdres}}}}, {{{{MusteriSehir}}}}</p><p>{{{{MusteriTelefon}}}}</p></div>
    </div>
    <div class=""summary-boxes"">
        <div class=""summary-box""><div class=""amount"" style=""color:#e74c3c"">{{{{ToplamBorc}}}} TL</div><div class=""label"">Borç</div></div>
        <div class=""summary-box""><div class=""amount"" style=""color:#27ae60"">{{{{ToplamAlacak}}}} TL</div><div class=""label"">Alacak</div></div>
        <div class=""summary-box""><div class=""amount"">{{{{Bakiye}}}} TL</div><div class=""label"">Bakiye · {{{{BakiyeYon}}}}</div></div>
    </div>
    <table><thead><tr><th style=""width:30px;text-align:center"">#</th><th>Tarih</th><th>İşlem</th><th>Ödeme</th><th>Açıklama</th><th style=""text-align:right"">Borç</th><th style=""text-align:right"">Alacak</th><th style=""text-align:right"">Bakiye</th><th style=""text-align:center"">B/A</th></tr></thead>
    <tbody>{{{{Hareketler}}}}</tbody>
    <tfoot><tr><td colspan=""5"" style=""text-align:right"">Toplam</td><td style=""text-align:right"">{{{{ToplamBorc}}}}</td><td style=""text-align:right"">{{{{ToplamAlacak}}}}</td><td style=""text-align:right"">{{{{Bakiye}}}}</td><td style=""text-align:center"">{{{{BakiyeYon}}}}</td></tr></tfoot></table>
    <p class=""footer-note"">{{{{Tarih}}}} · {{{{Saat}}}} · {{{{FirmaAdi}}}}</p>
</div>";

    private static string MinimalCariEkstre() => $@"{MinimalCss()}
<div class=""document"">
    <div class=""header"">
        <div>{{{{FirmaLogo}}}}<div class=""company"">{{{{FirmaAdi}}}}</div></div>
        <div><div class=""doc-title"">Hesap Ekstresi</div><div class=""doc-no"">{{{{EkstreTarihi}}}}</div><div class=""doc-date"">{{{{BaslangicTarihi}}}} — {{{{BitisTarihi}}}}</div></div>
    </div>
    <div class=""info-row"">
        <div class=""info-box""><div class=""box-title"">Müşteri</div><p><strong>{{{{MusteriAdi}}}}</strong> · {{{{MusteriKodu}}}}</p><p>{{{{MusteriAdres}}}}, {{{{MusteriSehir}}}}</p><p>{{{{MusteriTelefon}}}}</p><p>VKN: {{{{MusteriVergiNo}}}} · {{{{MusteriVergiDairesi}}}}</p></div>
        <div class=""info-box""><div class=""box-title"">Özet</div><p>Devreden: <strong>{{{{DevredenBakiye}}}} TL ({{{{DevredenYon}}}})</strong></p><p>Borç: <strong style=""color:#e74c3c"">{{{{ToplamBorc}}}} TL</strong></p><p>Alacak: <strong style=""color:#27ae60"">{{{{ToplamAlacak}}}} TL</strong></p><p style=""font-size:15px;margin-top:8px;padding-top:8px;border-top:1px solid #e0e0e0"">Bakiye: <strong>{{{{Bakiye}}}} TL ({{{{BakiyeYon}}}})</strong></p></div>
    </div>
    <table><thead><tr><th style=""width:30px;text-align:center"">#</th><th>Tarih</th><th>İşlem</th><th>Ödeme</th><th>Açıklama</th><th style=""text-align:right"">Borç</th><th style=""text-align:right"">Alacak</th><th style=""text-align:right"">Bakiye</th><th style=""text-align:center"">B/A</th></tr></thead>
    <tbody>{{{{Hareketler}}}}</tbody>
    <tfoot><tr><td colspan=""5"" style=""text-align:right"">Toplam</td><td style=""text-align:right"">{{{{ToplamBorc}}}}</td><td style=""text-align:right"">{{{{ToplamAlacak}}}}</td><td style=""text-align:right"">{{{{Bakiye}}}}</td><td style=""text-align:center"">{{{{BakiyeYon}}}}</td></tr></tfoot></table>
    <div class=""signatures""><div class=""sign-block""><div class=""sign-line"">Yetkili</div></div><div class=""sign-block""><div class=""sign-line"">Müşteri</div></div></div>
    <p class=""footer-note"">{{{{Tarih}}}} · {{{{Saat}}}} · {{{{FirmaAdi}}}}</p>
</div>";

    // ═══════════════════════════════════════════════════════════
    //  ÇITA RAPORU - Fiyatsız, aynı ürünler birleşik
    // ═══════════════════════════════════════════════════════════

    private static string ModernCitaRaporu() => $@"{ModernCss()}
<div class=""document"">
    <div class=""header"">
        <div>
            {{{{FirmaLogo}}}}
            <div class=""company"">{{{{FirmaAdi}}}}</div>
            <div class=""company-sub"">{{{{FirmaSlogan}}}}</div>
        </div>
        <div>
            <div class=""doc-title"">ÇITA RAPORU</div>
            <div class=""doc-no"">{{{{SiparisNo}}}}</div>
            <div class=""doc-date"">{{{{SiparisTarihi}}}}</div>
        </div>
    </div>

    <div class=""info-row"">
        <div class=""info-box"">
            <div class=""box-title"">Müşteri Bilgileri</div>
            <p><strong>{{{{MusteriAdi}}}}</strong></p>
            <p>{{{{MusteriAdres}}}}</p>
            <p>{{{{MusteriSehir}}}}</p>
            <p>Tel: {{{{MusteriTelefon}}}} &bull; E-posta: {{{{MusteriEmail}}}}</p>
        </div>
        <div class=""info-box"">
            <div class=""box-title"">Sipariş Bilgileri</div>
            <p>Sipariş No: <strong>{{{{SiparisNo}}}}</strong></p>
            <p>Sipariş Tarihi: <strong>{{{{SiparisTarihi}}}}</strong></p>
            <p>Teslim Tarihi: <strong>{{{{TeslimTarihi}}}}</strong></p>
            <p>Sipariş Müşterisi: <strong>{{{{SiparisMusterisi}}}}</strong></p>
            <p>Durum: <strong>{{{{Durum}}}}</strong></p>
        </div>
    </div>

    <table>
        <thead>
            <tr>
                <th style=""width:35px; text-align:center"">#</th>
                <th>Ürün / Reçete</th>
                <th style=""text-align:right"">En (mm)</th>
                <th style=""text-align:right"">Boy (mm)</th>
                <th style=""text-align:center"">Adet</th>
                <th style=""text-align:right"">Satır m²</th>
                <th>Özellikler</th>
                <th>Not</th>
                <th>Poz No</th>
            </tr>
        </thead>
        <tbody>
            {{{{Kalemler}}}}
        </tbody>
    </table>

    <div class=""totals"">
        <table>
            <tr><td>Toplam Adet:</td><td style=""text-align:right"">{{{{ToplamAdet}}}}</td></tr>
            <tr><td>Toplam Alan:</td><td style=""text-align:right"">{{{{ToplamAlan}}}} m²</td></tr>
        </table>
    </div>

    <div class=""notes""><strong>Notlar:</strong> {{{{Notlar}}}}</div>

    <div class=""signatures"">
        <div class=""sign-block""><div class=""sign-line"">Hazırlayan</div></div>
        <div class=""sign-block""><div class=""sign-line"">Üretim</div></div>
    </div>

    <p class=""footer-note"">Bu belge {{{{Tarih}}}} {{{{Saat}}}} tarihinde {{{{FirmaAdi}}}} sistemi tarafından oluşturulmuştur.</p>
</div>";

    private static string KlasikCitaRaporu() => $@"{KlasikCss()}
<div class=""document"">
    <div class=""header"">
        <div>{{{{FirmaLogo}}}}<div class=""company"">{{{{FirmaAdi}}}}</div><div class=""company-sub"">{{{{FirmaSlogan}}}}</div></div>
        <div><div class=""doc-title"">Çıta Raporu</div><div class=""doc-no"">{{{{SiparisNo}}}}</div><div class=""doc-date"">{{{{SiparisTarihi}}}}</div></div>
    </div>
    <div class=""info-row"">
        <div class=""info-box""><div class=""box-title"">Müşteri</div><p><strong>{{{{MusteriAdi}}}}</strong></p><p>{{{{MusteriAdres}}}}, {{{{MusteriSehir}}}}</p><p>Tel: {{{{MusteriTelefon}}}}</p></div>
        <div class=""info-box""><div class=""box-title"">Sipariş</div><p>No: <strong>{{{{SiparisNo}}}}</strong></p><p>Tarih: <strong>{{{{SiparisTarihi}}}}</strong></p><p>Teslim: <strong>{{{{TeslimTarihi}}}}</strong></p><p>Durum: <strong>{{{{Durum}}}}</strong></p></div>
    </div>
    <table><thead><tr><th style=""width:35px;text-align:center"">#</th><th>Ürün / Reçete</th><th style=""text-align:right"">En (mm)</th><th style=""text-align:right"">Boy (mm)</th><th style=""text-align:center"">Adet</th><th style=""text-align:right"">Satır m²</th><th>Özellikler</th><th>Not</th><th>Poz</th></tr></thead><tbody>{{{{Kalemler}}}}</tbody></table>
    <div class=""totals""><table><tr><td>Toplam Adet:</td><td style=""text-align:right"">{{{{ToplamAdet}}}}</td></tr><tr><td>Toplam Alan:</td><td style=""text-align:right"">{{{{ToplamAlan}}}} m²</td></tr></table></div>
    <div class=""notes""><strong>Notlar:</strong> {{{{Notlar}}}}</div>
    <div class=""signatures""><div class=""sign-block""><div class=""sign-line"">Hazırlayan</div></div><div class=""sign-block""><div class=""sign-line"">Üretim</div></div></div>
    <p class=""footer-note"">{{{{Tarih}}}} {{{{Saat}}}} — {{{{FirmaAdi}}}}</p>
</div>";

    private static string MinimalCitaRaporu() => $@"{MinimalCss()}
<div class=""document"">
    <div class=""header"">
        <div>{{{{FirmaLogo}}}}<div class=""company"">{{{{FirmaAdi}}}}</div></div>
        <div><div class=""doc-title"">Çıta Raporu</div><div class=""doc-no"">{{{{SiparisNo}}}}</div><div class=""doc-date"">{{{{SiparisTarihi}}}}</div></div>
    </div>
    <div class=""info-row"">
        <div class=""info-box""><div class=""box-title"">Müşteri</div><p><strong>{{{{MusteriAdi}}}}</strong></p><p>{{{{MusteriAdres}}}}, {{{{MusteriSehir}}}}</p><p>{{{{MusteriTelefon}}}}</p></div>
        <div class=""info-box""><div class=""box-title"">Sipariş</div><p>No: <strong>{{{{SiparisNo}}}}</strong> | Tarih: <strong>{{{{SiparisTarihi}}}}</strong></p><p>Teslim: <strong>{{{{TeslimTarihi}}}}</strong> | Durum: <strong>{{{{Durum}}}}</strong></p><p>Sipariş Müşterisi: <strong>{{{{SiparisMusterisi}}}}</strong></p></div>
    </div>
    <table><thead><tr><th style=""width:30px;text-align:center"">#</th><th>Ürün / Reçete</th><th style=""text-align:right"">En</th><th style=""text-align:right"">Boy</th><th style=""text-align:center"">Adet</th><th style=""text-align:right"">Satır m²</th><th>Özellikler</th><th>Not</th><th>Poz</th></tr></thead>
    <tbody>{{{{Kalemler}}}}</tbody></table>
    <div class=""totals""><table><tr><td>Toplam Adet:</td><td style=""text-align:right"">{{{{ToplamAdet}}}}</td></tr><tr><td>Toplam Alan:</td><td style=""text-align:right"">{{{{ToplamAlan}}}} m²</td></tr></table></div>
    <div class=""notes""><strong>Notlar:</strong> {{{{Notlar}}}}</div>
    <div class=""signatures""><div class=""sign-block""><div class=""sign-line"">Hazırlayan</div></div><div class=""sign-block""><div class=""sign-line"">Üretim</div></div></div>
    <p class=""footer-note"">{{{{Tarih}}}} · {{{{Saat}}}} · {{{{FirmaAdi}}}}</p>
</div>";

    // ═══════════════════════════════════════════════════════════
    //  FİYATSIZ SİPARİŞ FİŞİ - Sipariş fişi fiyatsız
    // ═══════════════════════════════════════════════════════════

    private static string ModernFiyatsizSiparis() => $@"{ModernCss()}
<div class=""document"">
    <div class=""header"">
        <div>
            {{{{FirmaLogo}}}}
            <div class=""company"">{{{{FirmaAdi}}}}</div>
            <div class=""company-sub"">{{{{FirmaSlogan}}}}</div>
        </div>
        <div>
            <div class=""doc-title"">SİPARİŞ FİŞİ</div>
            <div class=""doc-no"">{{{{SiparisNo}}}}</div>
            <div class=""doc-date"">{{{{SiparisTarihi}}}}</div>
        </div>
    </div>

    <div class=""info-row"">
        <div class=""info-box"">
            <div class=""box-title"">Müşteri Bilgileri</div>
            <p><strong>{{{{MusteriAdi}}}}</strong></p>
            <p>{{{{MusteriAdres}}}}</p>
            <p>{{{{MusteriSehir}}}}</p>
            <p>Tel: {{{{MusteriTelefon}}}} &bull; E-posta: {{{{MusteriEmail}}}}</p>
            <p>Vergi No: {{{{MusteriVergiNo}}}} &bull; V.D.: {{{{MusteriVergiDairesi}}}}</p>
        </div>
        <div class=""info-box"">
            <div class=""box-title"">Sipariş Bilgileri</div>
            <p>Sipariş No: <strong>{{{{SiparisNo}}}}</strong></p>
            <p>Sipariş Tarihi: <strong>{{{{SiparisTarihi}}}}</strong></p>
            <p>Teslim Tarihi: <strong>{{{{TeslimTarihi}}}}</strong></p>
            <p>Sipariş Müşterisi: <strong>{{{{SiparisMusterisi}}}}</strong></p>
            <p>Durum: <strong>{{{{Durum}}}}</strong></p>
        </div>
    </div>

    <table>
        <thead>
            <tr>
                <th style=""width:35px; text-align:center"">#</th>
                <th>Ürün / Reçete</th>
                <th style=""text-align:right"">En (mm)</th>
                <th style=""text-align:right"">Boy (mm)</th>
                <th style=""text-align:center"">Adet</th>
                <th style=""text-align:right"">Satır m²</th>
                <th>Özellikler</th>
                <th>Not</th>
                <th>Poz No</th>
            </tr>
        </thead>
        <tbody>
            {{{{Kalemler}}}}
        </tbody>
    </table>

    <div class=""totals"">
        <table>
            <tr><td>Toplam Adet:</td><td style=""text-align:right"">{{{{ToplamAdet}}}}</td></tr>
            <tr><td>Toplam Alan:</td><td style=""text-align:right"">{{{{ToplamAlan}}}} m²</td></tr>
                    </table>
    </div>

    <div class=""notes""><strong>Notlar:</strong> {{{{Notlar}}}}</div>

    <div class=""signatures"">
        <div class=""sign-block""><div class=""sign-line"">Hazırlayan</div></div>
        <div class=""sign-block""><div class=""sign-line"">Onaylayan</div></div>
        <div class=""sign-block""><div class=""sign-line"">Müşteri</div></div>
    </div>

    <p class=""footer-note"">Bu belge {{{{Tarih}}}} {{{{Saat}}}} tarihinde {{{{FirmaAdi}}}} sistemi tarafından oluşturulmuştur.</p>
</div>";

    private static string KlasikFiyatsizSiparis() => $@"{KlasikCss()}
<div class=""document"">
    <div class=""header"">
        <div>{{{{FirmaLogo}}}}<div class=""company"">{{{{FirmaAdi}}}}</div><div class=""company-sub"">{{{{FirmaSlogan}}}}</div></div>
        <div><div class=""doc-title"">Sipariş Fişi</div><div class=""doc-no"">{{{{SiparisNo}}}}</div><div class=""doc-date"">{{{{SiparisTarihi}}}}</div></div>
    </div>
    <div class=""info-row"">
        <div class=""info-box""><div class=""box-title"">Müşteri Bilgileri</div><p><strong>{{{{MusteriAdi}}}}</strong></p><p>{{{{MusteriAdres}}}}, {{{{MusteriSehir}}}}</p><p>Tel: {{{{MusteriTelefon}}}} | E-posta: {{{{MusteriEmail}}}}</p><p>Vergi No: {{{{MusteriVergiNo}}}} | V.D.: {{{{MusteriVergiDairesi}}}}</p></div>
        <div class=""info-box""><div class=""box-title"">Sipariş Bilgileri</div><p>No: <strong>{{{{SiparisNo}}}}</strong></p><p>Tarih: <strong>{{{{SiparisTarihi}}}}</strong></p><p>Teslim: <strong>{{{{TeslimTarihi}}}}</strong></p><p>Sipariş Müşterisi: <strong>{{{{SiparisMusterisi}}}}</strong></p><p>Durum: <strong>{{{{Durum}}}}</strong></p></div>
    </div>
    <table><thead><tr><th style=""width:35px;text-align:center"">#</th><th>Ürün / Reçete</th><th style=""text-align:right"">En (mm)</th><th style=""text-align:right"">Boy (mm)</th><th style=""text-align:center"">Adet</th><th style=""text-align:right"">Satır m²</th><th>Özellikler</th><th>Not</th><th>Poz</th></tr></thead><tbody>{{{{Kalemler}}}}</tbody></table>
    <div class=""totals""><table><tr><td>Toplam Adet:</td><td style=""text-align:right"">{{{{ToplamAdet}}}}</td></tr><tr><td>Toplam Alan:</td><td style=""text-align:right"">{{{{ToplamAlan}}}} m²</td></tr></table></div>
    <div class=""notes""><strong>Notlar:</strong> {{{{Notlar}}}}</div>
    <div class=""signatures""><div class=""sign-block""><div class=""sign-line"">Hazırlayan</div></div><div class=""sign-block""><div class=""sign-line"">Onaylayan</div></div><div class=""sign-block""><div class=""sign-line"">Müşteri</div></div></div>
    <p class=""footer-note"">{{{{Tarih}}}} {{{{Saat}}}} — {{{{FirmaAdi}}}}</p>
</div>";

    private static string MinimalFiyatsizSiparis() => $@"{MinimalCss()}
<div class=""document"">
    <div class=""header"">
        <div>{{{{FirmaLogo}}}}<div class=""company"">{{{{FirmaAdi}}}}</div></div>
        <div><div class=""doc-title"">Sipariş Fişi</div><div class=""doc-no"">{{{{SiparisNo}}}}</div><div class=""doc-date"">{{{{SiparisTarihi}}}}</div></div>
    </div>
    <div class=""info-row"">
        <div class=""info-box""><div class=""box-title"">Müşteri</div><p><strong>{{{{MusteriAdi}}}}</strong></p><p>{{{{MusteriAdres}}}}, {{{{MusteriSehir}}}}</p><p>{{{{MusteriTelefon}}}} | {{{{MusteriEmail}}}}</p><p>VKN: {{{{MusteriVergiNo}}}} · {{{{MusteriVergiDairesi}}}}</p></div>
        <div class=""info-box""><div class=""box-title"">Sipariş</div><p>No: <strong>{{{{SiparisNo}}}}</strong> | Tarih: <strong>{{{{SiparisTarihi}}}}</strong></p><p>Teslim: <strong>{{{{TeslimTarihi}}}}</strong> | Durum: <strong>{{{{Durum}}}}</strong></p><p>Sipariş Müşterisi: <strong>{{{{SiparisMusterisi}}}}</strong></p></div>
    </div>
    <table><thead><tr><th style=""width:30px;text-align:center"">#</th><th>Ürün / Reçete</th><th style=""text-align:right"">En</th><th style=""text-align:right"">Boy</th><th style=""text-align:center"">Adet</th><th style=""text-align:right"">Satır m²</th><th>Özellikler</th><th>Not</th><th>Poz</th></tr></thead>
    <tbody>{{{{Kalemler}}}}</tbody></table>
    <div class=""totals""><table><tr><td>Toplam Adet:</td><td style=""text-align:right"">{{{{ToplamAdet}}}}</td></tr><tr><td>Toplam Alan:</td><td style=""text-align:right"">{{{{ToplamAlan}}}} m²</td></tr></table></div>
    <div class=""notes""><strong>Notlar:</strong> {{{{Notlar}}}}</div>
    <div class=""signatures""><div class=""sign-block""><div class=""sign-line"">Hazırlayan</div></div><div class=""sign-block""><div class=""sign-line"">Onaylayan</div></div><div class=""sign-block""><div class=""sign-line"">Müşteri</div></div></div>
    <p class=""footer-note"">{{{{Tarih}}}} · {{{{Saat}}}} · {{{{FirmaAdi}}}}</p>
</div>";

    // ═══════════════════════════════════════════════════════════
    //  ETİKET ŞABLONLARI - Küçük boyut (~100x60mm), cam parça etiketi
    // ═══════════════════════════════════════════════════════════

    private static string ModernEtiket() => @"
<style>
    * { margin: 0; padding: 0; box-sizing: border-box; }
    body { font-family: Arial, Helvetica, sans-serif; background: #fff; margin: 0; }
    @page { size: 95mm 65mm; margin: 0; }
    .label {
        width: 95mm; height: 65mm; border: 1.5pt solid #003472;
        display: flex; flex-direction: row; page-break-after: always; background: #fff; overflow: hidden;
    }
    /* ── SOL: BİLGİ ALANI ── */
    .label-main {
        flex: 1; padding: 2mm 2.5mm 1.5mm 3mm;
        display: flex; flex-direction: column; justify-content: space-between;
    }
    .label-main table { width: 100%; border-collapse: collapse; }
    .label-main table tr { border-bottom: 0.5pt solid #e0e0e0; }
    .label-main table tr:last-child { border-bottom: none; }
    .label-main table td { padding: 1.2mm 0; font-size: 7pt; vertical-align: middle; }
    .label-main table td.lbl {
        font-weight: bold; color: #003472; width: 24mm; white-space: nowrap;
        font-size: 6.5pt; text-transform: uppercase; letter-spacing: 0.3px;
    }
    .label-main table td.sep { width: 3mm; text-align: center; color: #003472; font-weight: bold; }
    .label-main table td.val { color: #222; font-size: 7.5pt; }
    .label-main table tr.row-siparis td.val { font-weight: bold; font-size: 8pt; }
    .label-main table tr.row-cam td.val { font-size: 8pt; }
    .label-main table tr.row-ebat td.val { font-size: 12pt; font-weight: bold; color: #000; letter-spacing: 1px; }
    .label-main table tr.row-ebat td { padding: 1.8mm 0; }
    .label-main table tr.row-adet td.val { font-size: 11pt; font-weight: bold; color: #003472; }
    .label-main table tr.row-musteri td.val { font-weight: bold; font-size: 7.5pt; }
    .label-footer {
        display: flex; justify-content: space-between; align-items: flex-end;
        border-top: 0.5pt solid #ccc; padding-top: 1mm; margin-top: 0.5mm;
    }
    .label-footer .note { font-size: 4.5pt; color: #aaa; max-width: 42mm; line-height: 1.3; }
    .label-footer .meta { font-size: 5pt; color: #999; text-align: right; white-space: nowrap; }
    /* ── SAĞ: FİRMA SIDEBAR ── */
    .label-sidebar {
        width: 22mm; background: #003472; color: #fff;
        display: flex; flex-direction: column; align-items: center; justify-content: center;
        padding: 2mm 1.5mm; text-align: center; position: relative;
    }
    .label-sidebar .logo-area { margin-bottom: 3mm; }
    .label-sidebar .logo-area img { max-height: 16mm; max-width: 18mm; filter: brightness(0) invert(1); }
    .label-sidebar .brand {
        writing-mode: vertical-rl; text-orientation: mixed; transform: rotate(180deg);
        font-size: 8pt; font-weight: bold; letter-spacing: 2px; margin-top: 2mm;
    }
    .label-sidebar .sub-brand {
        font-size: 5pt; letter-spacing: 1px; opacity: 0.7; margin-top: 2mm;
    }
    @media print { body { margin: 0; } .label { border: 1.5pt solid #003472; } }
</style>
<div class=""label"">
    <div class=""label-main"">
        <table>
            <tr><td class=""lbl"">ÜRETİCİ FİRMA</td><td class=""sep"">:</td><td class=""val"">{{FirmaAdi}}</td></tr>
            <tr class=""row-siparis""><td class=""lbl"">SİPARİŞ NO</td><td class=""sep"">:</td><td class=""val"">{{SiparisNo}}</td></tr>
            <tr class=""row-cam""><td class=""lbl"">CAM KOMBİNASYONU</td><td class=""sep"">:</td><td class=""val"">{{UrunAdi}}</td></tr>
            <tr class=""row-ebat""><td class=""lbl"">EBAT (mm)</td><td class=""sep"">:</td><td class=""val"">{{Olcu}}</td></tr>
            <tr class=""row-adet""><td class=""lbl"">ADET</td><td class=""sep"">:</td><td class=""val"">{{Adet}}</td></tr>
            <tr><td class=""lbl"">EBAT / POZ NO</td><td class=""sep"">:</td><td class=""val"">{{PozNo}}</td></tr>
            <tr class=""row-musteri""><td class=""lbl"">MÜŞTERİ</td><td class=""sep"">:</td><td class=""val"">{{MusteriAdi}}</td></tr>
        </table>
        <div class=""label-footer"">
            <div class=""note"">Etiketin kolay çıkarılabilmesi için<br>montajdan hemen sonra sökünüz.</div>
            <div class=""meta"">{{Tarih}} {{Saat}}<br>#{{KalemNo}}</div>
        </div>
    </div>
    <div class=""label-sidebar"">
        <div class=""logo-area"">{{FirmaLogo}}</div>
        <div class=""brand"">{{FirmaAdi}}</div>
        <div class=""sub-brand"">TEMPERED GLASS</div>
    </div>
</div>";

    private static string KlasikEtiket() => @"
<style>
    * { margin: 0; padding: 0; box-sizing: border-box; }
    body { font-family: Arial, Helvetica, sans-serif; background: #fff; margin: 0; }
    @page { size: 95mm 65mm; margin: 0; }
    .label {
        width: 95mm; height: 65mm; padding: 2.5mm 3mm; border: 2px solid #003472;
        display: flex; flex-direction: column; justify-content: space-between;
        page-break-after: always; background: #fff;
    }
    .label-header {
        display: flex; justify-content: space-between; align-items: center;
        border-bottom: 2px solid #003472; padding-bottom: 1.5mm; margin-bottom: 1mm;
    }
    .label-header .logo-company { display: flex; align-items: center; gap: 2mm; }
    .label-header .logo-company img { max-height: 10mm; max-width: 20mm; }
    .label-header .company { font-size: 9pt; font-weight: bold; color: #003472; }
    .label-header .sub { font-size: 5pt; color: #003472; letter-spacing: 1px; }
    .label-body { flex: 1; }
    .label-body table { width: 100%; border-collapse: collapse; }
    .label-body table tr { border-bottom: 0.5pt solid #e8e8e8; }
    .label-body table td { padding: 1mm 0.5mm; font-size: 7pt; vertical-align: middle; }
    .label-body table td.lbl { font-weight: bold; width: 28mm; color: #003472; font-size: 6.5pt; text-transform: uppercase; }
    .label-body table td.sep { width: 3mm; text-align: center; color: #003472; font-weight: bold; }
    .label-body table td.val { color: #222; }
    .label-body .row-ebat td.val { font-size: 12pt; font-weight: bold; letter-spacing: 1px; }
    .label-body .row-ebat td { padding: 1.5mm 0.5mm; border-bottom: 1pt solid #003472; }
    .label-body .row-adet td.val { font-size: 11pt; font-weight: bold; color: #003472; }
    .label-body .row-musteri td.val { font-weight: bold; }
    .label-footer {
        display: flex; justify-content: space-between; align-items: flex-end;
        border-top: 1pt solid #003472; padding-top: 1mm; margin-top: 0.5mm;
    }
    .label-footer .note { font-size: 4.5pt; color: #aaa; line-height: 1.3; }
    .label-footer .meta { font-size: 5pt; color: #888; text-align: right; }
    @media print { body { margin: 0; } .label { border: 2px solid #003472; } }
</style>
<div class=""label"">
    <div class=""label-header"">
        <div class=""logo-company"">{{FirmaLogo}} <span class=""company"">{{FirmaAdi}}</span></div>
        <div><span class=""sub"">TEMPERED GLASS</span></div>
    </div>
    <div class=""label-body"">
        <table>
            <tr><td class=""lbl"">SİPARİŞ NO</td><td class=""sep"">:</td><td class=""val"" style=""font-weight:bold;font-size:8pt"">{{SiparisNo}}</td></tr>
            <tr><td class=""lbl"">CAM KOMBİNASYONU</td><td class=""sep"">:</td><td class=""val"">{{UrunAdi}}</td></tr>
            <tr class=""row-ebat""><td class=""lbl"">EBAT (mm)</td><td class=""sep"">:</td><td class=""val"">{{Olcu}}</td></tr>
            <tr class=""row-adet""><td class=""lbl"">ADET</td><td class=""sep"">:</td><td class=""val"">{{Adet}}</td></tr>
            <tr><td class=""lbl"">EBAT / POZ NO</td><td class=""sep"">:</td><td class=""val"">{{PozNo}}</td></tr>
            <tr class=""row-musteri""><td class=""lbl"">MÜŞTERİ</td><td class=""sep"">:</td><td class=""val"">{{MusteriAdi}}</td></tr>
        </table>
    </div>
    <div class=""label-footer"">
        <div class=""note"">Etiketin kolay çıkarılabilmesi için montajdan hemen sonra sökünüz.</div>
        <div class=""meta"">{{Tarih}} {{Saat}} · #{{KalemNo}}</div>
    </div>
</div>";

    private static string MinimalEtiket() => @"
<style>
    * { margin: 0; padding: 0; box-sizing: border-box; }
    body { font-family: Arial, Helvetica, sans-serif; background: #fff; }
    @page { size: 95mm 65mm; margin: 0; }
    .label {
        width: 95mm; height: 65mm; padding: 3mm 4mm;
        border: 1px solid #ccc; display: flex; flex-direction: column;
        justify-content: space-between; page-break-after: always; background: #fff;
    }
    .label-top { display: flex; justify-content: space-between; font-size: 7pt; color: #888; padding-bottom: 1mm; border-bottom: 0.5pt solid #ddd; }
    .label-center { flex: 1; display: flex; flex-direction: column; justify-content: center; text-align: center; }
    .label-center .product { font-size: 9pt; font-weight: 600; color: #333; margin-bottom: 1.5mm; }
    .label-center .size { font-size: 15pt; font-weight: bold; color: #111; letter-spacing: 2px; }
    .label-center .qty { font-size: 11pt; color: #003366; margin-top: 1.5mm; font-weight: bold; }
    .label-center .poz { font-size: 7pt; color: #888; margin-top: 1mm; }
    .label-bottom { display: flex; justify-content: space-between; font-size: 7pt; color: #666; border-top: 0.5pt solid #ddd; padding-top: 1mm; }
    .label-bottom .customer { font-weight: bold; color: #333; }
    @media print { body { margin: 0; } .label { border: none; } }
</style>
<div class=""label"">
    <div class=""label-top"">
        <span>{{FirmaAdi}}</span>
        <span>{{SiparisNo}}</span>
    </div>
    <div class=""label-center"">
        <div class=""product"">{{UrunAdi}}</div>
        <div class=""size"">{{Olcu}}</div>
        <div class=""qty"">{{Adet}} Adet</div>
        <div class=""poz"">Poz: {{PozNo}}</div>
    </div>
    <div class=""label-bottom"">
        <span class=""customer"">{{MusteriAdi}}</span>
        <span>{{Tarih}} · #{{KalemNo}}</span>
    </div>
</div>";

    private static string ModernCariBakiyeListesi() => $@"{ModernCss()}
<div class=""document"">
    <div class=""header"">
        <div>
            {{{{FirmaLogo}}}}
            <div class=""company"">{{{{FirmaAdi}}}}</div>
            <div class=""company-sub"">{{{{FirmaSlogan}}}}</div>
        </div>
        <div>
            <div class=""doc-title"">CARİ BAKİYE LİSTESİ</div>
            <div class=""doc-date"">{{{{Tarih}}}} {{{{Saat}}}}</div>
        </div>
    </div>

    <div class=""summary-boxes"">
        <div class=""summary-box red"">
            <div class=""amount"">{{{{ToplamBorc}}}} TL</div>
            <div class=""label"">Toplam Borç</div>
        </div>
        <div class=""summary-box green"">
            <div class=""amount"">{{{{ToplamAlacak}}}} TL</div>
            <div class=""label"">Toplam Alacak</div>
        </div>
        <div class=""summary-box blue"">
            <div class=""amount"">{{{{CariSayisi}}}}</div>
            <div class=""label"">Aktif Cari</div>
        </div>
    </div>

    <table>
        <thead>
            <tr>
                <th style=""width:35px; text-align:center"">#</th>
                <th>Kod</th>
                <th>Cari Ünvan</th>
                <th style=""text-align:right"">Bakiye (TL)</th>
                <th style=""text-align:center"">Yön</th>
            </tr>
        </thead>
        <tbody>
            {{{{Cariler}}}}
        </tbody>
    </table>

    <p class=""footer-note"">Bu rapor {{{{Tarih}}}} {{{{Saat}}}} tarihinde {{{{FirmaAdi}}}} sistemi tarafından oluşturulmuştur.</p>
</div>";
}
