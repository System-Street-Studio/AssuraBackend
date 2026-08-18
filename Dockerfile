# Stage 1: Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files for layer caching
COPY *.sln ./
COPY src/Assura.Domain/*.csproj ./src/Assura.Domain/
COPY src/Assura.Application/*.csproj ./src/Assura.Application/
COPY src/Assura.Infrastructure/*.csproj ./src/Assura.Infrastructure/
COPY src/Assura.API/*.csproj ./src/Assura.API/

# Restore dependencies
RUN dotnet restore src/Assura.API/Assura.API.csproj

# Copy the entire source code
COPY src/ ./src/

# Publish the release build
RUN dotnet publish src/Assura.API/Assura.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Production runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Configure default port for Railway
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

# Copy compiled artifacts from build stage
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Assura.API.dll"]
