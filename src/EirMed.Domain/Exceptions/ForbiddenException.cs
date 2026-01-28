namespace EirMed.Domain.Exceptions;

public class ForbiddenException : DomainException
{
    public ForbiddenException(string message = "Acesso proibido.")
        : base(message) { }
}
