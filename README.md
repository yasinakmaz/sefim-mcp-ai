# Şefim MCP Server

Şefim MCP Server, local Şefim POS uygulamasını MCP istemcilerine stdio üzerinden bağlar. Uygulama ve MSSQL aynı müşteri ortamında kalır; HTTP transport veya merkezi veritabanı kullanılmaz.

## Kurulum

SQL bağlantısını kaynak koduna koymayın. MCP host process'ine aşağıdaki environment variable'ı verin:

```bash
export SEFIM_SQL_CONNECTION_STRING='Server=localhost;Database=sefimm;Integrated Security=true;Encrypt=true;TrustServerCertificate=true'
```

Knowledge pack kullanılıyorsa aynı host process'inde `SEFIM_KNOWLEDGE_KEY` (base64 encoded 32-byte AES key) tanımlı olmalıdır.

```bash
dotnet restore
dotnet build
dotnet run --project sefim-ai-mcp
```

MCP server stdout'u yalnızca JSON-RPC stdio transport içindir. Tanı logları stderr'e gider.

## Knowledge

Detaylı authoring akışı için [knowledge/README.md](knowledge/README.md) ve [docs/knowledge-authoring.md](docs/knowledge-authoring.md) dosyalarına bakın.
