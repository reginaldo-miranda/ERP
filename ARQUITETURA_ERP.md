# 🏗️ Arquitetura do ERP Multi-Empresa

> Documento de referência da arquitetura técnica do sistema ERP.
> Versão: 1.0 | Data: 2026-08-29

---

## 1. Visão Geral

Sistema ERP multi-empresa de grande porte, modular, desenvolvido para operar em nuvem.
O sistema permite que múltiplas empresas compartilhem a mesma instância da aplicação,
com isolamento de dados por `EmpresaId` e controle granular de permissões por módulo e ação.

### 1.1 Objetivos

- **Multi-empresa**: Uma única instalação atendendo múltiplas empresas
- **Modular**: Cada módulo pode ser desenvolvido, testado e evoluído independentemente
- **Escalável**: Preparado para crescer conforme demanda
- **Auditável**: Toda alteração é rastreada automaticamente
- **Integrável**: Comunicação com sistema contábil existente via API REST
- **Cloud-agnostic**: Pode ser hospedado em qualquer provedor de nuvem

---

## 2. Stack Tecnológica

| Camada          | Tecnologia                          |
|-----------------|-------------------------------------|
| **Backend**     | C# / ASP.NET Core (.NET 8+)        |
| **Frontend**    | Blazor (Server/WebAssembly)         |
| **UI Library**  | MudBlazor (Material Design)         |
| **Banco de Dados** | PostgreSQL                       |
| **ORM**         | Entity Framework Core + Npgsql      |
| **Autenticação**| ASP.NET Identity + JWT              |
| **Cache**       | Redis (futuro) / MemoryCache (inicial) |
| **Mensageria**  | MediatR (in-process) / RabbitMQ (futuro) |
| **Containers**  | Docker + Docker Compose             |
| **CI/CD**       | GitHub Actions                      |
| **Versionamento** | Git + GitHub (repositório privado)|

---

## 3. Arquitetura de Software — Clean Architecture

O projeto segue o padrão **Clean Architecture**, organizado em 4 camadas concêntricas.
As dependências sempre apontam para dentro (das camadas externas para as internas).

```
┌─────────────────────────────────────────────────────────────────┐
│                      PRESENTATION                               │
│          Blazor UI  ·  Controllers  ·  ViewModels               │
│          Middlewares  ·  Filters  ·  SignalR Hubs               │
├─────────────────────────────────────────────────────────────────┤
│                      INFRASTRUCTURE                             │
│      EF Core DbContext  ·  Repositories  ·  External APIs      │
│      Identity  ·  JWT  ·  Email  ·  File Storage               │
│      Integração Contábil  ·  SEFAZ  ·  OFX Parser              │
├─────────────────────────────────────────────────────────────────┤
│                      APPLICATION                                │
│      Use Cases  ·  DTOs  ·  Validators  ·  Mappings            │
│      CQRS (Commands/Queries)  ·  MediatR Handlers              │
│      Interfaces de Serviços  ·  Event Handlers                  │
├─────────────────────────────────────────────────────────────────┤
│                        DOMAIN                                   │
│      Entities  ·  Value Objects  ·  Enums                       │
│      Domain Events  ·  Interfaces  ·  Business Rules            │
│      Aggregates  ·  Specifications                              │
└─────────────────────────────────────────────────────────────────┘
```

### 3.1 Camada Domain (Núcleo)

- **Sem dependências externas** — não referencia EF Core, ASP.NET ou qualquer framework
- Contém as **entidades de negócio**, **value objects**, **enums** e **regras de domínio**
- Define **interfaces** que serão implementadas pelas camadas externas
- Emite **Domain Events** para comunicação desacoplada entre módulos

### 3.2 Camada Application

- Orquestra os **casos de uso** do sistema
- Implementa o padrão **CQRS** (Command Query Responsibility Segregation) com **MediatR**
- Contém **Commands** (ações que alteram estado) e **Queries** (consultas)
- Define **DTOs** (Data Transfer Objects) para entrada e saída de dados
- Contém **validações** de entrada com FluentValidation
- Processa **Domain Events** e dispara ações em outros módulos

### 3.3 Camada Infrastructure

- Implementa as **interfaces definidas no Domain**
- Contém o **DbContext** do Entity Framework Core
- Implementa os **Repositories**
- Integra com **serviços externos**: SEFAZ, API contábil, parsers OFX
- Configura **Identity** e geração de **JWT**
- Implementa o **interceptor de auditoria** automática

