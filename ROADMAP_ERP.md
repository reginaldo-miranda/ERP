# 🗺️ Roadmap de Desenvolvimento — ERP Multi-Empresa

> Plano de desenvolvimento modular, fase por fase.
> Versão: 1.0 | Data: 2026-08-29

---

## Visão Geral das Fases

```
 FASE 1          FASE 2          FASE 3          FASE 4          FASE 5
┌───────────┐   ┌───────────┐   ┌───────────┐   ┌───────────┐   ┌───────────┐
│INFRAESTRU-│   │           │   │           │   │           │   │           │
│TURA BASE  │──>│CADASTROS  │──>│FINANCEIRO │──>│ ESTOQUE   │──>│  VENDAS   │
│+ Auth     │   │           │   │           │   │           │   │           │
└───────────┘   └───────────┘   └───────────┘   └───────────┘   └───────────┘
                                                                      │
 FASE 9          FASE 8          FASE 7          FASE 6               │
┌───────────┐   ┌───────────┐   ┌───────────┐   ┌───────────┐        │
│RELATÓRIOS │   │INTEGRAÇÃO │   │           │   │           │        │
│ DASHBOARD │<──│ CONTÁBIL  │<──│   FISCAL  │<──│  COMPRAS  │<───────┘
│           │   │           │   │           │   │           │
└───────────┘   └───────────┘   └───────────┘   └───────────┘

 FASES FUTURAS
┌───────────┐   ┌───────────┐   ┌───────────┐   ┌───────────┐
│    PDV    │   │    CRM    │   │  ORDENS   │   │    API    │
│  OFFLINE  │   │  BÁSICO   │   │ DE SERVIÇO│   │  PÚBLICA  │
└───────────┘   └───────────┘   └───────────┘   └───────────┘
```

---

## FASE 1 — Infraestrutura Base + Autenticação

### 🎯 Problema que Resolve

Cria o alicerce do sistema: a solution .NET, o banco de dados, a autenticação,
o sistema de permissões, a auditoria automática e o layout base do Blazor.
**Sem essa fase, nenhum módulo funciona.**

### 📦 Entregáveis

- [x] Solution .NET com Clean Architecture (4 projetos)
- [ ] Banco PostgreSQL com Docker Compose
- [ ] Entity Framework Core configurado com Npgsql
- [ ] ASP.NET Identity + JWT (login, registro, refresh token)
- [ ] Sistema de permissões granulares (Módulo.Ação)
- [ ] Multi-tenancy com filtro global por `EmpresaId`
- [ ] Interceptor de auditoria automática
- [ ] Layout base do Blazor com MudBlazor
- [ ] TopBar com seletor de empresa e sino de notificações
- [ ] NavMenu dinâmico baseado em permissões
- [ ] Tela de Login
- [ ] Middleware de tratamento de erros global
- [ ] Configuração de Docker Compose (app + PostgreSQL)
- [ ] GitHub Actions para build e testes

### 🗄️ Tabelas

| Tabela               | Descrição                                     |
|----------------------|-----------------------------------------------|
| `AspNetUsers`        | Usuários (ASP.NET Identity)                   |
| `AspNetRoles`        | Papéis (ASP.NET Identity)                     |
| `AspNetUserRoles`    | Vínculo usuário ↔ papel                       |
| `Empresas`           | Cadastro de empresas                          |
| `UsuarioEmpresas`    | Vínculo usuário ↔ empresa (N:N)               |
| `Permissoes`         | Permissões granulares                         |
| `PapelPermissoes`    | Vínculo papel ↔ permissão                     |
| `AuditLogs`          | Log de auditoria (JSONB)                      |
| `Notificacoes`       | Notificações internas                         |
| `Configuracoes`      | Configurações por empresa                     |
| `Moedas`             | Cadastro de moedas (BRL, USD, EUR, etc.)      |

### 🔌 APIs

| Método | Endpoint                        | Descrição                         |
|--------|----------------------------------|-----------------------------------|
| POST   | `/api/auth/login`               | Login (retorna JWT)               |
| POST   | `/api/auth/register`            | Registrar usuário                 |
| POST   | `/api/auth/refresh-token`       | Renovar token JWT                 |
| GET    | `/api/auth/me`                  | Dados do usuário logado           |
| POST   | `/api/auth/trocar-empresa`      | Alternar empresa ativa            |
| GET    | `/api/empresas`                 | Listar empresas do usuário        |
| POST   | `/api/empresas`                 | Criar empresa                     |
| PUT    | `/api/empresas/{id}`            | Atualizar empresa                 |
| DELETE | `/api/empresas/{id}`            | Desativar empresa (soft-delete)   |
| GET    | `/api/admin/usuarios`           | Listar usuários                   |
| PUT    | `/api/admin/usuarios/{id}/permissoes` | Gerenciar permissões        |
| GET    | `/api/notificacoes`             | Listar notificações do usuário    |
| PUT    | `/api/notificacoes/{id}/lida`   | Marcar como lida                  |

### 🖥️ Telas

| Tela                          | Descrição                                |
|-------------------------------|------------------------------------------|
| Login                         | Email + senha, com validação             |
| Layout Principal              | TopBar + NavMenu + conteúdo              |
| Gerenciar Empresas            | CRUD de empresas (admin)                 |
| Gerenciar Usuários            | CRUD de usuários + atribuir permissões   |
| Gerenciar Papéis/Perfis       | Criar perfis e associar permissões       |
| Perfil do Usuário             | Dados pessoais + alterar senha           |
| Painel de Notificações        | Lista de notificações com status         |

