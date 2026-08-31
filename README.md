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

### 1. Subir o banco de dados

```bash
docker-compose up -d
```

### 2. Aplicar migrations

```bash
cd src/ERP.Web
dotnet ef database update --project ../ERP.Infrastructure
```

### 3. Rodar a aplicação

```bash
cd src/ERP.Web
dotnet run
```

Acesse: `https://localhost:5001`

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
