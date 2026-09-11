# Kargonomi OpenAPI 3.1 Belgeleri

Bu klasor, Kargonomi API icin OpenAPI 3.1 sozlesmesi ve destek dosyalarini icerir. Urun surumu `2.5.0-Enterprise`, lisansi GPL-3.0'dir.

## Dosya Yapisi

| Dosya / Klasor | Aciklama |
|---|---|
| `kargonomi-openapi.yaml` | Insanlar tarafindan okunabilir YAML bicimindeki ana sozlesme |
| `kargonomi-openapi.json` | Makineler tarafindan okunabilir JSON bicimindeki eslenik sozlesme |
| `Contracts/endpoint-catalog.json` | Tum ucnoktalar, HTTP yontemleri ve yeniden gonderme guvenligi |
| `Schemas/endpoint-catalog.schema.json` | Katalog dosyasinin JSON Semasi |
| `Examples/` | Ucnokta gruplarına gore ayrilmis ornek istek ve yanitlar |

## Dogrulama

```powershell
npm run openapi:check
```

Bu komut `kargonomi-openapi.json` dosyasinin gecerli JSON olup olmadigini denetler. Buyuk degisikliklerden once ve sonra calistirin.

## Sozlesme Kurallari

- `x-provider-ambiguity` alani: Resmi teknik belgede belirsiz olan davranislari isaretler.
- `additionalProperties: true`: Yeni hizmet alanlari istemciyi bozmadan korunur.
- Tum hata yanitlari `components/responses` altinda merkezi tanimlidir.

Bu belge Hamza Deniz Yilmaz ve Beyza Gul tarafindan hazirlanmistir. Kargonomi resmi dokumantasyonu icin [kargonomi.com.tr](https://www.kargonomi.com.tr/help/api-dokumantasyonu/kargonomi-api/) adresini ziyaret edin.