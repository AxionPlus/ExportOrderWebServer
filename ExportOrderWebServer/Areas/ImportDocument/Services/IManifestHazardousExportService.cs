using ClosedXML.Excel;
using ExportOrderWebServer.Areas.ImportDocument.VesselCall.Dto;

namespace ExportOrderWebServer.Areas.ImportDocument.Services;

public interface IManifestHazardousExportService
{
    Task<byte[]> GenerateManifestAsync(VesselCallDto vesselCall);
    Task<string> GenerateManifestFileAsync(VesselCallDto vesselCall, string filePath);
}

public class ManifestHazardousExportService : IManifestHazardousExportService
{
    public async Task<byte[]> GenerateManifestAsync(VesselCallDto vesselCall)
    {
        var calculator = new ExcelRowHeightCalculator();
        var decimalFormat = "# ### ##0.000_-;-* # ### ##0,000_-;\"-\"??_-;_-@_-";
        var intFormat = "# ### ##0_-;-* # ### ##0_-;\"-\"??_-;_-@_-";

        var vesselName = vesselCall.Vessel.Name;
        var voyageNumber = vesselCall.VoyageNo;
        var onboardDate = vesselCall.ETA;
        var vesselFlag = vesselCall.Vessel.FlagEn;
        var loadingPort = vesselCall.PortOfLoading.FullEn;
        var dischargingPort = vesselCall.Terminal?.Name ?? "NUTEP";

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("CARGO MANIFEST");
        int currentRow = 1;

        // Сортировка коносаментов
        var sortedBillOfLadings = vesselCall.BillOfLadings
            .Where(s=> s.IsImo)
            .OrderBy(b => b.Num)
            .ToList();


        // Обработка каждого коносамента
        foreach (var billOfLading in sortedBillOfLadings)
        {
            // === ЗАГОЛОВОК КОНОСАМЕНТА (как в шаблоне) ===

            // Строка 1: Заголовки полей (шаблон строка 3)
            worksheet.Cell(currentRow, 1).Value = "1. B/L NO.";
            worksheet.Range(currentRow, 1, currentRow, 2).Merge();

            worksheet.Cell(currentRow, 4).Value = "2. VESSEL NAME";
            worksheet.Range(currentRow, 4, currentRow, 5).Merge();

            worksheet.Cell(currentRow, 7).Value = "3. VOYAGE";
            worksheet.Range(currentRow, 7, currentRow, 8).Merge();

            worksheet.Cell(currentRow, 10).Value = "4. ONBOARD DATE";
            worksheet.Range(currentRow, 10, currentRow, 11).Merge();

            worksheet.Cell(currentRow, 13).Value = "5. VESSEL FLAG";
            worksheet.Range(currentRow, 13, currentRow, 14).Merge();

            worksheet.Cell(currentRow, 15).Value = "6. LOADING PORT";

            worksheet.Cell(currentRow, 18).Value = "7. DISCHARGING PORT";
            // Не объединяем, оставляем один столбец

            // Стили для заголовков
            currentRow++;

            // Строка 2: Значения полей (шаблон строка 4)
            worksheet.Cell(currentRow, 1).Value = billOfLading.Num;
            worksheet.Range(currentRow, 1, currentRow, 2).Merge();

            worksheet.Cell(currentRow, 4).Value = vesselName;
            worksheet.Range(currentRow, 4, currentRow, 5).Merge();

            worksheet.Cell(currentRow, 7).Value = voyageNumber;
            worksheet.Range(currentRow, 7, currentRow, 8).Merge();

            worksheet.Cell(currentRow, 10).Value = billOfLading.TsDate?.ToString("dd.MM.yyyy") ?? string.Empty;
            worksheet.Range(currentRow, 10, currentRow, 11).Merge();

            worksheet.Cell(currentRow, 13).Value = vesselFlag;
            worksheet.Range(currentRow, 13, currentRow, 14).Merge();

            worksheet.Cell(currentRow, 15).Value = loadingPort;

            worksheet.Cell(currentRow, 18).Value = $"NOVOROSSIYSK ({dischargingPort})"; 

            currentRow++;
            worksheet.Row(currentRow).Height = 3.5;
            worksheet.Range(currentRow, 1, currentRow, 19).Style.Border.BottomBorder = XLBorderStyleValues.Double;
            currentRow++;

            // Строка 3: S:SHIPPER и заголовки таблицы (шаблон строка 5)
            worksheet.Cell(currentRow, 1).Value = "S:SHIPPER";

            worksheet.Cell(currentRow, 5).Value = "SHIPPING";
            worksheet.Range(currentRow, 5, currentRow, 6).Merge();

            worksheet.Cell(currentRow, 7).Value = "CARGO DESCRIPTION";
            worksheet.Range(currentRow, 7, currentRow + 1, 13).Merge();

            worksheet.Cell(currentRow, 14).Value = $"TOTAL TARE WT\n(KGS)";
            worksheet.Range(currentRow, 14, currentRow + 1, 15).Merge();

            worksheet.Cell(currentRow, 16).Value = $"TOTAL GROSS WT\n(KGS)";
            worksheet.Range(currentRow, 16, currentRow + 1, 17).Merge();

            worksheet.Cell(currentRow, 18).Value = $"TOTAL MEASUREMENT\n(CBM)";
            worksheet.Range(currentRow, 18, currentRow + 1, 19).Merge();


            // Центрирование заголовков таблицы
            currentRow++;

            // Строка 4: C:CONSIGNEE и MARKS (шаблон строка 6)
            worksheet.Cell(currentRow, 1).Value = "C:CONSIGNEE";

            worksheet.Cell(currentRow, 5).Value = "MARKS";
            worksheet.Range(currentRow, 5, currentRow, 6).Merge();

            worksheet.Range(currentRow, 1, currentRow, 19).Style.Border.BottomBorder = XLBorderStyleValues.Double;
            worksheet.Range(currentRow - 1, 7, currentRow, 19).Style.Alignment.WrapText = true;
            worksheet.Range(currentRow - 1, 7, currentRow, 19).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Range(currentRow - 1, 7, currentRow, 19).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Range(currentRow - 1, 1, currentRow, 19).Style.Font.Bold = true;
            worksheet.Range(currentRow - 1, 1, currentRow, 19).Style.Font.FontSize = 10;
            worksheet.Range(currentRow - 1, 1, currentRow, 19).Style.Font.Italic = true;


            currentRow++;

            worksheet.Row(currentRow).Height = 7;
            // Пустая строка (шаблон строка 7)
            currentRow++;
            // устанавливаем строку для Shipper Cargo
            var shipperCargoRow = currentRow;

            // CARGO DESCRIPTION - 84 width
            worksheet.Column(6).Width = 92;
            worksheet.Cell(currentRow, 6).Value = billOfLading.CargoDescription;
            worksheet.Cell(currentRow, 6).Style.Alignment.WrapText = true;

            worksheet.Row(currentRow).Height = calculator.CalculateRowHeightWithWordWrap(billOfLading.CargoDescription, 92, 11);

            // worksheet.Row(currentRow).Height = CalculateRowHeight(billOfLading.CargoDescription, 97, 11);
            // расчет высоты строки для 78 ширины
            worksheet.Range(currentRow, 6, currentRow, 13).Merge(); // 3 строки, 8 столбцов (E-L)
            worksheet.Range(currentRow, 6, currentRow, 13).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            worksheet.Range(currentRow, 6, currentRow, 13).Style.Alignment.WrapText = true;


            // Итоговые веса для этого коносамента
            double blTareWeight = billOfLading.ContainerRecords.Sum(c =>
                (double)c.TareWt);
            double blGrossWeight = billOfLading.ContainerRecords.Sum(c => c.GrossWeight);

            worksheet.Cell(currentRow, 14).Value = blTareWeight;
            worksheet.Range(currentRow, 14, currentRow, 15).Merge(); // N-O
            worksheet.Cell(currentRow, 14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(currentRow, 14).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            worksheet.Cell(currentRow, 14).Style.NumberFormat.Format = "# ### ##0";
            worksheet.Cell(currentRow, 14).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 14).Style.Font.FontSize = 12;

            worksheet.Cell(currentRow, 16).Value = blGrossWeight;
            worksheet.Cell(currentRow, 16).Value = blGrossWeight;
            worksheet.Range(currentRow, 16, currentRow, 17).Merge(); // P-Q
            worksheet.Cell(currentRow, 16).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(currentRow, 16).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            worksheet.Cell(currentRow, 16).Style.NumberFormat.Format = "# ### ##0.000";
            worksheet.Cell(currentRow, 16).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 16).Style.Font.FontSize = 12;

            worksheet.Cell(currentRow, 18).Value = ""; // TOTAL MEASUREMENT (пусто) (R-S)
            worksheet.Range(currentRow, 18, currentRow, 19).Merge();
            worksheet.Cell(currentRow, 18).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(currentRow, 18).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            worksheet.Cell(currentRow, 18).Style.NumberFormat.Format = "# ### ##0.000";
            worksheet.Cell(currentRow, 18).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 18).Style.Font.FontSize = 12;


