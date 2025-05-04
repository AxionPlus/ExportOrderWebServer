namespace ExportOrderEntites;

public class AppObjectResponse
{
    public object? Object { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new List<string>();
    public bool HasErrors => Errors.Any();
    public void ErrorAdd(string err) { Errors.Add(err); }
    public bool HasCriticalErrors { get; set; } = false;
}
