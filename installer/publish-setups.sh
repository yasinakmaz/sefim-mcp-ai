#!/usr/bin/env bash
# Publishes native 64-bit payloads and builds QtIFW setup packages into PUBLISH/.
#
# Defaults:
#   installer/publish-setups.sh
#     requests win-x64, win-arm64, linux-x64, linux-arm64, osx-x64 and osx-arm64.
#
# Native AOT and QtIFW installer generation are host-OS bound. Use
# --skip-unsupported when running locally on just one OS, or run the same script
# on each target OS/architecture. If a payload already exists under
# PUBLISH/payload/<rid>, the script can package it on a compatible QtIFW host.

set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
publish_dir="$repo_root/PUBLISH"
configuration="Release"
version=""
skip_tests=false
skip_unsupported=false
explicit_targets=false
targets=()

usage() {
    cat <<'USAGE'
Usage:
  installer/publish-setups.sh [options]

Target options:
  --windows          Build win-x64 and win-arm64 setups.
  --windows-x64      Build win-x64 setup.
  --windows-arm64    Build win-arm64 setup.
  --linux            Build linux-x64 and linux-arm64 setups.
  --linux-x64        Build linux-x64 setup.
  --linux-arm64      Build linux-arm64 setup.
  --macos            Build osx-x64 and osx-arm64 setups.
  --macos-x64        Build osx-x64 setup.
  --macos-arm64      Build osx-arm64 setup.
  --all              Build all x64 and arm64 setups for Windows, Linux and macOS.

Build options:
  --version VALUE        Override PackageVersion.
  --output-dir DIR       Output directory. Default: PUBLISH
  --configuration VALUE  Dotnet configuration. Default: Release
  --skip-tests           Do not run tests before publishing.
  --skip-unsupported     Skip targets this host cannot Native AOT publish.
  --help                 Show this help.

Examples:
  installer/publish-setups.sh --skip-unsupported
  installer/publish-setups.sh --windows
  installer/publish-setups.sh --windows-arm64
  installer/publish-setups.sh --linux
  installer/publish-setups.sh --macos --skip-unsupported
USAGE
}

add_target() {
    local rid="$1"
    for existing in "${targets[@]}"; do
        [[ "$existing" == "$rid" ]] && return
    done
    targets+=("$rid")
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --windows)
            explicit_targets=true
            add_target "win-x64"
            add_target "win-arm64"
            ;;
        --windows-x64)
            explicit_targets=true
            add_target "win-x64"
            ;;
        --windows-arm64)
            explicit_targets=true
            add_target "win-arm64"
            ;;
        --linux)
            explicit_targets=true
            add_target "linux-x64"
            add_target "linux-arm64"
            ;;
        --linux-x64)
            explicit_targets=true
            add_target "linux-x64"
            ;;
        --linux-arm64)
            explicit_targets=true
            add_target "linux-arm64"
            ;;
        --macos)
            explicit_targets=true
            add_target "osx-x64"
            add_target "osx-arm64"
            ;;
        --macos-x64)
            explicit_targets=true
            add_target "osx-x64"
            ;;
        --macos-arm64)
            explicit_targets=true
            add_target "osx-arm64"
            ;;
        --all)
            explicit_targets=true
            add_target "win-x64"
            add_target "win-arm64"
            add_target "linux-x64"
            add_target "linux-arm64"
            add_target "osx-x64"
            add_target "osx-arm64"
            ;;
        --version)
            [[ $# -ge 2 ]] || { echo "--version requires a value" >&2; exit 2; }
            version="$2"
            shift
            ;;
        --output-dir)
            [[ $# -ge 2 ]] || { echo "--output-dir requires a value" >&2; exit 2; }
            publish_dir="$2"
            shift
            ;;
        --configuration)
            [[ $# -ge 2 ]] || { echo "--configuration requires a value" >&2; exit 2; }
            configuration="$2"
            shift
            ;;
        --skip-tests)
            skip_tests=true
            ;;
        --skip-unsupported)
            skip_unsupported=true
            ;;
        --help)
            usage
            exit 0
            ;;
        *)
            echo "Unknown option: $1" >&2
            usage >&2
            exit 2
            ;;
    esac
    shift
done

if [[ "$explicit_targets" == false ]]; then
    add_target "win-x64"
    add_target "win-arm64"
    add_target "linux-x64"
    add_target "linux-arm64"
    add_target "osx-x64"
    add_target "osx-arm64"
fi

host_os="$(uname -s)"
host_arch="$(uname -m)"

host_can_publish_target() {
    local rid="$1"
    case "$rid" in
        win-x64|win-arm64)
            [[ "$host_os" == MINGW* || "$host_os" == MSYS* || "$host_os" == CYGWIN* ]]
            ;;
        linux-x64)
            [[ "$host_os" == Linux && ( "$host_arch" == x86_64 || "$host_arch" == amd64 ) ]]
            ;;
        linux-arm64)
            [[ "$host_os" == Linux && ( "$host_arch" == aarch64 || "$host_arch" == arm64 ) ]]
            ;;
        osx-x64)
            [[ "$host_os" == Darwin && ( "$host_arch" == x86_64 || "$host_arch" == amd64 ) ]]
            ;;
        osx-arm64)
            [[ "$host_os" == Darwin && "$host_arch" == arm64 ]]
            ;;
        *)
            return 1
            ;;
    esac
}

