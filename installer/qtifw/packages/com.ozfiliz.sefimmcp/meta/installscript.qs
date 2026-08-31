function Component()
{
    installer.addWizardPage(component, "ClientPage", QInstaller.TargetDirectory);
    installer.addWizardPage(component, "SefimPage", QInstaller.TargetDirectory);
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

Component.prototype.ClientPageCallback = function()
{
    var page = gui.pageWidgetByObjectName("DynamicClientPage");
    if (page == null)
        return;
}

Component.prototype.SefimPageCallback = function()
{
    var page = gui.pageWidgetByObjectName("DynamicSefimPage");
    if (page == null)
        return;
    if (page.sefimDirEdit.text.length === 0)
        page.sefimDirEdit.text = defaultSefimDir();
    if (page.imagesEdit.text.length === 0)
        page.imagesEdit.text = defaultImagesDir();

    if (component.sefimPageWired !== true) {
        page.sefimDirBrowse.clicked.connect(function() {
            var dir = QFileDialog.getExistingDirectory("Şefim kurulum klasörünü seçin", page.sefimDirEdit.text);
            if (dir.length > 0)
                page.sefimDirEdit.text = dir;
        });
        page.imagesBrowse.clicked.connect(function() {
            var dir = QFileDialog.getExistingDirectory("Görseller klasörünü seçin", page.imagesEdit.text);
            if (dir.length > 0)
                page.imagesEdit.text = dir;
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
    gui.currentPageWidget().complete = ok;
}

Component.prototype.createOperations = function()
{
    component.createOperations();

    var exeName = (systemInfo.kernelType === "winnt") ? "sefim-ai-mcp.exe" : "sefim-ai-mcp";
    var clientPage = gui.pageWidgetByObjectName("DynamicClientPage");
    var sefimPage = gui.pageWidgetByObjectName("DynamicSefimPage");

    var host = (clientPage != null && clientPage.chatgptRadio.checked) ? "chatgpt" : "claude";
    var sefimDir = (sefimPage != null) ? sefimPage.sefimDirEdit.text : "";
    var proImages = (sefimPage != null) ? sefimPage.imagesEdit.text : "";
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
    component.addOperation("Execute", args.concat(["{0,1}"]));
}

function Controller() {}

Controller.prototype.FinishedPageCallback = function()
{
    gui.clickButton(buttons.FinishButton);
}
