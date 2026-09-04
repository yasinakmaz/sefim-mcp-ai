function Controller()
{
}

function applyNsisButtonText(widget)
{
    if (widget == null)
        return;

    try {
        var wizard = widget.wizard();
        if (wizard == null)
            return;

        wizard.resize(503, 390);
        wizard.setMinimumSize(503, 390);
        wizard.setMaximumSize(503, 390);
        wizard.setButtonText(buttons.BackButton, "< Geri");
        wizard.setButtonText(buttons.NextButton, "İleri >");
        wizard.setButtonText(buttons.CommitButton, "Kur");
        wizard.setButtonText(buttons.FinishButton, "Bitir");
        wizard.setButtonText(buttons.CancelButton, "İptal");
    } catch (e) {
        // Some QtIFW builds do not expose QWizard::setButtonText to script.
    }
}

Controller.prototype.IntroductionPageCallback = function()
{
    var widget = gui.currentPageWidget();
    applyNsisButtonText(widget);
    if (widget == null)
        return;

    widget.title = "Şefim MCP Server " + installer.value("ProductVersion");
    widget.MessageLabel.setText(
        "Bu sihirbaz, Şefim POS verilerini yapay zekâ istemcilerine bağlayan MCP sunucusunu kurar.\n\n" +
        "Kurulum sırasında:\n" +
        "  •  Claude Desktop veya ChatGPT Desktop seçilir\n" +
        "  •  Şefim kurulumu ve connectionstring.txt otomatik bulunur\n" +
        "  •  appsettings.json ve istemci yapılandırması otomatik yazılır\n\n" +
        "Devam etmek için İleri'ye tıklayın."
    );
}

Controller.prototype.TargetDirectoryPageCallback = function()
{
    var widget = gui.currentPageWidget();
    applyNsisButtonText(widget);
    if (widget == null)
        return;

    widget.title = "Kurulum klasörü seçin";
    widget.MessageLabel.setText(
        "Şefim MCP Server aşağıdaki klasöre kurulacaktır. Her sürüm kendi klasöründe tutulur, böylece geri dönüş kolaydır."
    );
}

Controller.prototype.PerformInstallationPageCallback = function()
{
    applyNsisButtonText(gui.currentPageWidget());
}

Controller.prototype.FinishedPageCallback = function()
{
    var widget = gui.currentPageWidget();
    applyNsisButtonText(widget);
    if (widget == null)
        return;

    widget.title = "Kurulum tamamlandı";
    widget.MessageLabel.setText(
        "Şefim MCP Server kuruldu ve seçtiğiniz istemciye tanıtıldı.\n\n" +
        "Değişikliklerin görünmesi için istemci uygulamayı tamamen kapatıp yeniden açın."
    );
    if (widget.RunItCheckBox != null)
        widget.RunItCheckBox.visible = false;
}