### 🔗 Integrações

- Nenhuma nesta fase (é a base para todas as futuras)

### 📊 Eventos Contábeis

- Nenhum nesta fase

---

## FASE 2 — Módulo de Cadastros

### 🎯 Problema que Resolve

Fornece os cadastros fundamentais que todos os outros módulos dependem:
clientes, fornecedores, produtos, serviços, categorias e unidades de medida.
**Sem cadastros, não há vendas, compras ou estoque.**

### 📦 Entregáveis

- [ ] CRUD completo de Clientes (PF e PJ)
- [ ] CRUD completo de Fornecedores
- [ ] CRUD completo de Produtos (com suporte a lote, série, multi-depósito)
- [ ] CRUD completo de Serviços
- [ ] CRUD de Categorias (hierárquica, árvore)
- [ ] CRUD de Unidades de Medida
- [ ] Busca e filtros avançados em todos os cadastros
- [ ] Importação de dados via CSV/Excel (opcional)
- [ ] Validação de CPF/CNPJ
- [ ] Busca de CEP via API (ViaCEP)

### 🗄️ Tabelas

| Tabela               | Campos Principais                                              |
|----------------------|----------------------------------------------------------------|
| `Clientes`           | Nome, CPF/CNPJ, TipoPessoa, Email, Telefone, Endereco, Ativo  |
| `Fornecedores`       | RazaoSocial, CNPJ, Email, Telefone, Endereco, Ativo           |
| `Produtos`           | Codigo, Nome, CategoriaId, UnidadeMedidaId, PrecoVenda,       |
|                      | PrecoCusto, ControlaLote, ControlaSerie, EstoqueMinimo, NCM   |
| `Servicos`           | Codigo, Nome, CategoriaId, PrecoBase, Ativo                   |
| `Categorias`         | Nome, CategoriaPaiId, Tipo (Produto/Serviço), Nivel, Ativo    |
| `UnidadesMedida`     | Sigla, Descricao, Ativo                                       |
| `Enderecos`          | Logradouro, Numero, Complemento, Bairro, Cidade, UF, CEP      |

### 🔌 APIs

| Método | Endpoint                        | Descrição                         |
|--------|----------------------------------|-----------------------------------|
| GET    | `/api/clientes`                 | Listar com filtros e paginação    |
| POST   | `/api/clientes`                 | Criar cliente                     |
| GET    | `/api/clientes/{id}`            | Detalhar cliente                  |
| PUT    | `/api/clientes/{id}`            | Atualizar cliente                 |
| DELETE | `/api/clientes/{id}`            | Desativar (soft-delete)           |
| GET    | `/api/fornecedores`             | Listar fornecedores               |
| POST   | `/api/fornecedores`             | Criar fornecedor                  |
| PUT    | `/api/fornecedores/{id}`        | Atualizar fornecedor              |
| DELETE | `/api/fornecedores/{id}`        | Desativar fornecedor              |
| GET    | `/api/produtos`                 | Listar produtos                   |
| POST   | `/api/produtos`                 | Criar produto                     |
| PUT    | `/api/produtos/{id}`            | Atualizar produto                 |
| DELETE | `/api/produtos/{id}`            | Desativar produto                 |
| GET    | `/api/servicos`                 | Listar serviços                   |
| POST   | `/api/servicos`                 | Criar serviço                     |
| PUT    | `/api/servicos/{id}`            | Atualizar serviço                 |
| GET    | `/api/categorias`               | Listar categorias (árvore)        |
| POST   | `/api/categorias`               | Criar categoria                   |
| GET    | `/api/unidades-medida`          | Listar unidades                   |
| POST   | `/api/unidades-medida`          | Criar unidade                     |
| GET    | `/api/utils/cep/{cep}`          | Buscar endereço por CEP           |

### 🖥️ Telas

| Tela                          | Descrição                                |
|-------------------------------|------------------------------------------|
| Lista de Clientes             | Tabela com busca, filtros e paginação    |
| Formulário de Cliente         | Criar/Editar com abas (dados, endereço)  |
| Lista de Fornecedores         | Tabela com busca e filtros               |
| Formulário de Fornecedor      | Criar/Editar                             |
| Lista de Produtos             | Tabela com busca por nome/código/categoria|
| Formulário de Produto         | Criar/Editar com dados fiscais           |
| Lista de Serviços             | Tabela com busca                         |
| Formulário de Serviço         | Criar/Editar                             |
| Categorias                    | Visualização em árvore (TreeView)        |
| Unidades de Medida            | CRUD simples (tabela inline)             |

### 🔗 Integrações

- **API ViaCEP**: Busca automática de endereço ao digitar CEP
- **Validação CPF/CNPJ**: Algoritmo local de validação

### 📊 Eventos Contábeis

- Nenhum nesta fase (cadastros não geram lançamentos contábeis)

---

## FASE 3 — Módulo Financeiro

### 🎯 Problema que Resolve

Controla todo o fluxo de dinheiro da empresa: o que ela deve pagar, o que tem a receber,
movimentações de caixa e banco, formas de pagamento e conciliação bancária.
**É o coração financeiro do ERP.**

