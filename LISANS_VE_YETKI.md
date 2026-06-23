# Lisans ve yetki işletim rehberi

## Lisans kurulumu

Lisanslar RSA-3072/SHA-256 ile imzalanır. Özel anahtar yalnızca lisans üreticisinin güvenli ortamında tutulur; uygulamaya sadece açık anahtar verilir.

```bash
dotnet run --project tools/GlassSoft.LicenseGenerator -- init private.pem public.pem
dotnet run --project tools/GlassSoft.LicenseGenerator -- issue private.pem KURULUM_KIMLIGI "Müşteri Ünvanı" 365 20 "*" GS-2026-001 license.json
```

`public.pem` içeriği canlı ortam yapılandırmasındaki `License:PublicKeyPem` alanına konur. Kurulum kimliği Yönetim > Lisans Yönetimi ekranından alınır. Üretilen `license.json` aynı ekrandan etkinleştirilir.

Modül listesinde `*` tüm modülleri açar. Sınırlı lisanslarda şu teknik modül kodları kullanılır:

- `Customers` → Müşteriler
- `Sales` → Satış
- `Inventory` → Stok ve Ürünler
- `Production` → Üretim
- `Purchasing` → Satın Alma
- `Accounting` → Muhasebe
- `Reports` → Raporlar
- `Administration` → Yönetim

`License:BypassInDevelopment` sadece ASP.NET Core ortamı gerçekten geliştirme ortamı olduğunda etkilidir; canlı ortamda lisans zorunludur.

## Yetki parametreleri

Sunucu tarafı yetkiler teknik olarak `Modül.İşlem` biçiminde tutulur. Ekranda işlemler Türkçe gösterilir:

- `Read` → Görüntüleme
- `Create` → Oluşturma
- `Update` → Güncelleme
- `Delete` → Silme
- `Export` → Dışa Aktarma / Yazdırma

Roller > Düzenle ekranında parametreler rol bazında atanır. `Admin` rolü güvenli kurtarma ve ilk kurulum için tam yetkilidir.

Yeni bir denetleyici eklendiğinde `PermissionAuthorizationFilter.ControllerModules` eşlemesine modülü eklenmelidir. Eşlenen denetleyicilerdeki bilinmeyen POST işlemleri varsayılan olarak güncelleme kabul edilir.

## Canlı ortam gizli değerleri

Canlı ortamda repoya parola yazılmaz. Aşağıdaki değerler ortam değişkeni veya gizli değer deposu üzerinden verilmelidir:

- `ConnectionStrings__DefaultConnection`
- `BootstrapAdmin__Password` (yalnızca ilk admin henüz yoksa kullanılır)
- `License__PublicKeyPem`

Geliştirme ortamında ilk admin için geriye dönük yerel varsayılan parola kullanılabilir; canlı ortamda boş başlangıç parolasıyla yeni admin oluşturulmaz.
