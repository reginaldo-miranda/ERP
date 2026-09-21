using System.Security.Claims;
using ERP.Application.Auth.DTOs;
using ERP.Application.Common.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace ERP.Web.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedLocalStorage _localStorage;
    private readonly ICurrentEmpresaService _currentEmpresaService;
    private UsuarioDto? _usuario;
    private int? _empresaAtivaId;
    private string _empresaAtivaNome = "Empresa Matriz Padrão S.A.";
    private bool _initialized;

    public CustomAuthStateProvider(
        ProtectedLocalStorage localStorage,
        ICurrentEmpresaService currentEmpresaService)
    {
        _localStorage = localStorage;
        _currentEmpresaService = currentEmpresaService;
    }

    public UsuarioDto? Usuario => _usuario;
    public int? EmpresaAtivaId => _empresaAtivaId;
    public string EmpresaAtivaNome => _empresaAtivaNome;
    public List<EmpresaResumoDto> EmpresasDisponiveis => _usuario?.Empresas ?? new List<EmpresaResumoDto>();

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_initialized)
        {
            try
            {
                var storageResult = await _localStorage.GetAsync<UsuarioDto>("auth_user");
                if (storageResult.Success && storageResult.Value != null)
                {
                    ConfigurarUsuario(storageResult.Value);
                }
                _initialized = true;
            }
            catch
            {
                // JavaScript interop pode não estar pronto durante renderização inicial
            }
        }

        if (_usuario != null)
        {
            return new AuthenticationState(CriarPrincipal(_usuario));
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public async Task LoginAsync(UsuarioDto usuario)
    {
        ConfigurarUsuario(usuario);
        try
        {
            await _localStorage.SetAsync("auth_user", usuario);
        }
        catch { }

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task LogoutAsync()
    {
        _usuario = null;
        _empresaAtivaId = null;
        _empresaAtivaNome = "Empresa Matriz Padrão S.A.";
        try
        {
            await _localStorage.DeleteAsync("auth_user");
        }
        catch { }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
    }

    public async Task TrocarEmpresaAsync(int empresaId, string empresaNome)
    {
        _empresaAtivaId = empresaId;
        _empresaAtivaNome = empresaNome;
        _currentEmpresaService.SetEmpresaId(empresaId);

        if (_usuario != null)
        {
            _usuario.EmpresaAtualId = empresaId;
            try
            {
                await _localStorage.SetAsync("auth_user", _usuario);
            }
            catch { }
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }

    public void Login(UsuarioDto usuario) => _ = LoginAsync(usuario);
    public void Logout() => _ = LogoutAsync();
    public void TrocarEmpresa(int empresaId, string empresaNome) => _ = TrocarEmpresaAsync(empresaId, empresaNome);

    private void ConfigurarUsuario(UsuarioDto usuario)
    {
        _usuario = usuario;
        _empresaAtivaId = usuario.EmpresaAtualId;

        var empresa = usuario.Empresas.FirstOrDefault(e => e.Id == usuario.EmpresaAtualId) 
                      ?? usuario.Empresas.FirstOrDefault();

        if (empresa != null)
        {
            _empresaAtivaId = empresa.Id;
            _empresaAtivaNome = empresa.NomeFantasia ?? empresa.RazaoSocial;
            _currentEmpresaService.SetEmpresaId(empresa.Id);
        }
    }

    private ClaimsPrincipal CriarPrincipal(UsuarioDto usuario)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id),
            new(ClaimTypes.Name, usuario.NomeCompleto),
            new(ClaimTypes.Email, usuario.Email),
            new("EmpresaId", _empresaAtivaId?.ToString() ?? "1"),
            new("EmpresaNome", _empresaAtivaNome)
        };

        foreach (var papel in usuario.Papeis)
        {
            claims.Add(new Claim(ClaimTypes.Role, papel));
        }

        foreach (var perm in usuario.Permissoes)
        {
            claims.Add(new Claim("Permission", perm));
        }

        var identity = new ClaimsIdentity(claims, "CustomAuth");
        return new ClaimsPrincipal(identity);
    }
}
