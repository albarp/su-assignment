# Container for development
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dev
WORKDIR /mnt

# TODO: putted here for convenience of the assignament.
ENV ASPNETCORE_ENVIRONMENT=Development \
    ASPNETCORE_HTTP_PORTS=9090 \
    Logging__LogLevel__Default=Information \
    Logging__LogLevel__Microsoft.AspNetCore=Warning

# Just for documentation
EXPOSE 9090

# TODO: move publish to production here, as a stage
