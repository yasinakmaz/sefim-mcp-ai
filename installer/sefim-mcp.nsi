; ---------------------------------------------------------------------------
; Sefim MCP Server - Windows setup
;
; Build (from the repository root):
;   makensis -DVERSION=0.1.0-beta -DPAYLOAD_DIR=artifacts\win-x64 installer\sefim-mcp.nsi
;
; The installer only orchestrates. Everything that has to touch JSON, the Sefim
; connectionstring.txt or SQL Server is delegated to scripts\Configure-SefimMcp.ps1.
; ---------------------------------------------------------------------------

Unicode true
ManifestDPIAware true
; The stub stays x86-unicode: it runs on every x64 Windows, and not every NSIS
; distribution ships the amd64 stub. 32 bit Windows is refused in .onInit, and
; the 64 bit registry view is selected explicitly, so the stub architecture
; carries no behaviour. Build with /DTARGET_AMD64 to produce a 64 bit stub.
!ifdef TARGET_AMD64
    Target amd64-unicode
!endif
SetCompressor /SOLID lzma

!include "MUI2.nsh"
!include "LogicLib.nsh"
!include "WinVer.nsh"
!include "x64.nsh"
!include "FileFunc.nsh"
!include "TextFunc.nsh"
!include "nsDialogs.nsh"

!define /ifndef VERSION "0.1.0-beta"
!define /ifndef PAYLOAD_DIR "..\artifacts\win-x64"
!define /ifndef OUTPUT_FILE "SefimMcpSetup-${VERSION}-x64.exe"
!define /ifndef VERSION_NUMERIC "0.1.0.0"

!define PRODUCT_NAME "Şefim MCP Server"
!define COMPANY_NAME "OZFİLİZ YAZILIM"
!define COMPANY_DIR "OZFILIZYAZILIM"
!define PRODUCT_DIR "SEFIM-MCP"
!define EXE_NAME "sefim-ai-mcp.exe"
!define SERVER_KEY "sefim"
!define REG_UNINSTALL "Software\Microsoft\Windows\CurrentVersion\Uninstall\SefimMcpServer"
!define REG_PRODUCT "Software\${COMPANY_DIR}\${PRODUCT_DIR}"
!define DEFAULT_SEFIM_DIR "C:\Program Files (x86)\Vega\Sefim"

Name "${PRODUCT_NAME} ${VERSION}"
OutFile "${OUTPUT_FILE}"
InstallDir "$PROGRAMFILES32\${COMPANY_DIR}\${PRODUCT_DIR}\${VERSION}"
RequestExecutionLevel admin
ShowInstDetails show
ShowUninstDetails show
BrandingText "${COMPANY_NAME}"

VIProductVersion "${VERSION_NUMERIC}"
VIAddVersionKey "ProductName" "${PRODUCT_NAME}"
VIAddVersionKey "CompanyName" "${COMPANY_NAME}"
VIAddVersionKey "LegalCopyright" "© ${COMPANY_NAME}"
VIAddVersionKey "FileDescription" "${PRODUCT_NAME} kurulum programı"
VIAddVersionKey "FileVersion" "${VERSION}"
VIAddVersionKey "ProductVersion" "${VERSION}"

; ---------------------------------------------------------------------------
; Interface
; ---------------------------------------------------------------------------

!define MUI_ABORTWARNING
!define MUI_ICON "assets\install.ico"
!define MUI_UNICON "assets\uninstall.ico"

!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_RIGHT
!define MUI_HEADERIMAGE_BITMAP "assets\header.bmp"
!define MUI_HEADERIMAGE_UNBITMAP "assets\header.bmp"
!define MUI_WELCOMEFINISHPAGE_BITMAP "assets\welcome.bmp"
!define MUI_UNWELCOMEFINISHPAGE_BITMAP "assets\welcome.bmp"

