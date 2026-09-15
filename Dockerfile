# Estágio 1: Build da aplicação
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# Instalar Entity Framework CLI globalmente
RUN dotnet tool install -g dotnet-ef

WORKDIR /TaskList.API

# Copiar csproj e restaurar dependências
COPY TaskList.API/TaskList.csproj .
RUN dotnet restore

# Copiar todo o código e publicar
COPY TaskList.API .
RUN dotnet publish -c Release -o /app/publish

# Estágio 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copiar os arquivos publicados
COPY --from=build /app/publish .

# Expor portas
EXPOSE 80
EXPOSE 443

# Entrypoint que inicia a API
ENTRYPOINT ["dotnet", "TaskList.API.dll"]
