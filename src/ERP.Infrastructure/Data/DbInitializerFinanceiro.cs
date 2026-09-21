using ERP.Domain.Core.Entities;
using ERP.Domain.Core.Entities.Financeiro;
using ERP.Domain.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Data;

public static class DbInitializerFinanceiro
{
    public static async Task SeedFinanceiroAsync(ApplicationDbContext context)
    {
        // 1. Catálogo de Bancos Brasileiros (Global)
        if (!await context.Bancos.AnyAsync())
        {
            var bancos = new List<Banco>
            {
                new() { Codigo = "001", Nome = "Banco do Brasil S.A.", NomeReduzido = "Banco do Brasil", Ativo = true },
                new() { Codigo = "104", Nome = "Caixa Econômica Federal", NomeReduzido = "Caixa", Ativo = true },
                new() { Codigo = "237", Nome = "Banco Bradesco S.A.", NomeReduzido = "Bradesco", Ativo = true },
                new() { Codigo = "341", Nome = "Itaú Unibanco S.A.", NomeReduzido = "Itaú", Ativo = true },
                new() { Codigo = "033", Nome = "Banco Santander (Brasil) S.A.", NomeReduzido = "Santander", Ativo = true },
                new() { Codigo = "260", Nome = "Nu Pagamentos S.A.", NomeReduzido = "Nubank", Ativo = true },
                new() { Codigo = "077", Nome = "Banco Inter S.A.", NomeReduzido = "Banco Inter", Ativo = true },
                new() { Codigo = "756", Nome = "Banco Cooperativo Sicoob S.A.", NomeReduzido = "Sicoob", Ativo = true },
                new() { Codigo = "748", Nome = "Banco Cooperativo Sicredi S.A.", NomeReduzido = "Sicredi", Ativo = true },
                new() { Codigo = "422", Nome = "Banco Safra S.A.", NomeReduzido = "Safra", Ativo = true }
            };

            await context.Bancos.AddRangeAsync(bancos);
            await context.SaveChangesAsync();
        }

        // 2. Dados Padrão por Empresa (Formas de Pagamento, Plano de Contas, Centros de Custo, Caixas)
        var empresas = await context.Empresas.ToListAsync();
        var bb = await context.Bancos.FirstOrDefaultAsync(b => b.Codigo == "001");

        foreach (var emp in empresas)
        {
            // Formas de Pagamento
            if (!await context.FormasPagamento.IgnoreQueryFilters().AnyAsync(f => f.EmpresaId == emp.Id))
            {
                var formas = new List<FormaPagamento>
                {
                    new() { EmpresaId = emp.Id, Nome = "Dinheiro", Tipo = TipoFormaPagamento.Dinheiro, DiasCompensacao = 0, Ativa = true },
                    new() { EmpresaId = emp.Id, Nome = "Pix", Tipo = TipoFormaPagamento.Pix, DiasCompensacao = 0, Ativa = true },
                    new() { EmpresaId = emp.Id, Nome = "Boleto Bancário", Tipo = TipoFormaPagamento.Boleto, DiasCompensacao = 2, Ativa = true },
                    new() { EmpresaId = emp.Id, Nome = "Cartão de Crédito", Tipo = TipoFormaPagamento.CartaoCredito, DiasCompensacao = 30, Ativa = true },
                    new() { EmpresaId = emp.Id, Nome = "Cartão de Débito", Tipo = TipoFormaPagamento.CartaoDebito, DiasCompensacao = 1, Ativa = true },
                    new() { EmpresaId = emp.Id, Nome = "Transferência / TED", Tipo = TipoFormaPagamento.Transferencia, DiasCompensacao = 0, Ativa = true }
                };
                await context.FormasPagamento.AddRangeAsync(formas);
            }

            // Centros de Custo
            if (!await context.CentrosCusto.IgnoreQueryFilters().AnyAsync(c => c.EmpresaId == emp.Id))
            {
                var centros = new List<CentroCusto>
                {
                    new() { EmpresaId = emp.Id, Codigo = "01", Descricao = "Administrativo & Financeiro", Ativo = true },
                    new() { EmpresaId = emp.Id, Codigo = "02", Descricao = "Comercial & Vendas", Ativo = true },
                    new() { EmpresaId = emp.Id, Codigo = "03", Descricao = "Operacional & Produção", Ativo = true }
                };
                await context.CentrosCusto.AddRangeAsync(centros);
            }

            // Contas Bancárias / Caixas
            if (!await context.ContasBancarias.IgnoreQueryFilters().AnyAsync(c => c.EmpresaId == emp.Id))
            {
                var caixas = new List<ContaBancaria>
                {
                    new()
                    {
                        EmpresaId = emp.Id,
                        Descricao = "Caixa Geral / Tesouraria",
                        Tipo = TipoContaBancaria.CaixaFisico,
                        SaldoInicial = 0,
                        SaldoAtual = 0,
                        Ativa = true
                    },
                    new()
                    {
                        EmpresaId = emp.Id,
                        BancoId = bb?.Id,
                        Descricao = "Conta Principal",
                        Tipo = TipoContaBancaria.Corrente,
                        Agencia = "1234",
                        Conta = "56789-0",
                        SaldoInicial = 0,
                        SaldoAtual = 0,
                        Ativa = true
                    }
                };
                await context.ContasBancarias.AddRangeAsync(caixas);
            }

            // Plano de Contas
            if (!await context.PlanosContas.IgnoreQueryFilters().AnyAsync(p => p.EmpresaId == emp.Id))
            {
                // Grupos Sintéticos Principais
                var recSintetica = new PlanoConta { EmpresaId = emp.Id, Codigo = "1", Descricao = "RECEITAS", Tipo = TipoPlanoConta.Receita, Sintetica = true, Ativo = true };
                var despSintetica = new PlanoConta { EmpresaId = emp.Id, Codigo = "2", Descricao = "DESPESAS", Tipo = TipoPlanoConta.Despesa, Sintetica = true, Ativo = true };

                await context.PlanosContas.AddRangeAsync(recSintetica, despSintetica);
                await context.SaveChangesAsync(); // salvar para gerar Ids para os filhos

                // Subgrupos Receitas
                var recOp = new PlanoConta { EmpresaId = emp.Id, Codigo = "1.1", Descricao = "Receitas Operacionais", Tipo = TipoPlanoConta.Receita, Sintetica = true, PlanoContaPaiId = recSintetica.Id, Ativo = true };
                var recOut = new PlanoConta { EmpresaId = emp.Id, Codigo = "1.2", Descricao = "Outras Receitas", Tipo = TipoPlanoConta.Receita, Sintetica = true, PlanoContaPaiId = recSintetica.Id, Ativo = true };

                // Subgrupos Despesas
                var despCusto = new PlanoConta { EmpresaId = emp.Id, Codigo = "2.1", Descricao = "Custos Operacionais", Tipo = TipoPlanoConta.Despesa, Sintetica = true, PlanoContaPaiId = despSintetica.Id, Ativo = true };
                var despAdm = new PlanoConta { EmpresaId = emp.Id, Codigo = "2.2", Descricao = "Despesas Administrativas", Tipo = TipoPlanoConta.Despesa, Sintetica = true, PlanoContaPaiId = despSintetica.Id, Ativo = true };
                var despFin = new PlanoConta { EmpresaId = emp.Id, Codigo = "2.3", Descricao = "Despesas Financeiras e Fiscais", Tipo = TipoPlanoConta.Despesa, Sintetica = true, PlanoContaPaiId = despSintetica.Id, Ativo = true };

                await context.PlanosContas.AddRangeAsync(recOp, recOut, despCusto, despAdm, despFin);
                await context.SaveChangesAsync();

                // Analíticas (aceitam lançamentos)
                var analiticas = new List<PlanoConta>
                {
                    new() { EmpresaId = emp.Id, Codigo = "1.1.01", Descricao = "Venda de Produtos", Tipo = TipoPlanoConta.Receita, Sintetica = false, PlanoContaPaiId = recOp.Id, Ativo = true },
                    new() { EmpresaId = emp.Id, Codigo = "1.1.02", Descricao = "Prestação de Serviços", Tipo = TipoPlanoConta.Receita, Sintetica = false, PlanoContaPaiId = recOp.Id, Ativo = true },
                    new() { EmpresaId = emp.Id, Codigo = "1.2.01", Descricao = "Rendimentos de Aplicações", Tipo = TipoPlanoConta.Receita, Sintetica = false, PlanoContaPaiId = recOut.Id, Ativo = true },

                    new() { EmpresaId = emp.Id, Codigo = "2.1.01", Descricao = "Compra de Mercadorias para Revenda", Tipo = TipoPlanoConta.Despesa, Sintetica = false, PlanoContaPaiId = despCusto.Id, Ativo = true },
                    new() { EmpresaId = emp.Id, Codigo = "2.1.02", Descricao = "Insumos e Matérias-Primas", Tipo = TipoPlanoConta.Despesa, Sintetica = false, PlanoContaPaiId = despCusto.Id, Ativo = true },

                    new() { EmpresaId = emp.Id, Codigo = "2.2.01", Descricao = "Salários e Pró-Labore", Tipo = TipoPlanoConta.Despesa, Sintetica = false, PlanoContaPaiId = despAdm.Id, Ativo = true },
                    new() { EmpresaId = emp.Id, Codigo = "2.2.02", Descricao = "Aluguel e Condomínio", Tipo = TipoPlanoConta.Despesa, Sintetica = false, PlanoContaPaiId = despAdm.Id, Ativo = true },
                    new() { EmpresaId = emp.Id, Codigo = "2.2.03", Descricao = "Energia Elétrica, Água e Internet", Tipo = TipoPlanoConta.Despesa, Sintetica = false, PlanoContaPaiId = despAdm.Id, Ativo = true },
                    new() { EmpresaId = emp.Id, Codigo = "2.2.04", Descricao = "Software e Licenças de TI", Tipo = TipoPlanoConta.Despesa, Sintetica = false, PlanoContaPaiId = despAdm.Id, Ativo = true },

                    new() { EmpresaId = emp.Id, Codigo = "2.3.01", Descricao = "Impostos e Tributos (Simples, ICMS, etc)", Tipo = TipoPlanoConta.Despesa, Sintetica = false, PlanoContaPaiId = despFin.Id, Ativo = true },
                    new() { EmpresaId = emp.Id, Codigo = "2.3.02", Descricao = "Tarifas Bancárias e de Cartão", Tipo = TipoPlanoConta.Despesa, Sintetica = false, PlanoContaPaiId = despFin.Id, Ativo = true },
                    new() { EmpresaId = emp.Id, Codigo = "2.3.03", Descricao = "Juros e Multas Pagos", Tipo = TipoPlanoConta.Despesa, Sintetica = false, PlanoContaPaiId = despFin.Id, Ativo = true }
                };

                await context.PlanosContas.AddRangeAsync(analiticas);
            }

            await context.SaveChangesAsync();
        }
    }
}
