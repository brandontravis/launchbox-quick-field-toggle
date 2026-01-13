#!/bin/bash
# QuickFieldToggle Release Builder
# 
# Usage: ./build-release.sh path/to/QuickFieldToggle.dll [version]
# Example: ./build-release.sh ~/Desktop/QuickFieldToggle.dll 1.0.0
#
# The DLL must be built on Windows first, then copy it to your Mac.

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
DLL_PATH="$1"
VERSION="${2:-1.0.0}"

if [ -z "$DLL_PATH" ]; then
    echo "Usage: $0 <path-to-dll> [version]"
    echo ""
    echo "Example:"
    echo "  $0 ~/Desktop/QuickFieldToggle.dll 1.0.0"
    echo ""
    echo "Build the DLL on Windows first, then copy it to your Mac."
    exit 1
fi

if [ ! -f "$DLL_PATH" ]; then
    echo "Error: DLL not found at: $DLL_PATH"
    exit 1
fi

echo "Building QuickFieldToggle v$VERSION..."

# Clean and create output directory
OUTPUT_DIR="$SCRIPT_DIR/output"
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR/QuickFieldToggle/icons"

# Copy files
echo "Packaging files..."
cp "$DLL_PATH" "$OUTPUT_DIR/QuickFieldToggle/"
cp "$SCRIPT_DIR/release/README.md" "$OUTPUT_DIR/QuickFieldToggle/"
cp "$SCRIPT_DIR/release/quickfieldtoggle.sample.json" "$OUTPUT_DIR/QuickFieldToggle/"

# Create zip
echo "Creating zip..."
cd "$OUTPUT_DIR"
ZIP_NAME="QuickFieldToggle_v$VERSION.zip"
rm -f "$ZIP_NAME"
zip -r "$ZIP_NAME" QuickFieldToggle

echo ""
echo "SUCCESS!"
echo "Output: $OUTPUT_DIR/$ZIP_NAME"
echo ""
echo "Contents:"
echo "  QuickFieldToggle/"
echo "  ├── QuickFieldToggle.dll"
echo "  ├── README.md"
echo "  ├── quickfieldtoggle.sample.json"
echo "  └── icons/"
echo ""
echo "Upload this zip to GitHub Releases!"

