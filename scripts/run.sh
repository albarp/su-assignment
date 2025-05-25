#!/bin/bash

# Set environment variables
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_HTTP_PORTS=9090

cd publish
dotnet PurchaseCart.API.dll