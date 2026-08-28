# MCP Contract

Ana discovery tools: `get_application_overview`, `search_application_knowledge`, `list_documented_tables`, `describe_table`, `get_business_rules`, `get_workflow`. Bunların hepsi read-only, closed-world ve structured output desteklidir. Workflow ve rule tool'ları identifier ister; tüm bilgi havuzunu döndüren bir tool yoktur.

Resources tamamlayıcıdır: `sefim://overview` ve `sefim://schema/table/{schema}/{table}`. Farklı MCP client resource davranışları değişebileceği için discovery için tools esas alınır.

`AiSelectQuery` uyumluluk nedeniyle korunur. Tek SELECT/CTE SELECT kabul eder, birden fazla statement ve veri değiştiren token'ları reddeder, sonucu 200 satırla sınırlar. Bu kontrol ek savunmadır; deployment SQL hesabı ayrıca database seviyesinde read-only yetki ile sınırlandırılmalıdır.

Tool annotation'ları MCP client'a hint verir; authorization veya approval garantisi değildir. Domain mutation'lar server-side policy ile kontrol edilmelidir. `prepare_destructive_operation` kısa süreli, hedefe bağlı challenge üretmek için ortak guard altyapısıdır.