### 📦 Entregáveis

- [ ] Contas a Pagar (CRUD + baixa + parcelas)
- [ ] Contas a Receber (CRUD + baixa + parcelas)
- [ ] Cadastro de Bancos e Contas Bancárias
- [ ] Cadastro de Formas de Pagamento
- [ ] Movimentação de Caixa/Banco (entradas e saídas)
- [ ] Fluxo de Caixa (projeção e realizado)
- [ ] Importação de extrato OFX/CSV
- [ ] Conciliação bancária assistida (match automático por valor/data)
- [ ] Relatório de contas vencidas/a vencer
- [ ] Notificações de contas a vencer

### 🗄️ Tabelas

| Tabela                    | Campos Principais                                          |
|---------------------------|------------------------------------------------------------|
| `Bancos`                  | Codigo, Nome, Ativo                                        |
| `ContasBancarias`         | BancoId, Agencia, Conta, Descricao, SaldoInicial, Ativa   |
| `FormasPagamento`         | Nome, Tipo (Dinheiro/Cartão/Boleto/Pix/etc.), Ativa       |
| `ContasPagar`             | FornecedorId, Descricao, ValorOriginal, ValorPago,         |
|                           | DataEmissao, DataVencimento, DataPagamento, Status,        |
|                           | FormaPagamentoId, ContaBancariaId, NumeroParcela,          |
|                           | TotalParcelas, DocumentoOrigem, DocumentoOrigemId          |
| `ContasReceber`           | ClienteId, Descricao, ValorOriginal, ValorRecebido,        |
|                           | DataEmissao, DataVencimento, DataRecebimento, Status,      |
|                           | FormaPagamentoId, ContaBancariaId, NumeroParcela,          |
|                           | TotalParcelas, DocumentoOrigem, DocumentoOrigemId          |
| `MovimentacoesFinanceiras`| ContaBancariaId, Tipo (Entrada/Saída), Valor, Data,        |
|                           | Descricao, Origem (Manual/Pagamento/Recebimento),          |
|                           | OrigemId, MoedaSecundariaId, ValorMoedaSecundaria,         |
|                           | TaxaCambio                                                 |
| `ExtratosImportados`      | ContaBancariaId, DataImportacao, Arquivo, TotalRegistros   |
| `ExtratosImportadosItens` | ExtratoId, Data, Valor, Descricao, StatusConciliacao,      |
|                           | MovimentacaoFinanceiraId (vínculo se conciliado)           |

### 🔌 APIs

| Método | Endpoint                              | Descrição                         |
|--------|----------------------------------------|-----------------------------------|
| GET    | `/api/contas-pagar`                   | Listar com filtros                |
| POST   | `/api/contas-pagar`                   | Criar conta a pagar               |
| PUT    | `/api/contas-pagar/{id}`              | Atualizar                         |
| POST   | `/api/contas-pagar/{id}/baixar`       | Registrar pagamento (baixa)       |
| POST   | `/api/contas-pagar/{id}/estornar`     | Estornar pagamento                |
| GET    | `/api/contas-receber`                 | Listar com filtros                |
| POST   | `/api/contas-receber`                 | Criar conta a receber             |
| POST   | `/api/contas-receber/{id}/baixar`     | Registrar recebimento (baixa)     |
| POST   | `/api/contas-receber/{id}/estornar`   | Estornar recebimento              |
| GET    | `/api/contas-bancarias`               | Listar contas bancárias           |
| POST   | `/api/contas-bancarias`               | Criar conta bancária              |
| GET    | `/api/movimentacoes-financeiras`      | Listar movimentações              |
| POST   | `/api/movimentacoes-financeiras`      | Criar movimentação manual         |
| GET    | `/api/fluxo-caixa`                    | Fluxo de caixa (período)         |
| POST   | `/api/conciliacao/importar`           | Importar OFX/CSV                  |
| GET    | `/api/conciliacao/pendentes`          | Itens pendentes de conciliação    |
| POST   | `/api/conciliacao/conciliar`          | Conciliar item com movimentação   |
| GET    | `/api/bancos`                         | Listar bancos                     |
| GET    | `/api/formas-pagamento`               | Listar formas de pagamento        |

### 🖥️ Telas

| Tela                          | Descrição                                |
|-------------------------------|------------------------------------------|
| Contas a Pagar                | Lista com filtros (status, período, fornecedor) |
| Form. Conta a Pagar           | Criar/editar com parcelas                |
| Baixar Conta a Pagar          | Dialog de pagamento (valor, data, banco) |
| Contas a Receber              | Lista com filtros                        |
| Form. Conta a Receber         | Criar/editar com parcelas                |
| Baixar Conta a Receber        | Dialog de recebimento                    |
| Contas Bancárias              | CRUD de bancos e contas                  |
| Formas de Pagamento           | CRUD simples                             |
| Movimentações Financeiras     | Lista de movimentações por conta/período |
| Fluxo de Caixa                | Visão de projeção vs. realizado          |
| Conciliação Bancária          | Importar OFX + tela de match            |

### 🔗 Integrações

- **Módulo Cadastros**: Usa Clientes e Fornecedores como referência
- **Parser OFX**: Leitura de arquivos de extrato bancário

### 📊 Eventos Contábeis

