namespace LogMate.Common.Http;

public class ApiResponse
{
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public Dictionary<string, object>? Extensions { get; set; }
}