### 3.4 Camada Presentation

- Interface com o usuário via **Blazor**
- **Controllers/Endpoints** da API REST
- **ViewModels** e componentes Blazor
- **Middlewares** de tratamento de erros, logging e multi-tenancy
- **SignalR Hubs** para notificações em tempo real

---

## 4. Estrutura de Projetos (Solution)

```
ERP.sln
│
├── src/
│   │
│   ├── ERP.Domain/                        # Camada de Domínio
│   │   ├── Common/                        # Base classes, interfaces comuns
│   │   │   ├── BaseEntity.cs
│   │   │   ├── BaseAuditableEntity.cs
│   │   │   ├── IAggregateRoot.cs
│   │   │   ├── IDomainEvent.cs
│   │   │   └── IRepository.cs
│   │   ├── Cadastros/                     # Módulo Cadastros
│   │   │   ├── Entities/
│   │   │   ├── ValueObjects/
│   │   │   ├── Enums/
│   │   │   └── Interfaces/
│   │   ├── Financeiro/                    # Módulo Financeiro
│   │   ├── Vendas/                        # Módulo Vendas
│   │   ├── Compras/                       # Módulo Compras
│   │   ├── Estoque/                       # Módulo Estoque
│   │   ├── Fiscal/                        # Módulo Fiscal
│   │   └── Contabilidade/                 # Integração Contábil
│   │       ├── Entities/
│   │       │   ├── EventoContabil.cs
│   │       │   └── MapeamentoContabil.cs
│   │       ├── Enums/
│   │       │   └── TipoEventoContabil.cs
│   │       └── Interfaces/
│   │           └── IEventoContabilRepository.cs
│   │
│   ├── ERP.Application/                   # Camada de Aplicação
│   │   ├── Common/                        # Behaviors, Exceptions, Mappings
│   │   │   ├── Behaviors/
│   │   │   │   ├── ValidationBehavior.cs
│   │   │   │   ├── LoggingBehavior.cs
│   │   │   │   └── AuditBehavior.cs
│   │   │   ├── Exceptions/
│   │   │   ├── Interfaces/
│   │   │   │   ├── IApplicationDbContext.cs
│   │   │   │   └── ICurrentUserService.cs
│   │   │   └── Mappings/
│   │   ├── Cadastros/
│   │   │   ├── Commands/
│   │   │   │   ├── CriarEmpresa/
│   │   │   │   │   ├── CriarEmpresaCommand.cs
│   │   │   │   │   ├── CriarEmpresaCommandHandler.cs
│   │   │   │   │   └── CriarEmpresaCommandValidator.cs
│   │   │   │   └── .../
│   │   │   ├── Queries/
│   │   │   │   ├── ListarEmpresas/
│   │   │   │   └── .../
│   │   │   ├── DTOs/
│   │   │   └── EventHandlers/
│   │   ├── Financeiro/
│   │   ├── Vendas/
│   │   ├── Compras/
│   │   ├── Estoque/
│   │   ├── Fiscal/
│   │   └── Contabilidade/
│   │
│   ├── ERP.Infrastructure/                # Camada de Infraestrutura
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Configurations/            # Fluent API configs por entidade
│   │   │   ├── Interceptors/
│   │   │   │   ├── AuditableEntityInterceptor.cs
│   │   │   │   └── MultiTenancyInterceptor.cs
│   │   │   ├── Migrations/
│   │   │   └── Repositories/
│   │   ├── Identity/
│   │   │   ├── IdentityService.cs
│   │   │   ├── JwtTokenService.cs
│   │   │   └── PermissionService.cs
│   │   ├── ExternalServices/
│   │   │   ├── ContabilidadeApiClient.cs  # Integração com sistema contábil
│   │   │   ├── SefazService.cs            # Comunicação NF-e/NFC-e
│   │   │   └── OfxParserService.cs        # Parser de arquivos OFX
│   │   └── DependencyInjection.cs
│   │
│   ├── ERP.Web/                           # Camada de Apresentação (Blazor)
│   │   ├── Components/
│   │   │   ├── Layout/
│   │   │   │   ├── MainLayout.razor
│   │   │   │   ├── NavMenu.razor
│   │   │   │   └── TopBar.razor           # Seletor de empresa + notificações
│   │   │   ├── Shared/                    # Componentes reutilizáveis
│   │   │   └── Modules/
│   │   │       ├── Cadastros/
│   │   │       ├── Financeiro/
│   │   │       ├── Vendas/
│   │   │       ├── Compras/
│   │   │       ├── Estoque/
│   │   │       ├── Fiscal/
│   │   │       └── Dashboard/
│   │   ├── Services/                      # Serviços do Blazor (HttpClient, State)
│   │   ├── wwwroot/
│   │   └── Program.cs
│   │
│   └── ERP.PDV/                           # Módulo PDV (aplicação separada)
│       ├── Components/
│       ├── Services/
│       │   ├── OfflineService.cs          # IndexedDB / Service Worker
│       │   └── SyncService.cs             # Sincronização com backend
│       └── Program.cs
│
├── tests/
│   ├── ERP.Domain.Tests/
│   ├── ERP.Application.Tests/
│   ├── ERP.Infrastructure.Tests/
│   └── ERP.Web.Tests/
│
├── docker-compose.yml
├── .github/
│   └── workflows/
│       ├── build.yml
│       └── deploy.yml
├── ARQUITETURA_ERP.md
├── ROADMAP_ERP.md
└── README.md
```

