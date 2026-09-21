# 🏢 ERP Multi-Empresa

Sistema ERP multi-empresa de grande porte, modular, desenvolvido com Clean Architecture.

## 🛠️ Stack Tecnológica

| Camada          | Tecnologia                     |
|-----------------|--------------------------------|
| Backend         | C# / ASP.NET Core (.NET 8)     |
| Frontend        | Blazor Server + MudBlazor      |
| Banco de Dados  | PostgreSQL 16                  |
| ORM             | Entity Framework Core + Npgsql |
| Autenticação    | ASP.NET Identity + JWT         |
| Containers      | Docker + Docker Compose        |

## 📁 Estrutura do Projeto

```
ERP.sln
├── src/
│   ├── ERP.Domain/           # Entidades, regras de negócio, interfaces
│   ├── ERP.Application/      # Casos de uso, CQRS, DTOs, validações
│   ├── ERP.Infrastructure/   # EF Core, Identity, serviços externos
│   └── ERP.Web/              # Blazor UI, Controllers, Layout
├── tests/
│   └── ...
├── docker-compose.yml
└── README.md
```

## 🚀 Como Rodar

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### 1. Subir a aplicação e o banco com Docker (Recomendado)

```bash
docker compose up -d
```
> As migrações do banco e os dados iniciais (Admin, Empresa Padrão e Cadastros) são aplicados automaticamente no startup.

- **Web (Blazor)**: `http://localhost:5219` ou `http://<SEU_IP_LOCAL>:5219`
- **API Swagger**: `http://localhost:5219/swagger`

### 2. Rodar localmente com .NET SDK (Opcional)

Suba o PostgreSQL:
```bash
docker compose up -d erp-db
```
Execute a aplicação:
```bash
cd src/ERP.Web
dotnet run
```
Acesse: `http://localhost:5219`

### Usuário padrão

| Campo | Valor                    |
|-------|--------------------------|
| Email | admin@erp.com            |
| Senha | Admin@123                |

## 📖 Documentação

- [Arquitetura do Sistema](ARQUITETURA_ERP.md)
- [Roadmap de Desenvolvimento](ROADMAP_ERP.md)

## 📄 Licença

Proprietário — Todos os direitos reservados.
