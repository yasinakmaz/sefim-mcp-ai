# Architecture

```text
MCP host (stdio)
  -> Mcp/Tools and Mcp/Resources
  -> Application/Policies + Knowledge/Runtime
  -> Repository + SqlService + Infrastructure/Metadata
  -> Local Microsoft SQL Server
```

`Knowledge/Authoring` development-time Markdown workflow'ünü sağlar. `knowledge/private` source'tur; validate/pack akışı AES-256-GCM ile `knowledge.pack` üretir. Runtime yalnızca pack içindeki `exposure: model` belgelerini yükler. Metadata service SQL Server catalog view'ları üzerinden tablonun kolon, key ve foreign-key bilgisini getirir, fakat MCP yalnızca documented table allowlist'i üzerinden çağırır.

MCP generic registrations (`WithTools<T>`, `WithResources<T>`) compile-time bilinen tipler kullanır. Stdio transport ana ve tek transporttur.

## Knowledge retrieval katmanı

Pack yüklenirken belgeler bir kez chunk'lanır ve bellek içi bir arama indeksine dönüşür:

- `KnowledgeChunker` her belgeyi `##` / `###` başlıklarına böler, glossary belgelerinde her tanım satırını ayrı chunk yapar ve her belge için `summary` + `aliases` alanlarından sentetik bir "Identity" chunk'ı üretir.
- `KnowledgeText` Türkçe katlama yapar (`ç ğ ı İ I i ö ş ü â î û` ASCII'ye iner), böylece `Ürün`, `URUN` ve `urun` aynı terimdir.
- `KnowledgeIndex` BM25 (k1=1.2, b=0.75) ile skorlar. Başlıkta geçen terim ağırlıklıdır. Türkçe eklemeli bir dil olduğu için stemmer yerine en az 4 karakterlik ortak önek eşleşmesi kullanılır: `ürünün` ve `ürünler` aynı köke düşer.

Arama tüm belgeyi değil, ilgili section'ı ve `sefim://knowledge/{kind}/{id}#{heading}` URI'sini döndürür. Bu, retrieval maliyetini belge boyutundan bağımsız tutar.

## Tool profile

Tool tanımları her istekte model context'ine girer, dolayısıyla tool yüzeyi bir maliyet kalemidir. Server iki profil yayınlar:

| Profil | Tool sayısı | Yaklaşık tool tanımı maliyeti | İçerik |
| --- | --- | --- | --- |
| `full` (varsayılan) | 182 | ~23k token | Tüm domain tool'ları |
| `core` | 21 | ~3.6k token | Knowledge, report ve operation guard tool'ları |

`SEFIM_TOOL_PROFILE=core` veya `Mcp:ToolProfile=core` ile seçilir. Seçim `Program.cs` içinde compile-time bilinen tiplerle yapılır; assembly scanning yoktur, Native AOT korunur.
