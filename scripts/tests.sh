#!/bin/bash
echo "Testing and preparing for production..."
cd src

# Run tests
echo "Running tests..."
dotnet test -c Release

# Only proceed with publish if tests pass
if [ $? -eq 0 ]; then
    # Publish for the current runtime
    echo "Publishing API project..."
    dotnet publish PurchaseCart.API/PurchaseCart.API.csproj -c Release -o ../publish

    echo "API is published in ../publish directory"
else
    echo "Tests failed. Build aborted."
    exit 1
fi