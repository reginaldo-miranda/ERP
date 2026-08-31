using System.Security.Claims;
using ERP.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ERP.Infrastructure.Services;

public class CurrentEmpresaService : ICurrentEmpresaService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private int? _empresaIdOverride;

    public CurrentEmpresaService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? EmpresaId
    {
        get
        {
            if (_empresaIdOverride.HasValue)
                return _empresaIdOverride.Value;

            var claimValue = _httpContextAccessor.HttpContext?.User?.FindFirstValue("EmpresaId");
            if (int.TryParse(claimValue, out var empresaId))
                return empresaId;

            return null;
        }
    }

    public void SetEmpresaId(int empresaId)
    {
        _empresaIdOverride = empresaId;
    }
}