| Evento                     | Débito                    | Crédito                   |
|----------------------------|---------------------------|---------------------------|
| Pagamento de fornecedor    | Fornecedores              | Banco / Caixa             |
| Recebimento de cliente     | Banco / Caixa             | Clientes                  |
| Movimentação manual (entrada) | Banco / Caixa          | Conta configurável        |
| Movimentação manual (saída)   | Conta configurável     | Banco / Caixa             |

---

## FASE 4 — Módulo de Estoque

### 🎯 Problema que Resolve

Controla as quantidades e custos dos produtos em cada depósito/armazém:
entradas, saídas, transferências, inventário físico, estoque mínimo e
custeio (Custo Médio ou PEPS configurável por empresa).

### 📦 Entregáveis

- [ ] Cadastro de Depósitos/Armazéns
- [ ] Saldo de estoque por produto/depósito
- [ ] Movimentações de estoque (entrada, saída, transferência, ajuste)
- [ ] Cálculo automático de custo (Médio Ponderado ou PEPS)
- [ ] Controle de lotes (opcional por produto)
- [ ] Controle de número de série (opcional por produto)
- [ ] Transferência entre depósitos
- [ ] Inventário físico (contagem → ajuste automático)
- [ ] Alerta de estoque mínimo (notificação)
- [ ] Histórico completo de movimentação por produto

### 🗄️ Tabelas

| Tabela                    | Campos Principais                                          |
|---------------------------|------------------------------------------------------------|
| `Depositos`               | Nome, Endereco, Responsavel, Ativo                         |
| `EstoqueProdutos`         | ProdutoId, DepositoId, Quantidade, CustoMedio,             |
|                           | CustoUltimaCompra, EstoqueMinimo, EstoqueMaximo            |
| `MovimentacoesEstoque`    | ProdutoId, DepositoOrigemId, DepositoDestinoId,            |
|                           | Tipo (Entrada/Saída/Transferência/Ajuste),                 |
|                           | Quantidade, CustoUnitario, CustoTotal,                     |
|                           | DocumentoOrigem (Compra/Venda/Inventário/Manual),          |
|                           | DocumentoOrigemId, LoteId, NumeroSerieId                   |
| `Lotes`                   | ProdutoId, CodigoLote, DataFabricacao, DataValidade,       |
|                           | QuantidadeAtual                                            |
| `NumerosSerie`            | ProdutoId, Numero, Status (Disponível/Vendido/Devolvido),  |
|                           | LoteId                                                     |
| `Inventarios`             | DepositoId, DataInicio, DataFim, Status (Aberto/Fechado),  |
|                           | Responsavel                                                |
| `InventarioItens`         | InventarioId, ProdutoId, QtdSistema, QtdContada,           |
|                           | Diferenca, AjusteAplicado                                  |

### 🔌 APIs

| Método | Endpoint                              | Descrição                         |
|--------|----------------------------------------|-----------------------------------|
| GET    | `/api/depositos`                      | Listar depósitos                  |
| POST   | `/api/depositos`                      | Criar depósito                    |
| GET    | `/api/estoque`                        | Saldo de estoque (filtros)        |
| GET    | `/api/estoque/produto/{id}`           | Estoque de um produto em todos os depósitos |
| GET    | `/api/estoque/movimentacoes`          | Histórico de movimentações        |
| POST   | `/api/estoque/entrada`                | Registrar entrada manual          |
| POST   | `/api/estoque/saida`                  | Registrar saída manual            |
| POST   | `/api/estoque/transferencia`          | Transferir entre depósitos        |
| POST   | `/api/inventario`                     | Iniciar inventário                |
| PUT    | `/api/inventario/{id}/contagem`       | Registrar contagem                |
| POST   | `/api/inventario/{id}/finalizar`      | Finalizar e aplicar ajustes       |
| GET    | `/api/estoque/alertas`                | Produtos abaixo do estoque mínimo |
| GET    | `/api/lotes`                          | Listar lotes de um produto        |
| GET    | `/api/numeros-serie`                  | Listar números de série           |

### 🖥️ Telas

| Tela                          | Descrição                                |
|-------------------------------|------------------------------------------|
| Depósitos                     | CRUD de depósitos                        |
| Posição de Estoque            | Saldo por produto/depósito com filtros   |
| Movimentações                 | Histórico de movimentações               |
| Entrada Manual                | Formulário de entrada                    |
| Saída Manual                  | Formulário de saída                      |
| Transferência                 | Origem → Destino com lista de produtos   |
| Inventário                    | Iniciar, contar, finalizar               |
| Alertas de Estoque            | Produtos abaixo do mínimo               |
| Ficha de Produto (Estoque)    | Histórico completo de um produto         |

### 🔗 Integrações

- **Módulo Cadastros**: Produtos, Unidades de Medida
- **Módulo Vendas** (futuro): Baixa automática ao vender
- **Módulo Compras** (futuro): Entrada automática ao receber compra

### 📊 Eventos Contábeis

| Evento                     | Débito                    | Crédito                   |
|----------------------------|---------------------------|---------------------------|
| Entrada de estoque (compra)| Estoque de Mercadorias    | (gerado pelo módulo Compras)|
| Saída de estoque (venda)   | CMV (Custo Mercadoria Vendida) | Estoque de Mercadorias |
| Ajuste positivo (inventário)| Estoque de Mercadorias   | Ajustes de Inventário     |
| Ajuste negativo (inventário)| Ajustes de Inventário    | Estoque de Mercadorias    |

