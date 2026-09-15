<<<<<<< HEAD
![GitHub repo size](https://img.shields.io/github/repo-size/mayckl2/README-template?style=for-the-badge)
![Bitbucket open issues](https://img.shields.io/bitbucket/issues/mayckl2/README-template?style=for-the-badge)
![Bitbucket open pull requests](https://img.shields.io/bitbucket/pr-raw/mayckl2/README-template?style=for-the-badge)

<img width="1304" height="410" alt="image" src="https://github.com/user-attachments/assets/ed3c13ce-c341-4e4e-9ce0-3f3c6a38a8c8" />

> Aplicação de uma web api rest que realiza as funções de uma lista de tarefas, incluindo criar, editar, deletar e completar tarefas. Todas as alterações são armazenas em um banco de dados para futuras consultas.
> Projeto documentado via swagger para futuras implementações.

## Ajustes e melhorias

O projeto ainda está em desenvolvimento e as próximas atualizações serão voltadas para as seguintes tarefas:

- [x] Autenticação JWT
- [x] Rotas autorizadas para usuarios autenticados
- [ ] Refresh token
- [ ] Transferir projeto para uma imagem docker
- [ ] Authenticação por roles e claims

## 💻 Pré-requisitos

Antes de começar, verifique se você atendeu aos seguintes requisitos:

- Você instalou a versão mais recente de `<C# / ASP.Net Core / Git>`
- Você tem uma máquina `<Windows / Linux / Mac>`. Indique qual sistema operacional é compatível / não compatível.

## 🚀 Instalando TaskList

Para instalar o TaskList, siga estas etapas:

### Windows:

1. Instalar o .NET 10 SDK
Baixe e instale o SDK mais recente do .NET 10 em:
🔗 dotnet.microsoft.com/en-us/download/dotnet/10.0

Verificar instalação:

```powershell
dotnet --version
# Deve mostrar algo como: 10.0.100
```

2. Instalar o SQL Server
Opção 1: SQL Server LocalDB (recomendado para desenvolvimento)

powershell
### Instalar via winget
```powershell
winget install Microsoft.SqlServer.LocalDB
```
Opção 2: SQL Server Express
Baixe em: 🔗 microsoft.com/pt-br/sql-server/sql-server-downloads

3. Instalar o Git (opcional)
powershell
```powershell
winget install Git.Git
```
🚀 Executar a API
```powershell
# 1. Clonar o repositório (ou extrair os arquivos)

git clone https://github.com/seu-usuario/tasklist-api.git
cd tasklist-api

# 2. Restaurar pacotes
dotnet restore

# 3. Configurar o banco de dados
dotnet ef database update

# 4. Executar a API
dotnet run

# A API estará disponível em:
# HTTP: http://localhost:5245
# HTTPS: https://localhost:5245
# Swagger: http://localhost:5245/swagger
```

### Linux (Ubuntu/Debian/Fedora):

1. Instalar o .NET 10 SDK
Ubuntu/Debian:

```bash
# Adicionar repositório Microsoft
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
```

### Instalar SDK
```bash
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0
```

Fedora:

```bash
# Adicionar repositório Microsoft
sudo rpm --import https://packages.microsoft.com/keys/microsoft.asc
sudo wget -O /etc/yum.repos.d/microsoft-prod.repo https://packages.microsoft.com/config/fedora/38/prod.repo

### Instalar SDK
sudo dnf install dotnet-sdk-10.0
```

Verificar instalação:

```bash
dotnet --version
# Deve mostrar algo como: 10.0.100
```

2. Instalar o SQL Server no Linux
```bash
Ubuntu 22.04:

# Importar chave GPG
curl https://packages.microsoft.com/keys/microsoft.asc | sudo tee /etc/apt/trusted.gpg.d/microsoft.asc

# Adicionar repositório
sudo add-apt-repository "$(curl https://packages.microsoft.com/config/ubuntu/22.04/mssql-server-2022.list)"

# Instalar SQL Server
sudo apt-get update
sudo apt-get install -y mssql-server

# Configurar (definir senha SA)
sudo /opt/mssql/bin/mssql-conf setup

# Instalar ferramentas de linha de comando
sudo apt-get install -y mssql-tools18
```

3. Instalar Git

```bash
sudo apt-get update && sudo apt-get install git
```

### ☕ Usando TaskList

Para usar TaskList, siga estas etapas:

```bash
# 1. Clonar repositório
git clone https://github.com/seu-usuario/tasklist-api.git
cd tasklist-api

# 2. Restaurar pacotes
dotnet restore

# 3. Ajustar string de conexão no appsettings.json
# Altere "Server=localhost;..." (SQL Server já está rodando)

# 4. Criar banco e aplicar migrations
dotnet ef database update

# 5. Executar a API
dotnet run --urls="http://0.0.0.0:5000"

# Ou com HTTPS (precisa configurar certificado)
# dotnet run

# A API estará disponível em:
# HTTP: http://localhost:5245
# Swagger: http://localhost:5245/swagger
```

### macOS:

Instalar o .NET 10 SDK

```bash
# Instalar via Homebrew
brew install --cask dotnet-sdk
```

### Ou baixar do site oficial
#### 🔗 https://dotnet.microsoft.com/en-us/download/dotnet/10.0
Verificar instalação:

```bash
dotnet --version
# Deve mostrar algo como: 10.0.100
```

2. Instalar o SQL Server no macOS
Como o SQL Server não tem suporte nativo para macOS, as alternativas são:

```bash
# Instalar PostgreSQL (alternativa)
brew install postgresql@15
brew services start postgresql@15

# Criar banco
createdb TaskListDb
```

3. Instalar Git
```bash
brew install git
```

🚀 Executar a API
```bash
# 1. Clonar repositório
git clone https://github.com/seu-usuario/tasklist-api.git
cd tasklist-api

# 2. Restaurar pacotes
dotnet restore

# 3. Ajustar string de conexão no appsettings.json
# Se for usar SQLite, configure conforme acima

# 4. Criar banco e aplicar migrations
dotnet ef database update

# 5. Executar a API
dotnet run

# A API estará disponível em:
# HTTP: http://localhost:5245
# HTTPS: https://localhost:5245
# Swagger: http://localhost:5245/swagger
```

## 😄 Seja um dos contribuidores

Quer fazer parte desse projeto? Clique [AQUI](CONTRIBUTING.md) e leia como contribuir.

## 📝 Licença

Esse projeto está sob licença. Veja o arquivo [LICENÇA](LICENSE.md) para mais detalhes.
=======
![Capa da TaskList API](https://i.imgur.com/sua-imagem-aqui.png)

# 🚀 TaskList API

> API RESTful completa para gerenciamento de tarefas, construída com **.NET 10**, autenticação JWT, cache distribuído, monitoramento e muito mais.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-13.0-239120?style=for-the-badge&logo=c-sharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?style=for-the-badge&logo=microsoft-sql-server)](https://www.microsoft.com/sql-server)
[![Redis](https://img.shields.io/badge/Redis-7.0-DC382D?style=for-the-badge&logo=redis)](https://redis.io/)
[![Docker](https://img.shields.io/badge/Docker-24.0-2496ED?style=for-the-badge&logo=docker)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge)](LICENSE)

---

## 📋 Sobre o Projeto

A **TaskList API** é uma solução completa para gerenciamento de tarefas, desenvolvida como projeto de estudo e portfólio para demonstrar boas práticas de desenvolvimento backend com .NET.

### ✨ Principais Funcionalidades

- 🔐 **Autenticação e Autorização** com JWT e ASP.NET Core Identity
- 👥 **Gerenciamento de Usuários e Roles** (Admin, Manager, User)
- ✅ **CRUD completo de Tarefas**
- ⚡ **Cache Distribuído** com Redis
- 📊 **Monitoramento de Jobs** com Hangfire
- 📈 **Health Checks** para API e Banco de Dados
- 🎯 **GraphQL** para consultas flexíveis (HotChocolate)
- 📝 **Logs Estruturados** com Serilog + Seq
- 🐳 **Containerização** com Docker e Docker Compose
- 📚 **Documentação Interativa** com Scalar (OpenAPI 3.1)
- 🧪 **Testes Unitários e de Integração** (xUnit, NSubstitute, Bogus, Playwright)

---

## 🛠️ Tecnologias Utilizadas

### Backend
| Tecnologia | Versão | Descrição |
|------------|--------|-----------|
| **.NET** | 10.0 | Framework principal |
| **C#** | 13.0 | Linguagem de programação |
| **ASP.NET Core** | 10.0 | Framework web |
| **Entity Framework Core** | 10.0 | ORM para banco de dados |
| **SQL Server** | 2022 | Banco de dados relacional |
| **Redis** | 7.0 | Cache distribuído |
| **Hangfire** | 1.8 | Processamento de jobs em background |
| **HotChocolate** | 16.x | GraphQL server |
| **Serilog** | 8.0 | Logs estruturados |
| **Scalar** | 1.x | Documentação da API |
| **Docker** | 24.0 | Containerização |

### Testes
| Tecnologia | Descrição |
|------------|-----------|
| **xUnit** | Framework de testes |
| **NSubstitute** | Mocking |
| **Bogus** | Geração de dados falsos |

---

## 🏗️ Arquitetura

O projeto segue os princípios da **Clean Architecture**, com separação clara de responsabilidades:

# 🐳 Docker

### Comandos Úteis

```bash
# Subir os containers
docker compose up -d

# Ver logs
docker compose logs -f api

# Parar os containers
docker compose down

# Parar e remover volumes (limpa banco e redis)
docker compose down -v

# Rebuild da API
docker compose build api
```

### Estrutura dos Containers

| Container | Imagem | Porta | Descrição |
|-----------|--------|-------|-----------|
| **tasklist-api** | Custom | 8080 | API .NET 10 |
| **tasklist-db** | SQL Server 2022 | 1433 | Banco de dados |
| **tasklist-redis** | Redis Alpine | 6379 | Cache |

---

# 📈 Monitoramento

### Seq (Logs)

- **URL:** [http://localhost:5341](http://localhost:5341)
- **Função:** Visualização e busca de logs estruturados

- **Login/Senha**: admin
### Hangfire Dashboard (Jobs)

- **URL:** [http://localhost:8080/hangfire](http://localhost:8080/hangfire)
- **Login:** `admin`
- **Senha:** `Hangfire@123`
- **Função:** Monitoramento de jobs em background

### Health Checks

- **URL:** [http://localhost:8080/health](http://localhost:8080/health)
- **Função:** Verificação de saúde da API e dependências

---

# 🤝 Contribuindo

Contribuições são bem-vindas! Sinta-se à vontade para:

1. Fazer um **Fork** do projeto
2. Criar uma **branch** para sua feature (`git checkout -b feature/nova-feature`)
3. **Commit** suas mudanças (`git commit -m 'Adiciona nova feature'`)
4. **Push** para a branch (`git push origin feature/nova-feature`)
5. Abrir um **Pull Request**

---

# 📄 Licença

Este projeto está sob a licença **MIT**. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

---

# 👨‍💻 Autor

**[Seu Nome]**

- **GitHub:** [@MayckL2](https://github.com/mayckl2 )
- **LinkedIn:** [https://www.linkedin.com/in/mayck-luciano](https://linkedin.com/in/seu-perfil)
- **Email:** mayckluciano2@gmail.com

---


**⭐ Se este projeto te ajudou, considere dar uma estrela! ⭐**

Feito com ❤️ por [Mayck]

>>>>>>> developer
