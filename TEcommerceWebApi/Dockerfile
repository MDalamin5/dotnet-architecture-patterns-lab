# ==========================================
# 1. BUILD STAGE (.NET 10 SDK)
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies (caching layer)
COPY ["TEcommerceWebApi.csproj", "./"]
RUN dotnet restore "TEcommerceWebApi.csproj"

# Copy the rest of the source code and publish
COPY . .
RUN dotnet publish "TEcommerceWebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ==========================================
# 2. RUNTIME STAGE (Lightweight ASP.NET Runtime)
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Expose HTTP port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Copy compiled binaries from build stage
COPY --from=build /app/publish .

# Run application
ENTRYPOINT ["dotnet", "TEcommerceWebApi.dll"]