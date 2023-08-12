
namespace ExportOrderEntites;

public class AppObjectResponse
{
    public object? Object { get; set; }
    public bool HasError => Error.Count > 0;
    public List<string> Error { get; set; } = new List<string>();

    public void ErrorAdd(string err) { Error.Add(err); }
}
