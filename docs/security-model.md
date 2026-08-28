# Security Model

| Boundary | Trust | Rule |
| --- | --- | --- |
| User | Untrusted input | User intent doğrulanır; destructive action için host/UI confirmation gerekir. |
| AI host / MCP client | Partially trusted | Tool annotations ve instructions security boundary değildir. |
| Local MCP server | Trusted enforcement point | Policy, limits, DTO redaction ve allowlist burada uygulanır. |
| Knowledge pack | Confidential at rest only | AES-GCM tamper detection sağlar; key process dışından sağlanır. |
| Local MSSQL | Trusted data store, untrusted text fields | Least privilege SQL account kullanılır; DB text asla instruction kabul edilmez. |
| Operating system | Deployment trust boundary | Key OS secret store/environment ile process'e verilir; erişimi olan kullanıcı process memory'yi inceleyebilir. |

Release artifact plaintext private Markdown taşımaz. Bununla birlikte uygulama runtime'da decrypt edebiliyorsa local-machine privilege, process inspection veya reverse engineering karşısında mutlak gizlilik garanti edilemez.

Windows'ta deployment key'i DPAPI ile korunan installer/service secret store üzerinden environment'a aktarılabilir. Cross-platform, Native AOT uyumlu tek bir OS secret-store API'si BCL içinde yoktur; `IKnowledgeKeyProvider` bu yüzden abstraction'dır. Uygulama key'i binary, repository veya appsettings içine koymaz.

MSSQL'den gelen `ProductName`, `CustomerName`, `Description`, `Note` ve benzeri alanlar potentially untrusted application data'dır. Server instruction, policy veya executable komut sayılmaz.