---

## 5. Multi-Tenancy (Multi-Empresa)

### 5.1 Estratégia: Banco Único com Discriminador

Todas as entidades que pertencem a uma empresa herdam de `BaseEmpresaEntity`:

```csharp
public abstract class BaseEmpresaEntity : BaseAuditableEntity
{
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
}
```

### 5.2 Filtro Global Automático

O `DbContext` aplica um **Global Query Filter** em todas as entidades multi-empresa,
garantindo que um usuário **nunca** veja dados de outra empresa por acidente:

```csharp
// No ApplicationDbContext
protected override void OnModelCreating(ModelBuilder builder)
{
    // Para cada entidade que herda BaseEmpresaEntity:
    builder.Entity<Cliente>().HasQueryFilter(x => x.EmpresaId == _currentEmpresaId);
    builder.Entity<Produto>().HasQueryFilter(x => x.EmpresaId == _currentEmpresaId);
    // ... aplicado automaticamente via reflection
}
```

### 5.3 Seleção de Empresa

- O usuário seleciona a empresa ativa no **TopBar** do layout
- O `EmpresaId` ativo fica armazenado no **claim do JWT** e na **sessão do Blazor**
- Todo request ao backend carrega o `EmpresaId` via middleware de multi-tenancy

---

## 6. Autenticação e Autorização

### 6.1 Modelo de Permissões

```
Usuário
  └── pertence a N Empresas (via UsuarioEmpresa)
        └── em cada empresa, tem N Papéis (Roles)
              └── cada Papel tem N Permissões
                    └── cada Permissão = "Modulo.Acao"
```

Exemplos de permissões:

| Permissão                  | Descrição                              |
|----------------------------|----------------------------------------|
| `Cadastros.Empresas.Criar` | Pode criar novas empresas              |
| `Vendas.Pedidos.Visualizar`| Pode visualizar pedidos de venda       |
| `Vendas.Pedidos.Excluir`   | Pode excluir pedidos de venda          |
| `Financeiro.ContasPagar.Aprovar` | Pode aprovar pagamentos           |
| `Estoque.Inventario.Executar` | Pode executar inventário            |
| `Admin.Usuarios.Gerenciar` | Pode gerenciar usuários e permissões   |

### 6.2 Fluxo de Autenticação

```
┌──────────┐     POST /api/auth/login      ┌──────────────┐
│  Blazor  │ ──────────────────────────────>│  AuthController│
│  Client  │     { email, senha }           │              │
│          │<──────────────────────────────  │  Valida via  │
│          │     { token JWT, refresh }     │  Identity    │
└──────────┘                                └──────────────┘
     │
     │  Token JWT contém:
     │  - UserId
     │  - EmpresaId (empresa ativa)
     │  - EmpresaIds[] (empresas permitidas)
     │  - Permissões[]
     │
     ▼
  Cada request subsequente envia o JWT no header Authorization
```

---

## 7. Integração Contábil Centralizada

### 7.1 Arquitetura de Eventos Contábeis

O ERP **NÃO** faz lançamentos contábeis diretamente nos módulos.
Em vez disso, cada evento do ERP gera um **EventoContabil** padronizado
que fica numa fila interna centralizada.

