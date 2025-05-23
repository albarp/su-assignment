FROM mcr.microsoft.com/dotnet/sdk:8.0 as dev
WORKDIR /mnt

ENV ASPNETCORE_ENVIRONMENT=Development \
    ASPNETCORE_HTTP_PORTS=9090 \
    Logging__LogLevel__Default=Information \
    Logging__LogLevel__Microsoft.AspNetCore=Warning

