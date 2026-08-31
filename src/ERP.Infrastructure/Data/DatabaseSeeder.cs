using ERP.Domain.Core.Entities;
using ERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Data;

/// <summary>
/// Serviço de Seed inicial do banco de dados.
/// Executado automaticamente no startup para garantir que a Empresa Padrão e o Usuário Admin existam.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // 1. Garantir que as migrations foram aplicadas
        if (context.Database.IsNpgsql())
        {
            await context.Database.MigrateAsync();
        }

        // 2. Permissões Iniciais
        var permissoesPadrao = new List<(string Codigo, string Modulo, string SubModulo, string Acao, string Descricao)>
        {
            ("Cadastros.Empresas.Visualizar", "Cadastros", "Empresas", "Visualizar", "Visualizar cadastro de empresas"),
            ("Cadastros.Empresas.Criar", "Cadastros", "Empresas", "Criar", "Criar novas empresas"),
            ("Cadastros.Empresas.Editar", "Cadastros", "Empresas", "Editar", "Editar dados das empresas"),
            ("Cadastros.Empresas.Excluir", "Cadastros", "Empresas", "Excluir", "Desativar empresas"),

            ("Cadastros.Clientes.Visualizar", "Cadastros", "Clientes", "Visualizar", "Visualizar clientes"),
            ("Cadastros.Clientes.Criar", "Cadastros", "Clientes", "Criar", "Criar clientes"),
            ("Cadastros.Clientes.Editar", "Cadastros", "Clientes", "Editar", "Editar clientes"),

            ("Financeiro.ContasPagar.Visualizar", "Financeiro", "ContasPagar", "Visualizar", "Visualizar contas a pagar"),
            ("Financeiro.ContasReceber.Visualizar", "Financeiro", "ContasReceber", "Visualizar", "Visualizar contas a receber"),

            ("Vendas.Pedidos.Visualizar", "Vendas", "Pedidos", "Visualizar", "Visualizar pedidos de venda"),
            ("Estoque.Posicao.Visualizar", "Estoque", "Posicao", "Visualizar", "Visualizar posição de estoque"),
            ("Fiscal.NFe.Visualizar", "Fiscal", "NFe", "Visualizar", "Visualizar notas fiscais"),
            ("Contabilidade.Eventos.Visualizar", "Contabilidade", "Eventos", "Visualizar", "Visualizar eventos contábeis"),

            ("Admin.Usuarios.Gerenciar", "Admin", "Usuarios", "Gerenciar", "Gerenciar usuários e permissões do sistema")
        };

        foreach (var p in permissoesPadrao)
        {
            if (!await context.Permissoes.AnyAsync(x => x.Codigo == p.Codigo))
            {
                context.Permissoes.Add(new Permissao
                {
                    Codigo = p.Codigo,
                    Modulo = p.Modulo,
                    SubModulo = p.SubModulo,
                    Acao = p.Acao,
                    Descricao = p.Descricao,
                    Ativo = true
                });
            }
        }
        await context.SaveChangesAsync();

        // 3. Papéis Iniciais (Admin / Operador)
        string adminRoleName = "Administrador";
        if (!await roleManager.RoleExistsAsync(adminRoleName))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRoleName));
        }

        var adminRole = await roleManager.FindByNameAsync(adminRoleName);
        if (adminRole != null)
        {
            var todasPermissoes = await context.Permissoes.ToListAsync();
            foreach (var perm in todasPermissoes)
            {
                if (!await context.PapelPermissoes.AnyAsync(pp => pp.PapelId == adminRole.Id && pp.PermissaoId == perm.Id))
                {
                    context.PapelPermissoes.Add(new PapelPermissao
                    {
                        PapelId = adminRole.Id,
                        PermissaoId = perm.Id
                    });
                }
            }
            await context.SaveChangesAsync();
        }

        // 4. Empresa Padrão
        Empresa? empresaPadrao = await context.Empresas.IgnoreQueryFilters().FirstOrDefaultAsync();
        if (empresaPadrao == null)
        {
            empresaPadrao = new Empresa
            {
                RazaoSocial = "Empresa Matriz Padrão S.A.",
                NomeFantasia = "ERP Matriz",
                Cnpj = "00.000.000/0001-91",
                Email = "contato@empresa.com.br",
                Telefone = "(11) 99999-9999",
                Cidade = "São Paulo",
                Uf = "SP",
                MetodoCusteio = "CustoMedio",
                RegimeTributario = "SimplesNacional",
                Ativo = true,
                CriadoEm = DateTime.UtcNow
            };
            context.Empresas.Add(empresaPadrao);
            await context.SaveChangesAsync();
        }

        // 5. Usuário Admin Padrão
        string adminEmail = "admin@erp.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                NomeCompleto = "Administrador do Sistema",
                Ativo = true,
                CriadoEm = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, adminRoleName);

                // Vincular Admin à Empresa Padrão
                context.UsuarioEmpresas.Add(new UsuarioEmpresa
                {
                    UsuarioId = adminUser.Id,
                    EmpresaId = empresaPadrao.Id,
                    Ativo = true
                });
                await context.SaveChangesAsync();
            }
        }
    }
}
