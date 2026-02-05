using ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Dto;
using ClosedXML.Excel;
namespace ExportOrderWebServer.Areas.ImportDocument.Services;

public interface IExcelExportService
{
    Task<byte[]> ExportBillOfLadingToExcelAsync(List<BillOfLadingBaseDto> billOfLadingList);
    Task<byte[]> ExportBillOfLadingToExcelAsync(BillOfLadingBaseDto billOfLading);
    Task<string> ExportBillOfLadingToExcelFileAsync(List<BillOfLadingBaseDto> billOfLadingList, string filePath);
    Task<byte[]> ExportBillOfLadingContainersToExcelAsync(BillOfLadingBaseDto billOfLading);


}

// ClosedXmlExcelExportService.cs




public class ClosedXmlExcelExportService : IExcelExportService
{
    public async Task<byte[]> ExportBillOfLadingToExcelAsync(List<BillOfLadingBaseDto> billOfLadingList)
    {
        if (billOfLadingList == null || !billOfLadingList.Any())
            throw new ArgumentException("Список коносаментов пуст");

        using var workbook = new XLWorkbook();

        // 1. Лист со сводной информацией
        var summaryWorksheet = workbook.Worksheets.Add("Сводка по коносаментам");
        FillSummaryWorksheet(summaryWorksheet, billOfLadingList);

        // 2. Лист с детальной информацией по всем коносаментам
        var detailsWorksheet = workbook.Worksheets.Add("Детали коносаментов");
        FillDetailsWorksheet(detailsWorksheet, billOfLadingList);

        // 3. Лист с контейнерами (группировка по коносаментам)
        var containersWorksheet = workbook.Worksheets.Add("Контейнеры");
        FillAllContainersWorksheet(containersWorksheet, billOfLadingList);

        // 4. Отдельные листы для каждого коносамента с контейнерами
        //foreach (var bol in billOfLadingList)
        //{
        //    var worksheetName = GetValidWorksheetName($"Конт {bol.Num}");
        //    var bolWorksheet = workbook.Worksheets.Add(worksheetName);
        //    FillBolContainerWorksheet(bolWorksheet, bol);
        //}

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return await Task.FromResult(stream.ToArray());
    }

    public async Task<byte[]> ExportBillOfLadingToExcelAsync(BillOfLadingBaseDto billOfLading)
    {
        return await ExportBillOfLadingToExcelAsync(new List<BillOfLadingBaseDto> { billOfLading });
    }

    public async Task<string> ExportBillOfLadingToExcelFileAsync(List<BillOfLadingBaseDto> billOfLadingList, string filePath)
    {
        var bytes = await ExportBillOfLadingToExcelAsync(billOfLadingList);
        await File.WriteAllBytesAsync(filePath, bytes);
        return filePath;
    }

