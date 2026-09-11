<p align="center">
  <a href="https://www.bilhost.com/"><img src="assets/branding/bilhost-logo.svg" alt="Bilhost" height="58"></a>
  &nbsp;&nbsp;&nbsp;&nbsp;
  <a href="https://www.kargonomi.com.tr/"><img src="assets/branding/kargonomi-logo.png" alt="Kargonomi" height="58"></a>
</p>

<h1 align="center">Kargonomi API Entegrasyon Altyapısı</h1>

<p align="center">
  ASP.NET Core, Node.js, PHP ve Python tabanlı sunucu uygulamaları için çok dilli API entegrasyonu
</p>

<p align="center">
  <img src="https://img.shields.io/badge/sürüm-2.5.0--Enterprise-1f6feb" alt="Sürüm 2.5.0-Enterprise">
  <a href="LICENSE"><img src="https://img.shields.io/badge/lisans-GPL--3.0-blue" alt="GPL-3.0 lisansı"></a>
</p>

Bu depo, Kargonomi API işlemlerini ASP.NET Core, Node.js, PHP ve Python tabanlı sunucu uygulamalarında kullanmak için geliştirilen dört ayrı entegrasyon uygulamasını bir arada tutar. Her uygulama; kimlik doğrulama, HTTP istekleri, yanıt modelleri, hata yönetimi ve webhook doğrulaması gibi işlemleri kendi çalışma ortamına uygun biçimde yürütür.

Proje Hamza Deniz Yılmaz ve Beyza Gül tarafından geliştirilmektedir. Kargonomi'nin resmî ürünü değildir; API'nin güncel davranışı ve kullanım koşulları için Kargonomi belgeleri esas alınmalıdır.

## Depo yapısı

| Dizin | İçerik |
|---|---|
| `ASP.net/` | .NET 10 ve ASP.NET Core ile geliştirilen API entegrasyon uygulaması |
| `NodeJS/` | Node.js 24 ve TypeScript ile geliştirilen API entegrasyon uygulaması |
| `PHP/` | PHP 8.5 ile geliştirilen API entegrasyon uygulaması |
| `Python/` | Python 3.14 ile geliştirilen eşzamanlı ve eşzamansız API entegrasyon uygulaması |
| `Swagger/` | OpenAPI 3.1 tanımları, şemalar ve örnek yanıtlar |
| `Postman/` | Postman koleksiyonu ve ortam dosyası |

## Desteklenen işlemler

| Kaynak | İşlemler |
|---|---|
| Gönderiler | Listeleme, görüntüleme, oluşturma, güncelleme, kısmi güncelleme, iptal ve silme |
| Fiyatlandırma | Fiyat karşılaştırma ve kargo firması seçimini onaylama |
| Depolar | Depo oluşturma |
| Konumlar | İl ve ilçe listelerini alma |
| Barkod | Gönderi barkodunu PDF olarak alma |
| Hesap | Kullanılabilir kredi bilgisini alma |
| Webhook'lar | Listeleme, görüntüleme, oluşturma, güncelleme ve silme |

Uç noktaların tamamı [`Swagger/Contracts/endpoint-catalog.json`](Swagger/Contracts/endpoint-catalog.json) dosyasında, API sözleşmesi ise [`Swagger/kargonomi-openapi.yaml`](Swagger/kargonomi-openapi.yaml) ve [`Swagger/kargonomi-openapi.json`](Swagger/kargonomi-openapi.json) dosyalarında bulunur.

## Gereksinimler

Yalnızca projenizde kullanacağınız yazılım ortamının araçlarını kurmanız yeterlidir.

| Platform | Gereksinim |
|---|---|
| ASP.NET Core | .NET SDK 10 veya üzeri |
| Node.js | Node.js 24 ve npm 12 veya üzeri |
| PHP | PHP 8.5, Composer 2.10, cURL ve JSON eklentileri |
| Python | Python 3.14 ve güncel bir `uv` sürümü |
| Veritabanı | SQL Server 2016 SP1 veya üzeri ya da Azure SQL |

## Yapılandırma

Her entegrasyon uygulaması, temel API adresini kendi `appsettings.json` dosyasından okur:

| Platform | Yapılandırma dosyası |
|---|---|
| ASP.NET Core | `ASP.net/KargonomiAsp/appsettings.json` |
| Node.js | `NodeJS/appsettings.json` |
| PHP | `PHP/appsettings.json` |
| Python | `Python/src/kargonomi/appsettings.json` |

API anahtarını bu dosyalara veya kaynak koda yazmayın. Anahtarı ortam değişkeninden ya da kullandığınız sistemin gizli bilgi yöneticisinden alın.

## Kurulum ve ilk kullanım

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

