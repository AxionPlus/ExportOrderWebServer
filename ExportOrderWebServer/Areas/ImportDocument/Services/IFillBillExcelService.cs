using ClosedXML.Excel;
using ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Dto;
using ExportOrderWebServer.Areas.ImportDocument.VesselCall.Dto;

namespace ExportOrderWebServer.Areas.ImportDocument.Services;

public interface IFillBillExcelService
{
    public Task<byte[]> GenerateExcel(VesselCallDto vesselCall);
}



public class FillBillExcelService : IFillBillExcelService
{
    /// <summary>
    /// Генерация Excel файла из данных VesselCallDto
    /// </summary>
    public async Task<byte[]> GenerateExcel(VesselCallDto vesselCall)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("ПРИХОД");

        // Настройка стилей
        var headerStyle = worksheet.Style;
        headerStyle.Font.Bold = true;
        headerStyle.Font.FontSize = 10;
        headerStyle.Font.FontName = "Calibri";
        headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        headerStyle.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        headerStyle.Border.OutsideBorder = XLBorderStyleValues.Thin;

        // ============================================
        // ЗАГОЛОВОК
        // ============================================
        int row = 1;

        worksheet.Cell(row, 9).Value = "Подписант: Фамилия";
        worksheet.Cell(row, 10).Value = vesselCall.CaptainLastName;
        row++;

        worksheet.Cell(row, 1).Value = "Название парохода";
        worksheet.Cell(row, 2).Value = vesselCall.Vessel?.Name ?? string.Empty;

        worksheet.Cell(row, 9).Value = "Имя и Отчество";
        worksheet.Cell(row, 10).Value = vesselCall.CaptainName;
        row++;

        worksheet.Cell(row, 1).Value = "Флаг";
        worksheet.Cell(row, 2).Value = vesselCall.Vessel?.FlagRu ?? string.Empty;

        worksheet.Cell(row, 9).Value = "Должность";
        worksheet.Cell(row, 10).Value = "КАПИТАН";
        row++;

        worksheet.Cell(row, 1).Value = "Номер рейса";
        worksheet.Cell(row, 2).Value = vesselCall.VoyageNo;
        worksheet.Cell(row, 9).Value = "Дата подписи";
        worksheet.Cell(row, 10).Value = vesselCall.ETA?.ToString("dd/MM/yy") ?? string.Empty;
        row++;

        worksheet.Cell(row, 9).Value = "Порт отправления";
        worksheet.Cell(row, 10).Value = vesselCall.PortOfLoading?.NameRu ?? string.Empty;
        worksheet.Cell(row, 1).Value = "Код ТО назначения";
        worksheet.Cell(row, 2).Value = vesselCall.Terminal.CustomsPost;
        row++;

        worksheet.Cell(row, 9).Value = "Страна отправления";
        worksheet.Cell(row, 10).Value = vesselCall.PortOfLoading?.CountryRu ?? string.Empty;
        row++;

        worksheet.Cell(row, 1).Value = "Перевозчик";
        worksheet.Cell(row, 2).Value = "ALPHA SHIPPING (SHANGHAI) LTD";
        worksheet.Cell(row, 9).Value = "ПРИХОД ДАТА";
        worksheet.Cell(row, 10).Value = vesselCall.ETA?.ToString("dd/MM/yy") ?? string.Empty;
        row++;

        row++; // Пустая строка для разделения

        worksheet.Cell(row, 1).Value = "Страна";
        worksheet.Cell(row, 2).Value = "КИТАЙ";

        row++;


        // ============================================
        // ЗАГОЛОВОК ТАБЛИЦЫ КОНТЕЙНЕРОВ
        // ============================================
        int headerRow = row;

        var headers = new[]
        {
            "Номер Контейнера", "Тип", "Размер", "Вес порож", "Пломба", "Температура",
            "Вес груза", "Кол-во", "Груз англ", "Груз", "Код ТНВЭД",
            "Класс опасности IMO", "Класс опасности UNNO", "Коносамент", "Коносамент Дата",
            "Место выпуска", "Код порта погрузки", "Отправитель", "Отправитель Страна",
            "Отправитель Город", "Адрес отправителя", "Получатель", "Получатель Страна",
            "Получатель Город", "Адрес", "Notify", "Notify Страна", "Notify Город",
            "Адрес", "Таможенный режим"
        };

