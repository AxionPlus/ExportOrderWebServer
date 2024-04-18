using Newtonsoft.Json;

namespace ExportOrderEntites;

public class ControllerPassObject<T>
{
    public string? Remarks { get; set; }
    public string User { get; set; }
    public object Object { get; set; }
    public T? GetObject => JsonConvert.DeserializeObject<T>(Object.ToString());
}
