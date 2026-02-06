namespace EirMed.Domain.Exceptions;

public class NotFoundException : DomainException
{
    public string EntityName { get; }
    public object? EntityId { get; }

    public NotFoundException(string entityName, object? entityId = null)
        : base($"{entityName}{(entityId is not null ? $" com identificador '{entityId}'" : "")} não foi encontrado(a).")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}
