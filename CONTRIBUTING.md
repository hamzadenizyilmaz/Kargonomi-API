# Kargonomi API Katkı Rehberi

Bu çalışma alanı Hamza Deniz Yılmaz ve Beyza Gül tarafından yönetilir, Bilhost tarafından desteklenir ve Kargonomi API ile bütünleşir. Katkılar GPL-3.0 lisansı kapsamında kabul edilir.

## Katkı hazırlığı

1. Değişikliğin etkilediği bileşenleri ve uç noktaları belirleyin.
2. API davranışını Kargonomi'nin güncel teknik belgeleriyle karşılaştırın.
3. Geçerli API anahtarı, webhook gizli anahtarı, müşteri adı, adres, telefon, barkod veya gerçek gönderi verisi kullanmayın.
4. Değişikliği tek bir teknik amaçla sınırlayın ve gerekçesini açıkça yazın.
5. Değiştireceğiniz dosyaların mevcut davranışını anlamadan uygulamaya başlamayın.

## Kod ilkeleri

- API alan adlarını aktarım biçiminde değiştirmeyin. Her dilde, API alanları ile dile özgü adlandırmalar arasında açık dönüşüm uygulayın.
- Temel API adresini kaynak koda yazmayın; ilgili bileşenin `appsettings.json` dosyasını kullanın.
- `POST`, `PUT`, `PATCH` ve `DELETE` isteklerine otomatik yeniden gönderme davranışı eklemeyin.
- Düz HTTP bağlantılarını, yönlendirme izlemeyi veya erişim bilgilerinin kaydedilmesini etkinleştirmeyin.
- Veritabanı değişikliği gerektiğinde yalnızca ilgili MSSQL dosyasını güncelleyin. Entity Framework, SQLite, otomatik şema geçişi veya uygulama içi veritabanı oluşturma davranışı eklemeyin.
- C# kaynaklarına kodun zaten anlattığı bilgileri tekrarlayan `///` açıklamaları eklemeyin. Kullanıcıya yönelik açıklamaları Markdown belgelerinde tutun.
- `TODO`, `FIXME`, kullanılmayan sınıf, boş soyutlama veya geçici örnek kod bırakmayın.
- Yeni bir uç nokta desteği ekleniyorsa OpenAPI belgesini, uç nokta kataloğunu ve etkilenen istemci kitaplıklarını birlikte güncelleyin.

## Yerel denetimler

Değişiklik önerisi göndermeden önce etkilenen bileşenin kod biçimi, tür denetimi ve derleme komutlarını çalıştırın. Kullanılabilecek temel komutlar ana [README.md](README.md#yerel-doğrulama-komutları) belgesinde verilmiştir.

MSSQL şeması değiştirildiyse ilgili SQL dosyasını boş bir SQL Server örneğinde çalıştırın. Ardından aynı dosyayı ikinci kez çalıştırarak varlık denetimlerinin doğru çalıştığını doğrulayın. Bu denetimde gerçek Kargonomi hesabı veya müşteri verisi kullanmayın.

## Değişiklik önerisi içeriği

Değişiklik önerisi aşağıdaki bilgileri içermelidir:

- sorunun ve çözümün kısa açıklaması;
- etkilenen bileşenler, uç noktalar ve dosyalar;
- güvenlik ve geriye uyumluluk etkisi;
- çalıştırılan yerel denetim komutları ve sonuçları;
- API sözleşmesi değiştiyse dayanak alınan teknik belge;
- kullanıcı davranışı değiştiyse `README.md` ve `CHANGELOG.md` güncellemesi.

## Lisans ve marka kullanımı

Katkı göndererek kodunuzun GPL-3.0 kapsamında kullanılmasını kabul edersiniz. Bilhost ve Kargonomi logolarını değiştirmeyin. Marka varlıkları ilgili hak sahiplerine aittir ve proje Kargonomi'nin resmî ürünü olarak tanıtılamaz.
