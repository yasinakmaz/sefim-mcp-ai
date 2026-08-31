# Knowledge Authoring

Yeni bir tabloyu AI'ya öğretmek için:

1. SQL erişimi varsa `dotnet run --project sefim-ai-mcp -- knowledge generate-table-doc dbo.X` komutunu çalıştırın.
2. Oluşan `knowledge/private/tables/dbo.X.md` dosyasında sadece `<!-- USER: ... -->` alanlarını doldurun.
3. Uygulama özeti için `knowledge/templates/application-overview.template.md` dosyasını `knowledge/private/application/overview.md` olarak kopyalayın ve aynı alanları doldurun.
4. Workflow ve kural için ilgili template'i `knowledge/private/workflows/` veya `knowledge/private/business-rules/` altına kopyalayın.
5. `dotnet run --project sefim-ai-mcp -- knowledge validate` çalıştırın.
6. `dotnet run --project sefim-ai-mcp -- knowledge stats` ile belge sayısını, section sayısını ve eksik `summary`/`aliases` alanlarını görün.
7. `SEFIM_KNOWLEDGE_KEY` tanımlıyken `dotnet run --project sefim-ai-mcp -- knowledge pack` çalıştırın.

**Template'lere gerçek içerik yazmayın.** `knowledge/templates/` NuGet paketine düz metin olarak girer; gerçek iş bilgisi yalnızca `knowledge/private/` altında yaşar ve şifreli pack'e girer. Build, template içine yazılmış içeriği `SEFIM001` hatasıyla reddeder.

## Frontmatter

| Alan | Zorunlu | Amaç |
| --- | --- | --- |
| `kind` | evet | `application`, `glossary`, `guidance`, `table`, `workflow`, `business-rule` |
| `id` | evet | Tool'ların istediği identifier |
| `status` | evet | `draft` modele eksik bilgi olduğunu bildirir |
| `exposure` | evet | `model` veya `private` |
| `summary` | hayır ama önerilir | Arama sonucunda gösterilir ve arama indeksinde ağırlıklı geçer |
| `aliases` | hayır ama önerilir | Belgenin gövdesinde geçmeyen ama kullanıcının yazacağı kelimeler: `stok kart, urun karti` |

`summary` ve `aliases` retrieval kalitesinin en ucuz kaldıracıdır: arama motoru her belge için bu alanlardan sentetik bir "Identity" kaydı üretir, böylece belge gövdesi farklı kelimeler kullansa bile doğru belge bulunur.

## Arama nasıl çalışır

`knowledge pack` içindeki her belge `##` ve `###` başlıklarına göre section'lara bölünür; glossary belgelerinde her `- **Terim:** ...` satırı kendi section'ı olur. Sorgu ve belge Türkçe katlama ile aynı forma indirgenir (`Ürün`, `URUN`, `urun` aynı terim), BM25 ile skorlanır ve en az 4 karakter ortak önek kuralıyla ekler tolere edilir (`ürünün`, `ürünler`). Sonuç tüm belge değil, ilgili section ve `sefim://knowledge/{kind}/{id}#{heading}` URI'sidir.

`status: draft` eksik semantic bilgi olduğunu modele açıkça bildirir. `exposure: private` pack'e girebilir ama runtime tarafından model-visible knowledge olarak dönmez; gerçek server-private policy ise Markdown'a dahi yazılmamalı, C# policy katmanında tutulmalıdır.
