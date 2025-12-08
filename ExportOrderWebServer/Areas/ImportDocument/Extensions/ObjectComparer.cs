using System.Text.Json;

namespace ExportOrderWebServer.Areas.ImportDocument.Extensions;

public static class ObjectComparer
{
    public static bool AreEqualByJsonBytes<T>(T obj1, T obj2)
    {
        if (ReferenceEquals(obj1, obj2)) return true;
        if (obj1 == null || obj2 == null) return false;

        var bytes1 = JsonSerializer.SerializeToUtf8Bytes(obj1);
        var bytes2 = JsonSerializer.SerializeToUtf8Bytes(obj2);

        return bytes1.AsSpan().SequenceEqual(bytes2);
    }
}
public static class BackupObject
{

    public static T DeepCopy<T>(T obj)
    {
        string json = JsonSerializer.Serialize(obj);
        return JsonSerializer.Deserialize<T>(json)!;
    }


    public static T CreateBackup<T>(T original) where T : new()
    {
        var backup = new T();
        var properties = typeof(T).GetProperties();

        foreach (var prop in properties)
        {
            if (prop.CanRead && prop.CanWrite)
            {
                var value = prop.GetValue(original);
                prop.SetValue(backup, value);
            }
        }

        return backup;
    }
    public static void CreateObjectBackup<T>(T backup, object original)
    {
        var properties = typeof(T).GetProperties();

        foreach (var prop in properties)
        {
            if (prop.CanRead && prop.CanWrite)
            {
                var value = prop.GetValue(backup);
                prop.SetValue(original, value);
            }
        }
    }



}
