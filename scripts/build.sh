#!/bin/bash
echo "Building for production..."
cd src

# Build in Release configuration
dotnet build -c Release
