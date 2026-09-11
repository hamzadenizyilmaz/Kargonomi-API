# Kargonomi Python İstemcisi

Bu paket, Kargonomi API için Python 3.14 ve `httpx` ile geliştirilmiş eşzamanlı ve eşzamansız istemci kitaplığıdır. Ürün sürümü `2.5.0-Enterprise`, lisansı GPL-3.0'dır.

## Kurulum

```powershell
cd Python
uv sync
```

## Eşzamanlı kullanım

```python
import os
from kargonomi import KargonomiClient

with KargonomiClient(os.environ["KARGONOMI_API_TOKEN"]) as client:
    print(client.shipments.list().total)
```

## Eşzamansız kullanım

```python
import os
from kargonomi import AsyncKargonomiClient

async with AsyncKargonomiClient(os.environ["KARGONOMI_API_TOKEN"]) as client:
    page = await client.shipments.list()
    print(page.total)
```

Temel API adresi `src/kargonomi/appsettings.json` içindeki `Kargonomi.BaseUrl` değerinden okunur. `base_url` seçeneğiyle farklı bir adres verilecekse HTTPS kullanılmalıdır. API anahtarı ayar dosyasına veya kaynak koda yazılmamalıdır.

## Yerel doğrulama

```powershell
uv run ruff check .
uv run mypy
uv build
```

Bu komutlar kod biçimini, türleri ve Python paket yapısını denetler.

Python bileşeni için ayrı MSSQL şeması yoktur. Ortak mimari, veritabanı ve güvenlik bilgileri için kök [README.md](../README.md) ile [SECURITY.md](../SECURITY.md) belgelerine bakın.

Bu bileşen Hamza Deniz Yılmaz ve Beyza Gül tarafından geliştirilmiştir. Bilhost proje destekçisi, Kargonomi ise entegre edilen hizmetin markasıdır.
