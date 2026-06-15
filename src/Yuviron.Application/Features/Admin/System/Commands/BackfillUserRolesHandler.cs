using MediatR;
using Yuviron.Application.Abstractions.Identity;
using System.Threading;
using System.Threading.Tasks;

namespace Yuviron.Application.Features.Admin.System.Commands;

public sealed class BackfillUserRolesHandler : IRequestHandler<BackfillUserRolesCommand, Unit>
{
    private readonly IRoleBackfillService _roleBackfillService;

    public BackfillUserRolesHandler(IRoleBackfillService roleBackfillService)
    {
        _roleBackfillService = roleBackfillService;
    }

    public async Task<Unit> Handle(BackfillUserRolesCommand request, CancellationToken cancellationToken)
    {
        await _roleBackfillService.BackfillUserRolesAsync(cancellationToken);
        return Unit.Value;
    }
}