!define MUI_WELCOMEPAGE_TITLE "${PRODUCT_NAME} ${VERSION}"
!define MUI_WELCOMEPAGE_TEXT "Bu sihirbaz, Şefim POS verilerini yapay zekâ istemcilerine bağlayan MCP sunucusunu kurar.$\r$\n$\r$\nKurulum sırasında:$\r$\n  •  Claude Desktop veya ChatGPT Desktop seçilir$\r$\n  •  Şefim kurulumu ve connectionstring.txt otomatik bulunur$\r$\n  •  appsettings.json ve istemci yapılandırması otomatik yazılır$\r$\n$\r$\nDevam etmek için İleri'ye tıklayın."

!define MUI_DIRECTORYPAGE_TEXT_TOP "${PRODUCT_NAME} aşağıdaki klasöre kurulacaktır. Her sürüm kendi klasöründe tutulur, böylece geri dönüş kolaydır."

!define MUI_FINISHPAGE_TITLE "Kurulum tamamlandı"
!define MUI_FINISHPAGE_TEXT "${PRODUCT_NAME} kuruldu ve seçtiğiniz istemciye tanıtıldı.$\r$\n$\r$\nDeğişikliklerin görünmesi için istemci uygulamayı tamamen kapatıp yeniden açın."
!define MUI_FINISHPAGE_SHOWREADME ""
!define MUI_FINISHPAGE_SHOWREADME_TEXT "Kurulum klasörünü aç"
!define MUI_FINISHPAGE_SHOWREADME_FUNCTION OpenInstallFolder
!define MUI_FINISHPAGE_LINK "OZFİLİZ YAZILIM"
!define MUI_FINISHPAGE_LINK_LOCATION "https://github.com/yasinakmaz/sefim-mcp-ai"

; ---------------------------------------------------------------------------
; State
; ---------------------------------------------------------------------------

Var HostChoice
Var ClaudeFound
Var ClaudePath
Var ChatGptFound
Var ChatGptPath
Var SefimDir
Var ProImages
Var CsFile
Var SqlServer
Var SqlDatabase
Var SqlUser
Var SqlPassword
Var KnowledgeKey
Var Status
Var Message
Var HostConfigPath

Var Dialog
Var FontBold
Var RadioClaude
Var RadioChatGpt
Var TxtSefimDir
Var TxtServer
Var TxtDatabase
Var TxtUser
Var TxtPassword
Var TxtKnowledgeKey
Var LabelSefimState
Var LabelTestState

; ---------------------------------------------------------------------------
; Pages
; ---------------------------------------------------------------------------

!insertmacro MUI_PAGE_WELCOME
Page custom HostPageCreate HostPageLeave
!insertmacro MUI_PAGE_DIRECTORY
Page custom SefimPageCreate SefimPageLeave
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "Turkish"

; ---------------------------------------------------------------------------
; PowerShell helper plumbing
; ---------------------------------------------------------------------------

!define PS_EXE "$SYSDIR\WindowsPowerShell\v1.0\powershell.exe"

!macro RunHelper Mode ScriptPath Quiet
  !if "${Quiet}" == "quiet"
    nsExec::ExecToStack '"${PS_EXE}" -NoLogo -NonInteractive -NoProfile -ExecutionPolicy Bypass -File "${ScriptPath}" -Mode ${Mode} -InputFile "$PLUGINSDIR\sefim-input.txt" -ResultFile "$PLUGINSDIR\sefim-result.txt"'
    Pop $0
    Pop $1
  !else
    nsExec::ExecToLog '"${PS_EXE}" -NoLogo -NonInteractive -NoProfile -ExecutionPolicy Bypass -File "${ScriptPath}" -Mode ${Mode} -InputFile "$PLUGINSDIR\sefim-input.txt" -ResultFile "$PLUGINSDIR\sefim-result.txt"'
    Pop $0
  !endif
!macroend

; Splits "$R0" on the first "=" into $R1 (key) and $R2 (value).
Function SplitKeyValue
  StrCpy $R1 ""
  StrCpy $R2 ""
  StrLen $R3 $R0
  StrCpy $R4 0

  split_loop:
    ${If} $R4 >= $R3
      Return
    ${EndIf}
    StrCpy $R5 $R0 1 $R4
    ${If} $R5 == "="
      StrCpy $R1 $R0 $R4
      IntOp $R6 $R4 + 1
      StrCpy $R2 $R0 "" $R6
      Return
    ${EndIf}
    IntOp $R4 $R4 + 1
    Goto split_loop
