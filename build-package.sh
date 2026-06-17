#!/usr/bin/env bash
# build-package.sh — build the module in Release mode and zip the output for
# upload via the host's /admin/modules page.
#
# Usage:  ./build-package.sh
# Output: dist/FlexCms.InvestPro-{version}.zip
#
# Requires: .NET 10 SDK, zip (or PowerShell's Compress-Archive on WSL/Git Bash)
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

MODULE_ID="FlexCms.InvestPro"
CSPROJ="$MODULE_ID.csproj"
MANIFEST="module.json"

if [[ ! -f "$CSPROJ" ]]; then
  echo "ERROR: $CSPROJ not found in $SCRIPT_DIR" >&2
  exit 1
fi

# Read version out of module.json so the zip name matches the release.
VERSION=$(grep -E '"Version"' "$MANIFEST" | head -1 | sed -E 's/.*"Version":\s*"([^"]+)".*/\1/')
VERSION="${VERSION:-unknown}"

echo "==> Building $MODULE_ID v$VERSION (Release)"
dotnet build "$CSPROJ" -c Release --nologo

OUT_DIR="bin/Release/net10.0"
if [[ ! -f "$OUT_DIR/$MODULE_ID.dll" ]]; then
  echo "ERROR: build did not produce $OUT_DIR/$MODULE_ID.dll" >&2
  exit 1
fi

STAGING="$(mktemp -d -t fcms-pkg-XXXXXX)"
trap 'rm -rf "$STAGING"' EXIT
STAGED="$STAGING/$MODULE_ID"
mkdir -p "$STAGED"

echo "==> Staging output → $STAGED"
cp -r "$OUT_DIR/." "$STAGED/"

# Framework-side DLLs the host already supplies. Bundling them risks type
# identity bugs because the host's loader will pick whichever copy loads first.
echo "==> Removing host-provided framework files"
rm -f "$STAGED/FlexCms.Framework.dll" "$STAGED/FlexCms.Framework.pdb"

mkdir -p dist
ZIP_PATH="dist/$MODULE_ID-$VERSION.zip"
rm -f "$ZIP_PATH"

echo "==> Zipping → $ZIP_PATH"
if command -v zip >/dev/null 2>&1; then
  (cd "$STAGING" && zip -r -q "$SCRIPT_DIR/$ZIP_PATH" "$MODULE_ID")
elif command -v powershell.exe >/dev/null 2>&1; then
  # Git Bash on Windows: powershell.exe can't read /c/... or /d/... paths.
  # Convert via cygpath when available, otherwise rely on cmd's WSLENV-style
  # absolute path resolution.
  if command -v cygpath >/dev/null 2>&1; then
    WIN_STAGED=$(cygpath -w "$STAGED")
    WIN_ZIP=$(cygpath -w "$SCRIPT_DIR/$ZIP_PATH")
  else
    WIN_STAGED="$STAGED"
    WIN_ZIP="$SCRIPT_DIR/$ZIP_PATH"
  fi
  powershell.exe -NoProfile -Command "Compress-Archive -Path '$WIN_STAGED\\*' -DestinationPath '$WIN_ZIP' -Force"
else
  echo "ERROR: neither 'zip' nor 'powershell.exe' available to create archive" >&2
  exit 1
fi

SIZE=$(du -h "$ZIP_PATH" | cut -f1)
echo
echo "✓ Built $ZIP_PATH ($SIZE)"
echo "  Upload via the host's /admin/modules page, then restart the host."
