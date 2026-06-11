using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Exceptions;

public sealed class SocialLinkAlreadyExistsException : DomainException
{
    public SocialLinkAlreadyExistsException(SocialLinkType type) 
        : base($"A social link for {type} already exists. You must remove it before adding a new one.")
    {
    }
}