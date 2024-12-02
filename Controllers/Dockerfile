# Build aşaması
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app

# Proje ve bağımlılık dosyalarını kopyala
COPY *.sln ./
COPY YourProjectName/*.csproj ./YourProjectName/
RUN dotnet restore

# Kaynak kodları kopyala ve derle
COPY . .
WORKDIR /app/YourProjectName
RUN dotnet publish -c Release -o /app/publish

# Runtime aşaması
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Uygulama çalıştırma
ENTRYPOINT ["dotnet", "YourProjectName.dll"]
