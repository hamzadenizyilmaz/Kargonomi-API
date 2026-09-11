<p align="center">
  <a href="https://www.bilhost.com/"><img src="assets/branding/bilhost-logo.svg" alt="Bilhost logosu" height="62"></a>
  &nbsp;&nbsp;&nbsp;&nbsp;
  <a href="https://www.kargonomi.com.tr/"><img src="assets/branding/kargonomi-logo.png" alt="Kargonomi logosu" height="62"></a>
</p>

<h1 align="center">Kargonomi API - Kurumsal Dilli SDK</h1>

<p align="center">
  ASP.NET Core, Node.js, PHP ve Python istemci kitaplıkları.<br>
  <strong>Ürün sürümü: 2.5.0-Enterprise / Lisans: GPL-3.0</strong>
</p>

> Bu depo, Hamza Deniz Yılmaz ve Beyza Gül tarafından geliştirilen bağımsız bir açık kaynak entegrasyonudur. Kargonomi'nin resmî yazılım geliştirme kiti değildir. Kargonomi API davranışı için yetkili kaynak, Kargonomi'nin güncel teknik belgeleridir.

## İçindekiler

- [Proje amacı](#proje-amacı)
- [Desteklenen işlemler](#desteklenen-işlemler)
- [Sistem gereksinimleri](#sistem-gereksinimleri)
- [Kurulum ve kullanım](#kurulum-ve-kullanım)
- [Merkezi API yapılandırması](#merkezi-api-yapılandırması)
- [MSSQL veritabanları](#mssql-veritabanları)
- [Güvenlik ilkeleri](#güvenlik-ilkeleri)
- [Yerel doğrulama komutları](#yerel-doğrulama-komutları)
- [Sürüm ve lisans gösterimi](#sürüm-ve-lisans-gösterimi)
- [Geliştiriciler ve marka bilgileri](#geliştiriciler-ve-marka-bilgileri)
- [Proje belgeleri](#proje-belgeleri)

## Proje amacı

Bu çalışma alanı, Kargonomi API işlemlerini farklı yazılım dillerinde tutarlı ve güvenli biçimde kullanmak için hazırlanmıştır. Her istemci kendi ekosisteminin kurallarına uyar; buna karşılık HTTP yöntemleri, uç noktalar, kimlik doğrulama, veri modelleri ve hata davranışları ortak API sözleşmesiyle eşleştirilir.

Temel tasarım ilkeleri şunlardır:

- API anahtarı kaynak kodunda veya ayar dosyalarında tutulmaz.
- Temel API adresi yalnızca ilgili `appsettings.json` dosyasından okunur.
- Düz HTTP bağlantıları ve farklı bir etki alanına yönlendiren istekler reddedilir.
- Veri değiştiren istekler otomatik olarak yeniden gönderilmez.
- Kimlik, telefon, vergi numarası ve barkod gibi değerler baştaki sıfırlar korunacak biçimde metin olarak işlenir.
- Yeni hizmet durumu veya olay türü geldiğinde ham değer korunur; istemci gereksiz yere çalışmayı durdurmaz.
- Uygulamalar veritabanı oluşturmaz, şema güncellemez ve Entity Framework kullanmaz.

Depo; SDK geliştirme, API sözleşmesinin incelenmesi ve MSSQL şemasının elle kurulması için gereken kaynakları içerir. Sunucu yönetimi ve uzak hizmet kurulum süreçleri bu deponun kapsamı dışındadır.

## Desteklenen işlemler

| Alan | İşlemler |
|---|---|
| Gönderiler | Listeleme, ayrıntı görüntüleme, oluşturma, tam güncelleme, kısmi güncelleme ve silme |
| Fiyatlandırma | Kargo fiyatlarını karşılaştırma ve seçilen kargo firmasını onaylama |
| İptal | Gönderi iptal talebi oluşturma |
| Depolar | Depo oluşturma |
| Barkod | Gönderi barkodunu alma |
| Hesap | Kullanılabilir kredi bilgisini alma |
| Konumlar | Ülkeye bağlı illeri ve ile bağlı ilçeleri alma |
| Webhook | Listeleme, ayrıntı görüntüleme, oluşturma, güncelleme ve silme |

Makine tarafından okunabilir uç nokta listesi `Swagger/Contracts/endpoint-catalog.json` dosyasındadır. Depo oluşturma ile webhook güncelleme ve silme davranışlarında hizmet belgesinden kaynaklanan belirsizlikler katalogda ayrıca işaretlenmiştir.

## Sistem gereksinimleri

| Bileşen | Gerekli sürüm |
|---|---|
| .NET SDK | 10.0 veya üzeri |
| Node.js | 24 veya üzeri |
| npm | 12 veya üzeri |
| PHP | 8.5 veya üzeri |
| Composer | 2.10 veya üzeri |
| Python | 3.14 veya üzeri |
| `uv` | Güncel kararlı sürüm |
| SQL Server | SQL Server 2016 SP1 veya üzeri; Azure SQL ile uyumlu özellikler |

## Kurulum ve kullanım

### Node.js ve TypeScript

```powershell
cd NodeJS
npm ci
npm run build
```

```ts
import { KargonomiClient } from '@kargonomi/client';

const client = new KargonomiClient({
  apiToken: process.env.KARGONOMI_API_TOKEN!,
});

const page = await client.shipments.list();
console.log(page.total);
```

### ASP.NET Core

```powershell
dotnet restore ASP.net/KargonomiAsp.sln --locked-mode
dotnet build ASP.net/KargonomiAsp.sln --no-restore
```

```csharp
builder.Services.AddKargonomi(options =>
{
    options.ApiToken = builder.Configuration["KARGONOMI_API_TOKEN"]!;
    options.BaseUri = builder.Configuration.GetValue<Uri>("Kargonomi:BaseUrl")!;
});
```

### PHP

```powershell
cd PHP
composer install
```

```php
use Kargonomi\KargonomiClient;

$client = new KargonomiClient((string) getenv('KARGONOMI_API_TOKEN'));
$page = $client->shipments()->list();
```

### Python

```powershell
cd Python
uv sync
```

```python
import os
from kargonomi import KargonomiClient

with KargonomiClient(os.environ["KARGONOMI_API_TOKEN"]) as client:
    print(client.shipments.list().total)
```

## Merkezi API yapılandırması

Temel API adresi kaynak kodunda sabit olarak bulunmaz. Her bileşen bu değeri kendi `appsettings.json` dosyasından okur.

| Bileşen | Ayar dosyası | JSON anahtarı |
|---|---|---|
| ASP.NET Core | `ASP.net/KargonomiAsp/appsettings.json` | `Kargonomi:BaseUrl` |
| Node.js | `NodeJS/appsettings.json` | `Kargonomi.BaseUrl` |
| PHP | `PHP/appsettings.json` | `Kargonomi.BaseUrl` |
| Python | `Python/src/kargonomi/appsettings.json` | `Kargonomi.BaseUrl` |

Yapılandırılan adres:

```text
https://app.kargonomi.com.tr/api/v1/
```

Özel bir adres kullanılacaksa HTTPS olmalı ve geçerli bir mutlak adres içermelidir. İstek yolu, yapılandırılan etki alanının dışına çıkamaz. API anahtarı ayar dosyasına eklenmemeli; `KARGONOMI_API_TOKEN` ortam değişkeni veya işletim sisteminin güvenli kimlik bilgisi saklama alanı kullanılmalıdır.

## MSSQL veritabanları

Projede SQLite, Entity Framework, otomatik şema geçişi veya uygulama çalışırken veritabanı hazırlayan bir bileşen yoktur. Her teknoloji için tek bir T-SQL kurulum dosyası bulunur.

| Bileşen | SQL dosyası | Veritabanı adı |
|---|---|---|
| Node.js | `NodeJS/Database/Database_JS.sql` | `Kargonomi_JS` |
| PHP | `PHP/Database/Database_PHP.sql` | `Kargonomi_PHP` |
| ASP.NET Core | `ASP.net/KargonomiAsp/Database/Database_Asp.sql` | `Kargonomi_Asp` |

Bu dosyalar SQL Server Management Studio veya Azure Data Studio üzerinden bir SQL Server yöneticisi tarafından elle çalıştırılır. Her dosya aşağıdaki nesneleri oluşturur:

| Nesne | Amaç |
|---|---|
| `ApplicationMetadata` | Ürün adı, sürüm, lisans ve geliştirici bilgilerini saklar. |
| `WebhookEvents` | Webhook içeriğini, işleme durumunu ve benzersiz tekrar önleme anahtarını saklar. |
| `ApiRequestLogs` | API isteği için yöntem, yol, durum kodu, süre ve zaman bilgisini saklar. |

Şemalarda JSON geçerlilik denetimi, benzersiz `IdempotencyKey` kısıtı, UTC zaman alanları, `ROWVERSION` eşzamanlılık alanı ve sorgu amaçlı dizinler bulunur. SQL dosyaları nesnelerin varlığını denetlediği için aynı kurulum üzerinde yeniden çalıştırılabilir. API anahtarı ve müşteri bilgileri SQL dosyalarına yazılmaz.

Uygulama hesabına `CREATE DATABASE` yetkisi verilmemelidir. Kurulumdan sonra uygulama hesabı yalnızca ihtiyaç duyduğu tablo ve işlemler için en düşük yetkiyle sınırlandırılmalıdır.

## Güvenlik ilkeleri

- API anahtarı ve webhook gizli anahtarı Git geçmişine, günlük kayıtlarına, ekran görüntülerine veya destek kayıtlarına eklenmez.
- Webhook imzası, JSON ayrıştırılmadan önce alınan ham istek gövdesinin baytları üzerinden HMAC-SHA256 ile doğrulanır.
- İmza karşılaştırmasında zamanlama saldırılarına dayanıklı, sabit süreli karşılaştırma kullanılır.
- `POST`, `PUT`, `PATCH` ve `DELETE` istekleri otomatik olarak yeniden gönderilmez.
- Yönlendirme izlenmez; böylece yetkilendirme başlığının farklı bir etki alanına taşınması engellenir.
- Tanılama kayıtlarında API anahtarı, gizli anahtar, adres, telefon ve diğer kişisel veriler maskelenir.

Bir güvenlik açığı bildirmeden önce [SECURITY.md](SECURITY.md) belgesini okuyun. Gerçek müşteri verilerini veya geçerli erişim bilgilerini açık bir GitHub kaydına eklemeyin.

## Yerel doğrulama komutları

Aşağıdaki komutlar kaynak kodu ve makine tarafından okunabilir belgeleri yerel çalışma ortamında doğrular:

```powershell
npm ci --prefix NodeJS
npm --prefix NodeJS run verify
npm run openapi:check
npm run postman:check
dotnet restore ASP.net/KargonomiAsp.sln --locked-mode
dotnet build ASP.net/KargonomiAsp.sln --no-restore
composer --working-dir PHP validate --strict
composer --working-dir PHP check
uv run --directory Python ruff check .
uv run --directory Python mypy
uv build --directory Python
```

Bu komutlar Node.js kod biçimini ve türlerini denetler, istemci kitaplığını derler, paket içeriğini kontrol eder ve OpenAPI ile Postman JSON dosyalarını ayrıştırır. Gerçek bir Kargonomi hesabında veri değiştiren istek gönderilmez.

## Sürüm ve lisans gösterimi

Projenin kullanıcıya gösterilen sürümü `2.5.0-Enterprise`, lisansı GPL-3.0'dır.

Python ve Composer sürüm kuralları `2.5.0-Enterprise` biçimini kabul etmediği için bu iki paket tanımında standartlara uygun `2.5.0+enterprise` değeri kullanılır. PHP paketindeki `extra.product-version` alanı ürün sürümünü değiştirmeden korur.

Paket tanımları, güncel SPDX karşılığı olan `GPL-3.0-only` ifadesini kullanır. Bu ifade, depodaki `LICENSE` dosyasında bulunan GNU General Public License 3.0 metniyle uyumludur.

## Geliştiriciler ve marka bilgileri

- [Hamza Deniz Yılmaz](https://github.com/hamzadenizyilmaz) — proje sahibi ve geliştirici
- [Beyza Gül](https://github.com/beyzagul02) — geliştirici ve katkı sahibi
- [Bilhost](https://www.bilhost.com/) — proje destekçisi
- [Kargonomi](https://www.kargonomi.com.tr/) — entegre edilen hizmetin markası

Bilhost ve Kargonomi adları ile logoları kendi hak sahiplerine aittir. Bu varlıkların depoda bulunması yalnızca destek ve entegrasyon ilişkisini açıklar; projeye resmî ürün niteliği kazandırmaz.

## Proje belgeleri

| Belge | İçerik |
|---|---|
| [SECURITY.md](SECURITY.md) | Güvenlik açığı bildirme süreci ve güvenli kullanım kuralları |
| [CONTRIBUTING.md](CONTRIBUTING.md) | Katkı hazırlama, kod ilkeleri ve değişiklik denetimi |
| [CHANGELOG.md](CHANGELOG.md) | Sürüme göre teknik değişikliklerin özeti |
| [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) | Katılımcılar için davranış kuralları |
| [Swagger/README.md](Swagger/README.md) | OpenAPI belgelerinin yapısı ve doğrulama yöntemi |
| [Postman/README.md](Postman/README.md) | Postman koleksiyonunun güvenli kullanımı |

Kaynak kod GNU General Public License v3.0 kapsamında sunulur. Tam lisans metni [LICENSE](LICENSE) dosyasındadır. Katkı gönderen kişiler, katkılarının aynı lisans kapsamında kullanılmasını kabul eder.