FunctionEnd

Function WriteHelperInput
  ClearErrors
  FileOpen $0 "$PLUGINSDIR\sefim-input.txt" w
  ${If} ${Errors}
    Return
  ${EndIf}
  FileWriteUTF16LE /BOM $0 "installdir=$INSTDIR$\r$\n"
  FileWriteUTF16LE $0 "hostapp=$HostChoice$\r$\n"
  FileWriteUTF16LE $0 "sefimdir=$SefimDir$\r$\n"
  FileWriteUTF16LE $0 "server=$SqlServer$\r$\n"
  FileWriteUTF16LE $0 "database=$SqlDatabase$\r$\n"
  FileWriteUTF16LE $0 "userid=$SqlUser$\r$\n"
  FileWriteUTF16LE $0 "password=$SqlPassword$\r$\n"
  FileWriteUTF16LE $0 "knowledgekey=$KnowledgeKey$\r$\n"
  FileWriteUTF16LE $0 "serverkey=${SERVER_KEY}$\r$\n"
  FileClose $0
FunctionEnd

; Reads the helper result file back into the installer variables.
Function ReadHelperResult
  StrCpy $Status ""
  StrCpy $Message ""

  ClearErrors
  FileOpen $9 "$PLUGINSDIR\sefim-result.txt" r
  ${If} ${Errors}
    StrCpy $Status "error"
    StrCpy $Message "Yapılandırma yardımcısı sonuç üretmedi. PowerShell çalıştırılamamış olabilir."
    Return
  ${EndIf}

  read_loop:
    ClearErrors
    FileReadUTF16LE $9 $R0
    ${If} ${Errors}
      Goto read_done
    ${EndIf}
    ${TrimNewLines} "$R0" $R0
    Call SplitKeyValue
    ${Select} $R1
      ${Case} "claude"
        StrCpy $ClaudeFound $R2
      ${Case} "claudepath"
        StrCpy $ClaudePath $R2
      ${Case} "chatgpt"
        StrCpy $ChatGptFound $R2
      ${Case} "chatgptpath"
        StrCpy $ChatGptPath $R2
      ${Case} "sefimdir"
        StrCpy $SefimDir $R2
      ${Case} "proimages"
        StrCpy $ProImages $R2
      ${Case} "csfile"
        StrCpy $CsFile $R2
      ${Case} "server"
        StrCpy $SqlServer $R2
      ${Case} "database"
        StrCpy $SqlDatabase $R2
      ${Case} "userid"
        StrCpy $SqlUser $R2
      ${Case} "password"
        StrCpy $SqlPassword $R2
      ${Case} "hostconfig"
        StrCpy $HostConfigPath $R2
      ${Case} "status"
        StrCpy $Status $R2
      ${Case} "message"
        StrCpy $Message $R2
    ${EndSelect}
    Goto read_loop

  read_done:
  FileClose $9
  Delete "$PLUGINSDIR\sefim-result.txt"
FunctionEnd

; ---------------------------------------------------------------------------
; Start-up checks
; ---------------------------------------------------------------------------

