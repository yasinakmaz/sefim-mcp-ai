#!/usr/bin/env bash
set -euo pipefail

publish_dir="$1"
if find "$publish_dir" -path '*/knowledge/private/*' -o -name '*.private.md' | grep -q .; then
  echo "ERROR: plaintext private knowledge was found in publish output." >&2
  exit 1
fi

echo "publish knowledge leakage check: OK"