---

## FASE 5 — Módulo de Vendas

### 🎯 Problema que Resolve

Gerencia todo o ciclo de vendas: desde o orçamento para o cliente,
passando pelo pedido de venda, até o faturamento. Mantém o histórico
completo de vendas e integra com estoque e financeiro.

### 📦 Entregáveis

- [ ] Orçamentos (CRUD + impressão/PDF + conversão para pedido)
- [ ] Pedidos de Venda (CRUD + aprovação + faturamento)
- [ ] Faturamento (geração automática ou manual)
- [ ] Histórico de Vendas (relatórios e consultas)
- [ ] Integração automática: venda → baixa de estoque + conta a receber
- [ ] Impressão/PDF de orçamento e pedido
- [ ] Cálculo automático de descontos e totais

### 🗄️ Tabelas

| Tabela                    | Campos Principais                                          |
|---------------------------|------------------------------------------------------------|
| `Orcamentos`              | ClienteId, DataOrcamento, DataValidade, Status,            |
|                           | ValorTotal, Desconto, Observacao                           |
| `OrcamentoItens`          | OrcamentoId, ProdutoId/ServicoId, Quantidade,              |
|                           | PrecoUnitario, Desconto, SubTotal                          |
| `PedidosVenda`            | ClienteId, OrcamentoId (opcional), DataPedido,             |
|                           | Status (Pendente/Aprovado/Faturado/Cancelado),             |
|                           | FormaPagamentoId, CondicaoPagamento, ValorTotal,           |
|                           | DepositoId, VendedorId                                     |
| `PedidoVendaItens`        | PedidoVendaId, ProdutoId/ServicoId, Quantidade,            |
|                           | PrecoUnitario, Desconto, SubTotal, CustoUnitario           |
| `Faturamentos`            | PedidoVendaId, DataFaturamento, NotaFiscalId (futuro),     |
|                           | ValorTotal, Status                                         |

### 🔌 APIs

| Método | Endpoint                              | Descrição                         |
|--------|----------------------------------------|-----------------------------------|
| GET    | `/api/orcamentos`                     | Listar orçamentos                 |
| POST   | `/api/orcamentos`                     | Criar orçamento                   |
| PUT    | `/api/orcamentos/{id}`               | Atualizar orçamento               |
| POST   | `/api/orcamentos/{id}/converter`      | Converter em pedido de venda      |
| GET    | `/api/orcamentos/{id}/pdf`            | Gerar PDF do orçamento            |
| GET    | `/api/pedidos-venda`                  | Listar pedidos                    |
| POST   | `/api/pedidos-venda`                  | Criar pedido                      |
| PUT    | `/api/pedidos-venda/{id}`             | Atualizar pedido                  |
| POST   | `/api/pedidos-venda/{id}/aprovar`     | Aprovar pedido                    |
| POST   | `/api/pedidos-venda/{id}/cancelar`    | Cancelar pedido                   |
| POST   | `/api/pedidos-venda/{id}/faturar`     | Faturar pedido                    |
| GET    | `/api/vendas/historico`               | Histórico de vendas (relatório)   |

### 🖥️ Telas

| Tela                          | Descrição                                |
|-------------------------------|------------------------------------------|
| Lista de Orçamentos           | Tabela com filtros e status              |
| Formulário de Orçamento       | Cabeçalho + itens (produtos/serviços)    |
| Lista de Pedidos de Venda     | Tabela com status e filtros              |
| Formulário de Pedido          | Cabeçalho + itens + forma pagamento      |
| Detalhes do Pedido            | Visualização completa + ações            |
| Faturamento                   | Lista de pedidos aprovados para faturar  |
| Histórico de Vendas           | Relatório por período/cliente/produto    |

### 🔗 Integrações

- **Módulo Cadastros**: Clientes, Produtos, Serviços
- **Módulo Estoque**: Baixa automática ao faturar (Domain Event: `VendaFaturadaEvent`)
- **Módulo Financeiro**: Geração automática de Conta a Receber (Domain Event: `VendaFaturadaEvent`)
- **Módulo Fiscal** (futuro): Geração de NF-e/NFC-e ao faturar

### 📊 Eventos Contábeis

| Evento                     | Débito                    | Crédito                   |
|----------------------------|---------------------------|---------------------------|
| Venda à vista              | Caixa / Banco             | Receita de Vendas         |
| Venda a prazo              | Clientes                  | Receita de Vendas         |
| CMV da venda               | CMV                       | Estoque de Mercadorias    |
| Desconto concedido         | Descontos Concedidos      | Clientes                  |

---

## FASE 6 — Módulo de Compras

### 🎯 Problema que Resolve

Gerencia o ciclo de compras: desde a solicitação interna, passando pelo
pedido ao fornecedor, até a entrada das mercadorias no estoque.
Integra automaticamente com estoque e financeiro.

### 📦 Entregáveis

- [ ] Solicitação de Compra (requisição interna)
- [ ] Pedido de Compra (CRUD + envio ao fornecedor + aprovação)
- [ ] Entrada de Mercadorias (recebimento + conferência)
- [ ] Integração automática: compra → entrada de estoque + conta a pagar
- [ ] Comparativo de preços entre fornecedores
- [ ] Histórico de compras por fornecedor/produto