host_can_package_setup() {
    local rid="$1"
    case "$rid" in
        win-*)
            [[ "$host_os" == MINGW* || "$host_os" == MSYS* || "$host_os" == CYGWIN* ]]
            ;;
        linux-*)
            [[ "$host_os" == Linux && ( "$host_arch" == x86_64 || "$host_arch" == amd64 ) ]]
            ;;
        osx-x64)
            [[ "$host_os" == Darwin && ( "$host_arch" == x86_64 || "$host_arch" == amd64 ) ]]
            ;;
        osx-arm64)
            [[ "$host_os" == Darwin && "$host_arch" == arm64 ]]
            ;;
        *)
            return 1
            ;;
    esac
}

label_for_rid() {
    case "$1" in
        win-x64) echo "win-x64" ;;
        win-arm64) echo "win-arm64" ;;
        linux-x64) echo "linux-x64" ;;
        linux-arm64) echo "linux-arm64" ;;
        osx-x64) echo "macos-x64" ;;
        osx-arm64) echo "macos-arm64" ;;
        *) echo "$1" ;;
    esac
}

exe_for_rid() {
    case "$1" in
        win-*) echo "sefim-ai-mcp.exe" ;;
        *) echo "sefim-ai-mcp" ;;
    esac
}

resolve_version() {
    if [[ -n "$version" ]]; then
        echo "$version"
        return
    fi

    sed -n 's:.*<PackageVersion>\(.*\)</PackageVersion>.*:\1:p' \
        "$repo_root/sefim-ai-mcp/sefim-ai-mcp.csproj" | head -1
}

version="$(resolve_version)"
if [[ -z "$version" ]]; then
    echo "Version could not be resolved. Use --version." >&2
    exit 1
fi

allow_missing_pack=true
if [[ -f "$repo_root/sefim-ai-mcp/knowledge.pack" ]]; then
    header="$(head -c 8 "$repo_root/sefim-ai-mcp/knowledge.pack")"
    if [[ "$header" != "SEFIMKP1" ]]; then
        echo "knowledge.pack missing SEFIMKP1 header." >&2
        exit 1
    fi
    allow_missing_pack=false
fi

mkdir -p "$publish_dir"

echo "Version: $version"
echo "Output : $publish_dir"
echo "Targets: ${targets[*]}"
echo

dotnet restore "$repo_root/sefim-ai-mcp.sln"

if [[ "$skip_tests" == false ]]; then
    dotnet test "$repo_root/tests/SefimMcp.Tests/SefimMcp.Tests.csproj" \
        -c "$configuration" --no-restore
fi

python3 "$repo_root/installer/qtifw/assets/generate-assets.py"

built=()
skipped=()

for rid in "${targets[@]}"; do
    label="$(label_for_rid "$rid")"
    payload_dir="$publish_dir/payload/$rid"

    exe_name="$(exe_for_rid "$rid")"

    if host_can_publish_target "$rid"; then
        echo
        echo "Publishing payload: $rid"
        dotnet publish "$repo_root/sefim-ai-mcp/sefim-ai-mcp.csproj" \
            -c "$configuration" -r "$rid" --self-contained true \
            -p:AllowMissingKnowledgePack="$allow_missing_pack" \
            -p:PackageVersion="$version" \
            -o "$payload_dir"
    elif [[ -f "$payload_dir/$exe_name" ]]; then
        echo
        echo "Using existing payload: $payload_dir"
    else
        message="Skipping $rid payload: Native AOT publish must run on a compatible host."
        if [[ "$skip_unsupported" == true ]]; then
            echo "$message"
            skipped+=("$rid")
            continue
        fi

        echo "$message" >&2
        echo "Run again on the matching host, or pass --skip-unsupported for local partial output." >&2
        exit 1
    fi

    [[ -f "$payload_dir/$exe_name" ]] || {
        echo "Publish output is missing $payload_dir/$exe_name" >&2
        exit 1
    }
    [[ -f "$payload_dir/appsettings.json" ]] || {
        echo "Publish output is missing $payload_dir/appsettings.json" >&2
        exit 1
    }

    if [[ "$allow_missing_pack" == false ]]; then
        "$repo_root/scripts/verify-publish-knowledge.sh" "$payload_dir"
    elif find "$payload_dir" -iname '*.md' | head -1 | grep -q .; then
        echo "Plaintext markdown found under $payload_dir" >&2
        exit 1
    fi

    if ! host_can_package_setup "$rid"; then
        message="Skipping $rid setup: QtIFW binarycreator must run on a compatible installer host."
        if [[ "$skip_unsupported" == true ]]; then
            echo "$message"
            skipped+=("$rid setup")
            continue
        fi

        echo "$message" >&2
        echo "Run again on the matching host, or pass --skip-unsupported for local partial output." >&2
        exit 1
    fi

    echo "Building setup: $label"
    "$repo_root/installer/qtifw/build-installer.sh" "$payload_dir" "$version" "$label" "$publish_dir"
    built+=("$label")
done

echo
echo "PUBLISH contents:"
find "$publish_dir" -maxdepth 4 -type f | sort | sed 's/^/  /'

echo
if [[ ${#built[@]} -gt 0 ]]; then
    echo "Built setups: ${built[*]}"
else
    echo "Built setups: none"
fi
if [[ ${#skipped[@]} -gt 0 ]]; then
    echo "Skipped targets: ${skipped[*]}"
fi