Function .onInit
  ${IfNot} ${AtLeastWin10}
    MessageBox MB_OK|MB_ICONSTOP "Bu kurulum yalnızca Windows 10 ve üzeri sürümlerde çalışır.$\r$\n$\r$\nİşletim sisteminizi Windows 10 veya üzerine yükseltmeniz gerekiyor."
    Abort
  ${EndIf}

  ${IfNot} ${RunningX64}
    MessageBox MB_OK|MB_ICONSTOP "32 bit Windows desteklenmiyor.$\r$\n$\r$\n${PRODUCT_NAME} yalnızca 64 bit (x64) Windows üzerinde çalışır."
    Abort
  ${EndIf}

  SetRegView 64
  InitPluginsDir
  File "/oname=$PLUGINSDIR\Configure-SefimMcp.ps1" "scripts\Configure-SefimMcp.ps1"

  StrCpy $SefimDir "${DEFAULT_SEFIM_DIR}"
  Call WriteHelperInput
  !insertmacro RunHelper "Detect" "$PLUGINSDIR\Configure-SefimMcp.ps1" "quiet"
  Call ReadHelperResult

  ; Claude Desktop and ChatGPT Desktop install into the hidden WindowsApps
  ; folder, which is not readable, so their presence cannot be detected
  ; reliably. Detection only preselects a client; the setup runs either way.
  ${If} $ChatGptFound == "1"
  ${AndIf} $ClaudeFound != "1"
    StrCpy $HostChoice "chatgpt"
  ${Else}
    StrCpy $HostChoice "claude"
  ${EndIf}

  ${If} $SefimDir == ""
    StrCpy $SefimDir "${DEFAULT_SEFIM_DIR}"
  ${EndIf}

  CreateFont $FontBold "$(^Font)" "$(^FontSize)" 700
FunctionEnd

Function un.onInit
  SetRegView 64
  InitPluginsDir
FunctionEnd

; ---------------------------------------------------------------------------
; Page 1: MCP host selection
; ---------------------------------------------------------------------------

Function HostPageCreate
  !insertmacro MUI_HEADER_TEXT "Yapay zekâ istemcisi" "Şefim MCP sunucusunun tanıtılacağı uygulamayı seçin."

  nsDialogs::Create 1018
  Pop $Dialog
  ${If} $Dialog == error
    Abort
  ${EndIf}

  ${NSD_CreateLabel} 0 0 100% 24u "Sunucu, seçtiğiniz uygulamanın yapılandırma dosyasına eklenir. Dosyadaki diğer sunucular ve ayarlar korunur."
  Pop $0

  ${NSD_CreateGroupBox} 0 28u 100% 34u "  Claude Desktop  "
  Pop $0

  ${NSD_CreateRadioButton} 8u 40u 60% 12u "Claude Desktop kullan"
  Pop $RadioClaude
  SendMessage $RadioClaude ${WM_SETFONT} $FontBold 0
  ${NSD_OnClick} $RadioClaude OnPickClaude

  ${NSD_CreateLabel} 12u 51u 90% 10u ""
  Pop $0
  ${If} $ClaudeFound == "1"
    ${NSD_SetText} $0 "Bulundu: $ClaudePath"
  ${Else}
    ${NSD_SetText} $0 "Kurulu görünmüyor; kayıt yine de eklenir."
  ${EndIf}

  ${NSD_CreateGroupBox} 0 68u 100% 34u "  ChatGPT Desktop  "
  Pop $0

  ${NSD_CreateRadioButton} 8u 80u 60% 12u "ChatGPT Desktop kullan"
  Pop $RadioChatGpt
  SendMessage $RadioChatGpt ${WM_SETFONT} $FontBold 0
  ${NSD_OnClick} $RadioChatGpt OnPickChatGpt

  ${NSD_CreateLabel} 12u 91u 90% 10u ""
  Pop $0
  ${If} $ChatGptFound == "1"
    ${NSD_SetText} $0 "Bulundu: $ChatGptPath"
  ${Else}
    ${NSD_SetText} $0 "Kurulu görünmüyor; kayıt yine de eklenir."
  ${EndIf}

  ${If} $HostChoice == "claude"
    ${NSD_Check} $RadioClaude
  ${Else}
    ${NSD_Check} $RadioChatGpt
  ${EndIf}

  ${NSD_CreateLabel} 0 108u 100% 20u "Not: Yapılandırma değişikliğinin etkili olması için kurulum bittikten sonra seçtiğiniz uygulamayı tamamen kapatıp yeniden açın."
  Pop $0

  nsDialogs::Show
FunctionEnd

; The two radio buttons sit in separate group boxes, so exclusivity is enforced
; here instead of relying on the dialog's WS_GROUP order.
Function OnPickClaude
  Pop $0
  ${NSD_Check} $RadioClaude
  ${NSD_Uncheck} $RadioChatGpt
  StrCpy $HostChoice "claude"
