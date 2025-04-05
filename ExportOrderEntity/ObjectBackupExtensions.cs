namespace ExportOrderEntites;

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class ObjectBackupExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true, // Для удобочитаемости резервных файлов
        ReferenceHandler = ReferenceHandler.Preserve, // Для обработки циклических ссылок
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull // Пропускать null значения
    };

    /// <summary>
    /// Создает глубокую копию объекта через JSON сериализацию
    /// </summary>
    /// <typeparam name="T">Тип объекта</typeparam>
    /// <param name="obj">Объект для резервного копирования</param>
    /// <returns>Новая копия объекта</returns>
    public static T CreateJsonBackup<T>(this T obj)
    {
        if (obj == null) return default;

        try
        {
            var json = JsonSerializer.Serialize(obj, _jsonOptions);
            return JsonSerializer.Deserialize<T>(json, _jsonOptions)!;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Ошибка при создании резервной копии через JSON сериализацию", ex);
        }
    }

    /// <summary>
    /// Сохраняет резервную копию объекта в JSON файл
    /// </summary>
    /// <typeparam name="T">Тип объекта</typeparam>
    /// <param name="obj">Объект для резервного копирования</param>
    /// <param name="filePath">Путь к файлу для сохранения</param>
    public static void SaveJsonBackupToFile<T>(this T obj, string filePath)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));

        try
        {
            var json = JsonSerializer.Serialize(obj, _jsonOptions);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex) when (ex is JsonException || ex is IOException)
        {
            throw new InvalidOperationException($"Ошибка при сохранении резервной копии в файл {filePath}", ex);
        }
    }

    /// <summary>
    /// Восстанавливает объект из JSON файла резервной копии
    /// </summary>
    /// <typeparam name="T">Тип объекта</typeparam>
    /// <param name="filePath">Путь к файлу резервной копии</param>
    /// <returns>Восстановленный объект</returns>
    public static T RestoreFromJsonBackup<T>(string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(json, _jsonOptions)!;
        }
        catch (Exception ex) when (ex is JsonException || ex is IOException)
        {
            throw new InvalidOperationException($"Ошибка при восстановлении из резервной копии {filePath}", ex);
        }
    }
}


