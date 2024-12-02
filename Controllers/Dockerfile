# Base Image
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build Aþamasý
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Proje Dosyalarýný Kopyala ve Restore Et
COPY ["src/Controllers/Controllers.csproj", "Controllers/"]
COPY ["src/Interfaces/Interfaces.csproj", "Interfaces/"]
COPY ["src/Models/Models.csproj", "Models/"]
COPY ["src/Services/Services.csproj", "Services/"]
RUN dotnet restore "Controllers/Controllers.csproj"

# Kaynak Kodlarý Kopyala ve Build Et
COPY . .
WORKDIR "/src/Controllers"
RUN dotnet build "Controllers.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish Aþamasý
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Controllers.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final Image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Controllers.dll"]
