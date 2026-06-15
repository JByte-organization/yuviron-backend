using System.Threading;
using System.Threading.Tasks;

namespace Yuviron.Application.Abstractions.Identity;

public interface IRoleBackfillService
{
    Task BackfillUserRolesAsync(CancellationToken cancellationToken);
}
