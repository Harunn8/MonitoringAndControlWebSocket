# Build aşaması
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app

# Proje ve bağımlılık dosyalarını kopyala
COPY *.sln ./
COPY Controllers/Controllers.csproj ./Controllers/
COPY Interfaces/Interfaces.csproj ./Interfaces/
COPY Models/Models.csproj ./Models/
COPY Services/Services.csproj ./Services/
COPY Infrastructure/Infrastructure.csproj ./Infrastructure/

# Bağımlılıkları yükle
RUN dotnet restore "./Controllers/Controllers.csproj"

# Kaynak kodları kopyala ve derle
COPY . .
WORKDIR /app/Controllers
RUN dotnet publish "./Controllers.csproj" -c Release -o /app/publish

# Runtime aşaması
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Uygulama çalıştırma
ENTRYPOINT ["dotnet", "Controllers.dll"]
