using MediatR;
using System;

namespace Yuviron.Application.Features.Client.Settings.Queries.GetUserSettings;

public record GetUserSettingsQuery : IRequest<UserSettingsDto>;