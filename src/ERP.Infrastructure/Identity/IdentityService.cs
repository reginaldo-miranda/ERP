using ERP.Domain.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<(bool Sucesso, string? UsuarioId, string[] Erros)> CriarUsuarioAsync(string email, string senha, string nomeCompleto)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            NomeCompleto = nomeCompleto,
            EmailConfirmed = true,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, senha);

        if (result.Succeeded)
        {
            return (true, user.Id, Array.Empty<string>());
        }

        return (false, null, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<(bool Sucesso, string? UsuarioId)> ValidarCredenciaisAsync(string email, string senha)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !user.Ativo)
            return (false, null);

        var valido = await _userManager.CheckPasswordAsync(user, senha);
        return (valido, valido ? user.Id : null);
    }

    public async Task<bool> UsuarioExisteAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user != null;
    }

    public async Task<bool> AdicionarAoPapelAsync(string usuarioId, string papel)
    {
        var user = await _userManager.FindByIdAsync(usuarioId);
        if (user == null) return false;

        if (!await _roleManager.RoleExistsAsync(papel))
        {
            await _roleManager.CreateAsync(new IdentityRole(papel));
        }

        var result = await _userManager.AddToRoleAsync(user, papel);
        return result.Succeeded;
    }

    public async Task<bool> RemoverDoPapelAsync(string usuarioId, string papel)
    {
        var user = await _userManager.FindByIdAsync(usuarioId);
        if (user == null) return false;

        var result = await _userManager.RemoveFromRoleAsync(user, papel);
        return result.Succeeded;
    }

    public async Task<IList<string>> ObterPapeisAsync(string usuarioId)
    {
        var user = await _userManager.FindByIdAsync(usuarioId);
        if (user == null) return new List<string>();

        return await _userManager.GetRolesAsync(user);
    }

    public async Task<bool> AlterarSenhaAsync(string usuarioId, string senhaAtual, string novaSenha)
    {
        var user = await _userManager.FindByIdAsync(usuarioId);
        if (user == null) return false;

        var result = await _userManager.ChangePasswordAsync(user, senhaAtual, novaSenha);
        return result.Succeeded;
    }

    public async Task<(string? NomeCompleto, string? Email)> ObterDadosUsuarioAsync(string usuarioId)
    {
        var user = await _userManager.FindByIdAsync(usuarioId);
        if (user == null) return (null, null);

        return (user.NomeCompleto, user.Email);
    }

    public async Task<List<(string Id, string NomeCompleto, string Email, bool Ativo, DateTime CriadoEm, IList<string> Papeis)>> ObterTodosUsuariosAsync()
    {
        var users = await _userManager.Users.OrderBy(u => u.NomeCompleto).ToListAsync();
        var result = new List<(string Id, string NomeCompleto, string Email, bool Ativo, DateTime CriadoEm, IList<string> Papeis)>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            result.Add((u.Id, u.NomeCompleto, u.Email ?? "", u.Ativo, u.CriadoEm, roles));
        }
        return result;
    }

    public async Task<bool> AtualizarUsuarioAsync(string usuarioId, string nomeCompleto, string email, bool ativo, IEnumerable<string> papeis)
    {
        var user = await _userManager.FindByIdAsync(usuarioId);
        if (user == null) return false;

        user.NomeCompleto = nomeCompleto;
        user.Email = email;
        user.UserName = email;
        user.Ativo = ativo;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return false;

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRolesAsync(user, papeis);

        return true;
    }

    public async Task<bool> AlternarStatusUsuarioAsync(string usuarioId)
    {
        var user = await _userManager.FindByIdAsync(usuarioId);
        if (user == null) return false;

        user.Ativo = !user.Ativo;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> ResetarSenhaAsync(string usuarioId, string novaSenha)
    {
        var user = await _userManager.FindByIdAsync(usuarioId);
        if (user == null) return false;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, novaSenha);
        return result.Succeeded;
    }

    public async Task<List<string>> ObterTodosPapeisAsync()
    {
        return await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
    }
}
