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
