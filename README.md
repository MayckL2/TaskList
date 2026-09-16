# 🚀 TaskList API

[![Build and Test](https://github.comayckl2io/tasklipi/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/seu-usuario/tasklist-api/actions/workflows/build-and-test.yml)

![Capa da TaskList API](https://i.imgur.com/JLpeWyF.png)

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

