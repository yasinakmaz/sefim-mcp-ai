# Şefim MCP Server – Windows kurulum paketi

NSIS tabanlı setup. Sunucu ikilisini `Program Files (x86)\OZFILIZYAZILIM\SEFIM-MCP\{SÜRÜM}\`
altına kurar, Şefim kurulumunu bulur, `appsettings.json` dosyasını üretir ve seçilen
yapay zekâ istemcisinin yapılandırmasına MCP kaydını ekler.

## Kurulum akışı

| Adım | Davranış |
| --- | --- |
| Sistem kontrolü | Windows 10 altı sürümlerde ve 32 bit Windows'ta kurulum başlamaz. |
| İstemci kontrolü | Yapılmaz. Claude Desktop ve ChatGPT Desktop, okunamayan `WindowsApps` klasörüne kurulabildiği için varlıkları güvenilir biçimde saptanamaz; kurulum her durumda başlar. |
| İstemci seçimi | Her iki istemci de seçilebilir. Tespit edilebilen istemci yalnızca varsayılan seçimi belirler. |
| Hedef klasör | Varsayılan `C:\Program Files (x86)\OZFILIZYAZILIM\SEFIM-MCP\{SÜRÜM}` |
| Şefim tespiti | `C:\Program Files (x86)\Vega\Sefim` başta olmak üzere bilinen konumlar taranır. |
| Bağlantı bilgisi | Şefim klasöründeki `connectionstring.txt` dosyasından yalnızca `Data Source`, `Initial Catalog`, `User ID`, `Password` alanları okunur. |
| Görseller | Şefim klasöründe `proimages` varsa `SEFIM:ImageLocation` olarak yazılır. |
| Yapılandırma | `appsettings.json` ve istemci yapılandırma dosyası yazılır; her ikisinde de diğer anahtarlar korunur. |

Kurulumda üretilen bağlantı dizesi sabit biçimlidir:

```
Server=<Data Source>;Database=<Initial Catalog>;User Id=<User ID>;Password=<Password>;TrustServerCertificate=True;Encrypt=True;Connection Timeout=30;Max Pool Size=100;Min Pool Size=10;MultipleActiveResultSets=True;Application Name=VeposTransferCenterAPI;Language=Turkish;
```

## İstemci yapılandırma dosyaları

| İstemci | Dosya |
| --- | --- |
| Claude Desktop | `%APPDATA%\Claude\claude_desktop_config.json` |
| ChatGPT Desktop | `%APPDATA%\ChatGPT\mcp_config.json` (varsa `%APPDATA%\OpenAI\ChatGPT\...` tercih edilir) |
| Codex CLI | `%USERPROFILE%\.codex\config.toml` (yalnızca dosya zaten varsa güncellenir) |

Yazma davranışı:

* Dosya yoksa oluşturulur, varsa önce `*.sefim-backup-<zaman>` yedeği alınır.
* `mcpServers` dışındaki tüm anahtarlar ve diğer MCP sunucuları olduğu gibi kalır.
* Eski bir sürüm klasörünü gösteren Şefim kaydı bulunursa silinir ve yerine yeni sürüm yazılır.
* Kaldırma sırasında yalnızca Şefim kaydı silinir.

> ChatGPT Desktop, yerel stdio MCP sunucuları için belgelenmiş tek bir yapılandırma
> dosyası yayınlamıyor. Kurulum bilinen konumları sırayla dener ve hiçbiri yoksa
> `%APPDATA%\ChatGPT\mcp_config.json` dosyasını oluşturur. Uygulama tarafında
> geliştirici modunun açık olması gerekebilir.

## Derleme

Sunucu native AOT ile yayınlandığı için payload **Windows üzerinde** üretilmelidir.

İş bilgisi setup'a `sefim-ai-mcp/knowledge.pack` üzerinden girer. Dosya AES-256-GCM
ile şifrelendiği ve `SEFIM_KNOWLEDGE_KEY` depoya hiç girmediği için sürüm kontrolünde
tutulur; iş akışı paketi doğrudan oradan alır. `knowledge/private` altındaki belgeler
değiştiğinde paket yeniden üretilip commit edilmelidir:

```bash
SEFIM_KNOWLEDGE_KEY="..." dotnet run --project sefim-ai-mcp -- knowledge pack \
  --output sefim-ai-mcp/knowledge.pack
```

Paket depoda yoksa iş akışı uyarı verip iş bilgisi içermeyen bir setup üretir.
Anahtar kurulum sihirbazındaki "Knowledge key" alanına girilir ve istemci
yapılandırmasına `SEFIM_KNOWLEDGE_KEY` olarak yazılır.

GitHub Actions (önerilen):

```bash
git tag v0.1.0-beta && git push origin v0.1.0-beta
# veya: Actions > Windows installer > Run workflow
```

Windows'ta elle:

```powershell
dotnet publish sefim-ai-mcp\sefim-ai-mcp.csproj -c Release -r win-x64 --self-contained true `
  -p:AllowMissingKnowledgePack=true -o artifacts\win-x64
makensis /INPUTCHARSET UTF8 /DVERSION=0.1.0-beta /DVERSION_NUMERIC=0.1.0.0 /DPAYLOAD_DIR=..\artifacts\win-x64 installer\sefim-mcp.nsi
```

Linux'ta (payload Windows'tan kopyalandıysa):

```bash
installer/build-installer.sh artifacts/win-x64 0.1.0-beta
```

> Debian/Ubuntu paketindeki `makensis` 3.10 ikilisi, kendi başlık dosyalarındaki
> `GetWinVer ... Product` alanını tanımıyor. Yalnızca bu ortamda derlemek için
> `-DWinVer_v3_7` tanımı eklenebilir; GitHub Actions'taki Windows derlemesinde
> gerekmez.

## Testler

`installer/tests/Test-Configure.ps1`, sentetik bir kurulum üzerinde bağlantı dizesi
ayrıştırmasını, `appsettings.json` üretimini ve istemci JSON birleştirmesini doğrular.
CI'da her derlemede çalışır; elle çalıştırmak için Windows PowerShell 5.1 gerekir:

```powershell
.\installer\tests\Test-Configure.ps1
```

## Dosyalar

| Dosya | İşlev |
| --- | --- |
| `sefim-mcp.nsi` | Setup betiği, sihirbaz sayfaları, kayıt defteri ve kaldırma (UTF-8 BOM ile saklanır; BOM'suz derlemede Windows üzerindeki `makensis` kaynağı ANSI sayar ve Türkçe karakterler bozulur) |
| `scripts/Configure-SefimMcp.ps1` | Tespit, connectionstring.txt ayrıştırma, SQL testi, JSON birleştirme |
| `tests/Test-Configure.ps1` | Yapılandırma yardımcısının doğrulama testleri |
| `assets/generate-assets.py` | Sihirbaz görsellerini ve ikonları üretir |
| `build-installer.sh` | Yerel makensis derlemesi |
