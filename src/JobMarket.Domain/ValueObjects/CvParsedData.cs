namespace JobMarket.Domain.ValueObjects;

public class CvParsedData
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public List<string> Skills { get; init; } = new();
    public List<WorkExperience> Experience { get; init; } = new();
    public List<Education> Education { get; init; } = new();
    public List<string> Languages { get; init; } = new();
    public int TotalYearsExperience { get; init; }
}
