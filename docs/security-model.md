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

## Pack formatı

`knowledge.pack` düzeni: `SEFIMKP1` magic + 12 byte nonce + AES-256-GCM ciphertext + 16 byte tag. Magic AAD olarak kullanılır, yani header değiştirilirse decrypt başarısız olur. Key `SEFIM_KNOWLEDGE_KEY` (base64, 32 byte) environment değişkeninden gelir ve kullanımdan sonra `CryptographicOperations.ZeroMemory` ile temizlenir.

Pack, belgelerin `kind|id|body` değerlerinden hesaplanmış bir SHA-256 fingerprint taşır (`SourceFingerprint`, ilk 16 hex karakter). Aynı fingerprint aynı içerik demektir; hangi pack'in hangi kaynaktan üretildiği bu değerle takip edilir. `knowledge stats` bu değeri yazdırır.

## Build-time garantiler

Plaintext knowledge'ın artifact'e sızmasına karşı üç MSBuild target vardır:

| Kod | Target | Ne yapar |
| --- | --- | --- |
| `SEFIM001` | `VerifyKnowledgeTemplatesAreBlank` | `knowledge/templates/*.md` içinde iskelet dışı içerik varsa `Pack` ve `Publish` durur. Template'ler pakete düz metin girdiği için gerçek iş bilgisi buraya yazılamaz. |
| `SEFIM002` | `VerifyKnowledgePackPresent` | `knowledge.pack` yoksa publish durur; bilgi taşımayan bir release engellenir. Bilinçli atlamak için `-p:AllowMissingKnowledgePack=true`. |
| `SEFIM003` | `VerifyNoPlaintextKnowledgeInPublish` | Publish dosya listesine `.md` veya `knowledge/private` girerse build kırılır. |

`scripts/verify-publish-knowledge.sh` aynı kuralları build'den bağımsız olarak publish çıktısı üzerinde doğrular: markdown yokluğu, `SEFIMKP1` header, pack içinde okunabilir iş kelimesi olmaması, `appsettings.Local.json` yayınlanmamış olması ve published json içinde credential bulunmaması. CI'da veya imzalamadan önce çalıştırın.

## SQL guard

`ReadOnlySqlGuard` hem `AiSelectQuery` hem documented report SQL'i için tek doğrulama noktasıdır. Reddedilenler: birden fazla statement, yorum içine gizlenmiş ikinci ifade, veri değiştiren token'lar (`INSERT`, `UPDATE`, `DELETE`, `MERGE`, `EXEC`, DDL), `sys.*` ve `INFORMATION_SCHEMA` erişimi, cross-database referans ve adında `PASSWORD`, `SECRET`, `APIKEY`, `TOKEN`, `CREDENTIAL`, `CONNECTIONSTRING` geçen identifier'lar. Guard ek savunmadır; deployment SQL hesabı ayrıca database seviyesinde read-only yetkilendirilmelidir.

## Destructive operation guard

`OperationGuard` hedefe bağlı, iki dakika ömürlü challenge üretir. Aynı challenge ikinci kez kullanılamaz, farklı hedefe uygulanamaz ve bekleyen kayıt tablosu 256 girdiyle sınırlıdır; her `Prepare`/`Consume` çağrısında süresi geçmiş kayıtlar temizlenir. Bu, model tarafının yanlışlıkla zincirlediği bir silme işlemine karşı server-side frendir, kullanıcı onayının yerine geçmez.
