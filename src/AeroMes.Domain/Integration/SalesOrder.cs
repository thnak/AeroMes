using AeroMes.Domain.Common;

namespace AeroMes.Domain.Integration;

public enum SalesOrderStatus { Open, Closed, Cancelled }

public class SalesOrder : Entity
{
    public int SOID { get; private set; }
    public string SOCode { get; private set; } = string.Empty;
    public string? CustomerName { get; private set; }
    public DateTime OrderDate { get; private set; }
    public DateTime? DeliveryDate { get; private set; }
    public SalesOrderStatus Status { get; private set; } = SalesOrderStatus.Open;
    public DateTime SyncedAt { get; private set; }

    private SalesOrder() { }

    public static SalesOrder CreateFromErp(
        string soCode,
        DateTime orderDate,
        string? customerName = null,
        DateTime? deliveryDate = null)
    {
        return new SalesOrder
        {
            SOCode = soCode.Trim().ToUpperInvariant(),
            CustomerName = customerName,
            OrderDate = orderDate,
            DeliveryDate = deliveryDate,
            Status = SalesOrderStatus.Open,
            SyncedAt = DateTime.UtcNow,
        };
    }

    public void Close() => Status = SalesOrderStatus.Closed;
    public void Cancel() => Status = SalesOrderStatus.Cancelled;
    public void Resync() => SyncedAt = DateTime.UtcNow;
}
