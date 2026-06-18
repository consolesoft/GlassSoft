# GLASSOFT ERP - Kullanma Kılavuzu

**Sürüm:** 1.1.0
**Geliştirici:** ConsoleSoft
**Platform:** Web Tabanlı (ASP.NET Core)
**Tarih:** Nisan 2026

---

## İçindekiler

1. [Giriş](#1-giriş)
2. [Sisteme Giriş](#2-sisteme-giriş)
3. [Dashboard (Ana Sayfa)](#3-dashboard-ana-sayfa)
4. [Satış Modülü](#4-satış-modülü)
   - 4.1 [Müşteri Yönetimi](#41-müşteri-yönetimi)
   - 4.2 [Sipariş Yönetimi](#42-sipariş-yönetimi)
5. [Ürün & Stok Modülü](#5-ürün--stok-modülü)
   - 5.1 [Ürün Grupları](#51-ürün-grupları)
   - 5.2 [Ürünler](#52-ürünler)
   - 5.3 [Plaka Tanımları](#53-plaka-tanımları)
   - 5.4 [Stok Yönetimi](#54-stok-yönetimi)
6. [Reçete (BOM) Modülü](#6-reçete-bom-modülü)
7. [Üretim & Kesim Modülü](#7-üretim--kesim-modülü)
   - 7.1 [İş Emri Oluşturma](#71-iş-emri-oluşturma)
   - 7.2 [Kesim Optimizasyonu](#72-kesim-optimizasyonu)
   - 7.3 [İş Emri Tamamlama & Stok Düşme](#73-iş-emri-tamamlama--stok-düşme)
8. [Satın Alma Modülü](#8-satın-alma-modülü)
   - 8.1 [Tedarikçiler](#81-tedarikçiler)
   - 8.2 [Satın Alma Siparişleri](#82-satın-alma-siparişleri)
9. [Ön Muhasebe Modülü](#9-ön-muhasebe-modülü)
   - 9.1 [Cari Hareketler](#91-cari-hareketler)
   - 9.2 [Kasa Yönetimi](#92-kasa-yönetimi)
   - 9.3 [Gider Yönetimi](#93-gider-yönetimi)
   - 9.4 [Çek/Senet Takibi](#94-çeksenet-takibi)
   - 9.5 [Döviz Kurları](#95-döviz-kurları)
10. [Yazdırma & Çıktı Modülü](#10-yazdırma--çıktı-modülü)
    - 10.1 [Yazdırma Şablonları](#101-yazdırma-şablonları)
    - 10.2 [Çıktı Alma](#102-çıktı-alma)
11. [Yönetim Modülü](#11-yönetim-modülü)
    - 11.1 [Kullanıcı Yönetimi](#111-kullanıcı-yönetimi)
    - 11.2 [Rol Yönetimi](#112-rol-yönetimi)
12. [İş Akışları](#12-iş-akışları)
13. [Sık Sorulan Sorular](#13-sık-sorulan-sorular)

---

## 1. Giriş

GLASSOFT, cam sektörüne özel geliştirilmiş bir ERP (Kurumsal Kaynak Planlama) yazılımıdır. Sipariş girişinden cam kesim optimizasyonuna, stok takibinden ön muhasebeye kadar tüm iş süreçlerinizi tek bir platformda yönetmenizi sağlar.

### Temel Özellikler

- **Sipariş Yönetimi**: Müşteri siparişleri oluşturma, durum takibi, Excel'den toplu kalem aktarma, anlık toplam hesaplama
- **Reçete (BOM) Sistemi**: Isıcam gibi çok katmanlı ürün reçeteleri tanımlama
- **Kesim Optimizasyonu**: Guillotine algoritması ile cam plakadan minimum fire ile parça kesimi
- **Stok Takibi**: m² ve plaka bazlı stok giriş/çıkış
- **Satın Alma**: Tedarikçi yönetimi ve satın alma siparişleri
- **Ön Muhasebe**: Cari hesap, çek/senet takibi, döviz kuru, kasa yönetimi, gider takibi
- **Yazdırma & Çıktı**: Sipariş fişi, teslimat irsaliyesi, üretim emri, cari rapor ve ekstre şablonları
- **Kullanıcı Yönetimi**: Rol bazlı yetkilendirme

---

## 2. Sisteme Giriş

Tarayıcınızdan sisteme eriştiğinizde giriş ekranı karşınıza gelir.

1. **E-posta** alanına kayıtlı e-posta adresinizi girin
2. **Şifre** alanına şifrenizi girin
3. **Beni Hatırla** seçeneğini işaretlerseniz oturumunuz açık kalır
4. **Giriş** butonuna tıklayın

> **Varsayılan Yönetici Hesabı:**
> E-posta: `admin@glassoft.com`
> Şifre: `Admin123!`

Giriş başarılı olduğunda Dashboard (Ana Sayfa) açılır.

---

## 3. Dashboard (Ana Sayfa)

Dashboard, işletmenizin anlık durumunu özetleyen bir gösterge panelidir.

### KPI Kartları

| Kart | Açıklama |
|------|----------|
| **Aktif Siparişler** | Onaylanmış ve üretimde olan sipariş sayısı |
| **Üretimde** | Tamamlanmamış iş emri sayısı |
| **Müşteriler** | Toplam müşteri sayısı |
| **GLASSOFT ERP** | Sistem sürüm bilgisi |

### Son Siparişler

Dashboard'ın alt bölümünde en son oluşturulan 8 sipariş listelenir. Sipariş numarasına tıklayarak detayına gidebilirsiniz.

### Hızlı Erişim

Sağ üst köşedeki **+ Yeni Sipariş** butonu ile doğrudan yeni sipariş oluşturabilirsiniz.

---

## 4. Satış Modülü

Sol menüdeki **Satış** başlığı altında Müşteriler ve Siparişler bölümleri bulunur.

### 4.1 Müşteri Yönetimi

**Menü:** Satış → Müşteriler

#### Müşteri Listesi

Tüm müşterileriniz tablo halinde listelenir. Tablo üzerinden:
- **Arama**: Arama kutusuna yazarak filtreleyebilirsiniz
- **Sıralama**: Kolon başlıklarına tıklayarak sıralayabilirsiniz
- **Sayfalama**: Çok sayıda kayıt için sayfa geçişi yapabilirsiniz

#### Yeni Müşteri Ekleme

1. Sağ üstteki **+ Yeni Müşteri** butonuna tıklayın
2. Formu doldurun:

| Alan | Açıklama | Zorunlu |
|------|----------|---------|
| **Unvan** | Firma veya kişi adı | Evet |
| **Vergi No** | Vergi kimlik numarası | Hayır |
| **Vergi Dairesi** | Vergi dairesi adı | Hayır |
| **Telefon** | İletişim telefonu | Hayır |
| **E-posta** | E-posta adresi | Hayır |
| **Adres** | Açık adres | Hayır |
| **Şehir** | İl | Hayır |
| **Para Birimi** | Varsayılan para birimi (TRY/USD/EUR) | Evet |
| **Aktif** | Müşteri aktif mi? | Evet |

3. **Kaydet** butonuna tıklayın

#### Müşteri Bakiyesi

Müşteri listesinde her müşterinin **Bakiye (TRY)** kolonu görünür:
- **Yeşil**: Müşterinin alacağı var (lehimize)
- **Kırmızı**: Müşterinin borcu var

#### Müşteri Detay Sayfası

Müşteri satırındaki **Detay** butonuna tıklayarak müşteri detay sayfasına erişebilirsiniz:
- **Müşteri bilgileri**: Unvan, iletişim, vergi bilgileri
- **Cari bakiye kartı**: Güncel borç/alacak durumu (renkli gösterim)
- **Son hareketler**: Son 20 cari hareket tablosu
- **Yazdırma**: Cari hareket raporu ve hesap ekstresi yazdırma butonları
- **Hızlı işlemler**: Yeni hareket ekleme, müşteriyi düzenleme linkleri

#### Müşteri Düzenleme / Silme

- **Düzenle**: İlgili satırdaki kalem ikonuna tıklayın
- **Sil**: İlgili satırdaki çöp kutusu ikonuna tıklayın (onay sorulur)

> **Not:** Silme işlemi kalıcı değildir. Müşteri pasif yapılarak soft-delete uygulanır.

---

### 4.2 Sipariş Yönetimi

**Menü:** Satış → Siparişler

#### Sipariş Listesi

Tüm siparişler tablo halinde gösterilir:
- **Sipariş No**: Otomatik oluşturulur (Format: `SIP-yyyyAA-001`)
- **Müşteri**: Siparişin ait olduğu müşteri
- **Tarih**: Sipariş tarihi
- **Teslim**: Teslim tarihi
- **Kalem**: Sipariş kalem sayısı
- **Tutar**: Toplam tutar ve para birimi
- **Durum**: Mevcut sipariş durumu
- **İşlemler**: Detay, düzenle, sil butonları

#### Sipariş Durumları

| Durum | Renk | Açıklama |
|-------|------|----------|
| **Taslak** | Gri | Yeni oluşturulmuş, henüz onaylanmamış |
| **Onaylandı** | Mavi | Onaylanmış, üretime hazır |
| **Üretimde** | Sarı | İş emri oluşturulmuş, üretim devam ediyor |
| **Tamamlandı** | Yeşil | Üretim ve teslimat tamamlanmış |
| **İptal** | Kırmızı | İptal edilmiş sipariş |

#### Yeni Sipariş Oluşturma

1. **+ Yeni Sipariş** butonuna tıklayın
2. Üst bölümü doldurun:

| Alan | Açıklama |
|------|----------|
| **Müşteri** | Açılır listeden müşteri seçin (arama destekli) |
| **Sipariş Tarihi** | Otomatik bugünün tarihi gelir |
| **Teslim Tarihi** | Teslim edilmesi gereken tarih |
| **Para Birimi** | TRY, USD veya EUR |
| **Notlar** | Sipariş hakkında ek notlar |

3. **Sipariş kalemleri** ekleyin:
   - **Kalem Ekle** butonuna tıklayın
   - **Ürün/Reçete** seçin (Ürünler ve Reçeteler gruplu olarak listelenir, arama destekli)
   - Ürün/reçete seçtiğinizde **birim fiyat otomatik** dolar
   - **En (mm)** ve **Boy (mm)** ölçülerini milimetre cinsinden girin
   - **Adet** girin
   - **Birim Fiyat** kontrol edin (otomatik gelen fiyatı değiştirebilirsiniz)
   - **Toplam** kolonu anlık olarak hesaplanır (m² × Adet × Birim Fiyat)
   - Sağ paneldeki **Genel Toplam** kartı tüm kalemlerin toplamını gösterir
   - İstediğiniz kadar kalem ekleyebilirsiniz
   - Bir kalemi silmek için satırın sonundaki kırmızı **X** butonuna tıklayın

4. **Kaydet** butonuna tıklayın

> **Canlı Hesaplama:**
> Her alanı değiştirdiğinizde satır toplamı ve genel toplam anlık güncellenir.
> Toplam = Birim Fiyat × m² × Adet
> m² = (En mm × Boy mm) / 1.000.000

#### Excel'den Toplu Kalem Aktarma

Çok sayıda kalemi tek tek girmek yerine Excel'den toplu aktarabilirsiniz:

1. **Şablon** butonuna tıklayarak Excel şablonunu indirin
2. Şablonu açın ve kalemleri doldurun:
   - **Ürün/Reçete Kodu**: Ürün veya reçete kodu
   - **En (mm)**: Milimetre cinsinden genişlik
   - **Boy (mm)**: Milimetre cinsinden yükseklik
   - **Adet**: Miktar
   - **Birim Fiyat**: m² başına birim fiyat
3. Dosyayı kaydedin
4. Sipariş formunda **Excel Aktar** butonuna tıklayın
5. Açılan pencerede dosyanızı seçin
6. Sistem dosyayı okuyup eşleştirme sonuçlarını gösterir:
   - Eşleşen kalemler yeşil ile gösterilir
   - Eşleşmeyen kalemler (yanlış kod vs.) sarı uyarı ile gösterilir
7. **Aktar** butonuna tıklayarak eşleşen kalemleri forma aktarın
8. Gerekirse düzenleyip **Kaydet**'e tıklayın

#### Sipariş Detayı

Sipariş detay sayfasında:
- Sipariş bilgileri (müşteri, tarihler, para birimi)
- Sipariş kalemleri tablosu (ürün, ölçüler, m², adet, birim fiyat, toplam)
- Genel toplam
- Durum değiştirme butonları

#### Durum Değiştirme

Detay sayfasındaki butonlarla siparişin durumunu değiştirebilirsiniz:

| Mevcut Durum | Kullanılabilir Aksiyonlar |
|-------------|--------------------------|
| Taslak | Onayla, İptal Et |
| Onaylandı | Üretime Al, İptal Et |
| Üretimde | Tamamla, İptal Et |

> **Otomatik Cari Hareket:** Sipariş onaylandığında müşterinin cari hesabına otomatik **borç** kaydı oluşturulur (sipariş tutarı kadar).

> **Önemli:** Durum değişiklikleri geri alınamaz. İptal edilen sipariş tekrar aktif yapılamaz.

---

## 5. Ürün & Stok Modülü

Sol menüdeki **Ürün & Stok** başlığı altında 4 bölüm bulunur.

### 5.1 Ürün Grupları

**Menü:** Ürün & Stok → Ürün Grupları

Ürünleri kategorize etmek için kullanılır.

**Örnek Gruplar:**
- Düz Cam
- Isıcam
- Çıta / Ara Parça
- Sarf Malzeme

Her grupta:
- **Ad**: Grup adı
- **Açıklama**: Grup açıklaması
- **Aktif**: Aktif/Pasif durumu

### 5.2 Ürünler

**Menü:** Ürün & Stok → Ürünler

Sisteme tanımlı tüm ürünler (cam, çıta, sarf malzeme vb.) burada yönetilir.

| Alan | Açıklama |
|------|----------|
| **Ad** | Ürün adı (örn: "4mm Düz Cam") |
| **Kod** | Ürün kodu |
| **Ürün Grubu** | Ait olduğu grup |
| **Birim** | Ölçü birimi (m², adet, metre, kg, litre) |
| **Kalınlık (mm)** | Cam kalınlığı (cam ürünleri için) |
| **Plaka Ürünü mü?** | Bu ürün plaka olarak mı alınıyor? |
| **Birim Fiyat** | Varsayılan birim fiyat |
| **Aktif** | Aktif/Pasif durumu |

> **Plaka Ürünü:** İşaretlenmiş ürünler kesim optimizasyonunda plaka olarak kullanılabilir.

### 5.3 Plaka Tanımları

**Menü:** Ürün & Stok → Plaka Tanımları

Standart cam plaka boyutlarını tanımladığınız bölümdür. Kesim optimizasyonu bu boyutları kullanır.

| Alan | Açıklama |
|------|----------|
| **Ad** | Plaka adı (örn: "3210x2250 Standart") |
| **En (mm)** | Plaka genişliği (mm) |
| **Boy (mm)** | Plaka yüksekliği (mm) |
| **Ürün** | İlişkili cam ürünü |
| **Varsayılan** | Kesim optimizasyonunda varsayılan plaka mı? |

**Yaygın Plaka Boyutları:**
- 3210 × 2250 mm (Standart)
- 3210 × 6000 mm (Jumbo)
- 2550 × 1605 mm (Küçük)

### 5.4 Stok Yönetimi

**Menü:** Ürün & Stok → Stok

#### Stok Özeti

Ana sayfada ürün bazlı güncel stok miktarları görüntülenir:
- Ürün adı
- Birim
- Mevcut miktar
- m² karşılığı (cam ürünleri için)

#### Stok Hareketleri

Her ürünün giriş/çıkış geçmişini görmek için **Hareketler** butonuna tıklayın:
- Tarih
- Hareket tipi (Giriş / Çıkış)
- Miktar
- Açıklama

#### Manuel Stok Girişi

1. **Stok Girişi** butonuna tıklayın
2. Ürün seçin
3. Miktar girin
4. Açıklama yazın (isteğe bağlı)
5. **Kaydet**'e tıklayın

> **Not:** İş emri tamamlandığında stok çıkışı otomatik olarak yapılır. Manuel giriş genelde mal kabul için kullanılır.

---

## 6. Reçete (BOM) Modülü

**Menü:** Reçeteler

Isıcam gibi çok katmanlı ürünlerin bileşen tanımlarını bu modülde yaparsınız.

### Reçete Nedir?

Bir reçete, bir ürünün hangi malzemelerden oluştuğunu tarif eder. Örneğin "4+12+4 Isıcam" reçetesi:

| Katman | Sıra | Malzeme | Kalınlık |
|--------|------|---------|----------|
| Cam | 1 | 4mm Düz Cam | 4 mm |
| Ara Parça | 2 | 12mm Çıta | 12 mm |
| Cam | 3 | 4mm Düz Cam | 4 mm |

### Yeni Reçete Oluşturma

1. **+ Yeni Reçete** butonuna tıklayın
2. Reçete bilgilerini girin:

| Alan | Açıklama |
|------|----------|
| **Kod** | Kısa kod (örn: "4124") |
| **Ad** | Tam ad (örn: "4+12+4 Isıcam") |
| **Açıklama** | Detaylı açıklama |

3. **Katmanlar** ekleyin:
   - **Katman Ekle** butonuna tıklayın
   - Katman tipi seçin: **Cam** veya **Ara Parça**
   - İlgili ürünü seçin (cam türü veya çıta türü)
   - Sıra numarası otomatik verilir
   - İstediğiniz kadar katman ekleyin

4. **Sarf Malzemeler** ekleyin (isteğe bağlı):
   - Butyl, Polisülfür, Moleküler elek gibi sarf malzemeleri
   - Her biri için miktar ve birim girin

5. **Kaydet**'e tıklayın

### Reçete Detayı

Detay sayfasında reçetenin görsel katman temsili gösterilir - cam ve çıta katmanları renkli bloklar halinde sergilenir.

> **Önemli:** Sipariş kalemlerinde reçete seçtiğinizde, iş emri oluşturulurken sistem otomatik olarak reçeteyi "patlatır" ve gerekli malzeme listesini çıkarır.

---

## 7. Üretim & Kesim Modülü

**Menü:** Üretim & Kesim → İş Emirleri

Bu modül GLASSOFT'un en kritik özelliğidir. Onaylanmış siparişlerdeki cam parçalarını en verimli şekilde plakalardan kesmeyi optimize eder.

### 7.1 İş Emri Oluşturma

1. **+ Yeni İş Emri** butonuna tıklayın
2. Sistem, onaylanmış siparişlerdeki **henüz optimize edilmemiş** kalemleri listeler

#### Kalem Seçimi

- Her kalem bir satırda gösterilir: Sipariş No, Müşteri, Ürün/Reçete, Ölçüler, Adet, m²
- Checkbox ile istediğiniz kalemleri seçin
- **Farklı siparişlerden** kalemleri aynı iş emrine dahil edebilirsiniz

#### Malzeme Filtresi

İlk kalemi seçtiğinizde sistem otomatik olarak:
- Seçilen kalemle **aynı malzemeye** sahip diğer kalemleri vurgular
- Farklı malzeme kalemlerini soluklaştırır
- Böylece aynı cam türünden olan parçaları kolayca toplu seçebilirsiniz

#### İş Emri Bilgileri

- **Planlanan Tarih**: Üretim için planlanan tarih
- **Notlar**: Ek notlar

3. **İş Emri Oluştur** butonuna tıklayın

> **İş Emri Numarası:** Otomatik oluşturulur. Format: `EM-yyyyAA-001`

Sistem otomatik olarak:
- Reçeteleri patlatarak malzeme listesi çıkarır
- Her cam tipi için ayrı iş emri satırları oluşturur
- İlgili sipariş kalemlerini "optimize edildi" olarak işaretler

### 7.2 Kesim Optimizasyonu

İş emri detay sayfasında **Kesim Optimizasyonu Çalıştır** butonuna tıklayın.

> Optimizasyon çalışırken ekranda **"Kesim optimizasyonu çalıştırılıyor... Lütfen bekleyiniz"** mesajı görünür. Parça sayısına göre birkaç saniye sürebilir.

#### Aynı Boyut Birleştirme

Aynı ölçülerdeki (En × Boy) parçalar otomatik birleştirilir. Örneğin 5 adet 500×300mm parça tek bir kesim grubu olarak işlenir, böylece optimizasyon daha verimli çalışır.

#### Algoritma

Sistem **Guillotine Bin Packing** algoritmasını kullanır:
- Cam kesim makinaları tek düz çizgi ile plakayı ikiye böler (guillotine kesim)
- Parçalar büyükten küçüğe sıralanır
- Her parça en uygun boş alana yerleştirilir
- Gerekirse parçalar 90° döndürülür
- Bir plaka dolduğunda yeni plaka açılır

#### Kesim Planı Görüntüleme

Optimizasyon tamamlandığında her plaka için:
- **SVG görsel**: Plaka üzerinde parçaların renkli yerleşimi
- **Fire oranı (%)**: Kullanılmayan alan yüzdesi
- Her parçanın üzerinde sipariş no, ölçüler ve adet bilgisi

#### Çıktı İndirme

| Buton | Format | Açıklama |
|-------|--------|----------|
| **CSV İndir** | .csv | Makine tarafından okunabilir kesim koordinatları |
| **DXF İndir** | .dxf | CNC/CAD uyumlu çizim formatı |

> **CSV Formatı:** Her satır bir kesim parçası - PlateNo, X, Y, Width, Height, Rotation
> **DXF Formatı:** AutoCAD ve çoğu CNC kesim makinası ile uyumlu

#### Yeniden Optimizasyon

Kesim planından memnun değilseniz **Yeniden Optimize Et** butonuyla tekrar çalıştırabilirsiniz.

### 7.3 İş Emri Tamamlama & Stok Düşme

Kesim tamamlandığında:

1. İş emri detay sayfasında **Tamamla & Stok Düş** butonuna tıklayın
2. Sistem otomatik olarak:
   - Her kullanılan plaka/malzeme için stok çıkışı oluşturur
   - İş emrini "Tamamlandı" olarak işaretler
   - İlgili sipariş kalemlerini günceller

> **Dikkat:** Bu işlem geri alınamaz. Stok miktarları otomatik düşürülür.

---

## 8. Satın Alma Modülü

Sol menüdeki **Satın Alma** başlığı altında Tedarikçiler ve Satın Alma Siparişleri bulunur.

### 8.1 Tedarikçiler

**Menü:** Satın Alma → Tedarikçiler

Müşteri yönetimiyle aynı yapıdadır: Unvan, vergi bilgileri, iletişim, adres.

### 8.2 Satın Alma Siparişleri

**Menü:** Satın Alma → Satın Alma Siparişleri

#### Yeni Satın Alma Siparişi

1. **+ Yeni Sipariş** butonuna tıklayın
2. Tedarikçi, tarih, teslim tarihi ve para birimi seçin
3. Sipariş kalemleri ekleyin (ürün, miktar, birim fiyat)
4. **Kaydet**'e tıklayın

#### Satın Alma Durumları

| Durum | Açıklama |
|-------|----------|
| **Taslak** | Oluşturulmuş, henüz onaylanmamış |
| **Onaylandı** | Tedarikçiye gönderilmiş |
| **Kısmi Teslim** | Bazı kalemler teslim alınmış |
| **Tamamlandı** | Tüm kalemler teslim alınmış |
| **İptal** | İptal edilmiş sipariş |

---

## 9. Ön Muhasebe Modülü

Sol menüdeki **Ön Muhasebe** başlığı altında 5 bölüm bulunur.

### 9.1 Cari Hareketler

**Menü:** Ön Muhasebe → Cari Hareketler

Müşterilerinizle olan borç/alacak ilişkilerini takip edin.

#### Hareket Listesi

Tüm cari hareketler tarih sırasıyla listelenir. Belirli bir müşterinin hareketlerini görmek için Müşteri Detay sayfasından **Cari Hareketler** linkine tıklayın. Müşteri bazlı görünümde üstte güncel **bakiye** bilgisi gösterilir.

#### Yeni Hareket Ekleme

1. **+ Yeni Hareket** butonuna tıklayın
2. Formu doldurun:

| Alan | Açıklama |
|------|----------|
| **Müşteri** | İlgili müşteri (arama destekli) |
| **Hareket Tipi** | Borç veya Alacak |
| **Tutar** | İşlem tutarı |
| **Para Birimi** | TRY, USD veya EUR |
| **Döviz Kuru** | Döviz ise çevrim kuru |
| **Ödeme Tipi** | Nakit, Havale, Çek, Senet, Kredi Kartı |
| **Tarih** | İşlem tarihi |
| **Açıklama** | İşlem açıklaması |
| **Kasa** | Tahsilat (Alacak) ise kasa seçimi (isteğe bağlı) |

3. **Kaydet**'e tıklayın

> **Borç:** Müşterinin size olan borcu (sipariş onaylandığında otomatik oluşur)
> **Alacak:** Müşteriden tahsil ettiğiniz tutar

> **Kasa Entegrasyonu:** Tahsilat (Alacak) tipinde hareket eklerken bir kasa seçerseniz, otomatik olarak kasaya **giriş** hareketi oluşturulur. Böylece tahsilat hem cari hesaba hem kasaya yansır.

### 9.2 Kasa Yönetimi

**Menü:** Ön Muhasebe → Kasa

İşletmenizin nakit akışını kasalar üzerinden takip edin.

#### Kasa Tanımlama

Birden fazla kasa tanımlayabilirsiniz (örn: Ana Kasa, POS Kasası, Banka).

| Alan | Açıklama |
|------|----------|
| **Ad** | Kasa adı |
| **Açıklama** | Kasa açıklaması |
| **Aktif** | Aktif/Pasif durumu |

#### Kasa Hareketleri

Her kasanın giriş/çıkış hareketleri takip edilir:
- **Giriş**: Kasaya para girişi (tahsilat, satış vb.)
- **Çıkış**: Kasadan para çıkışı (gider, ödeme vb.)
- **Bakiye**: Anlık kasa bakiyesi otomatik hesaplanır

Kasa hareketleri otomatik olarak oluşabilir:
- Tahsilat yapıldığında (Cari Hareket → Alacak + Kasa seçimi)
- Gider kaydedildiğinde (Gider + Kasa seçimi)

### 9.3 Gider Yönetimi

**Menü:** Ön Muhasebe → Giderler

İşletme giderlerini kaydedin ve takip edin.

#### Yeni Gider Ekleme

1. **+ Yeni Gider** butonuna tıklayın
2. Formu doldurun:

| Alan | Açıklama |
|------|----------|
| **Kategori** | Gider kategorisi |
| **Tutar** | Gider tutarı |
| **Tarih** | Gider tarihi |
| **Açıklama** | Gider açıklaması |
| **Kasa** | Hangi kasadan çıkış yapılacak (isteğe bağlı) |

3. **Kaydet**'e tıklayın

> **Kasa Entegrasyonu:** Gider eklerken kasa seçerseniz, otomatik olarak kasadan **çıkış** hareketi oluşturulur. Gider silindiğinde ilgili kasa hareketi de silinir.

### 9.4 Çek/Senet Takibi

**Menü:** Ön Muhasebe → Çek/Senet

Müşterilerden aldığınız çek ve senetleri takip edin.

#### Yeni Çek/Senet Ekleme

| Alan | Açıklama |
|------|----------|
| **Tip** | Çek veya Senet |
| **Müşteri** | İlgili müşteri |
| **Seri No** | Çek/Senet seri numarası |
| **Tutar** | Nominal tutar |
| **Para Birimi** | Para birimi |
| **Düzenleme Tarihi** | Düzenlendiği tarih |
| **Vade Tarihi** | Tahsil edilecek tarih |
| **Banka** | Banka adı |
| **Şube** | Şube adı |

#### Çek/Senet Durumları

| Durum | Açıklama |
|-------|----------|
| **Portföyde** | Elinizde bekliyor |
| **Tahsilde** | Bankaya tahsile verildi |
| **Tahsil Edildi** | Başarıyla tahsil edildi |
| **Karşılıksız** | Karşılıksız çıktı |
| **İade Edildi** | Müşteriye iade edildi |
| **Ciro Edildi** | Başka birine ciro edildi |

Detay sayfasından durum butonlarıyla geçiş yapabilirsiniz.

### 9.5 Döviz Kurları

**Menü:** Ön Muhasebe → Döviz Kurları

Günlük döviz kurlarını kaydedin. Sistem çoklu para birimi (TRY, USD, EUR) destekler.

---

## 10. Yazdırma & Çıktı Modülü

GLASSOFT, özelleştirilebilir HTML şablonları ile profesyonel çıktılar üretmenizi sağlar.

### 10.1 Yazdırma Şablonları

**Menü:** Yönetim → Yazdır Şablonları

#### Şablon Türleri

| Tür | Açıklama |
|-----|----------|
| **Sipariş Fişi** | Sipariş detay çıktısı (müşteri, kalemler, toplamlar) |
| **Teslimat İrsaliyesi** | Teslimat belgesi |
| **Üretim Emri** | İş emri çıktısı (malzeme listesi, ölçüler) |
| **Cari Hareketleri** | Müşteri cari hareket raporu (tarih aralıklı) |
| **Hesap Ekstresi** | Müşteri hesap ekstresi (tarih aralıklı) |

#### Şablon Yönetimi

- **Yeni Şablon**: Her tür için birden fazla şablon oluşturabilirsiniz
- **Varsayılan Şablon**: Her tür için bir şablon varsayılan olarak işaretlenebilir
- **Kopyala**: Mevcut bir şablonu kopyalayarak üzerinde değişiklik yapabilirsiniz
- **HTML Düzenleme**: Şablonlar HTML formatında düzenlenir

#### Placeholder Sistemi

Şablonlar `{{Placeholder}}` sözdizimi ile dinamik veri yerleştirmeyi destekler. Şablon düzenleme sayfasında her çıktı türü için kullanılabilir placeholder'lar listelenir.

**Örnek Placeholder'lar:**
- `{{SiparisNo}}`, `{{SiparisTarihi}}`, `{{MusteriAdi}}`
- `{{Kalemler}}` → Sipariş kalemleri tablosu
- `{{ToplamTutar}}`, `{{ToplamAdet}}`, `{{ToplamAlan}}`
- `{{Hareketler}}` → Cari hareket satırları
- `{{Bakiye}}`, `{{ToplamBorc}}`, `{{ToplamAlacak}}`

### 10.2 Çıktı Alma

Çıktı almak istediğiniz sayfada (Sipariş Detay, İş Emri Detay, Müşteri Detay) **Yazdır** butonuna tıklayın:

1. Açılan pencereden **şablon seçin**
2. Cari hareket/ekstre çıktıları için **tarih aralığı** belirleyin
3. **Önizle** butonuyla yeni sekmede çıktıyı görüntüleyin
4. **Yazdır** butonuyla doğrudan yazdırma diyaloğunu açın

> **Çıktı Noktaları:**
> - Sipariş detayında: Sipariş Fişi, Teslimat İrsaliyesi
> - İş emri detayında: Üretim Emri
> - Müşteri detayında: Cari Hareket Raporu, Hesap Ekstresi

---

## 11. Yönetim Modülü

Sol menüdeki **Yönetim** başlığı altında Kullanıcılar ve Roller bulunur.

### 11.1 Kullanıcı Yönetimi

**Menü:** Yönetim → Kullanıcılar

#### Yeni Kullanıcı Ekleme

| Alan | Açıklama |
|------|----------|
| **Ad** | Kullanıcının adı |
| **Soyad** | Kullanıcının soyadı |
| **E-posta** | Giriş için kullanılacak e-posta |
| **Şifre** | Giriş şifresi (min. 6 karakter, büyük/küçük harf, rakam, özel karakter) |
| **Rol** | Atanacak rol |

#### Kullanıcı Pasif Yapma

Bir kullanıcıyı silmek yerine **pasif** yapabilirsiniz. Pasif kullanıcılar sisteme giriş yapamaz ama kayıtları korunur.

### 11.2 Rol Yönetimi

**Menü:** Yönetim → Roller

Sistemde önceden tanımlı roller:
- **Admin**: Tam yetki
- **Satış**: Sipariş ve müşteri yönetimi
- **Üretim**: İş emri ve kesim işlemleri
- **Muhasebe**: Cari ve finansal işlemler
- **Depo**: Stok ve satın alma işlemleri

Yeni roller oluşturabilir veya mevcut rolleri düzenleyebilirsiniz.

---

## 12. İş Akışları

### Ana İş Akışı: Siparişten Teslimata

```
1. MÜŞTERİ KAYDI
   └─ Satış → Müşteriler → Yeni Müşteri

2. SİPARİŞ OLUŞTURMA
   └─ Satış → Siparişler → Yeni Sipariş
   └─ Kalemleri gir (tek tek veya Excel ile toplu)
   └─ Kaydet → Durum: TASLAK

3. SİPARİŞ ONAYLAMA
   └─ Sipariş Detay → "Onayla" butonu
   └─ Durum: ONAYLANDI

4. İŞ EMRİ OLUŞTURMA
   └─ Üretim & Kesim → İş Emirleri → Yeni İş Emri
   └─ Onaylı siparişlerden kalemleri seç
   └─ Aynı malzeme filtresini kullan
   └─ İş Emri Oluştur

5. KESİM OPTİMİZASYONU
   └─ İş Emri Detay → "Kesim Optimizasyonu Çalıştır"
   └─ Görsel kesim planını incele
   └─ CSV veya DXF indir → Makineye gönder

6. ÜRETİM TAMAMLAMA
   └─ İş Emri Detay → "Tamamla & Stok Düş"
   └─ Stok otomatik düşer

7. SİPARİŞ TAMAMLAMA
   └─ Sipariş Detay → "Tamamla" butonu
   └─ Durum: TAMAMLANDI
```

### Satın Alma Akışı

```
1. TEDARİKÇİ KAYDI
   └─ Satın Alma → Tedarikçiler → Yeni

2. SATIN ALMA SİPARİŞİ
   └─ Satın Alma → Siparişler → Yeni
   └─ Kalemleri gir → Kaydet
   └─ Durum: TASLAK → ONAYLANDI

3. MAL KABUL
   └─ Ürün & Stok → Stok → Stok Girişi
   └─ Satın Alma → Sipariş → Durum güncelle
```

### Tahsilat Akışı

```
1. SİPARİŞ ONAYLANDIĞINDA
   └─ Cari hesapta otomatik BORÇ kaydı oluşur

2. TAHSİLAT GELDİĞİNDE
   └─ Ön Muhasebe → Cari Hareketler → Yeni (Alacak)
   └─ Kasa seçilirse → Kasaya otomatik GİRİŞ
   └─ VEYA
   └─ Ön Muhasebe → Çek/Senet → Yeni

3. ÇEK TAKİBİ
   └─ Portföyde → Tahsilde → Tahsil Edildi
```

### Gider Akışı

```
1. GİDER KAYDI
   └─ Ön Muhasebe → Giderler → Yeni Gider
   └─ Kasa seçilirse → Kasadan otomatik ÇIKIŞ

2. GİDER İPTALİ
   └─ Gider silindiğinde ilgili kasa hareketi de silinir
```

### Yazdırma Akışı

```
1. ÇIKTI ŞABLONU HAZIRLAMA
   └─ Yönetim → Yazdır Şablonları → Yeni/Düzenle
   └─ HTML şablon + {{Placeholder}} ile tasarla

2. ÇIKTI ALMA
   └─ İlgili sayfa (Sipariş/İş Emri/Müşteri) → Yazdır butonu
   └─ Şablon seç → Önizle veya Yazdır
```

---

## 13. Sık Sorulan Sorular

**S: Sipariş numarası nasıl oluşuyor?**
C: Otomatik olarak `SIP-yyyyAA-001` formatında oluşur. yy=yıl, AA=ay. Her ay sıfırdan başlar.

**S: Silinen kayıtlar geri gelir mi?**
C: Evet. Silme işlemi soft-delete olarak çalışır, kayıtlar veritabanından silinmez, pasif yapılır.

**S: Farklı siparişlerden kalemleri aynı iş emrinde birleştirebilir miyim?**
C: Evet. İş emri oluştururken farklı siparişlerin kalemlerini seçebilirsiniz. Sistem aynı malzeme türündeki kalemleri otomatik filtreler.

**S: Kesim optimizasyonu ne kadar verimli?**
C: Guillotine Bin Packing algoritması kullanılır. Genellikle %5-15 arası fire oranı elde edilir. Fire oranı her kesim planında gösterilir.

**S: Hangi para birimleri destekleniyor?**
C: TRY (Türk Lirası), USD (Amerikan Doları), EUR (Euro).

**S: Excel şablonu hangi formatta olmalı?**
C: .xlsx (Excel 2007+) formatında. Sisteme ait şablonu indirip kullanmanız önerilir.

**S: Kesim planı çıktılarını hangi makinalar destekliyor?**
C: CSV formatı genel amaçlıdır. DXF formatı AutoCAD ve çoğu CNC kesim makinası ile uyumludur.

**S: Birden fazla kullanıcı aynı anda çalışabilir mi?**
C: Evet. Web tabanlı yapı sayesinde birden fazla kullanıcı farklı modüllerde eş zamanlı çalışabilir.

**S: Stok düşme işlemi geri alınabilir mi?**
C: Hayır. İş emri tamamlama ve stok düşme geri alınamaz. Manuel stok girişi yaparak düzeltme yapabilirsiniz.

**S: Sipariş onaylandığında cari hesapta ne olur?**
C: Sipariş onaylandığında müşterinin cari hesabına sipariş tutarı kadar otomatik borç kaydı oluşturulur.

**S: Tahsilat ve kasa nasıl bağlanır?**
C: Cari hareket eklerken tür olarak "Alacak" seçip bir kasa belirlerseniz, kasaya otomatik giriş hareketi oluşturulur. Aynı şekilde gider kaydında kasa seçilirse otomatik çıkış hareketi oluşur.

**S: Yazdırma şablonlarını nasıl özelleştirebilirim?**
C: Yönetim → Yazdır Şablonları bölümünden HTML şablonları düzenleyebilirsiniz. `{{Placeholder}}` sistemi ile dinamik verileri (müşteri adı, sipariş no, kalemler tablosu vb.) otomatik doldurabilirsiniz.

**S: Müşteri bakiyesini nereden görebilirim?**
C: Müşteri listesinde "Bakiye (TRY)" kolonu, müşteri detay sayfasında ise ayrıntılı cari bakiye kartı görüntülenir.

**S: Reçete patlatma nedir?**
C: Reçeteli bir ürün sipariş edildiğinde, iş emri oluşturulurken reçetenin katmanları ayrı ayrı malzeme satırlarına dönüştürülür. Böylece her cam türü için ayrı kesim optimizasyonu yapılabilir.

---

**GLASSOFT ERP v1.1.0** - ConsoleSoft tarafından cam sektörü için geliştirilmiştir.
Teknik destek ve öneriler için ConsoleSoft ile iletişime geçin.
