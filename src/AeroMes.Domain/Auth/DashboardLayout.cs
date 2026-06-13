namespace AeroMes.Domain.Auth;

public class DashboardLayout
{
    public int LayoutId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string LayoutJson { get; private set; } = "[]";
    public DateTime UpdatedAt { get; private set; }

    private DashboardLayout() { }

    public static DashboardLayout Create(string userId, string layoutJson) => new()
    {
        UserId = userId,
        LayoutJson = layoutJson,
        UpdatedAt = DateTime.UtcNow
    };

    public void Update(string layoutJson)
    {
        LayoutJson = layoutJson;
        UpdatedAt = DateTime.UtcNow;
    }
}
