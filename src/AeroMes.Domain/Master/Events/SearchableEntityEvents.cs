using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master.Events;

public record ProductSavedEvent(string ProductCode, string ProductName) : IDomainEvent;
public record ProductDeletedEvent(string ProductCode) : IDomainEvent;
public record CustomerSavedEvent(string CustomerCode, string CustomerName, string? TaxId, string? Address) : IDomainEvent;
public record CustomerDeletedEvent(string CustomerCode) : IDomainEvent;
public record EmployeeSavedEvent(string EmployeeCode, string FullName, string? Department) : IDomainEvent;
public record EmployeeDeletedEvent(string EmployeeCode) : IDomainEvent;