```
┌──────────┐    Domain Event     ┌────────────────────┐    API REST     ┌──────────────────┐
│  Módulo  │ ──────────────────> │  Fila de Eventos   │ ─────────────> │ Sistema Contábil │
│  do ERP  │  "VendaRealizada"   │  Contábeis (ERP)   │  POST /api/    │ (Next.js/MySQL)  │
│          │                     │                    │  lancamentos   │                  │
│ Vendas   │                     │ ┌────────────────┐ │                │ Recebe:          │
│ Compras  │                     │ │EventoContabil  │ │                │ - data           │
│ Financ.  │                     │ │- tipo          │ │                │ - valor          │
│          │                     │ │- valor         │ │                │ - historico      │
│          │                     │ │- contaDebito   │ │                │ - contaDebitoId  │
│          │                     │ │- contaCredito  │ │                │ - contaCreditoId │
│          │                     │ │- status        │ │                │ - empresaId      │
│          │                     │ │  (Pendente/    │ │                │                  │
│          │                     │ │   Enviado/Erro)│ │                │                  │
│          │                     │ └────────────────┘ │                │                  │
└──────────┘                     └────────────────────┘                └──────────────────┘
```

### 7.2 Mapeamento Contábil Configurável

Tela de configuração onde o usuário associa cada tipo de evento do ERP
às contas do plano de contas do sistema contábil:

| Tipo de Evento       | Conta Débito (Contábil)       | Conta Crédito (Contábil)      |
|----------------------|-------------------------------|-------------------------------|
| Venda à Vista        | 1.1.1.01 - Caixa              | 3.1.1.01 - Receita de Vendas  |
| Venda a Prazo        | 1.1.2.01 - Clientes           | 3.1.1.01 - Receita de Vendas  |
| Compra de Mercadoria | 1.1.4.01 - Estoque            | 2.1.1.01 - Fornecedores       |
| Pagamento Fornecedor | 2.1.1.01 - Fornecedores       | 1.1.1.01 - Caixa              |
| Recebimento Cliente  | 1.1.1.01 - Caixa              | 1.1.2.01 - Clientes           |
| Despesa Administrativa| 4.2.2.01 - Desp. Administrativas | 1.1.1.01 - Caixa           |

### 7.3 Status dos Eventos

| Status         | Descrição                                            |
|----------------|------------------------------------------------------|
| `Pendente`     | Evento gerado no ERP, aguardando envio               |
| `Enviado`      | Enviado com sucesso ao sistema contábil via API       |
| `Erro`         | Falha no envio (API fora do ar, conta inválida, etc.) |
| `Cancelado`    | Evento cancelado (ex: estorno de venda)               |

---

## 8. Comunicação entre Módulos

### 8.1 Padrão: Domain Events + MediatR

Os módulos se comunicam via **eventos de domínio**, sem dependências diretas entre eles.

```
Módulo Vendas                    Módulo Estoque              Módulo Financeiro
     │                               │                            │
     │  publica:                      │  escuta:                   │  escuta:
     │  VendaRealizadaEvent           │  VendaRealizadaEvent       │  VendaRealizadaEvent
     │  {PedidoId, Itens[],           │  → BaixarEstoque()         │  → GerarContaReceber()
     │   ClienteId, Valor}            │                            │
     │                                │                            │
     │                                │  publica:                  │
     │                                │  EstoqueBaixadoEvent       │
     │                                │                            │
     └────────────────────────────────┴────────────────────────────┘
                    ↓
          Módulo Contabilidade (escuta todos os eventos relevantes)
          → Gera EventoContabil padronizado
```

### 8.2 Tabela de Eventos entre Módulos

| Evento                    | Publicado por | Consumido por              |
|---------------------------|---------------|----------------------------|
| `VendaRealizadaEvent`     | Vendas        | Estoque, Financeiro, Contab.|
| `CompraRecebidaEvent`     | Compras       | Estoque, Financeiro, Contab.|
| `PagamentoRealizadoEvent` | Financeiro    | Contabilidade              |
| `RecebimentoRealizadoEvent`| Financeiro   | Contabilidade              |
| `EstoqueBaixadoEvent`     | Estoque       | (Log/Auditoria)            |
| `NotaFiscalEmitidaEvent`  | Fiscal        | Vendas, Contabilidade      |
| `TransferenciaEstoqueEvent`| Estoque      | (Log/Auditoria)            |

