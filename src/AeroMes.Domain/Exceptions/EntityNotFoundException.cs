namespace AeroMes.Domain.Exceptions;

public class EntityNotFoundException(string entityName, object key)
    : DomainException($"{entityName} '{key}' was not found.") { }
