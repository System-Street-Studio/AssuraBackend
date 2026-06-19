namespace Assura.Application.DTOs;

public class InformStakeholdersDto
{
    public int InformingId { get; set; }
    public int EmployeeId { get; set; }
    public bool DivisionHeadNotify { get; set; }
    public string? Remarks { get; set; }
}