            // Пустая строка (шаблон строка 11)
            currentRow++;

            // === ТАБЛИЦА КОНТЕЙНЕРОВ ===

            // Строка: Заголовок WEIGHT (KGS) (шаблон строка 12)
            worksheet.Cell(currentRow, 18).Value = "WEIGHT (KGS)";
            worksheet.Range(currentRow, 18, currentRow, 19).Merge(); // Q-R
            worksheet.Cell(currentRow, 18).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 18).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            currentRow++;

            // Строка: Заголовки столбцов таблицы (шаблон строка 13)
            worksheet.Cell(currentRow, 10).Value = "SEQ";
            worksheet.Cell(currentRow, 11).Value = "CNTR-NO";
            worksheet.Cell(currentRow, 12).Value = "SIZE";
            worksheet.Cell(currentRow, 13).Value = "SEAL-NO";
            worksheet.Cell(currentRow, 14).Value = "IMO";
            worksheet.Cell(currentRow, 15).Value = "UNNO";
            worksheet.Cell(currentRow, 16).Value = "STATUS";
            worksheet.Cell(currentRow, 17).Value = "PKG";
            worksheet.Cell(currentRow, 18).Value = "TARE";
            worksheet.Cell(currentRow, 19).Value = "GROSS";

            // Стили заголовков таблицы
            worksheet.Row(currentRow).Height = 19;
            worksheet.Range(currentRow, 10, currentRow, 19).Style.Border.BottomBorder = XLBorderStyleValues.Double;
            worksheet.Range(currentRow, 10, currentRow, 19).Style.Font.Bold = true;
            worksheet.Range(currentRow, 10, currentRow, 19).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Range(currentRow, 10, currentRow, 19).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            currentRow++;
            worksheet.Row(currentRow).Height = 3.5;
            currentRow++;

            // Данные контейнеров
            int seqNumber = 1;
            var sortedContainers = billOfLading.ContainerRecords
                .OrderBy(c => c.ContainerNo)
                .ToList();

            foreach (var container in sortedContainers)
            {
                worksheet.Cell(currentRow, 10).Value = seqNumber;

                worksheet.Cell(currentRow, 11).Value = container.ContainerNo;

                worksheet.Cell(currentRow, 12).Value = container.ContainerTypeSize;
                worksheet.Cell(currentRow, 13).Value = container.SealNo;
                worksheet.Cell(currentRow, 14).Value = container.IMCOClass;
                worksheet.Cell(currentRow, 15).Value = container.IMCONumber;

                // Статус: FCL/FCL или другие значения
                string status = container.FullOrEmpty == "F" ? "FCL/FCL" : "Empty";
                worksheet.Cell(currentRow, 16).Value = status;

                worksheet.Cell(currentRow, 17).Value = container.NoOfPackage;
                worksheet.Cell(currentRow, 17).Style.NumberFormat.Format = "0";

                double tareWeight = (double)container.TareWt;
                worksheet.Cell(currentRow, 18).Value = tareWeight;
                worksheet.Cell(currentRow, 18).Style.NumberFormat.Format = "# ### ##0";

                worksheet.Cell(currentRow, 19).Value = container.GrossWeight;
                worksheet.Cell(currentRow, 19).Style.NumberFormat.Format = "# ##0.000";

                worksheet.Range(currentRow, 10, currentRow, 19).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;


                currentRow++;
                seqNumber++;
            }

            worksheet.Range(currentRow, 1, currentRow, 19).Style.Border.BottomBorder = XLBorderStyleValues.Thick;


            // === ДАННЫЕ SHIPPER/CONSIGNEE И CARGO DESCRIPTION ===

            // Строка 5-7: Блок Shipper и Consignee + Cargo Description (шаблон строка 8-10)
            string shipperInfo = $"S: {billOfLading.ShipperName}\n{billOfLading.ShipperAddress}";
            string consigneeInfo = $"C: {billOfLading.ConsigneeName}\n{billOfLading.ConsigneeAddress}";
            string combinedInfo = $"{shipperInfo}\n\n{consigneeInfo}";

            var shipperRowHeight = calculator.CalculateRowHeightWithWordWrap(combinedInfo, 40, 11) * 0.7;

            double sumHeightForShipper = 0;

            for (int i = shipperCargoRow; i < currentRow; i++)
            {
                sumHeightForShipper += worksheet.Row(i).Height;
            }

            if (shipperRowHeight > sumHeightForShipper)
            {
                worksheet.Row(currentRow).Height = shipperRowHeight - sumHeightForShipper;
            }

            worksheet.Cell(shipperCargoRow, 1).Value = combinedInfo;
            worksheet.Range(shipperCargoRow, 1, currentRow, 4).Merge(); // 3 строки, 4 столбца (A-D)
            worksheet.Cell(shipperCargoRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            worksheet.Cell(shipperCargoRow, 1).Style.Alignment.WrapText = true;

            // Если следующий коносамент, добавляем разделитель
            if (billOfLading != sortedBillOfLadings.Last())
            {
                // Добавляем еще пустую строку
                currentRow += 2;
                // Начинаем следующий коносамент
            }
        }

        // === ПОСЛЕДНЯЯ СТРАНИЦА (ИТОГИ) ===

        var records = vesselCall.BillOfLadings.Where(s=>s.IsImo).SelectMany(s => s.ContainerRecords).ToList();

        var mty20Qty = records.Where(s => s.ContainerTypeId.Contains("20")).Count(s => s.FullOrEmpty != "F");
        var mty40Qty = records.Where(s => s.ContainerTypeId.Contains("40")).Count(s => s.FullOrEmpty != "F");
        var full20Qty = records.Where(s => s.ContainerTypeId.Contains("20")).Count(s => s.FullOrEmpty == "F");
        var full40Qty = records.Where(s => s.ContainerTypeId.Contains("40")).Count(s => s.FullOrEmpty == "F");


        var mty20Tare = records.Where(s => s.ContainerTypeId.Contains("20")).Where(s => s.FullOrEmpty != "F").Sum(s =>  s.TareWt);
        var mty40Tare = records.Where(s => s.ContainerTypeId.Contains("40")).Where(s => s.FullOrEmpty != "F").Sum(s =>  s.TareWt);
        var full20Tare = records.Where(s => s.ContainerTypeId.Contains("20")).Where(s => s.FullOrEmpty == "F").Sum(s => s.TareWt);
        var full40Tare = records.Where(s => s.ContainerTypeId.Contains("40")).Where(s => s.FullOrEmpty == "F").Sum(s => s.TareWt);
        var mty20Wt = records.Where(s => s.ContainerTypeId.Contains("20")).Where(s => s.FullOrEmpty != "F").Sum(s => s.GrossWeight);
        var mty40Wt = records.Where(s => s.ContainerTypeId.Contains("40")).Where(s => s.FullOrEmpty != "F").Sum(s => s.GrossWeight);
        var full20Wt = records.Where(s => s.ContainerTypeId.Contains("20")).Where(s => s.FullOrEmpty == "F").Sum(s => s.GrossWeight);
        var full40Wt = records.Where(s => s.ContainerTypeId.Contains("40")).Where(s => s.FullOrEmpty == "F").Sum(s => s.GrossWeight);

        currentRow += 2;

        // Строка: LAST PAGE слева и справа (шаблон строка 24)
        worksheet.Cell(currentRow, 1).Value = "LAST PAGE";
        worksheet.Cell(currentRow, 1).Style.Font.Bold = true;

        worksheet.Cell(currentRow, 18).Value = "LAST PAGE";
        worksheet.Cell(currentRow, 18).Style.Font.Bold = true;
        worksheet.Cell(currentRow, 18).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        currentRow++;

        // Строка: TOTAL SUMMARY (шаблон строка 25)
        worksheet.Cell(currentRow, 3).Value = "TOTAL SUMMARY";
        worksheet.Range(currentRow, 3, currentRow, 6).Merge(); // C-F
        worksheet.Cell(currentRow, 3).Style.Font.Bold = true;
        worksheet.Cell(currentRow, 3).Style.Font.FontSize = 14;
        worksheet.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        currentRow++;
        currentRow++; // Пустая строка (26)

        // Строка: Заголовки колонок итогов (шаблон строка 28)
        worksheet.Cell(currentRow, 5).Value = "QUANTITY";
        worksheet.Cell(currentRow, 7).Value = "TARE WEIGHT\n(KGS)";
        worksheet.Range(currentRow, 7, currentRow, 8).Merge(); // G-H
        worksheet.Cell(currentRow, 9).Value = "CARGO WEIGHT\n(KGS)";
        worksheet.Range(currentRow, 9, currentRow, 11).Merge(); // I-J
        worksheet.Cell(currentRow, 12).Value = "VGM WEIGHT\n(KGS)";
        worksheet.Range(currentRow, 12, currentRow, 13).Merge(); // K-L

        worksheet.Range(currentRow, 5, currentRow, 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        worksheet.Range(currentRow, 5, currentRow, 13).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        worksheet.Range(currentRow, 5, currentRow, 13).Style.Alignment.WrapText = true;

        worksheet.Row(currentRow).AdjustToContents();
        worksheet.Row(currentRow).Height = 34;


        // Стили заголовков итогов
        worksheet.Range(currentRow, 5, currentRow, 12).Style.Font.Bold = true;
        currentRow++;
        currentRow++; // Пустая строка (29)

        // Данные итогов (начиная с строки 30 в шаблоне)
        int startDataRow = currentRow;

        // TOTAL FULL 20' (шаблон строка 30)
        worksheet.Cell(currentRow, 3).Value = "TOTAL FULL 20'";
        worksheet.Range(currentRow, 3, currentRow, 4).Merge(); // C-D
        worksheet.Cell(currentRow, 5).Value = full20Qty; // QUANTITY (E)

        // TARE WEIGHT (KGS) - распределяем пропорционально

        worksheet.Cell(currentRow, 7).Value = full20Tare;
        worksheet.Range(currentRow, 7, currentRow, 8).Merge(); // G-H

        // CARGO WEIGHT (KGS) - только для FULL контейнеров

        worksheet.Cell(currentRow, 9).Value = full20Wt;
        worksheet.Range(currentRow, 9, currentRow, 11).Merge(); // I-J-K

        // VGM WEIGHT (KGS) = TARE + CARGO
        double vgmFull20 = full20Tare + full20Wt;
        worksheet.Cell(currentRow, 12).Value = vgmFull20;
        worksheet.Range(currentRow, 12, currentRow, 13).Merge(); // L-M
        currentRow++;

        // TOTAL FULL 40' (шаблон строка 31)
        worksheet.Cell(currentRow, 3).Value = "TOTAL FULL 40'";
        worksheet.Range(currentRow, 3, currentRow, 4).Merge(); // C-D
        worksheet.Cell(currentRow, 5).Value = full40Qty; // QUANTITY (E)

        worksheet.Cell(currentRow, 7).Value = full40Tare;
        worksheet.Range(currentRow, 7, currentRow, 8).Merge(); // G-H

        worksheet.Cell(currentRow, 9).Value = full40Wt;
        worksheet.Range(currentRow, 9, currentRow, 11).Merge(); // I-J-K

        double vgmFull40 = full40Tare + full40Wt;
        worksheet.Cell(currentRow, 12).Value = vgmFull40;
        worksheet.Range(currentRow, 12, currentRow, 13).Merge(); // L-M
        currentRow++;
        currentRow++; // Пустая строка (32)

        // TOTAL EMPTY 20' (шаблон строка 33)
        worksheet.Cell(currentRow, 3).Value = "TOTAL EMPTY 20'";
        worksheet.Range(currentRow, 3, currentRow, 4).Merge(); // C-D
        worksheet.Cell(currentRow, 5).Value = mty20Qty; // QUANTITY (E)

        worksheet.Cell(currentRow, 7).Value = mty20Tare;
        worksheet.Range(currentRow, 7, currentRow, 8).Merge(); // G-H

        worksheet.Cell(currentRow, 9).Value = 0; // CARGO для пустых = 0
        worksheet.Range(currentRow, 9, currentRow, 11).Merge(); // I-J-K

        // VGM = только TARE для пустых
        worksheet.Cell(currentRow, 12).Value = mty20Tare;
        worksheet.Range(currentRow, 12, currentRow, 13).Merge(); // L-M
        currentRow++;

        // TOTAL EMPTY 40' (шаблон строка 34)
        worksheet.Cell(currentRow, 3).Value = "TOTAL EMPTY 40'";
        worksheet.Range(currentRow, 3, currentRow, 4).Merge(); // C-D
        worksheet.Cell(currentRow, 5).Value = mty40Qty; // QUANTITY (E)

        worksheet.Cell(currentRow, 7).Value = mty40Tare;
        worksheet.Range(currentRow, 7, currentRow, 8).Merge(); // G-H

        worksheet.Cell(currentRow, 9).Value = 0; // CARGO для пустых = 0
        worksheet.Range(currentRow, 9, currentRow, 11).Merge(); // I-J-K

        // VGM = только TARE для пустых
        worksheet.Cell(currentRow, 12).Value = mty40Tare;
        worksheet.Range(currentRow, 12, currentRow, 13).Merge(); // L-M
        currentRow++;
        currentRow++; // Пустая строка (35)

        // ИТОГИ (TOTAL FULL) (шаблон строка 36)
        worksheet.Cell(currentRow, 3).Value = "TOTAL FULL";
        worksheet.Range(currentRow, 3, currentRow, 4).Merge(); // C-D

        // QUANTITY
        worksheet.Cell(currentRow, 5).Value = full20Qty + full40Qty;

        // TARE WEIGHT
        double totalTareFull = mty20Tare + mty40Tare + full20Tare + full40Tare;
        worksheet.Cell(currentRow, 7).Value = totalTareFull;
        worksheet.Range(currentRow, 7, currentRow, 8).Merge(); // G-H

        // CARGO WEIGHT
        worksheet.Cell(currentRow, 9).Value = full20Wt + full40Wt;
        worksheet.Range(currentRow, 9, currentRow, 11).Merge(); // I-J-K

        // VGM WEIGHT
        double totalVgmFull = totalTareFull + full20Wt + full40Wt;
        worksheet.Cell(currentRow, 12).Value = totalVgmFull;
        worksheet.Range(currentRow, 12, currentRow, 13).Merge(); // L-M

        // Форматирование чисел в итогах
        worksheet.Range(currentRow - 6, 5, currentRow, 8).Style.NumberFormat.Format = intFormat;
        worksheet.Range(currentRow - 6, 9, currentRow, 13).Style.NumberFormat.Format = decimalFormat;

        worksheet.Range(currentRow, 5, currentRow, 8).Style.NumberFormat.Format = intFormat;
        worksheet.Range(currentRow, 9, currentRow, 13).Style.NumberFormat.Format = decimalFormat;


        worksheet.Range(currentRow - 6, 5, currentRow, 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        worksheet.Range(currentRow - 6, 5, currentRow, 13).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        worksheet.Range(currentRow - 6, 5, currentRow, 13).Style.Font.FontSize = 12;

        // Жирный шрифт для итоговой строки
        worksheet.Range(currentRow, 3, currentRow, 13).Style.Font.Bold = true;

        // Настройка ширины столбцов согласно шаблону
        ConfigureColumnWidths(worksheet);

        // Настройки страницы
        ConfigurePageSettings(worksheet);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return await Task.FromResult(stream.ToArray());
    }

    private void ConfigureColumnWidths(IXLWorksheet worksheet)
    {
        // Ширина столбцов согласно шаблону (18 столбцов A-R)
        worksheet.Column(1).Width = 10;   // A
        worksheet.Column(2).Width = 10;   // B
        worksheet.Column(3).Width = 10;   // C
        worksheet.Column(4).Width = 10;  // D
        worksheet.Column(5).Width = 10;   // E
        worksheet.Column(6).Width = 7;   // F
        worksheet.Column(7).Width = 5;   // G
        worksheet.Column(8).Width = 14;   // H
        worksheet.Column(9).Width = 5;   // I
        worksheet.Column(10).Width = 7;  // J (SEQ)
        worksheet.Column(11).Width = 18; // K (CNTR-NO)
        worksheet.Column(12).Width = 12;  // L (SIZE)
        worksheet.Column(13).Width = 24; // M-O (SEAL-NO)
        worksheet.Column(14).Width = 8;  // N (SEAL-NO)
        worksheet.Column(15).Width = 8;  // O (SEAL-NO)
        worksheet.Column(16).Width = 10;  // P (STATUS)
        worksheet.Column(17).Width = 10;  // Q (PKG)
        worksheet.Column(18).Width = 12;  // R (TARE)
        worksheet.Column(19).Width = 18;  // S (GROSS)
    }

    private void ConfigurePageSettings(IXLWorksheet worksheet)
    {
        // Альбомная ориентация
        worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;

        // Поля
        worksheet.PageSetup.Margins.Top = 0.5;
        worksheet.PageSetup.Margins.Bottom = 0.5;
        worksheet.PageSetup.Margins.Left = 0.3;
        worksheet.PageSetup.Margins.Right = 0.3;
        worksheet.PageSetup.Margins.Header = 0.2;
        worksheet.PageSetup.Margins.Footer = 0.2;

        // Центрирование по горизонтали
        worksheet.PageSetup.CenterHorizontally = true;

        // Формат A4
        worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;

        // Масштаб 100%
        worksheet.PageSetup.AdjustTo(100);

        // Вписать по ширине на одну страницу
        worksheet.PageSetup.FitToPages(1, 0);

        // Колонтитулы
        var headerText = worksheet.PageSetup.Header.Center.AddText("D A N G E R O U S   C A R G O   M A N I F E S T");
        headerText.FontSize = 14;
        headerText.Bold = true;

        worksheet.PageSetup.Header.Right.AddText("Page &P of &N");
    }

    public async Task<string> GenerateManifestFileAsync(VesselCallDto vesselCall, string filePath)
    {
        var bytes = await GenerateManifestAsync(vesselCall);
        await File.WriteAllBytesAsync(filePath, bytes);
        return filePath;
    }


}