### 🗄️ Tabelas

| Tabela                    | Campos Principais                                          |
|---------------------------|------------------------------------------------------------|
| `SolicitacoesCompra`      | Solicitante, DataSolicitacao, Status, Justificativa        |
| `SolicitacaoCompraItens`  | SolicitacaoId, ProdutoId, Quantidade, Observacao           |
| `PedidosCompra`           | FornecedorId, SolicitacaoId (opcional), DataPedido,        |
|                           | DataPrevisaoEntrega, Status, FormaPagamentoId, ValorTotal, |
|                           | CondicaoPagamento, DepositoDestinoId                       |
| `PedidoCompraItens`       | PedidoCompraId, ProdutoId, Quantidade, PrecoUnitario,      |
|                           | SubTotal                                                   |
| `EntradasMercadoria`      | PedidoCompraId, DataEntrada, NotaFiscalFornecedor,         |
|                           | Status, Conferente                                         |
| `EntradaMercadoriaItens`  | EntradaId, ProdutoId, QtdPedida, QtdRecebida,              |
|                           | PrecoUnitario, LoteId                                      |

### 🔌 APIs

| Método | Endpoint                              | Descrição                         |
|--------|----------------------------------------|-----------------------------------|
| GET    | `/api/solicitacoes-compra`            | Listar solicitações               |
| POST   | `/api/solicitacoes-compra`            | Criar solicitação                 |
| POST   | `/api/solicitacoes-compra/{id}/aprovar`| Aprovar solicitação              |
| GET    | `/api/pedidos-compra`                 | Listar pedidos de compra          |
| POST   | `/api/pedidos-compra`                 | Criar pedido de compra            |
| PUT    | `/api/pedidos-compra/{id}`            | Atualizar pedido                  |
| POST   | `/api/pedidos-compra/{id}/aprovar`    | Aprovar pedido                    |
| POST   | `/api/pedidos-compra/{id}/cancelar`   | Cancelar pedido                   |
| POST   | `/api/entradas-mercadoria`            | Registrar entrada                 |
| PUT    | `/api/entradas-mercadoria/{id}`       | Atualizar conferência             |
| POST   | `/api/entradas-mercadoria/{id}/finalizar` | Finalizar entrada             |

### 🖥️ Telas

| Tela                          | Descrição                                |
|-------------------------------|------------------------------------------|
| Solicitações de Compra        | Lista com status e filtros               |
| Form. Solicitação             | Produtos necessários + justificativa     |
| Lista de Pedidos de Compra    | Tabela com status                        |
| Formulário Pedido de Compra   | Fornecedor + itens + condições           |
| Entrada de Mercadorias        | Conferência (pedido vs. recebido)        |
| Histórico de Compras          | Relatório por fornecedor/produto/período |

### 🔗 Integrações

- **Módulo Cadastros**: Fornecedores, Produtos
- **Módulo Estoque**: Entrada automática ao finalizar recebimento (`CompraRecebidaEvent`)
- **Módulo Financeiro**: Geração automática de Conta a Pagar (`CompraRecebidaEvent`)
- **Módulo Fiscal** (futuro): Vinculação com NF-e de entrada

### 📊 Eventos Contábeis

| Evento                     | Débito                    | Crédito                   |
|----------------------------|---------------------------|---------------------------|
| Compra de mercadoria       | Estoque de Mercadorias    | Fornecedores              |
| Compra de material/despesa | Despesa (configurável)    | Fornecedores              |
| Frete sobre compras        | Frete sobre Compras       | Fornecedores / Caixa      |

---

## FASE 7 — Módulo Fiscal

### 🎯 Problema que Resolve

Gerencia toda a parte tributária e fiscal: emissão de NF-e e NFC-e diretamente
pela SEFAZ, cálculo de impostos, tabelas fiscais (NCM, CFOP, CST, CSOSN) e
preparação para a reforma tributária.

### 📦 Entregáveis

- [ ] Tabelas fiscais (NCM, CFOP, CST, CSOSN) com carga inicial
- [ ] Configuração de regime tributário por empresa
- [ ] Regras de tributação configuráveis
- [ ] Geração de XML da NF-e conforme layout SEFAZ
- [ ] Assinatura digital com certificado A1
- [ ] Comunicação SOAP com webservices da SEFAZ
- [ ] Emissão de NFC-e (modelo 65)
- [ ] Consulta, cancelamento e carta de correção
- [ ] Armazenamento de XMLs
- [ ] Impressão de DANFE (NF-e) e DANFE simplificado (NFC-e)
- [ ] Contingência offline (NFC-e)
- [ ] Preparação para IBS/CBS (reforma tributária)

### 🗄️ Tabelas

