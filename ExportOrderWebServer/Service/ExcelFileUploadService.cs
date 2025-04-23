using System.Diagnostics;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExportOrderWebServer.Service;

public interface IExcelFileUploadService : IDisposable
{
    public Task<List<UploadExcelDTO>?> ReadImportListDeclarations(string filePath);
}

public class ExcelFileUploadService : IExcelFileUploadService
{
    private string? FilePath { get; set; }
    private readonly uint ExcelAppPid;

    private readonly Excel.Application ExcelApp;
    private readonly Excel.Workbooks Workbooks;
    private Excel.Workbook? Workbook;
    private Excel.Sheets? WorkSheets;
    private Excel.Worksheet? WorkSheet;
    private Excel.Range? Range;


    public ExcelFileUploadService()
    {
        ExcelApp = new Excel.Application();
        Workbooks = ExcelApp.Workbooks;

        var tid = GetWindowThreadProcessId(ExcelApp.Hwnd, out ExcelAppPid);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    public async Task<List<UploadExcelDTO>?> ReadImportListDeclarations(string filePath)
    {
        FilePath = filePath;

        if (!File.Exists(FilePath)) return null;

        Workbook = Workbooks?.Open(FilePath, 0, true);
        WorkSheets = Workbook?.Worksheets;
        WorkSheet = WorkSheets?.Item[1];

        var UploadExcel = new List<UploadExcelDTO>();

        try
        {
            uint columns = 12;
            uint row = 2;

            do
            {
                row++;
            } while (!string.IsNullOrWhiteSpace(WorkSheet!.Cells[row, 1].Text));

            var startCell = WorkSheet.Cells[2, 1];
            var endCell = WorkSheet.Cells[row - 1, columns];
            Range = WorkSheet.Range[startCell, endCell];

            string[][] sheetArray = GetStringArray(Range.Cells.Value);
            var recordsArray = sheetArray.ToList();

            foreach (var record in recordsArray)
            {
                UploadExcel.Add(new UploadExcelDTO()
                {
                    DocumentName = record[0]?.Trim(),
                    SeqCommodity = int.TryParse(record[1], out int _cIndex) ? _cIndex : 0,
                    CntrNum = record[2]?.ToUpper().Trim(),
                    CntrType = record[3]?.ToUpper().Trim(),
                    CntrTareWt = double.TryParse(record[4], out double _Tare) ? _Tare : 0,
                    Seal = record[5]?.Trim(),
                    PackageQty = uint.TryParse(record[6], out uint _pkgQty) ? _pkgQty : 0,
                    PackageName = record[7]?.Trim(),
                    NetWt = double.TryParse(record[8], out double _netWt) ? Math.Round(_netWt, 3, MidpointRounding.AwayFromZero)  : 0,
                    GrossWt = double.TryParse(record[9], out double _gwt) ? Math.Round( _gwt, 3, MidpointRounding.AwayFromZero) : 0,
                    SupplementaryUnitCode = ushort.TryParse(record[10]?.Trim(), out ushort _uc) ? _uc : null,
                    SupplementaryUnitQuantity = double.TryParse(record[11], out double _addu) ? Math.Round(_addu, 3, MidpointRounding.AwayFromZero) : null                    
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }

        await Task.Delay(2);
        return UploadExcel;
    }

    static string[][]? GetStringArray(Object rangeValues)
    {
        string[][]? stringArray = null;

        Array? array = rangeValues as Array;
        if (null != array)
        {
            int rank = array.Rank;
            if (rank > 1)
            {
                int rowCount = array.GetLength(0);
                int columnCount = array.GetUpperBound(1);

                stringArray = new string[rowCount][];

                for (int index = 0; index < rowCount; index++)
                {
                    stringArray[index] = new string[columnCount];

                    for (int index2 = 0; index2 < columnCount; index2++)
                    {
                        Object obj = array.GetValue(index + 1, index2 + 1)!;
                        if (obj != null)
                        {
                            string value = obj.ToString()!;

                            stringArray[index][index2] = value;
                        }
                    }
                }
            }
        }

        return stringArray;
    }

    public void Dispose()
    {
        try
        {
            Process[] process = Process.GetProcessesByName("Excel");
            foreach (Process p in process)
                if (!string.IsNullOrEmpty(p.ProcessName))
                    if (ExcelAppPid > 0)
                        if (p.Id == ExcelAppPid)
                            p.Kill();

            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
        catch (Exception ex) { Console.WriteLine(ex.Message); }
    }
}