FunctionEnd

Function OnPickChatGpt
  Pop $0
  ${NSD_Check} $RadioChatGpt
  ${NSD_Uncheck} $RadioClaude
  StrCpy $HostChoice "chatgpt"
FunctionEnd

Function HostPageLeave
  ${NSD_GetState} $RadioClaude $0
  ${If} $0 == ${BST_CHECKED}
    StrCpy $HostChoice "claude"
  ${Else}
    StrCpy $HostChoice "chatgpt"
  ${EndIf}
FunctionEnd

; ---------------------------------------------------------------------------
; Page 2: Sefim installation and SQL Server connection
; ---------------------------------------------------------------------------

Function SefimPageCreate
  !insertmacro MUI_HEADER_TEXT "Şefim ve veritabanı" "Şefim kurulumu ile SQL Server bağlantı bilgilerini doğrulayın."

  nsDialogs::Create 1018
  Pop $Dialog
  ${If} $Dialog == error
    Abort
  ${EndIf}

  ${NSD_CreateLabel} 0 0 100% 10u "Şefim kurulum klasörü"
  Pop $0
  SendMessage $0 ${WM_SETFONT} $FontBold 0

  ${NSD_CreateDirRequest} 0 11u 80% 12u "$SefimDir"
  Pop $TxtSefimDir

  ${NSD_CreateBrowseButton} 82% 11u 18% 12u "Gözat..."
  Pop $0
  ${NSD_OnClick} $0 OnBrowseClick

  ${NSD_CreateLabel} 0 25u 100% 12u ""
  Pop $LabelSefimState

  ${NSD_CreateLabel} 0 39u 100% 10u "SQL Server bağlantısı"
  Pop $0
  SendMessage $0 ${WM_SETFONT} $FontBold 0

  ${NSD_CreateLabel} 0 52u 24% 12u "Sunucu"
  Pop $0
  ${NSD_CreateText} 24% 50u 76% 12u "$SqlServer"
  Pop $TxtServer

  ${NSD_CreateLabel} 0 65u 24% 12u "Veritabanı"
  Pop $0
  ${NSD_CreateText} 24% 63u 76% 12u "$SqlDatabase"
  Pop $TxtDatabase

  ${NSD_CreateLabel} 0 78u 24% 12u "Kullanıcı"
  Pop $0
  ${NSD_CreateText} 24% 76u 76% 12u "$SqlUser"
  Pop $TxtUser

  ${NSD_CreateLabel} 0 91u 24% 12u "Parola"
  Pop $0
  ${NSD_CreatePassword} 24% 89u 76% 12u "$SqlPassword"
  Pop $TxtPassword

  ${NSD_CreateLabel} 0 104u 24% 12u "Knowledge key"
  Pop $0
  ${NSD_CreateText} 24% 102u 76% 12u "$KnowledgeKey"
  Pop $TxtKnowledgeKey

  ${NSD_CreateButton} 0 117u 32% 12u "Yeniden algıla"
  Pop $0
  ${NSD_OnClick} $0 OnDetectClick

  ${NSD_CreateButton} 34% 117u 32% 12u "Bağlantıyı test et"
  Pop $0
  ${NSD_OnClick} $0 OnTestClick

  ${NSD_CreateLabel} 0 131u 100% 12u ""
  Pop $LabelTestState

  Call RefreshSefimState
  nsDialogs::Show
FunctionEnd

Function RefreshSefimState
  ${If} $CsFile == ""
    ${NSD_SetText} $LabelSefimState "connectionstring.txt bulunamadı; bağlantı bilgilerini elle girin."
  ${ElseIf} $ProImages == ""
    ${NSD_SetText} $LabelSefimState "connectionstring.txt okundu. proimages klasörü bulunamadı."
  ${Else}
    ${NSD_SetText} $LabelSefimState "connectionstring.txt okundu. Görseller: $ProImages"
  ${EndIf}
FunctionEnd

