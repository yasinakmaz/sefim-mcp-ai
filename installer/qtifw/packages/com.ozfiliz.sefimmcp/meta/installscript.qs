// QtIFW's own 4.7 docs: "@ApplicationsDirUser@ ... is useful on macOS, on other platforms it
// is the same as @ApplicationsDir@" — i.e. /opt on Linux and C:\Program Files on Windows, both
// of which need root/admin and make a headless install abort with "Cannot elevate access
// rights". This installer is per-user by design, so resolve a genuinely per-user root instead.
function defaultTargetDir()
{
    var version = installer.value("ProductVersion");
    if (systemInfo.kernelType === "winnt")
        return installer.value("HomeDir") + "/AppData/Local/Programs/SefimMcp/" + version;
    if (systemInfo.kernelType === "darwin")
        return installer.value("HomeDir") + "/Applications/SefimMcp/" + version;
    return installer.value("HomeDir") + "/.local/share/SefimMcp/" + version;
}

function Component()
{
    // Only while installing: in maintenance/uninstall mode TargetDir must stay whatever the
    // original install recorded. In CLI mode, keep --root intact for unattended tests.
    if (installer.isInstaller()) {
        if (!installer.isCommandLineInstance())
            installer.setValue("TargetDir", defaultTargetDir());

        installer.setDefaultPageVisible(QInstaller.ComponentSelection, false);
        installer.setDefaultPageVisible(QInstaller.ReadyForInstallation, false);
        if (systemInfo.kernelType === "winnt")
            installer.setDefaultPageVisible(QInstaller.StartMenuSelection, false);
    }

    component.hostChoice = "claude";
    component.proImagesPath = defaultImagesDir();

    installer.installationFinished.connect(removeInstallerMetadata);
    installer.installationFinished.connect(scheduleInstallerMetadataCleanup);
    installer.finishButtonClicked.connect(removeInstallerMetadata);

    installer.addWizardPage(component, "ClientPage", QInstaller.TargetDirectory);
    installer.addWizardPage(component, "SefimPage", QInstaller.PerformInstallation);
}

function removeInstallerMetadata()
{
    if (!installer.isInstaller())
        return;

    var targetDir = installer.value("TargetDir");
    var maintenanceToolName = installer.value("MaintenanceToolName");
    var maintenanceTool = targetDir + "/" + maintenanceToolName;
    var packageName = component.name;
    var version = installer.value("ProductVersion");

    installer.performOperation("Delete", [targetDir + "/InstallationLog.txt"]);
    installer.performOperation("Delete", [targetDir + "/components.xml"]);
    installer.performOperation("Delete", [targetDir + "/network.xml"]);
    installer.performOperation("Delete", [targetDir + "/installer.dat"]);
    installer.performOperation("Delete", [maintenanceTool]);
    installer.performOperation("Delete", [maintenanceTool + ".exe"]);
    installer.performOperation("Delete", [maintenanceTool + ".dat"]);
    installer.performOperation("Delete", [maintenanceTool + ".ini"]);
    installer.performOperation("Delete", [maintenanceTool + ".new"]);
    installer.performOperation("Delete", [targetDir + "/installerResources/" + packageName + "/" + version + "content.txt"]);
    installer.performOperation("Rmdir", [targetDir + "/installerResources/" + packageName]);
    installer.performOperation("Rmdir", [targetDir + "/installerResources"]);
}

