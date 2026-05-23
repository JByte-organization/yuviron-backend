using MediatR;
using System;

namespace Yuviron.Application.Features.Client.Users.Queries.GetUserProfile;

public sealed record GetUserProfileQuery(Guid UserId) : IRequest<UserProfileDto>;