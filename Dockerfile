# syntax=docker/dockerfile:1

# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files first so `dotnet restore` is cached independently of source changes.
COPY AssuraBackend.sln .
COPY src/Assura.Domain/Assura.Domain.csproj src/Assura.Domain/
COPY src/Assura.Application/Assura.Application.csproj src/Assura.Application/
COPY src/Assura.Infrastructure/Assura.Infrastructure.csproj src/Assura.Infrastructure/
COPY src/Assura.API/Assura.API.csproj src/Assura.API/
RUN dotnet restore src/Assura.API/Assura.API.csproj

COPY src/ src/
RUN dotnet publish src/Assura.API/Assura.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ---- final ----
# "chiseled" = Microsoft's distroless-equivalent Ubuntu base: no shell, no package manager,
# non-root by default — meaningfully smaller attack surface than the standard aspnet image.
FROM mcr.microsoft.com/dotnet/aspnet:8.0-noble-chiseled AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0

USER app
EXPOSE 8080

ENTRYPOINT ["dotnet", "Assura.API.dll"]
