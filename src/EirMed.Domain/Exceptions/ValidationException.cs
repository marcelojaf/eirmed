namespace EirMed.Domain.Exceptions;

public class ValidationException : DomainException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Um ou mais erros de validação ocorreram.")
    {
        Errors = errors;
    }

    public ValidationException(string field, string message)
        : base("Um ou mais erros de validação ocorreram.")
    {
        Errors = new Dictionary<string, string[]>
        {
            { field, [message] }
        };
    }
}
