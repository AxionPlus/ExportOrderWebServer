using ClosedXML.Excel;
using ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Dto;
using ExportOrderWebServer.Areas.ImportDocument.VesselCall.Dto;

namespace ExportOrderWebServer.Areas.ImportDocument.Services;

public interface IArrivalNoticeExportService
{
    Task<byte[]> GenerateArrivalNoticeAsync(VesselCallDto vesselCall);

    Task<string> GenerateArrivalNoticeFileAsync(VesselCallDto vesselCall, string filePath);
}

public class ArrivalNoticeExportService : IArrivalNoticeExportService
{
    public async Task<byte[]> GenerateArrivalNoticeAsync(VesselCallDto vesselCall)
    {
        var vesselName = vesselCall.Vessel.Name;
        var voyageNumber = vesselCall.VoyageNo;
        var feederBl = vesselCall.FeederBlNo;
        var arrivalDate = vesselCall.ETA;
        var vesselFlag = vesselCall.Vessel.FlagRu;

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("УВЕДОМЛЕНИЕ");

        // Заголовок
        worksheet.Cell(1, 1).Value = "УВЕДОМЛЕНИЕ О ПРИБЫТИИ ТОВАРОВ";
        worksheet.Range(1, 1, 1, 14).Merge();
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;


        // Основная информация
        worksheet.Cell(2, 1).Value = "Порт выгрузки:";
        worksheet.Cell(2, 3).Value = "НОВОРОССИЙСК";

        worksheet.Cell(3, 1).Value = "Название судна:";
        worksheet.Cell(3, 3).Value = vesselName;
        worksheet.Cell(3, 5).Value = "Рейс:";
        worksheet.Cell(3, 6).Value = voyageNumber;

        worksheet.Cell(4, 1).Value = "Флаг судна:";
        worksheet.Cell(4, 3).Value = vesselFlag;

        worksheet.Cell(5, 1).Value = "Перевозчик:";
        worksheet.Cell(5, 3).Value = "ALPHA SHIPPING (SHANGHAI) LTD";
        worksheet.Cell(5, 7).Value = "Сервисный К/С:";
        worksheet.Cell(5, 9).Value = feederBl;

        worksheet.Cell(6, 1).Value = "Перевозчик страна:";
        worksheet.Cell(6, 3).Value = "КИТАЙ";
        worksheet.Cell(6, 7).Value = "Страна порта отправления:";

        worksheet.Cell(7, 1).Value = "Дата прихода:";
        worksheet.Cell(7, 3).Value = arrivalDate;
        worksheet.Range(7, 3, 7, 5).Merge();
        worksheet.Cell(7, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;


        // Заголовки таблицы (начиная с 9 строки)
        int startRow = 9;
        var headers = new[]
        {
            "№", "№контейнера", "Размер", "Тип", "№пломбы", "Кол-во мест",
            "Наименование заявленного груза", "Вес груза брт,кг",
            "Вес тары", "№коносамента", "Дата коносамента", "Грузоотправитель", "Грузополучатель", "Там. режим"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(startRow, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Alignment.WrapText = true;


            cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.RightBorder = XLBorderStyleValues.Thin;
        }

        // Данные
        int currentRow = startRow + 1;
        int itemNumber = 1;

        foreach (var bilOfLadingDto in vesselCall.BillOfLadings.OrderBy(s => s.TsDate).ThenBy(s => s.Num))
            foreach (var containerRecord in bilOfLadingDto.ContainerRecords.OrderBy(s => s.ContainerNo))
            {
                worksheet.Cell(currentRow, 1).Value = itemNumber;
                worksheet.Cell(currentRow, 1).Style.NumberFormat.Format = "0";

                worksheet.Cell(currentRow, 2).Value = containerRecord.ContainerNo;
                worksheet.Cell(currentRow, 3).Value = containerRecord.IsoCode;
                worksheet.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; 

                worksheet.Cell(currentRow, 4).Value = containerRecord.ContainerTypeId;
                worksheet.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(currentRow, 5).Value = containerRecord.SealNo;

                worksheet.Cell(currentRow, 6).Value = containerRecord.NoOfPackage;
                worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "0";
                worksheet.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(currentRow, 7).Value = containerRecord.CargoDescriptionRu;

                worksheet.Cell(currentRow, 8).Value = containerRecord.GrossWeight;
                worksheet.Cell(currentRow, 8).Style.NumberFormat.Format = @"# ##0.000\ _₽";
                worksheet.Cell(currentRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(currentRow, 9).Value = containerRecord.TareWt;
                worksheet.Cell(currentRow, 9).Style.NumberFormat.Format = "0";
                worksheet.Cell(currentRow, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(currentRow, 10).Value = bilOfLadingDto.Num;

                worksheet.Cell(currentRow, 11).Value = bilOfLadingDto.TsDate;
                worksheet.Cell(currentRow, 11).Style.NumberFormat.Format = "dd.MM.yy";
                worksheet.Cell(currentRow, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(currentRow, 12).Value = bilOfLadingDto.ShipperFullName;
                worksheet.Cell(currentRow, 13).Value = bilOfLadingDto.ConsigneeFullName;
                worksheet.Cell(currentRow, 14).Value = bilOfLadingDto.CustomsMode; // Таможенный режим
                worksheet.Cell(currentRow, 14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;


                // Добавление границ
                for (int col = 1; col <= headers.Length; col++)
                {
                    worksheet.Cell(currentRow, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(currentRow, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(currentRow, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                }

                currentRow++;
                itemNumber++;
            }

        // Итоги внизу таблицы
        int summaryStartRow = currentRow + 2;

        // Веса
        worksheet.Cell(summaryStartRow, 6).Value = "Вес груза:";
        worksheet.Cell(summaryStartRow, 7).FormulaA1 = $"SUM(I{startRow + 1}:I{currentRow - 1})";
        worksheet.Cell(summaryStartRow, 7).Style.NumberFormat.Format = @"# ##0.000\ _₽";

        worksheet.Cell(summaryStartRow + 1, 6).Value = "Вес тары:";
        worksheet.Cell(summaryStartRow + 1, 7).FormulaA1 = $"SUM(J{startRow + 1}:J{currentRow - 1})";
        worksheet.Cell(summaryStartRow + 1, 7).Style.NumberFormat.Format = @"# ##0.000\ _₽";

        worksheet.Cell(summaryStartRow + 2, 6).Value = "Общий вес с тарой:";
        worksheet.Cell(summaryStartRow + 2, 7).FormulaA1 =
            $"SUM(G{summaryStartRow}:H{summaryStartRow + 1})";
        worksheet.Cell(summaryStartRow + 2, 7).Style.NumberFormat.Format = @"# ##0.000\ _₽";

        // Статистика по контейнерам
        int containerStatsRow = summaryStartRow;
        int containerStatsCol = 11;

        worksheet.Cell(containerStatsRow, containerStatsCol).Value = "контейнеров:";
        worksheet.Cell(containerStatsRow, containerStatsCol + 1).FormulaA1 =
            $"SUM(K{containerStatsRow + 2}:K{containerStatsRow + 5})";

        worksheet.Cell(containerStatsRow + 1, containerStatsCol).Value = "20' порожние:";
        worksheet.Cell(containerStatsRow + 1, containerStatsCol + 1).Value = 0;
        worksheet.Cell(containerStatsRow + 1, containerStatsCol + 2).Value = "тара";
        worksheet.Cell(containerStatsRow + 1, containerStatsCol + 3).Value = 0;
        worksheet.Cell(containerStatsRow + 1, containerStatsCol + 4).Value = "весгруза";
        worksheet.Cell(containerStatsRow + 1, containerStatsCol + 5).Value = 0;
        worksheet.Cell(containerStatsRow + 1, containerStatsCol + 6).Value = "ИТОГО";
        worksheet.Cell(containerStatsRow + 1, containerStatsCol + 7).Value = 0;

        worksheet.Cell(containerStatsRow + 2, containerStatsCol).Value = "40' порожние:";
        worksheet.Cell(containerStatsRow + 2, containerStatsCol + 1).Value = 0;
        worksheet.Cell(containerStatsRow + 2, containerStatsCol + 2).Value = 0;
        worksheet.Cell(containerStatsRow + 2, containerStatsCol + 3).Value = 0;
        worksheet.Cell(containerStatsRow + 2, containerStatsCol + 4).Value = 0;
        worksheet.Cell(containerStatsRow + 2, containerStatsCol + 5).Value = 0;
        worksheet.Cell(containerStatsRow + 2, containerStatsCol + 6).Value = 0;
        worksheet.Cell(containerStatsRow + 2, containerStatsCol + 7).Value = 0;

        worksheet.Cell(containerStatsRow + 3, containerStatsCol).Value = "20' груженые:";
        worksheet.Cell(containerStatsRow + 3, containerStatsCol + 1).FormulaA1 =
            $"COUNTIF($C${startRow + 1}:$C${currentRow - 1},\"20\")";
        worksheet.Cell(containerStatsRow + 3, containerStatsCol + 2).FormulaA1 =
            $"SUMIF($C${startRow + 1}:$C${currentRow - 1},20,$J${startRow + 1}:$J${currentRow - 1})";
        worksheet.Cell(containerStatsRow + 3, containerStatsCol + 3).Value = "";
        worksheet.Cell(containerStatsRow + 3, containerStatsCol + 4).FormulaA1 =
            $"SUMIF($C${startRow + 1}:$C${currentRow - 1},20,$I${startRow + 1}:$I${currentRow - 1})";
        worksheet.Cell(containerStatsRow + 3, containerStatsCol + 5).FormulaA1 =
            $"L{containerStatsRow + 3}+R{containerStatsRow + 3}";

        worksheet.Cell(containerStatsRow + 4, containerStatsCol).Value = "40' груженые:";
        worksheet.Cell(containerStatsRow + 4, containerStatsCol + 1).FormulaA1 =
            $"COUNTIF($C${startRow + 1}:$C${currentRow - 1},\"40\")";
        worksheet.Cell(containerStatsRow + 4, containerStatsCol + 2).FormulaA1 =
            $"SUMIF($C${startRow + 1}:$C${currentRow - 1},40,$J${startRow + 1}:$J${currentRow - 1})";
        worksheet.Cell(containerStatsRow + 4, containerStatsCol + 3).Value = "";
        worksheet.Cell(containerStatsRow + 4, containerStatsCol + 4).FormulaA1 =
            $"SUMIF($C${startRow + 1}:$C${currentRow - 1},40,$I${startRow + 1}:$I${currentRow - 1})";
        worksheet.Cell(containerStatsRow + 4, containerStatsCol + 5).FormulaA1 =
            $"L{containerStatsRow + 4}+R{containerStatsRow + 4}";

        // Итоговые формулы
        worksheet.Cell(containerStatsRow + 6, containerStatsCol + 5).FormulaA1 =
            $"S{containerStatsRow + 3}+S{containerStatsRow + 4}+S{containerStatsRow + 1}";
        worksheet.Cell(containerStatsRow + 6, containerStatsCol + 7).FormulaA1 =
            $"S{containerStatsRow + 6}=G{summaryStartRow + 2}";

        // Установка фиксированных ширин столбцов
        worksheet.Column(1).Width = 5;   // №
        worksheet.Column(2).Width = 16;  // №контейнера
        worksheet.Column(3).Width = 5;  // Размер
        worksheet.Column(4).Width = 5;   // Тип
        worksheet.Column(5).Width = 20;  // №пломбы
        worksheet.Column(6).Width = 7;  // Кол-во мест
        worksheet.Column(7).Width = 60;  // Наименование заявленного груза
        worksheet.Column(8).Width = 12;  // Вес груза брт,кг
        worksheet.Column(9).Width = 7; // Вес тары
        worksheet.Column(10).Width = 18; // №коносамента
        worksheet.Column(11).Width = 12; // Дата коносамента
        worksheet.Column(12).Width = 40; // Грузоотправитель (формула)
        worksheet.Column(13).Width = 60; // Грузополучатель (формула)
        worksheet.Column(14).Width = 5; // Там. режим

        // 1. НАСТРОЙКА ОРИЕНТАЦИИ СТРАНИЦЫ
        worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape; // Альбомная ориентация

        // 2. НАСТРОЙКА ОТСТУПОВ (в дюймах)
        worksheet.PageSetup.Margins.Top = 0.3;      // Верхний отступ: 0.3 дюйма (0.76 см)
        worksheet.PageSetup.Margins.Bottom = 0.3;   // Нижний отступ: 0.3 дюйма
        worksheet.PageSetup.Margins.Left = 0.15;     // Левый отступ: 0.15 дюйма ()
        worksheet.PageSetup.Margins.Right = 0.15;    // Правый отступ: 0.15 дюйма
        worksheet.PageSetup.Margins.Header = 0.15;   // Отступ верхнего колонтитула
        worksheet.PageSetup.Margins.Footer = 0.15;   // Отступ нижнего колонтитула

        // 3. ДОПОЛНИТЕЛЬНЫЕ НАСТРОЙКИ СТРАНИЦЫ
        worksheet.PageSetup.CenterHorizontally = true;  // Центрировать по горизонтали
        worksheet.PageSetup.CenterVertically = false;   // Не центрировать по вертикали

        // 4. НАСТРОЙКА РАЗМЕРА БУМАГИ (опционально)
        worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper; // Формат A4

        // 5. ВПИСАТЬ ВСЕ СТОЛБЦЫ НА ОДНУ СТРАНИЦУ ПО ШИРИНЕ
        worksheet.PageSetup.AdjustTo(100); // Масштаб 100%
        worksheet.PageSetup.FitToPages(1,0); // По ширине на 1 страницу По высоте без ограничений (0 = не ограничивать)

        // Фиксируем заголовки
        worksheet.SheetView.FreezeRows(startRow);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return await Task.FromResult(stream.ToArray());
    }

    public async Task<string> GenerateArrivalNoticeFileAsync(VesselCallDto vesselCall, string filePath)
    {
        var bytes = await GenerateArrivalNoticeAsync(vesselCall);

        await File.WriteAllBytesAsync(filePath, bytes);
        return filePath;
    }
}