    public async Task<byte[]> ExportBillOfLadingContainersToExcelAsync(BillOfLadingBaseDto billOfLading)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add($"Контейнеры_{billOfLading.Num}");
        FillBolContainerWorksheet(worksheet, billOfLading);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return await Task.FromResult(stream.ToArray());
    }

    private void FillSummaryWorksheet(IXLWorksheet worksheet, List<BillOfLadingBaseDto> billOfLadingList)
    {
        // Заголовок
        worksheet.Cell(1, 1).Value = "Сводная информация по коносаментам";
        worksheet.Range(1, 1, 1, 5).Merge();
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 16;
        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        // Заголовки столбцов
        var headers = new[]
        {
            "Кол-во коносаментов", "Общее кол-во контейнеров",
            "Общий вес брутто (кг)", "Общее кол-во мест", "Коносаментов с переводом"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(3, i + 1).Value = headers[i];
            worksheet.Cell(3, i + 1).Style.Font.Bold = true;
            worksheet.Cell(3, i + 1).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            worksheet.Cell(3, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        // Данные
        var totalContainers = billOfLadingList.Sum(b => b.TotalContainers);
        var totalWeight = billOfLadingList.Sum(b => b.TotalGrossWeight);
        var totalPackages = billOfLadingList.Sum(b => b.TotalNoOfPackage);
        var withTranslation = billOfLadingList.Count(b => b.HasTranslate);

        worksheet.Cell(4, 1).Value = billOfLadingList.Count;
        worksheet.Cell(4, 2).Value = totalContainers;
        worksheet.Cell(4, 3).Value = totalWeight;
        worksheet.Cell(4, 4).Value = totalPackages;
        worksheet.Cell(4, 5).Value = withTranslation;

        // Форматирование
        worksheet.Cell(4, 3).Style.NumberFormat.Format = "0.00";
        worksheet.Range(4, 1, 4, 5).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        worksheet.Range(4, 1, 4, 5).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
        worksheet.Range(4, 1, 4, 5).Style.Border.RightBorder = XLBorderStyleValues.Thin;

        worksheet.Columns().AdjustToContents();
    }

    private void FillDetailsWorksheet(IXLWorksheet worksheet, List<BillOfLadingBaseDto> billOfLadingList)
    {
        // Заголовок
        worksheet.Cell(1, 1).Value = "Детали коносаментов";
        worksheet.Range(1, 1, 1, 20).Merge();
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Заголовки столбцов
        var headers = new[]
        {
            "№ Коносамента", "Дата", "Код клиента", "Грузоотправитель", "Грузоотправитель RU",
            "Адрес грузоотправителя", "Грузополучатель", "Грузополучатель RU",
            "Адрес грузополучателя", "Адрес RU", "Страна RU", "Порт погрузки",
            "Порт выгрузки", "Описание груза", "Описание груза RU", "Контейнеров",
            "Вес брутто", "Кол-во мест", "Перевод", "Перевозчик"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(3, i + 1).Value = headers[i];
            worksheet.Cell(3, i + 1).Style.Font.Bold = true;
            worksheet.Cell(3, i + 1).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            worksheet.Cell(3, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
            worksheet.Cell(3, i + 1).Style.Alignment.WrapText = true;
        }

        // Данные
        int row = 4;
        foreach (var bol in billOfLadingList)
        {
            worksheet.Cell(row, 1).Value = bol.Num;
            worksheet.Cell(row, 2).Value = bol.Date?.ToString("dd.MM.yyyy");
            worksheet.Cell(row, 3).Value = bol.CustomerCode;
            worksheet.Cell(row, 4).Value = bol.ShipperName;
            worksheet.Cell(row, 5).Value = bol.ShipperNameRu;
            worksheet.Cell(row, 6).Value = bol.ShipperAddress;
            worksheet.Cell(row, 7).Value = bol.ConsigneeName;
            worksheet.Cell(row, 8).Value = bol.ConsigneeNameRu;
            worksheet.Cell(row, 9).Value = bol.ConsigneeAddress;
            worksheet.Cell(row, 10).Value = bol.ConsigneeAddressRu;
            worksheet.Cell(row, 11).Value = bol.ConsigneeCountryRu;
            worksheet.Cell(row, 12).Value = bol.Pol?.IsoCode ?? string.Empty;
            worksheet.Cell(row, 13).Value = bol.Pod;
            worksheet.Cell(row, 14).Value = bol.CargoDescription;
            worksheet.Cell(row, 15).Value = bol.CargoDescriptionRu;
            worksheet.Cell(row, 16).Value = bol.TotalContainers;
            worksheet.Cell(row, 17).Value = bol.TotalGrossWeight;
            worksheet.Cell(row, 18).Value = bol.TotalNoOfPackage;
            worksheet.Cell(row, 19).Value = bol.HasTranslate ? "✓" : "✗";
            worksheet.Cell(row, 20).Value = bol.Carrier;

            // Форматирование
            worksheet.Cell(row, 16).Style.NumberFormat.Format = "0";
            worksheet.Cell(row, 17).Style.NumberFormat.Format = "0.00";
            worksheet.Cell(row, 18).Style.NumberFormat.Format = "0";

            // Цветовая индикация перевода
            worksheet.Cell(row, 19).Style.Font.FontColor = bol.HasTranslate
                ? XLColor.DarkGreen
                : XLColor.DarkRed;

            row++;
        }

        // Добавляем границы
        var dataRange = worksheet.Range(3, 1, row - 1, headers.Length);
        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        // Автоподбор ширины столбцов
        worksheet.Columns().AdjustToContents();

        // Фиксируем заголовки
        worksheet.SheetView.FreezeRows(3);
    }

    private void FillAllContainersWorksheet(IXLWorksheet worksheet, List<BillOfLadingBaseDto> billOfLadingList)
    {
        worksheet.Cell(1, 1).Value = "Все контейнеры";
        worksheet.Range(1, 1, 1, 18).Merge();
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        var headers = new[]
        {
            "№ Коносамента", "№ Контейнера", "Тип контейнера", "ISO код", "Вес тары",
            "Статус", "SOC", "№ Пломбы", "Тип упаковки", "Кол-во мест",
            "Вес брутто", "Ед. измерения", "Объем", "Температура",
            "Класс опасности","ООН Номер", "Booking №", "Контейнер как груз", "Описание RU"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(3, i + 1).Value = headers[i];
            worksheet.Cell(3, i + 1).Style.Font.Bold = true;
            worksheet.Cell(3, i + 1).Style.Fill.BackgroundColor = XLColor.LightGreen;
            worksheet.Cell(3, i + 1).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        }

        int row = 4;
        foreach (var bol in billOfLadingList)
        {
            foreach (var container in bol.ContainerRecords)
            {
                worksheet.Cell(row, 1).Value = bol.Num;
                worksheet.Cell(row, 2).Value = container.ContainerNo;
                worksheet.Cell(row, 3).Value = container.ContainerTypeId;
                worksheet.Cell(row, 4).Value = container.IsoCode;
                worksheet.Cell(row, 5).Value = container.TareWt;
                worksheet.Cell(row, 6).Value = container.FullOrEmpty;
                worksheet.Cell(row, 7).Value = container.IsSoc ? "Да" : "Нет";
                worksheet.Cell(row, 8).Value = container.SealNo;
                worksheet.Cell(row, 9).Value = container.PackageType;
                worksheet.Cell(row, 10).Value = container.NoOfPackage;
                worksheet.Cell(row, 11).Value = container.GrossWeight;
                worksheet.Cell(row, 12).Value = container.GrossWeightUOM;
                worksheet.Cell(row, 13).Value = container.Volume;
                worksheet.Cell(row, 14).Value = container.ReeferFullTemp;
                worksheet.Cell(row, 15).Value = container.IMCOClass;
                worksheet.Cell(row, 16).Value = container.IMCONumber;
                worksheet.Cell(row, 17).Value = container.BookingNo;
                worksheet.Cell(row, 18).Value = container.ContainerAsCargo ? "Да" : "Нет";
                worksheet.Cell(row, 19).Value = container.CargoDescriptionRu;

                // Форматирование
                worksheet.Cell(row, 10).Style.NumberFormat.Format = "0";
                worksheet.Cell(row, 11).Style.NumberFormat.Format = "0.00";
                worksheet.Cell(row, 13).Style.NumberFormat.Format = "0.00";

                row++;
            }
        }

        // Итоги внизу
        var totalContainers = billOfLadingList.Sum(b => b.TotalContainers);
        var totalWeight = billOfLadingList.Sum(b => b.TotalGrossWeight);
        var totalPackages = billOfLadingList.Sum(b => b.TotalNoOfPackage);

        worksheet.Cell(row + 1, 1).Value = "ИТОГО:";
        worksheet.Cell(row + 1, 1).Style.Font.Bold = true;
        worksheet.Cell(row + 1, 2).Value = $"Контейнеров: {totalContainers}";
        worksheet.Cell(row + 1, 3).Value = $"Вес: {totalWeight:F2} кг";
        worksheet.Cell(row + 1, 4).Value = $"Мест: {totalPackages}";

        // Границы
        var dataRange = worksheet.Range(3, 1, row - 1, headers.Length);
        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        worksheet.Columns().AdjustToContents();
        worksheet.SheetView.FreezeRows(3);
    }

    private void FillBolContainerWorksheet(IXLWorksheet worksheet, BillOfLadingBaseDto billOfLading)
    {
        // Заголовок с информацией о коносаменте
        worksheet.Cell(1, 1).Value = $"Коносамент: {billOfLading.Num}";
        worksheet.Range(1, 1, 1, 5).Merge();
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;

        worksheet.Cell(2, 1).Value = $"Дата: {billOfLading.Date?.ToString("dd.MM.yyyy")}";
        worksheet.Cell(2, 2).Value = $"Грузоотправитель: {billOfLading.ShipperName}";
        worksheet.Cell(2, 3).Value = $"Грузополучатель: {billOfLading.ConsigneeName}";
        worksheet.Cell(2, 4).Value = $"POL: {billOfLading.Pol?.IsoCode}";
        worksheet.Cell(2, 5).Value = $"POD: {billOfLading.Pod}";

        int startRow = 4;

        // Заголовки для контейнеров
        var headers = new[]
        {
            "№ Контейнера", "Тип", "ISO код", "Вес тары", "Статус",
            "SOC", "№ Пломбы", "Тип упаковки", "Кол-во мест", "Вес брутто",
            "Ед. веса", "Объем", "Температура", "Класс опасности",
            "Номер опасности", "Booking №", "Контейнер как груз", "Описание RU"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(startRow, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightSkyBlue;
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.RightBorder = XLBorderStyleValues.Thin;
        }

        // Данные контейнеров
        int row = startRow + 1;
        foreach (var container in billOfLading.ContainerRecords)
        {
            worksheet.Cell(row, 1).Value = container.ContainerNo;
            worksheet.Cell(row, 2).Value = container.ContainerTypeId;
            worksheet.Cell(row, 3).Value = container.IsoCode;
            worksheet.Cell(row, 4).Value = container.TareWt;
            worksheet.Cell(row, 5).Value = container.FullOrEmpty;
            worksheet.Cell(row, 6).Value = container.IsSoc ? "Да" : "Нет";
            worksheet.Cell(row, 7).Value = container.SealNo;
            worksheet.Cell(row, 8).Value = container.PackageType;
            worksheet.Cell(row, 9).Value = container.NoOfPackage;
            worksheet.Cell(row, 10).Value = container.GrossWeight;
            worksheet.Cell(row, 11).Value = container.GrossWeightUOM;
            worksheet.Cell(row, 12).Value = container.Volume;
            worksheet.Cell(row, 13).Value = container.ReeferFullTemp;
            worksheet.Cell(row, 14).Value = container.IMCOClass;
            worksheet.Cell(row, 15).Value = container.IMCONumber;
            worksheet.Cell(row, 16).Value = container.BookingNo;
            worksheet.Cell(row, 17).Value = container.ContainerAsCargo ? "Да" : "Нет";
            worksheet.Cell(row, 18).Value = container.CargoDescriptionRu;

            // Форматирование числовых полей
            worksheet.Cell(row, 9).Style.NumberFormat.Format = "0";
            worksheet.Cell(row, 10).Style.NumberFormat.Format = "0.00";
            worksheet.Cell(row, 12).Style.NumberFormat.Format = "0.00";

            // Границы для строки
            for (int col = 1; col <= headers.Length; col++)
            {
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
            }

            row++;
        }

        // Итоговая строка
        worksheet.Cell(row + 1, 1).Value = "ИТОГО ПО КОНОСАМЕНТУ:";
        worksheet.Cell(row + 1, 1).Style.Font.Bold = true;
        worksheet.Range(row + 1, 1, row + 1, 3).Merge();

        worksheet.Cell(row + 2, 1).Value = "Контейнеров:";
        worksheet.Cell(row + 2, 2).Value = billOfLading.TotalContainers;

        worksheet.Cell(row + 3, 1).Value = "Вес брутто:";
        worksheet.Cell(row + 3, 2).Value = billOfLading.TotalGrossWeight;
        worksheet.Cell(row + 3, 2).Style.NumberFormat.Format = "0.00";

        worksheet.Cell(row + 4, 1).Value = "Кол-во мест:";
        worksheet.Cell(row + 4, 2).Value = billOfLading.TotalNoOfPackage;

        worksheet.Columns().AdjustToContents();
    }

    private string GetValidWorksheetName(string name)
    {
        // Максимальная длина имени листа в Excel - 31 символ
        if (name.Length > 31)
            name = name.Substring(0, 31);

        // Запрещенные символы
        var invalidChars = new[] { ':', '\\', '/', '?', '*', '[', ']' };
        foreach (var c in invalidChars)
            name = name.Replace(c, '_');

        // Имя не может быть пустым
        if (string.IsNullOrWhiteSpace(name))
            name = "Sheet1";

        // Имя не может начинаться или заканчиваться апострофом
        name = name.Trim('\'');

        return name;
    }
}