namespace CareerConnect.Application.DTOs.Jobs;

public class SemanticSearchParams
{
    public string? Keyword { get; set; }
    public string? Location { get; set; }
    public string? JobType { get; set; }
    public decimal? MinSalary { get; set; }
    public string? Summary { get; set; }
}
