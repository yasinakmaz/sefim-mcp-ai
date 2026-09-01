#!/usr/bin/env bash
# Builds a QtIFW installer from an already-published, single-RID payload.
#
# Usage:
#   installer/qtifw/build-installer.sh <payload-dir> <version> <os-arch-label>
#
# Example:
#   installer/qtifw/build-installer.sh artifacts/linux-x64 0.1.0-beta linux-x64
#
# os-arch-label controls only the output filename; the actual target platform is
# whatever OS this script is invoked on (binarycreator is not cross-platform).

set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
qtifw_dir="$repo_root/installer/qtifw"
payload_dir="$1"
version="$2"
label="$3"

if [[ ! -f "$payload_dir/sefim-ai-mcp" && ! -f "$payload_dir/sefim-ai-mcp.exe" ]]; then
    echo "Payload not found under $payload_dir (expected sefim-ai-mcp or sefim-ai-mcp.exe)" >&2
    exit 1
fi

# --- 1. Ensure binarycreator is available -----------------------------------
tools_dir="$repo_root/.qtifw-tools"
if ! command -v binarycreator >/dev/null 2>&1 && [[ ! -x "$tools_dir/bin/binarycreator" && ! -x "$tools_dir/bin/binarycreator.exe" ]]; then
    echo "Installing Qt Installer Framework via aqtinstall..."
    python3 -m pip install --quiet --upgrade aqtinstall
    case "$(uname -s)" in
        Linux*)  host=linux ;;
        Darwin*) host=mac ;;
        *)       host=windows ;;
    esac
    python3 -m aqt install-tool "$host" desktop tools_ifw --outputdir "$tools_dir"
fi

bc="binarycreator"
if ! command -v binarycreator >/dev/null 2>&1; then
    bc="$(find "$tools_dir" -iname 'binarycreator*' -type f | head -1)"
fi

# --- 2. Stamp version into package.xml / config.xml --------------------------
work_dir="$(mktemp -d)"
trap 'rm -rf "$work_dir"' EXIT
# assets/ must be copied too: config.xml references ../assets/* relative to its own
# directory, so it has to stay a sibling of config/ inside the work dir.
cp -r "$qtifw_dir/config" "$qtifw_dir/packages" "$qtifw_dir/assets" "$work_dir/"

sed -i.bak "s/<Version>.*<\/Version>/<Version>${version}<\/Version>/" \
    "$work_dir/config/config.xml" "$work_dir/packages/com.ozfiliz.sefimmcp/meta/package.xml"
rm -f "$work_dir/config/config.xml.bak" "$work_dir/packages/com.ozfiliz.sefimmcp/meta/package.xml.bak"

# --- 3. Stage payload into the package's data/ dir ----------------------------
data_dir="$work_dir/packages/com.ozfiliz.sefimmcp/data"
mkdir -p "$data_dir"
cp -r "$payload_dir"/. "$data_dir/"

# Knowledge is only ever shipped as the encrypted knowledge.pack; refuse to package
# plaintext markdown, mirroring the check the old windows-installer.yml workflow ran.
if [ -n "$(find "$data_dir" -iname '*.md' -print -quit)" ]; then
    echo "Refusing to package plaintext knowledge (*.md found under payload)." >&2
    find "$data_dir" -iname '*.md' >&2
    exit 1
fi

# --- 4. Build ------------------------------------------------------------------
case "$(uname -s)" in
    Linux*)  ext="run" ;;
    Darwin*) ext="app" ;;
    *)       ext="exe" ;;
esac
output="$repo_root/SefimMcpSetup-${version}-${label}.${ext}"

"$bc" --offline-only -c "$work_dir/config/config.xml" -p "$work_dir/packages" "$output"

# On macOS binarycreator emits a .app *bundle directory*, which upload-artifact/gh-release
# cannot carry as a single file (structure and exec bits are lost). Zip it so every platform
# publishes exactly one file.
if [[ "$ext" == "app" ]]; then
    (cd "$(dirname "$output")" && zip -qry "$(basename "$output").zip" "$(basename "$output")")
    rm -rf "$output"
    output="${output}.zip"
fi

echo "Setup written to: $output"
