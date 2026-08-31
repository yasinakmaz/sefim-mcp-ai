# MCP Contract

Ana discovery tools: `search_application_knowledge`, `list_knowledge_documents`, `get_knowledge_document`, `lookup_glossary_terms`, `get_application_overview`, `list_documented_tables`, `describe_table`, `get_business_rules`, `get_workflow`. Bunların hepsi read-only, closed-world ve structured output desteklidir. Workflow ve rule tool'ları identifier ister; tüm bilgi havuzunu döndüren bir tool yoktur.

Beklenen kullanım sırası:

1. `search_application_knowledge` ile ilgili belge ve section başlığını bul. Arama Türkçe-duyarlıdır: `urun`, `Ürün` ve `ürünün` aynı belgeye gider.
2. `get_knowledge_document` ile yalnızca gereken belgeyi veya section'ı oku.
3. Anlaşılmayan Türkçe iş terimi için `lookup_glossary_terms`.

Resources tamamlayıcıdır: `sefim://overview`, `sefim://schema/table/{schema}/{table}`, `sefim://knowledge/{kind}/{id}` ve `sefim://knowledge/status`. Farklı MCP client resource davranışları değişebileceği için discovery için tools esas alınır.

`AiSelectQuery` ad-hoc sorgular için korunur. Tek SELECT/CTE SELECT kabul eder; birden fazla statement, veri değiştiren token'lar, `sys.*` / `INFORMATION_SCHEMA` / cross-database erişim ve credential adı taşıyan identifier'lar `ReadOnlySqlGuard` tarafından reddedilir, sonuç 200 satırla sınırlanır. Bu kontrol ek savunmadır; deployment SQL hesabı ayrıca database seviyesinde read-only yetki ile sınırlandırılmalıdır.

## Tool profile

Full profilde server 182 tool yayınlar; bu her istekte yaklaşık 23k token tool tanımı demektir. Yalnızca soru cevaplama ve raporlama gerekiyorsa `SEFIM_TOOL_PROFILE=core` (veya `Mcp:ToolProfile=core`) ile başlatın: knowledge, report ve operation tool'ları kalır, tool sayısı 21'e ve tool tanımı yaklaşık 3.6k token'a iner. Core profilde stock/customer/user/transaction/table-group/campaign yazma tool'ları yayınlanmaz.

Tool annotation'ları MCP client'a hint verir; authorization veya approval garantisi değildir. Domain mutation'lar server-side policy ile kontrol edilmelidir. `prepare_destructive_operation` kısa süreli, hedefe bağlı challenge üretmek için ortak guard altyapısıdır; challenge'lar iki dakika sonra düşer ve her kullanımda süresi geçmiş kayıtlar temizlenir.
