#!/usr/bin/env bash
# Verifies that a publish output ships knowledge only in encrypted form.
# The MSBuild targets in sefim-ai-mcp.csproj enforce the same rules at build time; this script is the
# independent check you can run in CI or before signing a release artifact.
set -euo pipefail

publish_dir="${1:?usage: verify-publish-knowledge.sh <publish-directory>}"
status=0

if find "$publish_dir" \( -path '*/knowledge/private/*' -o -name '*.private.md' -o -name '*.template.md' \) | grep -q .; then
  echo "ERROR: plaintext knowledge markdown was found in publish output." >&2
  status=1
fi

if find "$publish_dir" -name '*.md' | grep -q .; then
  echo "ERROR: markdown files were found in publish output; knowledge must ship only inside knowledge.pack." >&2
  find "$publish_dir" -name '*.md' >&2
  status=1
fi

if [ ! -f "$publish_dir/knowledge.pack" ]; then
  echo "ERROR: knowledge.pack is missing from publish output; the server would start with no business knowledge." >&2
  status=1
else
  # "SEFIMKP1" magic, then AES-256-GCM ciphertext. Readable business words here would mean the pack is not encrypted.
  header=$(head -c 8 "$publish_dir/knowledge.pack")
  if [ "$header" != "SEFIMKP1" ]; then
    echo "ERROR: knowledge.pack does not carry the expected SEFIMKP1 header." >&2
    status=1
  fi

  if strings -n 6 "$publish_dir/knowledge.pack" | grep -qiE 'kind: |exposure: |adisyon|urun|ürün'; then
    echo "ERROR: knowledge.pack contains readable document text; it is not encrypted." >&2
    status=1
  fi
fi

if grep -rlq 'SEFIM_KNOWLEDGE_KEY=' "$publish_dir" 2>/dev/null; then
  echo "ERROR: a knowledge key literal was found in publish output." >&2
  status=1
fi

if [ -f "$publish_dir/appsettings.Local.json" ]; then
  echo "ERROR: appsettings.Local.json was published; local secrets must stay on the developer machine." >&2
  status=1
fi

if grep -rliqE 'password=|user id=' "$publish_dir"/*.json 2>/dev/null; then
  echo "ERROR: a published settings file contains credentials; use SEFIM_SQL_CONNECTION_STRING instead." >&2
  status=1
fi

if [ "$status" -eq 0 ]; then
  echo "publish knowledge check: OK"
fi

exit "$status"