        for (int col = 0; col < headers.Length; col++)
        {
            var cell = worksheet.Cell(row, col + 1);
            cell.Value = headers[col];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }
        worksheet.SheetView.FreezeRows(row);

        row++;

        // ============================================
        // ДАННЫЕ КОНТЕЙНЕРОВ
        // ============================================

        var billOfLadings = vesselCall.BillOfLadings.OrderBy(s => s.Num).ToList();

        foreach (var billOfLading in billOfLadings)
            foreach (var container in billOfLading.ContainerRecords.OrderBy(s => s.ContainerNo).ToList())
            {
                if (container.ContainerAsCargo)
                {
                    container.GrossWeight += container.TareWt;
                    container.TareWt = 0;

                    container.NoOfPackage += 1;
                }

                int col = 1;

                worksheet.Cell(row, col++).Value = container.ContainerNo;
                worksheet.Cell(row, col++).Value = container.ContainerTypeId.Substring(0, 2);
                worksheet.Cell(row, col++).Value = container.ContainerTypeId.Substring(2, 2);
                worksheet.Cell(row, col++).Value = container.TareWt;
                worksheet.Cell(row, col++).Value = container.SealNo;
                worksheet.Cell(row, col++).Value = string.Empty;// Температура
                worksheet.Cell(row, col++).Value = container.GrossWeight;
                worksheet.Cell(row, col++).Value = container.NoOfPackage;
                worksheet.Cell(row, col++).Value = string.Empty; // Груз англ
                worksheet.Cell(row, col++).Value = container.CargoDescriptionRu;
                worksheet.Cell(row, col++).Value = string.Empty; // Код ТНВЭД
                worksheet.Cell(row, col++).Value = container.IMCOClass;
                worksheet.Cell(row, col++).Value = container.IMCONumber;
                worksheet.Cell(row, col++).Value = billOfLading.Num;
                worksheet.Cell(row, col++).Value = billOfLading.TsDate;
                worksheet.Cell(row, col++).Value = billOfLading.TsPort?.IsoCode ?? string.Empty;
                worksheet.Cell(row, col++).Value = billOfLading.Pol?.IsoCode ?? string.Empty;
                worksheet.Cell(row, col++).Value = billOfLading.ShipperNameRu;
                worksheet.Cell(row, col++).Value = billOfLading.Pol?.CountryRu;
                worksheet.Cell(row, col++).Value = string.Empty; // Shipper City
                worksheet.Cell(row, col++).Value = string.Empty; // Shipper Address;
                worksheet.Cell(row, col++).Value = billOfLading.ConsigneeNameRu;
                worksheet.Cell(row, col++).Value = billOfLading.ConsigneeCountryRu;
                worksheet.Cell(row, col++).Value = string.Empty; // Consignee City
                worksheet.Cell(row, col++).Value = billOfLading.ConsigneeAddressRu;
                worksheet.Cell(row, col++).Value = string.Empty; // Notify
                worksheet.Cell(row, col++).Value = string.Empty; // Notify Country
                worksheet.Cell(row, col++).Value = string.Empty; // Notify City
                worksheet.Cell(row, col++).Value = string.Empty; // Notify Address
                worksheet.Cell(row, col++).Value = billOfLading.CustomsMode;

                // Стиль ячеек

                worksheet.Range(row, 1, row, col).Style.Font.Bold = false;


                row++;
            }

        // ============================================
        // ИТОГОВАЯ СТРОКА
        // ============================================


        // Автофильтр
        worksheet.Range(headerRow, 1, headerRow, headers.Length).SetAutoFilter();

        // Автоподбор ширины колонок
        worksheet.Columns().AdjustToContents();

        worksheet.Column(2).Width = 5;
        worksheet.Column(3).Width = 5;
        worksheet.Column(4).Width = 7;
        worksheet.Column(6).Width = 5;
        worksheet.Column(10).Width = 120;
        worksheet.Column(11).Width = 5;
        worksheet.Column(12).Width = 9;
        worksheet.Column(13).Width = 9;
        worksheet.Column(18).Width = 45;
        worksheet.Column(19).Width = 30;
        worksheet.Column(20).Width = 5;
        worksheet.Column(21).Width = 5;
        worksheet.Column(24).Width = 5;
        worksheet.Column(26).Width = 5;
        worksheet.Column(27).Width = 5;
        worksheet.Column(28).Width = 5;
        worksheet.Column(29).Width = 5;


        // Сохранение в byte[]
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return await Task.FromResult(stream.ToArray());
    }


}