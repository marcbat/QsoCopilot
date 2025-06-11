# Dockerfile pour QSO Manager API
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copier les fichiers de projet
COPY ["QsoManager.Api/QsoManager.Api.csproj", "QsoManager.Api/"]
COPY ["QsoManager.Application/QsoManager.Application.csproj", "QsoManager.Application/"]
COPY ["QsoManager.Domain/QsoManager.Domain.csproj", "QsoManager.Domain/"]
COPY ["QsoManager.Infrastructure/QsoManager.Infrastructure.csproj", "QsoManager.Infrastructure/"]

# Restaurer les dépendances
RUN dotnet restore "QsoManager.Api/QsoManager.Api.csproj"

# Copier tout le code source
COPY . .

# Construire l'application
WORKDIR "/src/QsoManager.Api"
RUN dotnet build "QsoManager.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "QsoManager.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QsoManager.Api.dll"]
