#!/usr/bin/env bash
# Builds the Windows setup from an already published win-x64 payload.
#
# The payload itself must come from a Windows machine or from the
# "Windows installer" GitHub Actions workflow: the server is published with
# native AOT, which cannot be cross-compiled from Linux.
#
# Usage:
#   installer/build-installer.sh [payload-dir] [version]
#
# Defaults: payload-dir=artifacts/win-x64, version=PackageVersion from the csproj.

set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
payload_dir="${1:-$repo_root/artifacts/win-x64}"
version="${2:-}"

if [[ -z "$version" ]]; then
    version="$(sed -n 's:.*<PackageVersion>\(.*\)</PackageVersion>.*:\1:p' \
        "$repo_root/sefim-ai-mcp/sefim-ai-mcp.csproj" | head -1)"
fi

if [[ ! -f "$payload_dir/sefim-ai-mcp.exe" ]]; then
    echo "Payload not found: $payload_dir/sefim-ai-mcp.exe" >&2
    echo "Publish first on Windows:" >&2
    echo "  dotnet publish sefim-ai-mcp/sefim-ai-mcp.csproj -c Release -r win-x64 --self-contained true -o artifacts/win-x64" >&2
    exit 1
fi

numeric="$(printf '%s' "$version" | grep -oE '^[0-9]+(\.[0-9]+){0,3}' || echo '0.0.0')"
while [[ "$(tr -cd '.' <<<"$numeric" | wc -c)" -lt 3 ]]; do
    numeric="$numeric.0"
done

output="$repo_root/SefimMcpSetup-$version-x64.exe"

makensis -V3 -INPUTCHARSET UTF8 \
    "-DVERSION=$version" \
    "-DVERSION_NUMERIC=$numeric" \
    "-DPAYLOAD_DIR=$payload_dir" \
    "-DOUTPUT_FILE=$output" \
    "$repo_root/installer/sefim-mcp.nsi"

echo "Setup written to: $output"
