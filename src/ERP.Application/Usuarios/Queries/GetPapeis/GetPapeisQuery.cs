using ERP.Domain.Core.Interfaces;
using MediatR;

namespace ERP.Application.Usuarios.Queries.GetPapeis;

public record GetPapeisQuery : IRequest<List<string>>;

public class GetPapeisQueryHandler : IRequestHandler<GetPapeisQuery, List<string>>
{
    private readonly IIdentityService _identityService;

    public GetPapeisQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<List<string>> Handle(GetPapeisQuery request, CancellationToken cancellationToken)
    {
        return await _identityService.ObterTodosPapeisAsync();
    }
}
