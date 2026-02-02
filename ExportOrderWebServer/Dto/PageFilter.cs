using System.ComponentModel.DataAnnotations;

namespace ExportOrderWebServer.Dto;

public interface IPageFilter
{
    int PageNumber { get; set; }

    int PageSize { get; set; }
    // Сортировка
    string? SortBy { get; set; }
    bool SortDescending { get; set; }

    // Поиск по нескольким полям
    string? SearchTerm { get; set; }
}

public abstract class PageFilter : IPageFilter
{
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;

    [Range(1, 100)] public int PageSize { get; set; } = 25;
    // Сортировка
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }

    // Поиск по нескольким полям
    public string? SearchTerm { get; set; }
}
