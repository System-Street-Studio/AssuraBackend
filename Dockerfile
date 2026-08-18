# Stage 1: Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files first for optimal layer caching
COPY ["src/Assura.Domain/Assura.Domain.csproj", "src/Assura.Domain/"]
COPY ["src/Assura.Application/Assura.Application.csproj", "src/Assura.Application/"]
COPY ["src/Assura.Infrastructure/Assura.Infrastructure.csproj", "src/Assura.Infrastructure/"]
COPY ["src/Assura.API/Assura.API.csproj", "src/Assura.API/"]

# Restore NuGet packages
RUN dotnet restore "src/Assura.API/Assura.API.csproj"

# Copy source tree and compile
COPY src/ src/
WORKDIR "/src/src/Assura.API"
RUN dotnet publish "Assura.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Production Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Ensure ASP.NET Core listens on port 8080 (Railway default container port)
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

# Copy published artifacts from build stage
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Assura.API.dll"]
