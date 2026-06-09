# Usa uma imagem base do SDK do .NET para compilar o projeto
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Caso não existe pasta src, apontar para /app ou qualquer nome personalizado:
# WORkDIR /app

# Copia o arquivo de projeto e restaura as dependências
COPY ["src/TaskList.csproj", "."]
RUN dotnet restore

# Copia o resto do código e publica a aplicação
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Cria a imagem final, mais leve, para rodar a aplicação
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Diz ao container qual porta será exposta e qual comando executar ao iniciar
EXPOSE 80
ENTRYPOINT ["dotnet", "TaskList.dll"]