using MediatR;

public sealed record ChangePasswordCommand(
    string OldPassword,
    string NewPassword
) : IRequest<Unit>;