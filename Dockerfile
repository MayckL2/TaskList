# Estágio 1: Build da aplicação
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar csproj e restaurar dependências
COPY ["src/TaskList.csproj", "."]
RUN dotnet restore

# Copiar todo o código e publicar
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Estágio 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copiar os arquivos publicados
COPY --from=build /app/publish .

# 🔥 Instalar ferramentas do EF Core para migrations
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# Criar script de entrypoint para rodar migrations e iniciar a API
RUN echo '#!/bin/sh\n\
echo "⏳ Aguardando banco de dados ficar disponível..."\n\
until dotnet ef database update --no-build; do\n\
  echo "🔄 Tentando novamente em 5 segundos..."\n\
  sleep 5\n\
done\n\
echo "✅ Migrations aplicadas com sucesso!"\n\
exec dotnet TaskList.dll' > /entrypoint.sh && chmod +x /entrypoint.sh

# Expor portas
EXPOSE 80
EXPOSE 443

# Entrypoint que roda migrations e inicia a API
ENTRYPOINT ["/entrypoint.sh"]