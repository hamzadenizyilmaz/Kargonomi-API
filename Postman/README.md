# Kargonomi Postman Koleksiyonu

Bu klasor, Kargonomi API icin Postman koleksiyonu ve ortam degiskenleri dosyasini icerir. Urun surumu `2.5.0-Enterprise`, lisansi GPL-3.0'dir.

## Dosyalar

| Dosya | Aciklama |
|---|---|
| `Kargonomi.postman_collection.json` | Tum ucnoktalar icin istek, ornek yanit ve aciklama iceren koleksiyon |
| `Kargonomi.postman_environment.json` | `baseUrl`, `apiToken`, `shipmentId` gibi degiskenleri tanimlayan ortam |

## Kurulum

1. Postman'i acin.
2. **Import** dügmesiyle `Kargonomi.postman_collection.json` dosyasini iceri aktarin.
3. **Import** dügmesiyle `Kargonomi.postman_environment.json` dosyasini iceri aktarin.
4. Ortami secin ve `apiToken` degerini kendi API anahtarinizla doldurun.

> **Uyari:** `apiToken` degerini bos birakin; hicbir zaman koleksiyon veya ortam dosyasina gercek API anahtari yazmayin. Disa aktarilan dosyalari asla Git'e gondermeyin.

## Guvenli Kullanim

- `apiToken` degiskenini yalnizca Postman'in **secret** tipinde saklayin.
- Veri degistiren istekleri (POST, PUT, PATCH, DELETE) yalnizca kendi yetkili hesabinizda calistirin.
- Gercek musteri bilgisi veya siparis verisi iceren yanitvlari paylasmayin.

Bu belge Hamza Deniz Yilmaz ve Beyza Gul tarafindan hazirlanmistir.