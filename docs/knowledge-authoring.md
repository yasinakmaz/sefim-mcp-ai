# Knowledge Authoring

Yeni bir tabloyu AI'ya öğretmek için:

1. SQL erişimi varsa `dotnet run --project sefim-ai-mcp -- knowledge generate-table-doc dbo.X` komutunu çalıştırın.
2. Oluşan `knowledge/private/tables/dbo.X.md` dosyasında sadece `<!-- USER: ... -->` alanlarını doldurun.
3. Uygulama özeti için `knowledge/templates/application-overview.template.md` dosyasını `knowledge/private/application/overview.md` olarak kopyalayın ve aynı alanları doldurun.
4. Workflow ve kural için ilgili template'i `knowledge/private/workflows/` veya `knowledge/private/business-rules/` altına kopyalayın.
5. `dotnet run --project sefim-ai-mcp -- knowledge validate` çalıştırın.
6. `SEFIM_KNOWLEDGE_KEY` tanımlıyken `dotnet run --project sefim-ai-mcp -- knowledge pack` çalıştırın.

`status: draft` eksik semantic bilgi olduğunu modele açıkça bildirir. `exposure: private` pack'e girebilir ama runtime tarafından model-visible knowledge olarak dönmez; gerçek server-private policy ise Markdown'a dahi yazılmamalı, C# policy katmanında tutulmalıdır.
