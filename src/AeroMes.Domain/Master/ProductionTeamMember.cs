using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class ProductionTeamMember : Entity
{
    public int MemberId { get; private set; }
    public string TeamCode { get; private set; } = string.Empty;
    public string EmployeeCode { get; private set; } = string.Empty;
    public bool IsLeader { get; private set; }

    public Employee? Employee { get; private set; }

    private ProductionTeamMember() { }

    internal static ProductionTeamMember Create(string teamCode, string employeeCode, bool isLeader)
    {
        return new ProductionTeamMember
        {
            TeamCode = teamCode,
            EmployeeCode = employeeCode.Trim().ToUpperInvariant(),
            IsLeader = isLeader,
        };
    }
}
