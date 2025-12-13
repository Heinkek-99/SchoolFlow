FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["src/SchoolFlow.API/SchoolFlow.API.csproj", "SchoolFlow.API/"]
COPY ["src/SchoolFlow.Application/SchoolFlow.Application.csproj", "SchoolFlow.Application/"]
COPY ["src/SchoolFlow.Infrastructure/SchoolFlow.Infrastructure.csproj", "SchoolFlow.Infrastructure/"]
COPY ["src/SchoolFlow.Domain/SchoolFlow.Domain.csproj", "SchoolFlow.Domain/"]

RUN dotnet restore "SchoolFlow.API/SchoolFlow.API.csproj"

# Copy everything else and build
COPY src/ .
WORKDIR "/src/SchoolFlow.API"
RUN dotnet build "SchoolFlow.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SchoolFlow.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directories for logs and uploads
RUN mkdir -p /app/logs /app/wwwroot/photos /app/wwwroot/documents

ENTRYPOINT ["dotnet", "SchoolFlow.API.dll"]