namespace JobMarket.Domain.ValueObjects;

public class CvParsedData
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = new();
    public List<WorkExperience> Experience { get; set; } = new();
    public List<Education> Education { get; set; } = new();
    public List<string> Languages { get; set; } = new();
    public int TotalYearsExperience { get; set; }
}
