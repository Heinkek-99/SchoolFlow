# Use the official ASP.NET Core runtime as a base image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj files and restore
COPY ["src/SchoolFlow.Api/SchoolFlow.Api.csproj", "SchoolFlow.Api/"]
COPY ["src/SchoolFlow.Application/SchoolFlow.Application.csproj", "SchoolFlow.Application/"]
COPY ["src/SchoolFlow.Infrastructure/SchoolFlow.Infrastructure.csproj", "SchoolFlow.Infrastructure/"]
COPY ["src/SchoolFlow.Domain/SchoolFlow.Domain.csproj", "SchoolFlow.Domain/"]

RUN dotnet restore "SchoolFlow.Api/SchoolFlow.Api.csproj"

# Copy everything else and build
COPY src/ .

# Build the project
WORKDIR "/app/SchoolFlow.Api"
RUN dotnet build "SchoolFlow.Api.csproj" -c Release -o /app/build

# Publish the project
FROM build AS publish
RUN dotnet publish "SchoolFlow.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Installer curl pour healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published files
COPY --from=publish /app/publish .

# Create directories for logs and uploads
RUN mkdir -p /app/logs /app/wwwroot/photos /app/wwwroot/documents && \
    chmod -R 755 /app

# Script d'attente SQL Server
RUN echo '#!/bin/bash\n\
until nc -z sqlserver 1433; do\n\
  echo "Waiting for SQL Server..."\n\
  sleep 3\n\
done\n\
echo "SQL Server is ready!"\n\
exec "$@"' > /wait.sh && chmod +x /wait.sh


# Exposer les ports
EXPOSE 443
# EXPOSE 8081

# Variables d'environnement par défaut
ENV ASPNETCORE_URLS=https://+:443
ENV ASPNETCORE_HTTP_PORTS=443
ENV ASPNETCORE_ENVIRONMENT=Production

# Set the entry point
ENTRYPOINT ["dotnet", "SchoolFlow.Api.dll"]