Function ReadSefimPageFields
  ${NSD_GetText} $TxtSefimDir $SefimDir
  ${NSD_GetText} $TxtServer $SqlServer
  ${NSD_GetText} $TxtDatabase $SqlDatabase
  ${NSD_GetText} $TxtUser $SqlUser
  ${NSD_GetText} $TxtPassword $SqlPassword
  ${NSD_GetText} $TxtKnowledgeKey $KnowledgeKey
FunctionEnd

; Re-reads connectionstring.txt and proimages for the folder currently typed in.
Function DetectSefim
  Call ReadSefimPageFields
  Call WriteHelperInput
  !insertmacro RunHelper "Parse" "$PLUGINSDIR\Configure-SefimMcp.ps1" "quiet"
  Call ReadHelperResult

  ${NSD_SetText} $TxtServer "$SqlServer"
  ${NSD_SetText} $TxtDatabase "$SqlDatabase"
  ${NSD_SetText} $TxtUser "$SqlUser"
  ${NSD_SetText} $TxtPassword "$SqlPassword"
  Call RefreshSefimState
FunctionEnd

Function TestConnection
  Call ReadSefimPageFields
  ${If} $SqlServer == ""
  ${OrIf} $SqlDatabase == ""
    ${NSD_SetText} $LabelTestState "Sunucu ve veritabanı alanları boş bırakılamaz."
    Return
  ${EndIf}

  ${NSD_SetText} $LabelTestState "Bağlantı deneniyor..."
  Call WriteHelperInput
  !insertmacro RunHelper "Test" "$PLUGINSDIR\Configure-SefimMcp.ps1" "quiet"
  Call ReadHelperResult

  ${If} $Status == "ok"
    ${NSD_SetText} $LabelTestState "Bağlantı başarılı. $Message"
  ${Else}
    ${NSD_SetText} $LabelTestState "Bağlantı kurulamadı: $Message"
  ${EndIf}
FunctionEnd

Function OnBrowseClick
  Pop $0
  ${NSD_GetText} $TxtSefimDir $1
  nsDialogs::SelectFolderDialog "Şefim kurulum klasörünü seçin" "$1"
  Pop $2
  ${If} $2 != error
    ${NSD_SetText} $TxtSefimDir "$2"
    Call DetectSefim
  ${EndIf}
FunctionEnd

Function OnDetectClick
  Pop $0
  Call DetectSefim
FunctionEnd

Function OnTestClick
  Pop $0
  Call TestConnection
FunctionEnd

Function SefimPageLeave
  Call ReadSefimPageFields

  ${If} $SqlServer == ""
  ${OrIf} $SqlDatabase == ""
    MessageBox MB_OK|MB_ICONEXCLAMATION "SQL Server sunucusu ve veritabanı adı gereklidir.$\r$\n$\r$\nŞefim klasörünü seçip 'Yeniden algıla' düğmesini kullanabilir veya bilgileri elle girebilirsiniz."
    Abort
  ${EndIf}
FunctionEnd

Function OpenInstallFolder
  ExecShell "open" "$INSTDIR"
FunctionEnd

; ---------------------------------------------------------------------------
; Install
; ---------------------------------------------------------------------------

