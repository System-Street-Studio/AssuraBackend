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

# Numeric UID, not the name "app". Kubernetes' runAsNonRoot check has to prove the user is not
# root BEFORE starting the container, and it can only do that from a number — a username means
# nothing to the kubelet, which has no view inside the image's /etc/passwd. With `USER app` the
# pod failed to start outright: "container has runAsNonRoot and image has non-numeric user (app),
# cannot verify user is non-root". $APP_UID is set to 1654 by the chiseled base image, which
# already declared its user numerically; spelling it as a name here was a regression.
USER $APP_UID
EXPOSE 8080

ENTRYPOINT ["dotnet", "Assura.API.dll"]
