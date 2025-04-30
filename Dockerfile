# 1. Build aşaması
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyalarını kopyala ve restore et
COPY UrlShortener.API/ UrlShortener.API/
COPY UrlShortener.Domain/ UrlShortener.Domain/
COPY url-shortener-ui/ url-shortener-ui/
COPY *.sln ./
WORKDIR /src/UrlShortener.API
RUN dotnet restore

# Projeyi publish et
RUN dotnet publish -c Release -o /app/publish

# 2. Runtime aşaması
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Gerekirse portu ayarla (varsayılan: 80)
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

# Uygulamayı başlat
ENTRYPOINT ["dotnet", "UrlShortener.API.dll"] 