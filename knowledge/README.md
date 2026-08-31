# Şefim Knowledge Authoring

`private/` altındaki dosyalar business bilgisinin source halidir ve git'e eklenmez. Sadece `templates/` dosyalarını kopyalayın, `<!-- USER: ... -->` alanlarını doldurun, ardından doğrulayıp şifreli paketi üretin.

```bash
export SEFIM_KNOWLEDGE_KEY="$(openssl rand -base64 32)"
dotnet run --project sefim-ai-mcp -- knowledge validate
dotnet run --project sefim-ai-mcp -- knowledge stats
dotnet run --project sefim-ai-mcp -- knowledge pack
```

`knowledge.pack` release ile dağıtılabilir; private Markdown dağıtıma eklenmez. Key, deployment sırasında process environment veya işletim sistemi secret store üzerinden sağlanmalıdır.

## Klasörler

| Klasör | `kind` | İçerik |
| --- | --- | --- |
| `private/application/` | `application` | Uygulama özeti, modüller, kullanıcı rolleri |
| `private/glossary/` | `glossary` | Türkçe iş terimleri: `- **Terim:** açıklama` satırları |
| `private/tables/` | `table` | Tablo ve kolonların iş anlamı |
| `private/business-rules/` | `business-rule` | Değişmez kurallar |
| `private/workflows/` | `workflow` | Adım adım iş akışları |
| `private/guidance/` | `guidance` | AI'ya davranış rehberi |

## Template kuralı

`templates/` klasörü NuGet paketine düz metin girer. Buraya gerçek içerik yazmayın; build `SEFIM001` ile durur. Örnek görmek için [docs/knowledge-authoring.md](../docs/knowledge-authoring.md) dosyasına bakın.

## Retrieval kalitesi

Her belgeye `summary` yazın ve kullanıcının kullanabileceği ama gövdede geçmeyen kelimeleri `aliases` alanına ekleyin. Arama bu iki alandan belge başına bir kimlik kaydı üretir; en ucuz doğruluk kazancı buradadır. `knowledge stats` eksik `summary` ve `aliases` olan belgeleri listeler.