---

## 9. Multi-Moeda

### 9.1 Estrutura

- **Moeda principal**: Real (BRL) — todos os valores são armazenados em BRL
- **Moeda secundária**: Campo opcional para registrar o valor em outra moeda
- **Tabela de moedas**: Cadastro de moedas disponíveis (USD, EUR, etc.)
- **Taxa de câmbio**: Registrada no momento da transação

```
Transação Financeira
├── Valor (BRL)           → R$ 500,00  (obrigatório, sempre em BRL)
├── MoedaSecundariaId     → USD        (opcional)
├── ValorMoedaSecundaria  → $100.00    (opcional)
└── TaxaCambio            → 5.0000     (opcional)
```

---

## 10. Auditoria Completa

### 10.1 Estratégia: EF Core Interceptor

Toda alteração em qualquer entidade é capturada automaticamente via
`SaveChangesInterceptor` do Entity Framework Core.

### 10.2 Tabela de Auditoria

```
AuditLog
├── Id (BIGINT, PK)
├── EmpresaId (INT, FK)
├── UserId (VARCHAR, FK)          → Quem fez
├── Timestamp (TIMESTAMPTZ)       → Quando fez
├── TableName (VARCHAR)           → Qual tabela
├── EntityId (VARCHAR)            → Qual registro
├── Action (VARCHAR)              → INSERT / UPDATE / DELETE
├── OldValues (JSONB)             → Valores anteriores (null se INSERT)
├── NewValues (JSONB)             → Valores novos (null se DELETE)
├── ChangedColumns (JSONB)        → Lista de colunas alteradas
└── IpAddress (VARCHAR)           → IP do usuário
```

### 10.3 Campos de Auditoria em Todas as Entidades

```csharp
public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTime CriadoEm { get; set; }
    public string? CriadoPor { get; set; }
    public DateTime? AlteradoEm { get; set; }
    public string? AlteradoPor { get; set; }
}
```

---

## 11. Notificações Internas

### 11.1 Arquitetura

- **SignalR** para notificações em tempo real (push para o navegador)
- **Tabela de notificações** com status lida/não-lida
- **Filtro por permissão**: cada tipo de notificação é associado a uma permissão
- **Badge/sino** no TopBar com contagem de não-lidas

### 11.2 Tipos de Notificação

| Tipo                          | Permissão Necessária             |
|-------------------------------|----------------------------------|
| Conta a pagar vencendo        | `Financeiro.ContasPagar.Visualizar` |
| Estoque abaixo do mínimo     | `Estoque.Movimentacao.Visualizar`   |
| Pedido de venda aprovado      | `Vendas.Pedidos.Visualizar`        |
| Erro na integração contábil   | `Contabilidade.Eventos.Visualizar` |
| Nota fiscal rejeitada         | `Fiscal.NFe.Visualizar`           |

---

## 12. PDV — Módulo Separado

### 12.1 Arquitetura

O PDV é uma **aplicação Blazor WebAssembly separada** que se comunica com o backend
do ERP via API REST. Suporta operação **offline** com sincronização posterior.

```
┌───────────────────────────────┐         ┌────────────────────────┐
│        ERP.PDV (WASM)         │  API    │    ERP Backend         │
│                               │ REST    │    (ASP.NET Core)      │
│  ┌─────────────────────────┐  │────────>│                        │
│  │ Interface de Caixa      │  │         │  /api/vendas           │
│  │ - Busca produto (código)│  │<────────│  /api/produtos         │
│  │ - Lista itens           │  │         │  /api/clientes         │
│  │ - Formas de pagamento   │  │         │  /api/estoque          │
│  │ - Finaliza venda        │  │         │                        │
│  └─────────────────────────┘  │         └────────────────────────┘
│                               │
│  ┌─────────────────────────┐  │
│  │ Cache Offline            │  │
│  │ (IndexedDB)              │  │
│  │ - Produtos em cache      │  │
│  │ - Vendas pendentes sync  │  │
│  └─────────────────────────┘  │
└───────────────────────────────┘
```

---

## 13. Diagrama Geral da Arquitetura

