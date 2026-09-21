using ERP.Domain.Core.Entities.Estoque;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Data;

public static class DbInitializerEstoque
{
    public static async Task SeedEstoqueAsync(ApplicationDbContext context)
    {
        var empresas = await context.Empresas.IgnoreQueryFilters().ToListAsync();

        foreach (var emp in empresas)
        {
            // 1. Depósito Padrão por Empresa
            var depositoPadrao = await context.Depositos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(d => d.EmpresaId == emp.Id && d.Padrao);

            if (depositoPadrao == null)
            {
                depositoPadrao = await context.Depositos
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(d => d.EmpresaId == emp.Id);

                if (depositoPadrao == null)
                {
                    depositoPadrao = new Deposito
                    {
                        EmpresaId = emp.Id,
                        Nome = "Depósito Principal",
                        Codigo = "DEP-01",
                        Endereco = "Galpão Principal - Matriz",
                        Responsavel = "Gerente de Operações",
                        Ativo = true,
                        Padrao = true
                    };
                    context.Depositos.Add(depositoPadrao);
                    await context.SaveChangesAsync();
                }
                else
                {
                    depositoPadrao.Padrao = true;
                    await context.SaveChangesAsync();
                }
            }

            // 2. Garantir saldo de estoque inicial para os produtos cadastrados
            var produtos = await context.Produtos
                .IgnoreQueryFilters()
                .Where(p => p.EmpresaId == emp.Id)
                .ToListAsync();

            foreach (var produto in produtos)
            {
                var existeSaldo = await context.EstoqueProdutos
                    .IgnoreQueryFilters()
                    .AnyAsync(ep => ep.EmpresaId == emp.Id && ep.ProdutoId == produto.Id && ep.DepositoId == depositoPadrao.Id);

                if (!existeSaldo)
                {
                    context.EstoqueProdutos.Add(new EstoqueProduto
                    {
                        EmpresaId = emp.Id,
                        ProdutoId = produto.Id,
                        DepositoId = depositoPadrao.Id,
                        Quantidade = 0,
                        CustoMedio = produto.PrecoCusto > 0 ? produto.PrecoCusto : 0,
                        CustoUltimaCompra = produto.PrecoCusto > 0 ? produto.PrecoCusto : 0,
                        EstoqueMinimo = produto.EstoqueMinimo > 0 ? produto.EstoqueMinimo : 5,
                        EstoqueMaximo = 100
                    });
                }
            }
            await context.SaveChangesAsync();
        }
    }
}
