using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.Admin.Moods.Queries.DTOs;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Moods.Queries.GetMoodById;

public sealed class GetMoodByIdHandler : IRequestHandler<GetMoodByIdQuery, GetMoodByIdDto>
{
    private readonly IApplicationDbContext _context;

    public GetMoodByIdHandler(IApplicationDbContext context) => _context = context;

    public async Task<GetMoodByIdDto> Handle(GetMoodByIdQuery request, CancellationToken cancellationToken)
    {
        var mood = await _context.Moods
            .AsNoTracking()
            .Where(m => m.Id == request.Id)
            .Select(m => new GetMoodByIdDto(
                m.Id,
                m.Name,
                m.CoverUrl != null ? $"/storage/{m.CoverUrl}" : null
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (mood is null)
            throw new NotFoundException(nameof(Mood), request.Id);

        return mood;
    }
}