```
                          ┌─────────────────────────────────────┐
                          │           USUÁRIOS                  │
                          │   Navegador Web (Chrome, Edge, etc) │
                          └──────────┬──────────────────────────┘
                                     │
                          ┌──────────▼──────────────────────────┐
                          │         NGINX / Reverse Proxy       │
                          │         (SSL Termination)           │
                          └──────────┬──────────────────────────┘
                                     │
                 ┌───────────────────┼───────────────────┐
                 │                   │                   │
        ┌────────▼───────┐  ┌───────▼────────┐  ┌──────▼───────┐
        │   ERP.Web      │  │   ERP.PDV      │  │  API REST    │
        │   (Blazor      │  │   (Blazor WASM)│  │  Pública     │
        │    Server)     │  │   App Separada │  │  (Futuro)    │
        └────────┬───────┘  └───────┬────────┘  └──────┬───────┘
                 │                  │                   │
                 └──────────────────┼───────────────────┘
                                    │
                          ┌─────────▼──────────────────────────┐
                          │       ASP.NET Core Backend         │
                          │                                    │
                          │  ┌──────────────────────────────┐  │
                          │  │     Application Layer        │  │
                          │  │  Commands · Queries · Events │  │
                          │  │  MediatR · FluentValidation  │  │
                          │  └──────────────────────────────┘  │
                          │                                    │
                          │  ┌──────────────────────────────┐  │
                          │  │      Domain Layer            │  │
                          │  │  Entities · Rules · Events   │  │
                          │  └──────────────────────────────┘  │
                          │                                    │
                          │  ┌──────────────────────────────┐  │
                          │  │   Infrastructure Layer       │  │
                          │  │  EF Core · Repos · Services  │  │
                          │  └──────┬───────────┬───────────┘  │
                          └─────────┼───────────┼──────────────┘
                                    │           │
                     ┌──────────────┘           └─────────────┐
                     │                                        │
           ┌─────────▼──────────┐              ┌──────────────▼──────────┐
           │   PostgreSQL       │              │  Sistema Contábil      │
           │   (Banco ERP)      │              │  (Next.js + MySQL)     │
           │                    │              │                        │
           │ · Cadastros        │   API REST   │  POST /api/lancamentos │
           │ · Financeiro       │─────────────>│  GET  /api/contas      │
           │ · Vendas           │              │  GET  /api/balancete   │
           │ · Compras          │              │                        │
           │ · Estoque          │              └────────────────────────┘
           │ · Fiscal           │
           │ · Auditoria        │
           │ · Eventos Contábeis│
           └────────────────────┘
```

---

## 14. Entidades Principais do Banco de Dados

> ⚠️ As tabelas serão criadas **módulo por módulo** conforme o roadmap.
> Abaixo está a visão geral das entidades principais de cada módulo.

### 14.1 Módulo Core (Infraestrutura)

| Entidade             | Descrição                                              |
|----------------------|--------------------------------------------------------|
| `Empresa`            | Cadastro de empresas do grupo                          |
| `Usuario`            | Usuários do sistema (ASP.NET Identity)                 |
| `UsuarioEmpresa`     | Vínculo usuário ↔ empresa                              |
| `Papel` (Role)       | Papéis/perfis de acesso                                |
| `Permissao`          | Permissões granulares (Modulo.Acao)                    |
| `PapelPermissao`     | Vínculo papel ↔ permissão                              |
| `AuditLog`           | Log de auditoria completo (JSONB)                      |
| `Notificacao`        | Notificações internas do sistema                       |
| `Configuracao`       | Configurações por empresa (chave/valor)                |
| `Moeda`              | Cadastro de moedas                                     |

### 14.2 Módulo Cadastros

| Entidade             | Descrição                                              |
|----------------------|--------------------------------------------------------|
| `Cliente`            | Cadastro de clientes (PF/PJ)                           |
| `Fornecedor`         | Cadastro de fornecedores                               |
| `Produto`            | Cadastro de produtos                                   |
| `Servico`            | Cadastro de serviços                                   |
| `Categoria`          | Categorias de produtos/serviços (hierárquica)          |
| `UnidadeMedida`      | Unidades de medida (UN, KG, LT, MT, etc.)              |

### 14.3 Módulo Financeiro