function scheduleInstallerMetadataCleanup()
{
    if (!installer.isInstaller())
        return;

    var targetDir = installer.value("TargetDir");
    if (targetDir.length < 2)
        return;

    var maintenanceToolName = installer.value("MaintenanceToolName");
    var packageName = component.name;
    var version = installer.value("ProductVersion");

    if (systemInfo.kernelType === "winnt") {
        installer.executeDetached("powershell.exe", [
            "-NoProfile", "-ExecutionPolicy", "Bypass", "-Command",
            "Start-Sleep -Seconds 1; " +
            "$target=$args[0]; $mt=$args[1]; $pkg=$args[2]; $ver=$args[3]; " +
            "$paths=@(" +
            "(Join-Path $target 'InstallationLog.txt')," +
            "(Join-Path $target 'components.xml')," +
            "(Join-Path $target 'network.xml')," +
            "(Join-Path $target 'installer.dat')," +
            "(Join-Path $target $mt)," +
            "(Join-Path $target ($mt + '.exe'))," +
            "(Join-Path $target ($mt + '.dat'))," +
            "(Join-Path $target ($mt + '.ini'))," +
            "(Join-Path $target ($mt + '.new'))," +
            "(Join-Path $target 'installerResources')" +
            "); Remove-Item -LiteralPath $paths -Force -Recurse -ErrorAction SilentlyContinue",
            targetDir, maintenanceToolName, packageName, version
        ], targetDir);
        return;
    }

    installer.executeDetached("/bin/sh", [
        "-c",
        "sleep 1\n" +
        "target=$1\n" +
        "mt=$2\n" +
        "pkg=$3\n" +
        "ver=$4\n" +
        "rm -f \"$target/InstallationLog.txt\" \"$target/components.xml\" \"$target/network.xml\" \"$target/installer.dat\" \"$target/$mt\" \"$target/$mt.exe\" \"$target/$mt.dat\" \"$target/$mt.ini\" \"$target/$mt.new\" \"$target/installerResources/$pkg/${ver}content.txt\"\n" +
        "rm -rf \"$target/installerResources\"\n",
        "sh", targetDir, maintenanceToolName, packageName, version
    ], targetDir);
}

// Pre-fills OS-specific default guesses without touching the filesystem — the real
// scan (Detect) runs at install time via the C# 'setup detect' operation below, so a
// stale guess here is never authoritative, only a starting point for the user to edit.
function defaultSefimDir()
{
    if (systemInfo.kernelType === "winnt")
        return "C:/Program Files (x86)/Vega/Sefim";
    return "";
}

function defaultImagesDir()
{
    if (systemInfo.kernelType === "winnt")
        return "C:/Program Files (x86)/Vega/Sefim/proimages";
    if (systemInfo.kernelType === "darwin")
        return "/Volumes/Sefim/proimages";
    return "/mnt/proimages";
}

function hostConfigPath(host)
{
    var home = installer.value("HomeDir");
    if (host === "chatgpt") {
        if (systemInfo.kernelType === "winnt")
            return home + "/AppData/Roaming/ChatGPT/mcp_config.json";
        if (systemInfo.kernelType === "darwin")
            return home + "/Library/Application Support/ChatGPT/mcp_config.json";
        return home + "/.config/ChatGPT/mcp_config.json";
    }

    if (systemInfo.kernelType === "winnt")
        return home + "/AppData/Roaming/Claude/claude_desktop_config.json";
    if (systemInfo.kernelType === "darwin")
        return home + "/Library/Application Support/Claude/claude_desktop_config.json";
    return home + "/.config/Claude/claude_desktop_config.json";
}

function setClientState(label, host)
{
    var path = hostConfigPath(host);
    if (installer.fileExists(path))
        label.text = "Bulundu: " + path;
    else
        label.text = "Kurulu görünmüyor; kayıt yine de eklenir.";
}

function setHostChoice(page, host)
{
    component.hostChoice = host;
    page.claudeRadio.checked = (host === "claude");
    page.chatgptRadio.checked = (host === "chatgpt");
}

