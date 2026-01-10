#!/bin/bash

# Build documentation using DocFX

echo ""
echo "========================================"
echo "  JumpStart Documentation Builder"
echo "========================================"
echo ""

# Check if DocFX is installed
if ! command -v docfx &> /dev/null; then
    echo "ERROR: DocFX is not installed or not in PATH"
    echo ""
    echo "To install DocFX, run:"
    echo "  dotnet tool install -g docfx"
    echo ""
    echo "Or update if already installed:"
    echo "  dotnet tool update -g docfx"
    echo ""
    exit 1
fi

# Get DocFX version
echo "Checking DocFX version..."
docfx --version
echo ""

# Clean previous build
if [ -d "_site" ]; then
    echo "Cleaning previous build..."
    rm -rf "_site"
    echo ""
fi

if [ -d "api" ]; then
    echo "Cleaning previous API metadata..."
    rm -rf "api"
    echo ""
fi

if [ -d "obj" ]; then
    echo "Cleaning obj folder..."
    rm -rf "obj"
    echo ""
fi

# Build the documentation
echo "Building documentation..."
echo ""
docfx docfx.json

if [ $? -ne 0 ]; then
    echo ""
    echo "ERROR: Documentation build failed!"
    echo ""
    exit 1
fi

echo ""
echo "========================================"
echo "  Documentation build completed!"
echo "========================================"
echo ""
echo "Output location: _site/"
echo ""
echo "To view the documentation:"
echo "  1. Open _site/index.html in a browser"
echo "  2. Or run: docfx serve _site"
echo ""

# Ask if user wants to serve the docs
read -p "Would you like to serve the documentation now? (y/n): " -n 1 -r
echo ""
if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo ""
    echo "Starting documentation server..."
    echo "Press Ctrl+C to stop the server"
    echo ""
    docfx serve _site
fi