const shipments = await client.shipments.list();
console.log(shipments.total);
```

### ASP.NET Core

```powershell
dotnet restore ASP.net/KargonomiAsp.sln --locked-mode
dotnet build ASP.net/KargonomiAsp.sln --no-restore
```

ASP.NET Core entegrasyonunu bağımlılık enjeksiyonu konteynerine kaydedin:

```csharp
builder.Services.AddKargonomi(options =>
{
    options.ApiToken = builder.Configuration["Kargonomi:ApiToken"]!;
    options.BaseUri = builder.Configuration.GetValue<Uri>("Kargonomi:BaseUrl")!;
});
```

API anahtarı ASP.NET Core yapılandırmasına `Kargonomi__ApiToken` ortam değişkeniyle verilebilir.

```csharp
public sealed class ShipmentsService(KargonomiClient client)
{
    public Task<ShipmentPage> ListAsync(CancellationToken cancellationToken) =>
        client.Shipments.ListAsync(cancellationToken: cancellationToken);
}
```

### PHP

```powershell
cd PHP
composer install
```

```php
<?php

require __DIR__ . '/vendor/autoload.php';

use Kargonomi\KargonomiClient;

$client = new KargonomiClient((string) getenv('KARGONOMI_API_TOKEN'));
$shipments = $client->shipments()->list();

echo $shipments->total;
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
    shipments = client.shipments.list()
    print(shipments.total)
```

Python uygulaması, aynı işlemlerin eşzamansız yürütülmesi için `AsyncKargonomiClient` sınıfını da sağlar.

## MSSQL şemaları

Entegrasyon uygulamaları veritabanı oluşturmaz ve şema değişikliği çalıştırmaz. Gerekli T-SQL dosyaları bir SQL Server yöneticisi tarafından elle uygulanmalıdır.

| Platform | Kurulum dosyası | Veritabanı |
|---|---|---|
| ASP.NET Core | [`Database_Asp.sql`](ASP.net/KargonomiAsp/Database/Database_Asp.sql) | `Kargonomi_Asp` |
| Node.js | [`Database_JS.sql`](NodeJS/Database/Database_JS.sql) | `Kargonomi_JS` |
| PHP | [`Database_PHP.sql`](PHP/Database/Database_PHP.sql) | `Kargonomi_PHP` |

Her şemada uygulama bilgileri, webhook olayları ve API istek kayıtları için tablolar bulunur. Kurulum dosyaları nesnelerin varlığını denetler ve mevcut tabloları silmez. API anahtarları ile müşteri bilgileri SQL dosyalarına eklenmemiştir.

## Postman kullanımı

1. [`Kargonomi.postman_collection.json`](Postman/Kargonomi.postman_collection.json) koleksiyonunu içe aktarın.
2. [`Kargonomi.postman_environment.json`](Postman/Kargonomi.postman_environment.json) ortam dosyasını içe aktarın.
3. API anahtarını yalnız kendi Postman ortamınızın gizli değişkenine girin.
4. Veri oluşturan, güncelleyen veya silen istekleri göndermeden önce seçili ortamı ve isteği kontrol edin.

Depodaki Postman dosyalarında geçerli API anahtarı veya müşteri verisi bulunmaz.

## Doğrulama

Bir entegrasyon uygulamasında değişiklik yaptıktan sonra ilgili komutları çalıştırın:

```powershell
# Node.js
npm ci --prefix NodeJS
npm --prefix NodeJS run verify

# OpenAPI ve Postman JSON dosyaları
npm run openapi:check
npm run postman:check

# ASP.NET Core
dotnet restore ASP.net/KargonomiAsp.sln --locked-mode
dotnet build ASP.net/KargonomiAsp.sln --no-restore

# PHP
composer --working-dir PHP validate --strict
composer --working-dir PHP check

# Python
uv run --directory Python ruff check .
uv run --directory Python mypy
uv build --directory Python
```

Bu kontroller gerçek Kargonomi hesabına istek göndermez.

## Güvenlik

- API anahtarlarını, webhook gizli anahtarlarını ve müşteri verilerini Git geçmişine eklemeyin.
- Webhook imzasını JSON ayrıştırılmadan önce, alınan ham istek gövdesi üzerinden doğrulayın.
- Entegrasyon uygulamaları yönlendirmeleri izlemez ve yalnız HTTPS temel adreslerini kabul eder.
- Otomatik yeniden deneme yalnız güvenli `GET` isteklerinde uygulanır.

Bir güvenlik açığı bulduysanız herkese açık issue oluşturmadan önce [SECURITY.md](SECURITY.md) içindeki bildirim yolunu kullanın.

## Belgeler

- [Katkı rehberi](CONTRIBUTING.md)
- [Güvenlik politikası](SECURITY.md)
- [Değişiklik geçmişi](CHANGELOG.md)
- [Davranış kuralları](CODE_OF_CONDUCT.md)
- [OpenAPI tanımı](Swagger/kargonomi-openapi.yaml)
- [Postman koleksiyonu](Postman/Kargonomi.postman_collection.json)

## Geliştiriciler

- [Hamza Deniz Yılmaz](https://github.com/hamzadenizyilmaz)
- [Beyza Gül](https://github.com/beyzagul02)

Proje [Bilhost](https://www.bilhost.com/) desteğiyle geliştirilmektedir. Kargonomi adı ve logosu Kargonomi'ye, Bilhost adı ve logosu Bilhost'a aittir.

## Lisans

Kaynak kod [GNU General Public License v3.0](LICENSE) kapsamında sunulur. Paket tanımlarında aynı lisansın SPDX karşılığı olan `GPL-3.0-only` ifadesi kullanılır.
