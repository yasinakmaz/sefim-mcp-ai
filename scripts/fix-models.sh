#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
project_file="$repo_root/sefim-ai-mcp/sefim-ai-mcp.csproj"
context_file="$repo_root/sefim-ai-mcp/Models/SefimmContext.cs"

if [[ ! -f "$project_file" ]]; then
  echo "Project file not found: $project_file" >&2
  exit 1
fi

if [[ ! -f "$context_file" ]]; then
  echo "Context file not found: $context_file" >&2
  exit 1
fi

perl -0pi -e 's/\R\s*<Compile Remove="Models\\\*\*\\\*\.cs" \/>\R\s*<None Include="Models\\\*\*\\\*\.cs" \/>//g' "$project_file"

if ! grep -q '<Compile Remove="Models\\SefimmContext.cs" />' "$project_file"; then
  perl -0pi -e 's#(\R\s*<Folder Include="Models\\" />)#\n        <Compile Remove="Models\\SefimmContext.cs" />\n        <None Include="Models\\SefimmContext.cs" />$1#' "$project_file"
fi

dotnet build "$project_file"
