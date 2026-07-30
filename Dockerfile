FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Instalar CA corporativa (sin apt-get)
COPY certs/corp-root-ca.cer /usr/local/share/ca-certificates/corp-root-ca.crt
RUN update-ca-certificates

# Copiar csproj para restaurar dependencias con caché
COPY ["src/AuthService.Api/AuthService.Api.csproj", "AuthService.Api/"]
COPY ["src/AuthService.Application/AuthService.Application.csproj", "AuthService.Application/"]
COPY ["src/AuthService.Domain/AuthService.Domain.csproj", "AuthService.Domain/"]
COPY ["src/AuthService.Persistence/AuthService.Persistence.csproj", "AuthService.Persistence/"]

RUN dotnet restore "AuthService.Api/AuthService.Api.csproj"

# Copiar el resto del código
COPY . .

# Build
WORKDIR "/src/src/AuthService.Api"
RUN dotnet build "AuthService.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
WORKDIR "/src/src/AuthService.Api"
RUN dotnet publish "AuthService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "AuthService.Api.dll"]