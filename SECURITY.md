# Güvenlik Politikası

## Desteklenen sürüm

| Sürüm | Güvenlik durumu |
|---|---|
| `2.5.0-Enterprise` | Etkin destek |
| `0.x` ve daha eski çalışma kopyaları | Desteklenmiyor |

Kargonomi API entegrasyonu Hamza Deniz Yılmaz ve Beyza Gül tarafından sürdürülür; Bilhost proje destekçisidir. Bu depo, Kargonomi'nin resmî güvenlik bildirim kanalı değildir.

## Güvenlik açığı bildirimi

Bir güvenlik açığını açık GitHub kaydında paylaşmayın. Deponun özel güvenlik bildirimi özelliğini kullanın veya proje yöneticileriyle GitHub profilleri üzerinden özel iletişim kurun.

Bildirimde aşağıdaki bilgilere yer verin:

- etkilenen bileşen ve sürüm;
- gerçek müşteri verisi içermeyen en küçük yeniden oluşturma adımları;
- beklenen güvenlik etkisi;
- biliniyorsa sorunu sınırlandırmaya yönelik öneri.

Geçerli API anahtarı, webhook gizli anahtarı, müşteri bilgisi veya size ait olmayan bir hesabın verisini eklemeyin. Kargonomi ya da Bilhost sistemlerine ilişkin bir güvenlik sorunu tespit ettiyseniz doğrudan ilgili kurumun resmî güvenlik kanalını kullanın.

## Erişim bilgisi olayı

Bir API anahtarı veya gizli anahtar sohbet kaydında, günlükte, ekran görüntüsünde, Git geçmişinde ya da derleme çıktısında görünmüşse ele geçirilmiş kabul edilmelidir. İlgili erişim bilgisini hizmet yönetim ekranından iptal edin, yeni değer oluşturun, erişim kayıtlarını inceleyin ve saklanan kopyaları temizleyin.

Bir değeri dosyanın son sürümünden silmek, Git geçmişindeki eski kopyaları ortadan kaldırmaz. Geçmişte bulunan erişim bilgileri yeniden kullanılmamalıdır.

## Güvenlik sınırları

- API anahtarı yalnızca çalışma anında alınır ve `Authorization` başlığına eklenir.
- Temel API adresi `appsettings.json` üzerinden okunur; HTTPS zorunludur.
- Farklı etki alanlarına yönlendirme izlenmez.
- Webhook imzası, JSON ayrıştırılmadan önce alınan ham istek gövdesinin baytları üzerinden doğrulanır.
- İmza karşılaştırmasında sabit süreli karşılaştırma kullanılır.
- MSSQL `WebhookEvents` tablosu, `IdempotencyKey` alanını benzersiz kısıtla korur.
- Uygulama veritabanı oluşturmaz ve şema geçişi çalıştırmaz; SQL dosyalarının uygulanması veritabanı yöneticisinin sorumluluğundadır.
- Tanılama kayıtlarında erişim bilgileri ve kişisel veriler maskelenir.

## Güvenli kullanım denetim listesi

1. API anahtarını ortam değişkeni veya işletim sisteminin güvenli kimlik bilgisi saklama alanı üzerinden sağlayın.
2. MSSQL kurulum dosyasını uygulama hesabından ayrı, geçici ve yalnızca kurulum için yetkilendirilmiş bir hesapla çalıştırın.
3. Uygulama hesabına yalnızca gereksinim duyduğu tablo ve işlemler için izin verin.
4. TLS sertifika doğrulamasını kapatmayın.
5. Webhook alıcısında istek boyutu sınırı, istek sıklığı sınırı, imza denetimi ve tekrar önleme uygulayın.
6. Günlük saklama sürelerini KVKK kapsamındaki veri işleme yükümlülüklerinize göre belirleyin.

Kaynak kod GPL-3.0 kapsamında lisanslanmıştır. Bilhost ve Kargonomi adları ile marka varlıkları ilgili hak sahiplerine aittir.
