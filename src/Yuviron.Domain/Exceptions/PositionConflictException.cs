namespace Yuviron.Domain.Exceptions;

public class PositionConflictException : DomainException
{
    public PositionConflictException(int position, string entityName = "item")
        : base($"Position {position} is already taken by another {entityName}.")
    {
    }
}