# Şefim MCP Server

Şefim MCP Server, local Şefim POS uygulamasını MCP istemcilerine stdio üzerinden bağlar. Uygulama ve MSSQL aynı müşteri ortamında kalır; HTTP transport veya merkezi veritabanı kullanılmaz.

## Kurulum

SQL bağlantısı `appsettings.json` içindeki `SqlService:ConnectionString` alanından okunur. Dosya executable ile aynı klasörde durmalıdır:

```json
{
  "SqlService": {
    "ConnectionString": "Server=localhost;Database=sefimm;User Id=sa;Password=***;TrustServerCertificate=True;Encrypt=True;Connection Timeout=30;"
  }
}
```

Knowledge pack key'i binary'ye gömülüdür; ek bir environment değişkeni gerekmez.

```bash
dotnet restore
dotnet build
dotnet run --project sefim-ai-mcp
```

MCP server stdout'u yalnızca JSON-RPC stdio transport içindir. Tanı logları stderr'e gider. Açılışta stderr'e knowledge pack durumu (belge ve section sayısı) ve aktif tool profili yazılır; SQL yapılandırması eksikse server açıklayıcı bir mesajla `78` exit code ile durur.

## Environment

| Değişken | Zorunlu | Açıklama |
| --- | --- | --- |
| `SEFIM_KNOWLEDGE_KEY` | hayır | Base64 encoded 32-byte AES key. Yalnızca gömülü key'i geçersiz kılmak için; pack de aynı key ile üretilmelidir. |
| `SEFIM_TOOL_PROFILE` | hayır | `full` (varsayılan) veya `core`. |

`appsettings.json` publish çıktısına kopyalanır ve `SqlService:ConnectionString` değerini oradan alır. Windows kurulumunda bu dosyayı setup üretir (bkz. [installer/README.md](installer/README.md)). `appsettings.Local.json` yayınlanmaz.

## Tool profile

`full` profilde 182 tool yayınlanır ve her istekte yaklaşık 23k token tool tanımı gönderilir. Yalnızca soru cevaplama ve raporlama gerekiyorsa:

```bash
export SEFIM_TOOL_PROFILE=core
```

`core` profilde 21 tool kalır (knowledge, report, operation guard) ve tool tanımı maliyeti yaklaşık 3.6k token'a iner. Yazma tool'ları bu profilde yayınlanmaz.

## Knowledge

```bash
dotnet run --project sefim-ai-mcp -- knowledge validate
dotnet run --project sefim-ai-mcp -- knowledge stats
dotnet run --project sefim-ai-mcp -- knowledge pack
```

Detaylı authoring akışı için [knowledge/README.md](knowledge/README.md) ve [docs/knowledge-authoring.md](docs/knowledge-authoring.md) dosyalarına bakın.

## Release

```bash
dotnet test
dotnet publish sefim-ai-mcp/sefim-ai-mcp.csproj -c Release -r linux-x64
scripts/verify-publish-knowledge.sh sefim-ai-mcp/bin/Release/net10.0/linux-x64/publish
```

Publish, `knowledge.pack` yoksa (`SEFIM002`) veya çıktıya plaintext Markdown girerse (`SEFIM003`) durur. Bilinçli olarak pack'siz publish almak için `-p:AllowMissingKnowledgePack=true`.

## Dokümanlar

- [docs/mcp-contract.md](docs/mcp-contract.md) - tool ve resource yüzeyi
- [docs/architecture.md](docs/architecture.md) - katmanlar, retrieval ve report akışı
- [docs/security-model.md](docs/security-model.md) - trust boundary'ler, pack formatı, build guard'ları
- [docs/knowledge-authoring.md](docs/knowledge-authoring.md) - belge yazımı
