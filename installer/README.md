# Şefim MCP Server – kurulum paketi

Qt Installer Framework (QtIFW) tabanlı, çapraz platform setup. Sunucu ikilisini
kullanıcı bazlı (admin/root gerektirmeyen) bir klasöre kurar, sihirbazda girilen
bilgilerle `appsettings.json` dosyasını üretir ve seçilen yapay zekâ istemcisinin
yapılandırmasına MCP kaydını ekler. Windows, Linux ve macOS için ayrı ayrı üretilir;
her platformun setup'ı yalnızca o platformda çalışır.

## Kurulum yeri

Kurulum her zaman kullanıcı bazlıdır (`@ApplicationsDirUser@`), sistem geneline
kurulmaz:

| OS | Kurulum kökü |
| --- | --- |
| Windows | `%LOCALAPPDATA%\OZFILIZYAZILIM\SefimMcp\{SÜRÜM}` |
| Linux | `~/.local/share/OZFILIZYAZILIM/SefimMcp/{SÜRÜM}` |
| macOS | `~/Library/Application Support/OZFILIZYAZILIM/SefimMcp/{SÜRÜM}` |

(`SefimMcp.Setup.SetupPaths.InstallRoot`, `.NET`'in `SpecialFolder.LocalApplicationData`
karşılığını kullanır; QtIFW tarafında bu `TargetDir = @ApplicationsDirUser@/SefimMcp/@ProductVersion@`
olarak `installer/qtifw/config/config.xml` içinde tanımlıdır.)

## Kurulum akışı

Sihirbaz iki ek sayfa ekler (`ClientPage.ui`, `SefimPage.ui`), ardından paket
dosyalarını çıkarır ve `sefim-ai-mcp setup configure` komutunu çalıştırır:

| Adım | Davranış |
| --- | --- |
| İstemci seçimi (ClientPage) | Claude Desktop (varsayılan) veya ChatGPT Desktop seçilir. Tespit yapılmaz; kullanıcı elle seçer. |
| Şefim/DB sayfası (SefimPage) | Şefim kurulum klasörü **opsiyonel**; boşsa OS'e göre varsayılan tahmin (`installscript.qs`'teki `defaultSefimDir`/`defaultImagesDir`) alana yazılır, kullanıcı değiştirebilir. |
| Görseller klasörü | Ayrı bir alan (`imagesEdit`), opsiyonel. Varsayılanlar: Windows `C:/Program Files (x86)/Vega/Sefim/proimages`, macOS `/Volumes/Sefim/proimages`, Linux `/mnt/proimages`. |
| Sunucu / Veritabanı | **Zorunlu**. İkisi de doluymadan sihirbaz bir sonraki adıma geçmez (`validateSefimPage`). |
| Kullanıcı / Parola | Opsiyonel. Boş bırakılırsa `SefimDetection.BuildConnectionString` bağlantı dizesini `Integrated Security=True;` ile üretir; doluysa `User Id=...;Password=...;` kullanılır. |
| Dosya çıkarma | QtIFW payload'ı `TargetDir` altına açar. |
| Yapılandırma | `component.createOperations()`'dan sonra `Execute` operasyonu ile `<TargetDir>/sefim-ai-mcp[.exe] setup configure ...` çalışır; hata `{0,1}` toleransıyla non-fatal'dır — kurulum yarıda kesilmez, yalnızca yapılandırma başarısız olur. |

Üretilen bağlantı dizesi sabit biçimlidir:

```
Server=<Sunucu>;Database=<Veritabanı>;User Id=<Kullanıcı>;Password=<Parola>;TrustServerCertificate=True;Encrypt=True;Connection Timeout=30;Max Pool Size=100;Min Pool Size=10;MultipleActiveResultSets=True;Application Name=VeposTransferCenterAPI;Language=Turkish;
```

(Kullanıcı/parola boşsa `User Id=...;Password=...;` yerine `Integrated Security=True;` kullanılır.)

## `setup` CLI alt komutları

Cross-platform mantığın tamamı artık `sefim-ai-mcp` ikilisinin içinde
(`sefim-ai-mcp/Setup/SetupCommandRunner.cs`), ayrı bir PowerShell betiği yok.
`Program.cs`, `args[0] == "setup"` olduğunda normal MCP sunucu başlatmadan önce
bu komutlara yönlenir:

| Komut | İşlev |
| --- | --- |
| `setup detect` | Şefim klasörünü, `proimages` yolunu, `connectionstring.txt`'ten okunan bağlantı bilgilerini ve istemci config yollarını `anahtar=değer` satırları olarak yazar. |
| `setup test --server S --database D [--user-id U] [--password P]` | Verilen bilgilerle SQL bağlantısını dener (`SqlConnectionTester`), `status=ok|error` döner. |
| `setup configure --install-dir DIR --server S --database D [--user-id U] [--password P] [--sefim-dir DIR] [--pro-images DIR] [--host claude\|chatgpt] [--host-config PATH] [--tool-profile PROFILE] [--server-key KEY]` | `appsettings.json`'ı üretir/günceller ve seçilen istemcinin config dosyasına MCP kaydını ekler (diğer anahtarlar korunur). |
| `setup remove [--host claude\|chatgpt] [--server-key KEY]` | Yalnızca ilgili Şefim MCP kaydını istemci config dosyalarından (ve varsa Codex `config.toml`'dan) siler. |

`installscript.qs`, `createOperations()` içinde bu komutu şu şekilde çağırır:

```
@TargetDir@/sefim-ai-mcp setup configure --install-dir @TargetDir@ --host <claude|chatgpt>
  --sefim-dir <SefimPage.sefimDirEdit> --pro-images <SefimPage.imagesEdit>
  --server <SefimPage.serverEdit> --database <SefimPage.databaseEdit>
  --tool-profile full --server-key sefim
  [--user-id <SefimPage.userEdit>] [--password <SefimPage.passwordEdit>]
```

## İstemci yapılandırma dosyaları

`SefimMcp.Setup.SetupPaths` her OS için ayrı yollar döner:

| İstemci | Windows | macOS | Linux |
| --- | --- | --- | --- |
| Claude Desktop | `%APPDATA%\Claude\claude_desktop_config.json` | `~/Library/Application Support/Claude/claude_desktop_config.json` | `~/.config/Claude/claude_desktop_config.json` |
| ChatGPT Desktop | `%APPDATA%\ChatGPT\mcp_config.json` | `~/Library/Application Support/ChatGPT/mcp_config.json` | `~/.config/ChatGPT/mcp_config.json` |
| Codex CLI | `%USERPROFILE%\.codex\config.toml` (yalnızca dosya zaten varsa güncellenir, tüm platformlarda aynı `~/.codex/config.toml` mantığı) | | |

Yazma davranışı `ClientConfigWriter`/`AppSettingsWriter` tarafındandır:

* Dosya yoksa oluşturulur, varsa önce `*.sefim-backup-<zaman>` yedeği alınır.
* `mcpServers` dışındaki tüm anahtarlar ve diğer MCP sunucuları olduğu gibi kalır.
* `command` alanı `SEFIM-MCP` veya `sefim-ai-mcp` içeren, farklı bir anahtar altındaki eski sürüm
  kayıtları otomatik silinir; böylece istemci eski (kaldırılmış) sürüm klasöründeki ikiliyi
  başlatmaya devam etmez.
* `setup remove`, yalnızca ilgili `--server-key` kaydını siler; dosyanın geri kalanına dokunmaz.

## Yerel derleme

Sunucu self-contained yayınlandığı için payload, hedef platformda (veya o platform
için cross-publish destekleyen bir ortamda) üretilmelidir; installer ise
**binarycreator'ın çalıştığı OS için** üretilir (QtIFW cross-platform paket üretmez).

```bash
dotnet publish sefim-ai-mcp/sefim-ai-mcp.csproj -c Release -r <rid> --self-contained true \
  -o artifacts/<rid>
installer/qtifw/build-installer.sh artifacts/<rid> <version> <os-arch-label>
```

Örnek (Linux x64):

```bash
dotnet publish sefim-ai-mcp/sefim-ai-mcp.csproj -c Release -r linux-x64 --self-contained true \
  -o artifacts/linux-x64
installer/qtifw/build-installer.sh artifacts/linux-x64 0.1.0-beta linux-x64
```

`<rid>` seçenekleri: `win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `osx-x64`, `osx-arm64`.

`build-installer.sh`:

1. `binarycreator` bulunamazsa `aqtinstall` ile `.qtifw-tools/` altına indirir.
2. `config.xml` ve `package.xml` içindeki `<Version>` alanını verilen sürümle günceller.
3. Payload'ı paketin `data/` klasörüne kopyalar; payload altında `*.md` bulursa
   (şifrelenmemiş bilgi tabanı sızıntısı ihtimaline karşı) derlemeyi reddeder.
4. `binarycreator --offline-only` ile `SefimMcpSetup-<version>-<label>.<ext>` üretir
   (`.run` Linux'ta, `.app` macOS'ta, `.exe` Windows'ta).

İş bilgisi setup'a `sefim-ai-mcp/knowledge.pack` üzerinden girer. Paket AES-256-GCM ile
şifrelenir ve anahtar sunucu binary'sine gömülüdür (`EmbeddedKnowledgeKeyProvider`), bu
yüzden kurulum anahtar sormaz. `knowledge/private` altındaki belgeler değiştiğinde paket
yeniden üretilip commit edilmelidir:

```bash
dotnet run --project sefim-ai-mcp -- knowledge pack --output sefim-ai-mcp/knowledge.pack
```

Paket depoda yoksa `-p:AllowMissingKnowledgePack=true` ile iş bilgisi içermeyen bir
payload üretilebilir; aksi halde `dotnet publish` bunu reddeder.

## Release iş akışı

`.github/workflows/release.yml`, `v*` tag push'ında (veya elle `workflow_dispatch` ile)
şu matrisi çalıştırır ve altı setup'ı da GitHub Release'e ekler:

| OS | RID | Etiket |
| --- | --- | --- |
| windows-latest | win-x64 | win-x64 |
| windows-latest | win-arm64 | win-arm64 |
| ubuntu-latest | linux-x64 | linux-x64 |
| ubuntu-24.04-arm | linux-arm64 | linux-arm64 |
| macos-13 | osx-x64 | macos-x64 |
| macos-14 | osx-arm64 | macos-arm64 |

Her matris kolu: sürümü çözer (tag adı, `workflow_dispatch` girişi veya
`PackageVersion`'dan), `knowledge.pack`'in varlığını/`SEFIMKP1` başlığını kontrol eder,
`dotnet publish` ile payload'ı üretir, payload'ı doğrular (`*.exe`/`sefim-ai-mcp`,
`appsettings.json` var mı, `*.md` sızıntısı yok mu, pack varsa şifreli mi), QtIFW
araçlarını `aqtinstall` ile kurar, `build-installer.sh`'ı çalıştırır ve setup'ı artifact
olarak yükler. `linux-x64` kolunda ayrıca `dotnet test` çalışır. Son `release` job'u,
yalnızca `v*` tag push'ında, tüm artifact'leri indirip GitHub Release'e ekler:

```bash
git tag v0.1.0-beta && git push origin v0.1.0-beta
# veya: Actions > Release installers > Run workflow
```

## Dosyalar

| Dosya | İşlev |
| --- | --- |
| `qtifw/config/config.xml` | Setup meta bilgisi (isim, sürüm, ikonlar), `TargetDir` (`@ApplicationsDirUser@/SefimMcp/@ProductVersion@`), kurulum sonrası doğrulama komutu (`sefim-ai-mcp --version`) |
| `qtifw/packages/com.ozfiliz.sefimmcp/meta/package.xml` | Paket meta bilgisi, sihirbaz sayfalarının (`ClientPage.ui`, `SefimPage.ui`) ve `installscript.qs`'nin bağlanması |
| `qtifw/packages/com.ozfiliz.sefimmcp/meta/installscript.qs` | Sihirbaz sayfa mantığı (varsayılan yol tahminleri, doğrulama, Gözat düğmeleri) ve kurulum sonunda `setup configure` çağrısını üreten `createOperations()` |
| `qtifw/packages/com.ozfiliz.sefimmcp/meta/ClientPage.ui` | İstemci seçim sayfası (Claude Desktop / ChatGPT Desktop) |
| `qtifw/packages/com.ozfiliz.sefimmcp/meta/SefimPage.ui` | Şefim klasörü, görseller klasörü, Sunucu/Veritabanı (zorunlu), Kullanıcı/Parola (opsiyonel) alanları |
| `qtifw/assets/generate-assets.py` | Sihirbaz görsellerini ve ikonları üretir |
| `qtifw/assets/installer.ico` / `installer.icns` / `installer.png` / `logo.png` | Platforma göre kullanılan ikon/logo dosyaları |
| `qtifw/build-installer.sh` | Yerel/CI `binarycreator` derlemesi (bkz. Yerel derleme) |

Sunucu tarafındaki karşılıkları:

| Dosya | İşlev |
| --- | --- |
| `sefim-ai-mcp/Setup/SetupCommandRunner.cs` | `setup detect\|test\|configure\|remove` CLI komutları |
| `sefim-ai-mcp/Setup/SetupPaths.cs` | Platforma göre kurulum kökü, Şefim/proimages tahminleri, istemci config yolları |
| `sefim-ai-mcp/Setup/SefimDetection.cs` | `connectionstring.txt` ayrıştırma ve bağlantı dizesi üretimi |