Component.prototype.DynamicClientPageCallback = function()
{
    var page = gui.pageWidgetByObjectName("DynamicClientPage");
    if (page == null)
        return;

    page.windowTitle = "Yapay zekâ istemcisi";
    setClientState(page.claudeStateLabel, "claude");
    setClientState(page.chatgptStateLabel, "chatgpt");

    if (installer.fileExists(hostConfigPath("chatgpt")) && !installer.fileExists(hostConfigPath("claude")))
        component.hostChoice = "chatgpt";

    if (component.clientPageWired !== true) {
        page.claudeRadio.clicked.connect(function() { setHostChoice(page, "claude"); });
        page.chatgptRadio.clicked.connect(function() { setHostChoice(page, "chatgpt"); });
        component.clientPageWired = true;
    }

    setHostChoice(page, component.hostChoice);
}

function escapeRegExp(value)
{
    return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

function parseConnectionField(text, names)
{
    for (var i = 0; i < names.length; i++) {
        var pattern = new RegExp("(?:^|[;\\r\\n\\\"'<>])\\s*" + escapeRegExp(names[i]) + "\\s*=\\s*([^;\\r\\n\\\"']*)", "im");
        var match = pattern.exec(text);
        if (match != null && match.length > 1) {
            var value = match[1].replace(/^\\s+|\\s+$/g, "");
            if (value.length > 0)
                return value;
        }
    }
    return "";
}

function findConnectionStringFile(sefimDir)
{
    if (sefimDir.length === 0)
        return "";

    var candidates = [
        sefimDir + "/connectionstring.txt",
        sefimDir + "/ConnectionString.txt",
        sefimDir + "/Connectionstring.txt",
        sefimDir + "/connectionstrings.txt"
    ];
    for (var i = 0; i < candidates.length; i++) {
        if (installer.fileExists(candidates[i]))
            return candidates[i];
    }
    return "";
}

function findProImagesDir(sefimDir)
{
    var candidates = [];
    if (sefimDir.length > 0)
        candidates.push(sefimDir + "/proimages");
    candidates.push(defaultImagesDir());

    for (var i = 0; i < candidates.length; i++) {
        if (installer.fileExists(candidates[i]))
            return candidates[i];
    }
    return defaultImagesDir();
}

function detectSefimPageFields(page)
{
    var sefimDir = page.sefimDirEdit.text;
    var csFile = findConnectionStringFile(sefimDir);
    component.proImagesPath = findProImagesDir(sefimDir);

    if (csFile.length === 0) {
        page.sefimStateLabel.text = "connectionstring.txt bulunamadı; bağlantı bilgilerini elle girin.";
        validateSefimPage(page);
        return;
    }

    var text = installer.readFile(csFile, "UTF-8");
    var server = parseConnectionField(text, ["Data Source", "Server", "Address", "Addr", "Network Address"]);
    var database = parseConnectionField(text, ["Initial Catalog", "Database"]);
    var userId = parseConnectionField(text, ["User ID", "User Id", "UserId", "Uid"]);
    var password = parseConnectionField(text, ["Password", "Pwd"]);

    if (server.length > 0)
        page.serverEdit.text = server;
    if (database.length > 0)
        page.databaseEdit.text = database;
    if (userId.length > 0)
        page.userEdit.text = userId;
    if (password.length > 0)
        page.passwordEdit.text = password;

    if (component.proImagesPath.length > 0)
        page.sefimStateLabel.text = "connectionstring.txt okundu. Görseller: " + component.proImagesPath;
    else
        page.sefimStateLabel.text = "connectionstring.txt okundu. proimages klasörü bulunamadı.";

    validateSefimPage(page);
}

Component.prototype.DynamicSefimPageCallback = function()
{
    var page = gui.pageWidgetByObjectName("DynamicSefimPage");
    if (page == null)
        return;

    page.windowTitle = "Şefim ve veritabanı";
    if (page.sefimDirEdit.text.length === 0)
        page.sefimDirEdit.text = defaultSefimDir();

    if (component.sefimPageWired !== true) {
        page.sefimDirBrowse.clicked.connect(function() {
            var dir = QFileDialog.getExistingDirectory("Şefim kurulum klasörünü seçin", page.sefimDirEdit.text);
            if (dir.length > 0) {
                page.sefimDirEdit.text = dir;
                detectSefimPageFields(page);
            }
        });
        page.detectButton.clicked.connect(function() { detectSefimPageFields(page); });
        page.testButton.clicked.connect(function() {
            if (page.serverEdit.text.length === 0 || page.databaseEdit.text.length === 0) {
                page.testStateLabel.text = "Sunucu ve veritabanı alanları boş bırakılamaz.";
                return;
            }
            page.testStateLabel.text = "Bağlantı kurulurken test edilecek; sonuç kurulum ayrıntılarına yazılır.";
        });

        // Server + Database stay mandatory; User/Password are optional (Integrated Security fallback,
        // see SefimMcp.Setup.SefimDetection.BuildConnectionString).
        page.serverEdit.textChanged.connect(function() { validateSefimPage(page); });
        page.databaseEdit.textChanged.connect(function() { validateSefimPage(page); });
        component.sefimPageWired = true;
    }
    validateSefimPage(page);
}

function validateSefimPage(page)
{
    var ok = page.serverEdit.text.length > 0 && page.databaseEdit.text.length > 0;
    page.complete = ok;
    if (!ok && page.testStateLabel.text.length === 0)
        page.testStateLabel.text = "Sunucu ve veritabanı alanları boş bırakılamaz.";
    if (ok && page.testStateLabel.text === "Sunucu ve veritabanı alanları boş bırakılamaz.")
        page.testStateLabel.text = "";
}

Component.prototype.createOperations = function()
{
    component.createOperations();

    var exeName = (systemInfo.kernelType === "winnt") ? "sefim-ai-mcp.exe" : "sefim-ai-mcp";
    var clientPage = gui.pageWidgetByObjectName("DynamicClientPage");
    var sefimPage = gui.pageWidgetByObjectName("DynamicSefimPage");

    var host = (clientPage != null && clientPage.chatgptRadio.checked) ? "chatgpt" : component.hostChoice;
    var sefimDir = (sefimPage != null) ? sefimPage.sefimDirEdit.text : "";
    var proImages = component.proImagesPath;
    var server = (sefimPage != null) ? sefimPage.serverEdit.text : "";
    var database = (sefimPage != null) ? sefimPage.databaseEdit.text : "";
    var userId = (sefimPage != null) ? sefimPage.userEdit.text : "";
    var password = (sefimPage != null) ? sefimPage.passwordEdit.text : "";

    var args = [
        "@TargetDir@/" + exeName, "setup", "configure",
        "--install-dir", "@TargetDir@",
        "--host", host,
        "--sefim-dir", sefimDir,
        "--pro-images", proImages,
        "--server", server,
        "--database", database,
        "--tool-profile", "full",
        "--server-key", "sefim"
    ];
    if (userId.length > 0) { args.push("--user-id"); args.push(userId); }
    if (password.length > 0) { args.push("--password"); args.push(password); }

    // Runs after the implicit Extract operation added by component.createOperations() above,
    // so @TargetDir@/<exeName> already exists on disk. Non-fatal by design: a configuration
    // failure must not roll back a successful file install, matching the old NSIS behavior
    // (Section only showed a MessageBox warning, it never aborted).
    //
    // The UNDOEXECUTE clause is part of the SAME operation: at uninstall time QtIFW runs the
    // command after the separator token, which strips the MCP entry from the client config
    // before the extracted binary is deleted. The leading "{0,1}" tolerance only covers the
    // perform side (QtIFW splits arguments() at UNDOEXECUTE, and the undo half falls back to
    // requiring exit 0), so "setup remove" is written to always exit 0 itself — a failed
    // cleanup is logged as a warning rather than ever failing the uninstall operation.
    component.addOperation("Execute", ["{0,1}"].concat(args).concat([
        "UNDOEXECUTE",
        "@TargetDir@/" + exeName, "setup", "remove",
        "--host", host,
        "--server-key", "sefim"
    ]));
}