Section "Şefim MCP Server" SectionMain
  SectionIn RO
  SetRegView 64

  ; A previous version lives in its own folder. It is removed so only one server
  ; binary set remains on disk; the host configuration is rewritten below anyway.
  ReadRegStr $R0 HKLM "${REG_PRODUCT}" "InstallDir"
  ${If} $R0 != ""
  ${AndIf} $R0 != "$INSTDIR"
  ${AndIf} ${FileExists} "$R0\uninstall.exe"
    DetailPrint "Önceki sürüm kaldırılıyor: $R0"
    ExecWait '"$R0\uninstall.exe" /S _?=$R0'
    Delete "$R0\uninstall.exe"
    RMDir /r "$R0"
  ${EndIf}

  SetOutPath "$INSTDIR"
  File /r "${PAYLOAD_DIR}\*.*"

  SetOutPath "$INSTDIR\tools"
  File "scripts\Configure-SefimMcp.ps1"

  SetOutPath "$INSTDIR"
  DetailPrint "Yapılandırma yazılıyor (appsettings.json ve istemci ayarları)..."
  Call WriteHelperInput
  !insertmacro RunHelper "Configure" "$INSTDIR\tools\Configure-SefimMcp.ps1" "log"
  Call ReadHelperResult

  ${If} $Status != "ok"
    MessageBox MB_OK|MB_ICONEXCLAMATION "Dosyalar kuruldu, ancak otomatik yapılandırma tamamlanamadı:$\r$\n$\r$\n$Message$\r$\n$\r$\nBilgileri elle düzeltmek için: $INSTDIR\appsettings.json"
  ${Else}
    DetailPrint "İstemci yapılandırması: $HostConfigPath"
  ${EndIf}

  WriteRegStr HKLM "${REG_PRODUCT}" "InstallDir" "$INSTDIR"
  WriteRegStr HKLM "${REG_PRODUCT}" "Version" "${VERSION}"
  WriteRegStr HKLM "${REG_PRODUCT}" "Host" "$HostChoice"
  WriteRegStr HKLM "${REG_PRODUCT}" "SefimDir" "$SefimDir"

  WriteUninstaller "$INSTDIR\uninstall.exe"

  WriteRegStr HKLM "${REG_UNINSTALL}" "DisplayName" "${PRODUCT_NAME} ${VERSION}"
  WriteRegStr HKLM "${REG_UNINSTALL}" "DisplayVersion" "${VERSION}"
  WriteRegStr HKLM "${REG_UNINSTALL}" "Publisher" "${COMPANY_NAME}"
  WriteRegStr HKLM "${REG_UNINSTALL}" "InstallLocation" "$INSTDIR"
  WriteRegStr HKLM "${REG_UNINSTALL}" "DisplayIcon" "$INSTDIR\${EXE_NAME}"
  WriteRegStr HKLM "${REG_UNINSTALL}" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegStr HKLM "${REG_UNINSTALL}" "QuietUninstallString" '"$INSTDIR\uninstall.exe" /S'
  WriteRegDWORD HKLM "${REG_UNINSTALL}" "NoModify" 1
  WriteRegDWORD HKLM "${REG_UNINSTALL}" "NoRepair" 1

  ${GetSize} "$INSTDIR" "/S=0K" $0 $1 $2
  IntFmt $0 "0x%08X" $0
  WriteRegDWORD HKLM "${REG_UNINSTALL}" "EstimatedSize" "$0"
SectionEnd

; ---------------------------------------------------------------------------
; Uninstall
; ---------------------------------------------------------------------------

Section "Uninstall"
  SetRegView 64

  ${If} ${FileExists} "$INSTDIR\tools\Configure-SefimMcp.ps1"
    ClearErrors
    FileOpen $0 "$PLUGINSDIR\sefim-input.txt" w
    ${IfNot} ${Errors}
      FileWriteUTF16LE /BOM $0 "serverkey=${SERVER_KEY}$\r$\n"
      FileClose $0
    ${EndIf}
    DetailPrint "İstemci yapılandırmasından MCP kaydı siliniyor..."
    nsExec::ExecToLog '"$SYSDIR\WindowsPowerShell\v1.0\powershell.exe" -NoLogo -NonInteractive -NoProfile -ExecutionPolicy Bypass -File "$INSTDIR\tools\Configure-SefimMcp.ps1" -Mode Remove -InputFile "$PLUGINSDIR\sefim-input.txt" -ResultFile "$PLUGINSDIR\sefim-result.txt"'
    Pop $0
  ${EndIf}

  Delete "$INSTDIR\uninstall.exe"
  RMDir /r "$INSTDIR"

  ; Drop the versioned parent folders when nothing else is installed under them.
  RMDir "$PROGRAMFILES32\${COMPANY_DIR}\${PRODUCT_DIR}"
  RMDir "$PROGRAMFILES32\${COMPANY_DIR}"

  DeleteRegKey HKLM "${REG_UNINSTALL}"
  DeleteRegKey HKLM "${REG_PRODUCT}"
SectionEnd