| Entidade             | Descrição                                              |
|----------------------|--------------------------------------------------------|
| `ContaPagar`         | Contas a pagar                                         |
| `ContaReceber`       | Contas a receber                                       |
| `Banco`              | Cadastro de bancos                                     |
| `ContaBancaria`      | Contas bancárias da empresa                            |
| `FormaPagamento`     | Formas de pagamento                                    |
| `MovimentacaoFinanceira` | Movimentações de caixa/banco                       |
| `ConciliacaoBancaria`| Registros de conciliação                               |
| `ExtratoImportado`   | Extratos OFX/CSV importados                            |

### 14.4 Módulo Vendas

| Entidade             | Descrição                                              |
|----------------------|--------------------------------------------------------|
| `Orcamento`          | Orçamentos                                             |
| `OrcamentoItem`      | Itens do orçamento                                     |
| `PedidoVenda`        | Pedidos de venda                                       |
| `PedidoVendaItem`    | Itens do pedido                                        |
| `Faturamento`        | Faturamento de pedidos                                 |

### 14.5 Módulo Compras

| Entidade             | Descrição                                              |
|----------------------|--------------------------------------------------------|
| `SolicitacaoCompra`  | Solicitações de compra                                 |
| `PedidoCompra`       | Pedidos de compra                                      |
| `PedidoCompraItem`   | Itens do pedido de compra                              |
| `EntradaMercadoria`  | Entrada de mercadorias no estoque                      |

### 14.6 Módulo Estoque

| Entidade             | Descrição                                              |
|----------------------|--------------------------------------------------------|
| `Deposito`           | Depósitos/armazéns                                     |
| `EstoqueProduto`     | Saldo de estoque por produto/depósito                  |
| `MovimentacaoEstoque`| Movimentações de entrada/saída/transferência            |
| `Lote`               | Controle de lotes (opcional por produto)                |
| `NumeroSerie`        | Controle de números de série (opcional por produto)     |
| `Inventario`         | Cabeçalho do inventário                                |
| `InventarioItem`     | Itens contados no inventário                           |

### 14.7 Módulo Fiscal

| Entidade             | Descrição                                              |
|----------------------|--------------------------------------------------------|
| `NotaFiscal`         | Notas fiscais (NF-e / NFC-e)                           |
| `NotaFiscalItem`     | Itens da nota fiscal                                   |
| `Ncm`                | Tabela NCM                                             |
| `Cfop`               | Tabela CFOP                                            |
| `RegimeTributario`   | Configuração de regime por empresa                     |
| `RegraTributaria`    | Regras de tributação configuráveis                     |
| `XmlNotaFiscal`      | Armazenamento de XMLs                                  |

### 14.8 Módulo Contabilidade (Integração)

| Entidade             | Descrição                                              |
|----------------------|--------------------------------------------------------|
| `EventoContabil`     | Fila de eventos contábeis gerados pelo ERP             |
| `MapeamentoContabil` | De-Para: tipo de evento ↔ contas contábeis             |
| `ContaContabilCache` | Cache do plano de contas do sistema contábil           |

---

## 15. Estratégia de Banco de Dados

### 15.1 Migrations

- Entity Framework Core **Code-First** com Migrations
- Uma migration por módulo ao ser desenvolvido
- Migrations versionadas e rastreáveis no Git

### 15.2 Índices

- `EmpresaId` indexado em **todas** as tabelas multi-empresa
- Índices compostos para consultas frequentes
- Índices parciais para soft-delete (`WHERE Ativo = true`)

### 15.3 Soft Delete

Todas as entidades principais usam soft-delete (`Ativo = true/false`),
filtrado automaticamente via Global Query Filters.

---

## 16. Segurança

| Aspecto             | Implementação                                          |
|---------------------|--------------------------------------------------------|
| Autenticação        | ASP.NET Identity + JWT Bearer Tokens                   |
| Autorização         | Claims-based + Policy-based + permissões granulares    |
| Senhas              | BCrypt hash via Identity                               |
| HTTPS               | Obrigatório em produção (SSL/TLS)                      |
| CORS                | Configurado para domínios permitidos                   |
| Rate Limiting       | ASP.NET Core Rate Limiting middleware                  |
| Injeção SQL         | Prevenida pelo EF Core (queries parametrizadas)        |
| XSS                 | Prevenido pelo Blazor (renderização segura)            |
| CSRF                | Protegido por antiforgery tokens                       |
| Auditoria           | Todas as ações registradas com IP e usuário            |

---

> **Próximo documento**: ROADMAP_ERP.md — Cronograma detalhado de desenvolvimento fase por fase.
