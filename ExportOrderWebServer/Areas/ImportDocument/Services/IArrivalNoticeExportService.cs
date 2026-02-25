using ClosedXML.Excel;
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
        var decimalFormat = "# ### ##0.000_-; # ### ##0,000_-;_-* \"-\"??_-;_-@_-";
        var intFormat = "# ### ##0_-;# ### ##0_-;_-* \"-\"??_-;_-@_-";

        var vesselName = vesselCall.Vessel.Name;
        var voyageNumber = vesselCall.VoyageNo;
        var feederBl = vesselCall.FeederBlNo;
        var arrivalDate = vesselCall.ETA;
        var vesselFlag = vesselCall.Vessel.FlagRu;
        var portOfLoading = vesselCall.PortOfLoading.FullRu;

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("УВЕДОМЛЕНИЕ");

        // Заголовок
        worksheet.Cell(1, 1).Value = $"УВЕДОМЛЕНИЕ О ПРИБЫТИИ ТОВАРОВ ({vesselName} {voyageNumber}) - ALPHA SHIPPING (SHANGHAI) LTD";
        worksheet.Range(1, 1, 1, 14).Merge();
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;


        // Установить строки 1-7 как повторяющиеся на каждой странице
        worksheet.PageSetup.SetRowsToRepeatAtTop(1, 7);

        // Основная информация


        worksheet.Cell(2, 1).Value = "Порт выгрузки:";
        worksheet.Range(2, 1, 2, 2).Merge();

        worksheet.Cell(3, 1).Value = "Название судна / Рейс:";
        worksheet.Range(3, 1, 3, 2).Merge();

        worksheet.Cell(4, 1).Value = "Флаг судна:";
        worksheet.Range(4, 1, 4, 2).Merge();

        worksheet.Cell(5, 1).Value = "Дата прихода:";
        worksheet.Range(5, 1, 5, 2).Merge();

        worksheet.Cell(2, 3).Value = "НОВОРОССИЙСК";
        worksheet.Cell(3, 3).Value = $"{vesselName} {voyageNumber}";
        worksheet.Cell(4, 3).Value = vesselFlag;
        worksheet.Cell(5, 3).Value = arrivalDate;
        worksheet.Range(5, 3, 5, 5).Merge();

        worksheet.Range(2, 1, 5, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;


        worksheet.Cell(2, 7).Value = "Порт отправления:";
        worksheet.Cell(2, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        worksheet.Cell(2, 8).Value = portOfLoading;
        worksheet.Cell(2, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;



        worksheet.Cell(2, 12).Value = "Перевозчик:";
        worksheet.Cell(3, 12).Value = "Страна Перевозчика:";
        worksheet.Cell(4, 12).Value = "Сервисный К/С:";

        worksheet.Cell(2, 13).Value = "ALPHA SHIPPING (SHANGHAI) LTD";
        worksheet.Cell(3, 13).Value = "КИТАЙ";
        worksheet.Cell(4, 13).Value = feederBl;

        worksheet.Range(2, 12, 4, 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

        // Заголовки таблицы (начиная с 7 строки)
        int startRow = 7;
        worksheet.Row(6).Height = 9;
        worksheet.Row(7).Height = 28;
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

        var BillOfLadings = vesselCall.BillOfLadings.OrderBy(s => s.TsDate).ThenBy(s => s.Num);
        // 1. Находим все номера контейнеров, которые встречаются более одного раза
        var duplicateContainerNos = BillOfLadings
            .SelectMany(bl => bl.ContainerRecords)
            .Where(c => !string.IsNullOrWhiteSpace(c.ContainerNo))
            .GroupBy(c => c.ContainerNo)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToHashSet(); // Используем HashSet для быстрого поиска

        // 2. Проходим по всем записям и модифицируем дубликаты
        // Словарь для отслеживания количества встреченных дубликатов (чтобы не менять первый)
        var duplicateCounter = new Dictionary<string, int>();

        foreach (var bilOfLadingDto in BillOfLadings)
            foreach (var containerRecord in bilOfLadingDto.ContainerRecords.OrderBy(s => s.ContainerNo))
            {
                if (containerRecord.ContainerAsCargo)
                {
                    containerRecord.GrossWeight += containerRecord.TareWt;
                    containerRecord.TareWt = 0;

                    containerRecord.NoOfPackage += 1;
                }

                bool isDuplicate = false;

                // Проверка на дубликаты ContainerNo
                if (!string.IsNullOrWhiteSpace(containerRecord.ContainerNo) &&
                    duplicateContainerNos.Contains(containerRecord.ContainerNo))
                {
                    if (!duplicateCounter.ContainsKey(containerRecord.ContainerNo))
                    {
                        duplicateCounter[containerRecord.ContainerNo] = 0;
                    }

                    duplicateCounter[containerRecord.ContainerNo]++;

                    // Если это не первое вхождение дубликата
                    if (duplicateCounter[containerRecord.ContainerNo] > 1)
                    {
                        isDuplicate = true;
                        containerRecord.ContainerNo = $"{containerRecord.ContainerNo} ч";
                    }
                }

                // ==========================================
                // Запись данных в Excel
                // ==========================================

                // Колонка 1: Порядковый номер (пустой для дубликатов)
                if (isDuplicate)
                {
                    worksheet.Cell(currentRow, 1).Value = ""; // Пустое значение для дубликатов
                }
                else
                {
                    worksheet.Cell(currentRow, 1).Value = itemNumber;
                    worksheet.Cell(currentRow, 1).Style.NumberFormat.Format = "0";
                }

                worksheet.Cell(currentRow, 2).Value = containerRecord.ContainerNo;

                worksheet.Cell(currentRow, 3).Value = containerRecord.ContainerTypeId.Substring(2, 2);
                worksheet.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(currentRow, 4).Value = containerRecord.ContainerTypeId.Substring(0, 2);
                worksheet.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(currentRow, 5).Value = containerRecord.SealNo;

                worksheet.Cell(currentRow, 6).Value = containerRecord.NoOfPackage;
                worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "0";
                worksheet.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(currentRow, 7).Value = containerRecord.CargoDescriptionRu;
                worksheet.Cell(currentRow, 7).Style.Alignment.WrapText = true;

                worksheet.Cell(currentRow, 8).Value = containerRecord.GrossWeight;
                worksheet.Cell(currentRow, 8).Style.NumberFormat.Format = decimalFormat;
                worksheet.Cell(currentRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Колонка 9: TareWt (0 для дубликатов)
                if (isDuplicate)
                    containerRecord.TareWt = 0; // Ноль для дубликатов

                worksheet.Cell(currentRow, 9).Value = containerRecord.TareWt;

                worksheet.Cell(currentRow, 9).Style.NumberFormat.Format = "0";
                worksheet.Cell(currentRow, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(currentRow, 10).Value = bilOfLadingDto.Num;

                worksheet.Cell(currentRow, 11).Value = bilOfLadingDto.TsDate;
                worksheet.Cell(currentRow, 11).Style.NumberFormat.Format = "dd.MM.yy";
                worksheet.Cell(currentRow, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(currentRow, 12).Value = bilOfLadingDto.ShipperFullName;
                worksheet.Cell(currentRow, 13).Value = bilOfLadingDto.ConsigneeFullName;
                worksheet.Cell(currentRow, 14).Value = bilOfLadingDto.CustomsMode;
                worksheet.Cell(currentRow, 14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Добавление границ
                for (int col = 1; col <= headers.Length; col++)
                {
                    worksheet.Cell(currentRow, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(currentRow, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(currentRow, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                }

                worksheet.Range(currentRow, 1, currentRow, 14).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                // Инкремент счетчиков
                currentRow++;

                // itemNumber инкрементируем ТОЛЬКО для первых вхождений
                if (!isDuplicate)
                    itemNumber++;
            }

        // Итоги внизу таблицы
        int summaryStartRow = currentRow + 2;

        var records = BillOfLadings.SelectMany(s => s.ContainerRecords).ToList();

        var mty20Qty = records.Where(s => s.ContainerTypeId.Contains("20")).Count(s => s.FullOrEmpty != "F");
        var mty40Qty = records.Where(s => s.ContainerTypeId.Contains("40")).Count(s => s.FullOrEmpty != "F");
        var full20Qty = records.Where(s => s.ContainerTypeId.Contains("20")).Count(s => s.FullOrEmpty == "F");
        var full40Qty = records.Where(s => s.ContainerTypeId.Contains("40")).Count(s => s.FullOrEmpty == "F");

        var mty20Tare = records.Where(s => s.ContainerTypeId.Contains("20")).Where(s => s.FullOrEmpty != "F").Sum(s => s.TareWt);
        var mty40Tare = records.Where(s => s.ContainerTypeId.Contains("40")).Where(s => s.FullOrEmpty != "F").Sum(s => s.TareWt);
        var full20Tare = records.Where(s => s.ContainerTypeId.Contains("20")).Where(s => s.FullOrEmpty == "F").Sum(s => s.TareWt);
        var full40Tare = records.Where(s => s.ContainerTypeId.Contains("40")).Where(s => s.FullOrEmpty == "F").Sum(s => s.TareWt);

        var mty20Wt = records.Where(s => s.ContainerTypeId.Contains("20")).Where(s => s.FullOrEmpty != "F").Sum(s => s.GrossWeight);
        var mty40Wt = records.Where(s => s.ContainerTypeId.Contains("40")).Where(s => s.FullOrEmpty != "F").Sum(s => s.GrossWeight);
        var full20Wt = records.Where(s => s.ContainerTypeId.Contains("20")).Where(s => s.FullOrEmpty == "F").Sum(s => s.GrossWeight);
        var full40Wt = records.Where(s => s.ContainerTypeId.Contains("40")).Where(s => s.FullOrEmpty == "F").Sum(s => s.GrossWeight);

        // Веса
        worksheet.Cell(summaryStartRow, 4).Value = "Вес груза:";
        worksheet.Cell(summaryStartRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

        worksheet.Range(summaryStartRow, 5, summaryStartRow, 6).Merge();
        worksheet.Cell(summaryStartRow, 5).FormulaA1 = $"SUM(H{startRow + 1}:H{currentRow - 1})";
        worksheet.Cell(summaryStartRow, 5).Style.NumberFormat.Format = decimalFormat;

        summaryStartRow++;
        worksheet.Cell(summaryStartRow, 4).Value = "Вес тары:";
        worksheet.Cell(summaryStartRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

        worksheet.Range(summaryStartRow, 5, summaryStartRow, 6).Merge();
        worksheet.Cell(summaryStartRow, 5).FormulaA1 = $"SUM(I{startRow + 1}:I{currentRow - 1})";
        worksheet.Cell(summaryStartRow, 5).Style.NumberFormat.Format = intFormat;

        summaryStartRow++;
        worksheet.Cell(summaryStartRow, 4).Value = "Общий вес с тарой:";
        worksheet.Cell(summaryStartRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

        worksheet.Range(summaryStartRow, 5, summaryStartRow, 6).Merge();
        worksheet.Cell(summaryStartRow, 5).FormulaA1 =
            $"SUM(E{summaryStartRow - 3}:F{summaryStartRow - 1})";
        worksheet.Cell(summaryStartRow, 5).Style.NumberFormat.Format = decimalFormat;


        worksheet.Range(summaryStartRow - 3, 5, summaryStartRow, 6).Style.Font.Bold = true;
        worksheet.Range(summaryStartRow - 3, 5, summaryStartRow, 6).Style.Font.FontSize = 12;


        // Статистика по контейнерам
        int containerStatsRow = currentRow + 2;


        worksheet.Cell(containerStatsRow++, 7).Value = "20' порожние:";
        worksheet.Cell(containerStatsRow++, 7).Value = "40' порожние:";
        worksheet.Cell(containerStatsRow++, 7).Value = "20' груженые:";
        worksheet.Cell(containerStatsRow++, 7).Value = "40' груженые:";
        containerStatsRow++;
        worksheet.Cell(containerStatsRow, 7).Value = "Итого:";

        containerStatsRow = currentRow + 2;

        // кол-во -8
        worksheet.Cell(containerStatsRow - 1, 8).Value = "кол-во";
        worksheet.Cell(containerStatsRow++, 8).Value = mty20Qty;
        worksheet.Cell(containerStatsRow++, 8).Value = mty40Qty;
        worksheet.Cell(containerStatsRow++, 8).Value = full20Qty;
        worksheet.Cell(containerStatsRow++, 8).Value = full40Qty;

        containerStatsRow++;
        worksheet.Cell(containerStatsRow, 8).FormulaA1 = $"SUM(H{containerStatsRow - 5}:H{containerStatsRow - 2})";

        containerStatsRow = currentRow + 2;

        // тара - 9-10
        worksheet.Cell(containerStatsRow - 1, 9).Value = "вес тары";
        worksheet.Range(containerStatsRow - 1, 9, containerStatsRow - 1, 10).Merge();

        worksheet.Cell(containerStatsRow, 9).Value = mty20Tare;
        worksheet.Range(containerStatsRow, 9, containerStatsRow, 10).Merge();

        containerStatsRow++;
        worksheet.Cell(containerStatsRow, 9).Value = mty40Tare;
        worksheet.Range(containerStatsRow, 9, containerStatsRow, 10).Merge();

        containerStatsRow++;
        worksheet.Cell(containerStatsRow, 9).Value = full20Tare;
        worksheet.Range(containerStatsRow, 9, containerStatsRow, 10).Merge();

        containerStatsRow++;
        worksheet.Cell(containerStatsRow, 9).Value = full40Tare;
        worksheet.Range(containerStatsRow, 9, containerStatsRow, 10).Merge();

        containerStatsRow++;
        containerStatsRow++;
        worksheet.Cell(containerStatsRow, 9).FormulaA1 = $"SUM(I{containerStatsRow - 5}:J{containerStatsRow - 2})";
        worksheet.Range(containerStatsRow, 9, containerStatsRow, 10).Merge();


        containerStatsRow = currentRow + 2;

        // вес груза -11-12
        worksheet.Cell(containerStatsRow - 1, 11).Value = "вес груза";
        worksheet.Range(containerStatsRow - 1, 11, containerStatsRow - 1, 12).Merge();

        worksheet.Range(containerStatsRow, 11, containerStatsRow, 12).Merge();
        worksheet.Cell(containerStatsRow, 11).Value = mty20Wt;

        containerStatsRow++;
        worksheet.Range(containerStatsRow, 11, containerStatsRow, 12).Merge();
        worksheet.Cell(containerStatsRow, 11).Value = mty40Wt;

        containerStatsRow++;
        worksheet.Range(containerStatsRow, 11, containerStatsRow, 12).Merge();
        worksheet.Cell(containerStatsRow, 11).Value = full20Wt;

        containerStatsRow++;
        worksheet.Range(containerStatsRow, 11, containerStatsRow, 12).Merge();
        worksheet.Cell(containerStatsRow, 11).Value = full40Wt;

        containerStatsRow++;
        containerStatsRow++;
        worksheet.Range(containerStatsRow, 11, containerStatsRow, 12).Merge();
        worksheet.Cell(containerStatsRow, 11).FormulaA1 = $"SUM(K{containerStatsRow - 5}:L{containerStatsRow - 2})";

        containerStatsRow = currentRow + 2;
        // вес ИТОГО  тара + груз -13
        worksheet.Cell(containerStatsRow - 1, 13).Value = "вес тары + груза";

        worksheet.Cell(containerStatsRow++, 13).Value = mty20Wt + mty20Tare;
        worksheet.Cell(containerStatsRow++, 13).Value = mty40Wt + mty40Tare;
        worksheet.Cell(containerStatsRow++, 13).Value = full20Wt + full20Tare;
        worksheet.Cell(containerStatsRow++, 13).Value = full40Wt + full40Tare;

        containerStatsRow++;

        worksheet.Cell(containerStatsRow, 13).FormulaA1 = $"SUM(M{containerStatsRow - 5}:M{containerStatsRow - 2})";

        // форматирование тотал
        worksheet.Range(containerStatsRow - 5, 7, containerStatsRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        worksheet.Range(containerStatsRow - 6, 8, containerStatsRow, 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        worksheet.Range(containerStatsRow - 6, 11, containerStatsRow - 2, 13).Style.NumberFormat.Format = decimalFormat;
        worksheet.Range(containerStatsRow - 6, 8, containerStatsRow, 10).Style.NumberFormat.Format = intFormat;

        worksheet.Range(containerStatsRow, 9, containerStatsRow, 13).Style.NumberFormat.Format = decimalFormat;

        worksheet.Range(containerStatsRow, 7, containerStatsRow, 13).Style.Font.Bold = true;
        worksheet.Range(containerStatsRow, 7, containerStatsRow, 13).Style.Font.FontSize = 12;


        // Настройка нижнего колонтитула
        var footer = worksheet.PageSetup.Footer;

        // Очистить существующий колонтитул
        footer.Clear();

        // Вариант 1: Номер страницы по центру
        worksheet.PageSetup.Footer.Left.AddText($"{vesselName} / {voyageNumber}");
        worksheet.PageSetup.Footer.Right.AddText("Страница &P из &N");

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
        worksheet.PageSetup.FitToPages(1, 0); // По ширине на 1 страницу По высоте без ограничений (0 = не ограничивать)

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