| Tabela                    | Campos Principais                                          |
|---------------------------|------------------------------------------------------------|
| `Ncm`                     | Codigo, Descricao, AliquotaIPI                             |
| `Cfop`                    | Codigo, Descricao, Tipo (Entrada/Saída), Aplicacao         |
| `RegimesTributarios`      | EmpresaId, Tipo (SN/LP/LR/MEI), Configuracoes             |
| `RegrasTributarias`       | EmpresaId, NcmId, CfopId, CST/CSOSN, AliquotaICMS,        |
|                           | AliquotaPIS, AliquotaCOFINS, AliquotaIPI, UfOrigem,        |
|                           | UfDestino                                                  |
| `NotasFiscais`            | EmpresaId, Modelo (55=NFe/65=NFCe), Serie, Numero,         |
|                           | DataEmissao, ClienteId/FornecedorId, ValorTotal,           |
|                           | Status, ChaveAcesso, Protocolo, XMLEnvio, XMLRetorno,      |
|                           | PedidoVendaId/EntradaMercadoriaId                          |
| `NotaFiscalItens`         | NotaFiscalId, ProdutoId, Quantidade, ValorUnitario,        |
|                           | ValorTotal, NCM, CFOP, CST/CSOSN, BaseICMS, AliqICMS,     |
|                           | ValorICMS, BasePIS, AliqPIS, ValorPIS, etc.                |
| `XmlNotasFiscais`         | NotaFiscalId, TipoXml (Envio/Retorno/Cancelamento/CCe),   |
|                           | Xml (TEXT), DataGeracao                                    |
| `CertificadosDigitais`    | EmpresaId, Arquivo (bytes), Senha (encrypted), Validade   |

### 🔌 APIs

| Método | Endpoint                              | Descrição                         |
|--------|----------------------------------------|-----------------------------------|
| GET    | `/api/ncm`                            | Buscar NCM                        |
| GET    | `/api/cfop`                           | Buscar CFOP                       |
| GET    | `/api/regras-tributarias`             | Listar regras da empresa          |
| POST   | `/api/regras-tributarias`             | Criar regra tributária            |
| POST   | `/api/notas-fiscais/emitir`           | Emitir NF-e/NFC-e                |
| GET    | `/api/notas-fiscais/{id}`             | Detalhes da nota                  |
| POST   | `/api/notas-fiscais/{id}/cancelar`    | Cancelar nota                     |
| POST   | `/api/notas-fiscais/{id}/cce`         | Carta de correção                 |
| GET    | `/api/notas-fiscais/{id}/danfe`       | Gerar DANFE (PDF)                |
| GET    | `/api/notas-fiscais/{id}/xml`         | Download do XML                   |
| POST   | `/api/certificados`                   | Upload de certificado A1          |

### 🖥️ Telas

| Tela                          | Descrição                                |
|-------------------------------|------------------------------------------|
| Configuração Tributária       | Regime + regras por NCM/CFOP/UF          |
| Tabela NCM                    | Busca e consulta                         |
| Tabela CFOP                   | Busca e consulta                         |
| Emissão de NF-e               | Dados da nota + emitir para SEFAZ        |
| Consulta de Notas             | Lista com filtros e status               |
| Detalhes da Nota              | XML + DANFE + status SEFAZ               |
| Certificados Digitais         | Upload e gerenciamento de certificado A1 |

### 🔗 Integrações

- **Módulo Vendas**: Emissão de NF-e ao faturar pedido
- **Módulo Compras**: Vinculação com NF-e de entrada do fornecedor
- **SEFAZ**: Comunicação SOAP direta (webservices)
- **Certificado Digital A1**: Assinatura de XML

### 📊 Eventos Contábeis

| Evento                     | Débito                    | Crédito                   |
|----------------------------|---------------------------|---------------------------|
| ICMS a recolher            | ICMS sobre Vendas         | ICMS a Recolher           |
| PIS a recolher             | PIS sobre Vendas          | PIS a Recolher            |
| COFINS a recolher          | COFINS sobre Vendas       | COFINS a Recolher         |
| ICMS a recuperar (compra)  | ICMS a Recuperar          | ICMS sobre Compras        |

---

## FASE 8 — Integração Contábil

### 🎯 Problema que Resolve

Centraliza todos os eventos contábeis gerados pelos módulos do ERP e os envia
ao sistema de contabilidade existente (Next.js + MySQL) via API REST.
Fornece uma tela de gestão para revisar, enviar e monitorar os lançamentos.

### 📦 Entregáveis

- [ ] Tela de mapeamento contábil (tipo de evento → contas do plano de contas)
- [ ] Fila centralizada de eventos contábeis
- [ ] Tela de gestão de eventos (filtrar, revisar, enviar, reenviar)
- [ ] Envio automático ou manual para o sistema contábil via API REST
- [ ] Cache do plano de contas do sistema contábil (sync periódico)
- [ ] Relatório de eventos pendentes/enviados/erro
- [ ] Reenvio automático em caso de falha (retry com backoff)
- [ ] Log detalhado de cada tentativa de envio

### 🗄️ Tabelas

| Tabela                    | Campos Principais                                          |
|---------------------------|------------------------------------------------------------|
| `EventosContabeis`        | EmpresaId, Tipo, Data, Valor, Historico,                   |
|                           | ContaDebitoCodigo, ContaCreditoCodigo,                     |
|                           | ContaDebitoIdExterno, ContaCreditoIdExterno,               |
|                           | Status (Pendente/Enviado/Erro/Cancelado),                  |
|                           | DocumentoOrigem, DocumentoOrigemId,                        |
|                           | DataEnvio, ErroMensagem, Tentativas                        |
| `MapeamentosContabeis`    | EmpresaId, TipoEvento, ContaDebitoCodigo,                  |
|                           | ContaCreditoCodigo, Descricao, Ativo                       |
| `ContasContabeisCache`    | EmpresaId, IdExterno, Codigo, Nome, Tipo, Natureza,        |
|                           | Grupo, UltimaSincronizacao                                 |

