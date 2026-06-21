# Lisans ve yetki işletim rehberi

## Lisans kurulumu

Lisanslar RSA-3072/SHA-256 ile imzalanır. Private key yalnızca lisans üreticisinin güvenli ortamında tutulur; uygulamaya sadece public key verilir.

```bash
dotnet run --project tools/GlassSoft.LicenseGenerator -- init private.pem public.pem
dotnet run --project tools/GlassSoft.LicenseGenerator -- issue private.pem KURULUM_KIMLIGI "Müşteri Ünvanı" 365 20 "*" GS-2026-001 license.json
```

`public.pem` içeriği production yapılandırmasındaki `License:PublicKeyPem` alanına konur. Kurulum kimliği Yönetim > Lisans Yönetimi ekranından alınır. Üretilen `license.json` aynı ekrandan etkinleştirilir.

Modül listesinde `*` tüm modülleri açar. Sınırlı lisanslarda şu kodlar kullanılabilir: `Customers`, `Sales`, `Inventory`, `Production`, `Purchasing`, `Accounting`, `Reports`, `Administration`.

`License:BypassInDevelopment` sadece ASP.NET Core ortamı gerçekten `Development` olduğunda etkilidir; production ortamında lisans zorunludur.

## Yetki parametreleri

Sunucu tarafı yetkiler `Module.Action` biçimindedir. İşlemler `Read`, `Create`, `Update`, `Delete`, `Export` olarak tanımlıdır. Roller > Düzenle ekranında parametreler rol bazında atanır. `Admin` rolü güvenli kurtarma ve ilk kurulum için tam yetkilidir.

Yeni bir controller eklendiğinde `PermissionAuthorizationFilter.ControllerModules` eşlemesine modülü eklenmelidir. Eşlenen controller'lardaki bilinmeyen POST işlemleri varsayılan olarak `Update` kabul edilir.

## Production secret'ları

Production ortamında repoya parola yazılmaz. Aşağıdaki değerler environment variable veya secret store üzerinden verilmelidir:

- `ConnectionStrings__DefaultConnection`
- `BootstrapAdmin__Password` (yalnızca ilk admin henüz yoksa kullanılır)
- `License__PublicKeyPem`

Geliştirme ortamında ilk admin için geriye dönük yerel varsayılan parola kullanılabilir; production ortamında boş bootstrap parolasıyla yeni admin oluşturulmaz.
