# Değişiklik Günlüğü

Bu belge, kullanıcıları ve geliştiricileri etkileyen önemli teknik değişiklikleri sürümlere göre kaydeder. Projenin kullanıcıya gösterilen sürümü `2.5.0-Enterprise`, lisansı GPL-3.0'dır.

## 2.5.0-Enterprise — 11 Eylül 2026

### Eklenenler

- ASP.NET Core, Node.js, PHP ve Python için ortak API sözleşmesine bağlı istemci kitaplıkları eklendi.
- Her bileşen için `appsettings.json` tabanlı merkezi API adresi yapılandırması eklendi.
- Node.js, PHP ve ASP.NET Core için sırasıyla `Database_JS.sql`, `Database_PHP.sql` ve `Database_Asp.sql` adlı MSSQL kurulum dosyaları eklendi.
- Webhook tekrarlarını önleyen benzersiz anahtar, JSON geçerlilik denetimi, UTC denetim alanları ve API istek kayıtları için MSSQL şemaları eklendi.
- Bilhost ve Kargonomi marka varlıkları ile kapsamlı Türkçe proje belgeleri eklendi.

### Değiştirilenler

- Proje lisansı GNU General Public License v3.0 olarak düzenlendi.
- Paket bilgileri ürün sürümü `2.5.0-Enterprise` ile Hamza Deniz Yılmaz ve Beyza Gül geliştirici bilgilerine göre güncellendi.
- Temel API adresleri kaynak koddan çıkarıldı ve ilgili ayar dosyalarına taşındı.
- C# XML açıklama blokları kaldırıldı; kullanıcıya yönelik bilgiler Markdown belgelerinde toplandı.
- Teknik terimler, yazım kuralları ve başlıklar tutarlı Türkçe kullanımına göre yeniden düzenlendi.

### Kaldırılanlar

- SQLite dosyaları, bağımlılıkları ve depolama sınıfları kaldırıldı.
- Otomatik veritabanı hazırlama, şema geçişi ve inceleme komutları kaldırıldı.
- Entity Framework bağımlılığı kaldırıldı; proje Entity Framework kullanmaz.
- Deneme amacıyla oluşturulan geçici veritabanı ve doğrulama çıktıları temizlendi.

## Proje bilgileri

Proje Hamza Deniz Yılmaz ve Beyza Gül tarafından geliştirilir ve Bilhost tarafından desteklenir. Kargonomi, entegre edilen hizmetin markasıdır; bu depo Kargonomi'nin resmî yazılım geliştirme kiti değildir.