### 🔌 APIs

| Método | Endpoint                                   | Descrição                    |
|--------|---------------------------------------------|------------------------------|
| GET    | `/api/contabilidade/eventos`               | Listar eventos contábeis     |
| POST   | `/api/contabilidade/eventos/{id}/enviar`   | Enviar evento ao contábil    |
| POST   | `/api/contabilidade/eventos/enviar-lote`   | Enviar lote de eventos       |
| POST   | `/api/contabilidade/eventos/{id}/cancelar` | Cancelar evento              |
| GET    | `/api/contabilidade/mapeamento`            | Listar mapeamentos           |
| POST   | `/api/contabilidade/mapeamento`            | Criar/atualizar mapeamento   |
| POST   | `/api/contabilidade/sincronizar-contas`    | Sync plano de contas         |
| GET    | `/api/contabilidade/contas-cache`          | Plano de contas em cache     |

### 🖥️ Telas

| Tela                          | Descrição                                |
|-------------------------------|------------------------------------------|
| Mapeamento Contábil           | Config. tipo evento → contas contábeis   |
| Fila de Eventos Contábeis     | Lista com filtro por status/período      |
| Detalhes do Evento            | Dados + log de tentativas de envio       |
| Plano de Contas (Cache)       | Visualização do plano importado          |

### 🔗 Integrações

- **Sistema Contábil** (Next.js): `POST /api/lancamentos`, `GET /api/contas`
- **Todos os módulos do ERP**: Consome Domain Events de Vendas, Compras, Financeiro, Estoque, Fiscal

### 📊 Eventos Contábeis

- Este módulo **É** o gestor de todos os eventos contábeis das fases anteriores

---

## FASE 9 — Relatórios e Dashboard

### 🎯 Problema que Resolve

Fornece visibilidade gerencial sobre todos os módulos do ERP através de
dashboards interativos e relatórios detalhados, permitindo tomada de
decisão baseada em dados.

### 📦 Entregáveis

- [ ] Dashboard principal com widgets configuráveis por permissão do usuário
- [ ] Relatórios de Vendas (por período, cliente, produto, vendedor)
- [ ] Relatórios Financeiros (contas a pagar/receber, fluxo de caixa)
- [ ] Relatórios de Estoque (posição, movimentação, valorização, curva ABC)
- [ ] Relatórios de Compras (por fornecedor, produto, período)
- [ ] Relatórios Fiscais (notas emitidas, impostos por período)
- [ ] Exportação para PDF e Excel (CSV/XLSX)
- [ ] Gráficos interativos (MudBlazor Charts)

### 🖥️ Telas

| Tela                          | Descrição                                |
|-------------------------------|------------------------------------------|
| Dashboard                     | Widgets: vendas do dia, contas vencidas, estoque baixo, gráficos |
| Relatório de Vendas           | Filtros + tabela + gráficos              |
| Relatório Financeiro          | Fluxo de caixa, inadimplência            |
| Relatório de Estoque          | Posição, curva ABC, valorização          |
| Relatório de Compras          | Por fornecedor e produto                 |
| Relatório Fiscal              | Notas emitidas, impostos                 |

### 🔗 Integrações

- **Todos os módulos**: Consulta de dados para geração de relatórios

---

## FASES FUTURAS

### FASE 10 — PDV (Ponto de Venda)

- Aplicação Blazor WASM separada
- Interface otimizada para operação rápida de caixa
- Suporte offline com IndexedDB + Service Worker
- Sincronização automática quando voltar online
- Emissão de NFC-e integrada
- Impressão de cupom fiscal

### FASE 11 — CRM Básico

- Cadastro de Leads e Oportunidades
- Funil de vendas
- Atividades e follow-ups
- Conversão de lead em cliente

### FASE 12 — Ordens de Serviço

- CRUD de Ordens de Serviço
- Apontamento de horas e materiais
- Status do serviço (workflow)
- Faturamento de OS

### FASE 13 — Comissões de Vendedores

- Cadastro de vendedores e regras de comissão
- Cálculo automático por venda
- Relatório de comissões por período

### FASE 14 — Contratos Recorrentes

- Gestão de contratos com faturamento mensal/periódico
- Geração automática de contas a receber
- Renovação e reajuste automático

### FASE 15 — API Pública

- API REST documentada (Swagger/OpenAPI)
- Autenticação via API Key
- Rate limiting e versionamento
- Webhooks para eventos do ERP

---

## Convenções de Desenvolvimento

### Git

- **Branch principal**: `main` (produção)
- **Branch de desenvolvimento**: `develop`
- **Feature branches**: `feature/fase-X-nome-do-modulo`
- **Commits**: Conventional Commits (`feat:`, `fix:`, `docs:`, `refactor:`)

### Código

- **Idioma do código**: Inglês (nomes de classes, métodos, variáveis)
- **Idioma dos dados**: Português (nomes de campos no banco, labels da UI)
- **Nomenclatura**: PascalCase para classes/métodos, camelCase para variáveis
- **Testes**: xUnit + FluentAssertions + Moq

### Documentação

- Cada módulo terá seu próprio README com instruções
- APIs documentadas com Swagger
- Changelog atualizado a cada fase

---

> **Próximo passo**: Iniciar a **Fase 1 — Infraestrutura Base + Autenticação**
