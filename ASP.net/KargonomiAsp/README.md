# Kargonomi ASP.NET Core İstemcisi

Bu proje, Kargonomi API için .NET 10 ve ASP.NET Core tabanlı tür güvenli istemci kitaplığını, bağımlılık ekleme yapılandırmasını ve örnek Web API uygulamasını içerir. Ürün sürümü `2.5.0-Enterprise`, lisansı GPL-3.0'dır.

## Kurulum ve derleme

Depo kökünde aşağıdaki komutları çalıştırın:

```powershell
dotnet restore ASP.net/KargonomiAsp.sln --locked-mode
dotnet build ASP.net/KargonomiAsp.sln --no-restore
```

## Yapılandırma

Temel API adresi `appsettings.json` dosyasındaki `Kargonomi:BaseUrl` değerinden okunur. API anahtarı ayar dosyasına yazılmamalıdır. ASP.NET Core ortam değişkeni eşleştirmesi kullanılacaksa `Kargonomi__ApiToken` adı tercih edilmelidir.

```csharp
builder.Services.AddKargonomi(options =>
{
    options.ApiToken = builder.Configuration["Kargonomi:ApiToken"]!;
    options.BaseUri = builder.Configuration.GetValue<Uri>("Kargonomi:BaseUrl")!;
});
```

Proje; yönlendirme izlemeyi kapatır, düz HTTP adreslerini reddeder ve yalnızca güvenli HTTP yöntemlerinde sınırlı yeniden gönderme uygular.

## MSSQL

`Database/Database_Asp.sql`, `Kargonomi_Asp` veritabanının tek kurulum dosyasıdır. Uygulama bu dosyayı kendiliğinden çalıştırmaz; Entity Framework veya otomatik şema geçişi kullanılmaz.

Ortak mimari, güvenlik ve doğrulama bilgileri için kök [README.md](../../README.md) ile [SECURITY.md](../../SECURITY.md) belgelerine bakın.

Bu bileşen Hamza Deniz Yılmaz ve Beyza Gül tarafından geliştirilmiştir. Bilhost proje destekçisi, Kargonomi ise entegre edilen hizmetin markasıdır.
