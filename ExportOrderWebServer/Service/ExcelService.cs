using System.Diagnostics;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using ExportOrderWebServer.Areas.Cntr.Provider;

namespace ExportOrderWebServer.Service;

public class ExcelService : IDisposable
{
    public static ICntrTypeProvider? _cntrTypeProvider;
    public static IEnumerable<CntrTpSz>? CntrTypes { get; set; } = new List<CntrTpSz>();

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


    public ExcelService()
    {
        ExcelApp = new Excel.Application();
        Workbooks = ExcelApp.Workbooks;
        var tid = GetWindowThreadProcessId(ExcelApp.Hwnd, out ExcelAppPid);
    }
    public ExcelService(string filePath)
    {
        FilePath = filePath;
        ExcelApp = new Excel.Application();
        Workbooks = ExcelApp.Workbooks;
        var tid = GetWindowThreadProcessId(ExcelApp.Hwnd, out ExcelAppPid);
    }

    public IEnumerable<ExportOrderRecord> ReadUploadingFile()
    {
        if (!File.Exists(FilePath)) return Enumerable.Empty<ExportOrderRecord>();
        Workbook = Workbooks?.Open(FilePath, 0, true);
        WorkSheets = Workbook?.Worksheets;
        WorkSheet = WorkSheets?.Item[1];

        #region Column Name
        int colNumDT = 1;
        int colCntrNum = 2;
        int colCntrType = 3;
        int colCntrTareWt = 4;
        int colSeal = 5;
        int colPackages = 6;
        int colGross = 7;
        int colNet = 8;
        int colVolume = 9;
        #endregion

        var cntrNums = new List<string>();
        var exportOrderRecords = new List<ExportOrderRecord>();

        try
        {
            uint columns = 9;
            uint row = 2;

            do
            {
                cntrNums.Add(WorkSheet!.Cells[row, colCntrNum].Text);
                row++;
            } while (!string.IsNullOrWhiteSpace(WorkSheet!.Cells[row, colCntrNum].Text));

            cntrNums = cntrNums.Distinct().Select(s => s.Replace("\n", "")).ToList();

            var startCell = WorkSheet.Cells[2, 1];
            var endCell = WorkSheet.Cells[row - 1, columns - 1];
            Range = WorkSheet.Range[startCell, endCell];

            string[][] sheetAray = GetStringArray(Range.Cells.Value);

            foreach (var cntrNum in cntrNums)
            {
                //var exportOrderRecord = new ExportOrderRecord() { CntrNum = cntrNum };
                var exportOrderRecord = new ExportOrderRecord();
                bool blData = true;

                for (int i = 0; i < sheetAray.Length; i++)
                {
                    if (sheetAray[i][colCntrNum - 1].Contains(cntrNum))
                    {
                        if (blData)
                        {
                            exportOrderRecord.CntrNum = sheetAray[i][colCntrNum - 1];

                            //string typeUploaded = sheetAray[i][colCntrType - 1];
                            //var typesEntity = await _cntrTypeProvider.GetCntrTypes();
                            //var type = typesEntity.FirstOrDefault(t => t.Normolize == typeUploaded);
                            exportOrderRecord.CntrType = GetCntrTypeEntity(sheetAray[i][colCntrType - 1]);

                            exportOrderRecord.CntrTareWt = double.TryParse(sheetAray[i][colCntrTareWt - 1], out double _Twt) ? _Twt : 0;
                            exportOrderRecord.Seal = sheetAray[i][colSeal - 1];

                            blData = false;
                        }

                        exportOrderRecord.Contents.Add(new()
                        {
                            Quantity = int.TryParse(sheetAray[i][colPackages - 1], out int _pkgQty) ? _pkgQty : 0,
                            GrossWt = double.TryParse(sheetAray[i][colGross - 1], out double _gwt) ? _gwt : 0,
                            NetWt = double.TryParse(sheetAray[i][colNet - 1], out double _nwt) ? _nwt : 0,
                            Volume = double.TryParse(sheetAray[i][colVolume - 1], out double _vol) ? _vol : 0,
                        });
                    }
                }
                exportOrderRecords.Add(exportOrderRecord);
            }

        }
        catch (Exception ex) { var msg = ex.Message; }

        return exportOrderRecords;
    }


    static string[][]? GetStringArray(Object rangeValues)
    {
        string[][]? stringArray = null;

        Array array = rangeValues as Array;
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
                        Object obj = array.GetValue(index + 1, index2 + 1);
                        if (null != obj)
                        {
                            string value = obj.ToString();

                            stringArray[index][index2] = value;
                        }
                    }
                }
            }
        }

        return stringArray;
    }

    Func<string, CntrTpSz> GetCntrTypeEntity = (str) =>
    {   
        CntrTypes = (IEnumerable<CntrTpSz>)_cntrTypeProvider!.GetCntrTypes();
        var type = CntrTypes.FirstOrDefault(t => t.Normolize == str);

        if (type != null)
            return type;
        else
            return new CntrTpSz();
    };


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
