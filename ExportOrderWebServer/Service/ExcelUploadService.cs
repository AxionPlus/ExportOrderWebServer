using System.Diagnostics;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExportOrderWebServer.Service;

public class ExcelUploadService : IDisposable
{
    private string? FilePath { get; set; }
    private readonly uint ExcelAppPid;

    private Excel.Application? ExcelApp;
    private Excel.Workbooks? Workbooks;
    private Excel.Workbook? Workbook;
    private Excel.Sheets? WorkSheets;
    private Excel.Worksheet? WorkSheet;
    private Excel.Worksheet? WorkSheetData;
    private Excel.Range? Range;
    private Excel.Range? FilterRange;


    public ExcelUploadService(string filePath)
    {
        FilePath = filePath;

        ExcelApp = new Excel.Application();
        Workbooks = ExcelApp.Workbooks;

        var tid = GetWindowThreadProcessId(ExcelApp.Hwnd, out ExcelAppPid);
    }

    public async Task<List<UploadExcelDTO>> ReadUploadingFile()
    {
        if (!File.Exists(FilePath)) return (new List<UploadExcelDTO>());
        Workbook = Workbooks?.Open(FilePath, 0, true);
        WorkSheets = Workbook?.Worksheets;
        WorkSheet = WorkSheets?.Item[1];

        #region COLUMN NAMES

        int colDoc = 0;
        int colCargoIndex = 1;
        int colCntrNum = 2;
        int colCntrType = 3;
        int colCntrTareWt = 4;
        int colSeal = 5;
        int colPackageQty = 6;
        int colPackageName = 7;
        int colNet = 8;
        int colGross = 9;

        #endregion

        var UploadExcel = new List<UploadExcelDTO>();

        try
        {
            uint columns = 10;
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

            foreach (var records in recordsArray)
            {
                UploadExcel.Add(new UploadExcelDTO()
                {
                    DocumentName = (records[colDoc]).Trim(),
                    SeqCommodity = int.TryParse(records[colCargoIndex], out int _cIndex) ? _cIndex : 0,
                    CntrNum = records[colCntrNum].Trim(),
                    CntrType = string.IsNullOrEmpty(records[colCntrType]) ? null : records[colCntrType].ToUpper().Trim(),
                    CntrTareWt = double.TryParse(records[colCntrTareWt], out double _Tare) ? _Tare : 0,
                    Seal = records[colSeal],
                    PackageQty = uint.TryParse(records[colPackageQty], out uint _pkgQty) ? _pkgQty : 0,
                    PackageName = records[colPackageName],
                    NetWt = double.TryParse(records[colNet], out double _netWt) ? Math.Round(_netWt, 3, MidpointRounding.AwayFromZero)  : 0,
                    GrossWt = double.TryParse(records[colGross], out double _gwt) ? Math.Round( _gwt, 3, MidpointRounding.AwayFromZero) : 0,
                });
            }
        }
        catch (Exception ex)
        {
            var msg = ex.Message;
            return new List<UploadExcelDTO>();
        }

        await Task.Delay(200);
        return (UploadExcel);
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

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

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
