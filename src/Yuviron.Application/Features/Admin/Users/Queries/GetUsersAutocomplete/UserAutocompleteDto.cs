using System;
using MediatR;
using System.Collections.Generic;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Users.Queries.GetUsersAutocomplete;

public sealed record UserAutocompleteDto(
    Guid Id,
    string Email,
    string DisplayName,
    string? AvatarUrl
);