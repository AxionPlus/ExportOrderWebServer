using ExportOrderEntites.ExportOrder;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExportOrderWebServer.Service;

public class ExcelService : IDisposable
{
    private string? FilePath { get; set; }
    private readonly uint ExcelAppPid;
    private long EOid;

    //private ExportOrderEntity ExportOder = new ExportOrderEntity();
    private IEnumerable<CntrTpSz> CntrTypes = new List<CntrTpSz>();
    private IEnumerable<DocumentEntity> Documents = new List<DocumentEntity>();

    private Excel.Application? ExcelApp;
    private Excel.Workbooks? Workbooks;
    private Excel.Workbook? Workbook;
    private Excel.Sheets? WorkSheets;
    private Excel.Worksheet? WorkSheet;
    private Excel.Worksheet? WorkSheetData;
    private Excel.Range? Range;
    private Excel.Range? FilterRange;


    public ExcelService(string filePath, IEnumerable<CntrTpSz> cntrTypes, IEnumerable<DocumentEntity> documents, long eoId)
    {
        FilePath = filePath;
        CntrTypes = cntrTypes;
        Documents = documents;
        //ExportOder = exportOrder;
        EOid = eoId;

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

        #region COLUMN NAME

        int colDoc = 0;
        int colCargoIndex = 1;
        int colCntrNum = 2;
        int colCntrType = 3;
        int colCntrTareWt = 4;
        int colSeal = 5;
        int colPackageQty = 6;
        int colNet = 7;
        int colGross = 8;        
        
        #endregion
                
        var cntrNums = new List<string>();
        var records = new List<ExportOrderRecord>();                

        try
        {
            uint columns = 9;
            uint row = 2;

            do
            {
                cntrNums.Add(WorkSheet!.Cells[row, colCntrNum + 1].Text);
                row++;
            } while (!string.IsNullOrWhiteSpace(WorkSheet!.Cells[row, colDoc + 1].Text));
                        
            cntrNums = cntrNums.Distinct().Select(s => s.Replace("\n", "")).ToList();

            var startCell = WorkSheet.Cells[2, 1];
            var endCell = WorkSheet.Cells[row - 1, columns];
            Range = WorkSheet.Range[startCell, endCell];

            string[][] sheetArray = GetStringArray(Range.Cells.Value);

            long counter = 0;

            foreach (var cntrNum in cntrNums)
            {                
                var record = new ExportOrderRecord();                
                var document = new DocumentEntity();
                var documentRecord = new DocumentRecord();

                bool IsRecordData = true;
                counter ++;

                long counterContent = 0;

                for (int i = 0; i < sheetArray.Length; i++)
                {
                    if (sheetArray[i][colCntrNum].Contains(cntrNum))
                    {
                        // Container (record)
                        if (IsRecordData)
                        {
                            record.Id = counter;
                            record.CntrNum = sheetArray[i][colCntrNum];
                            record.CntrType = CntrTypes.FirstOrDefault(x => x.Normolize == sheetArray[i][colCntrType].ToUpper())!;
                            record.CntrTareWt = double.TryParse(sheetArray[i][colCntrTareWt], out double _Twt) ? _Twt : 0;
                            record.Seal = sheetArray[i][colSeal];
                            //record.ExportOrder.Id = 1;
                            //record.ExportOrder.Id = EOid;

                            IsRecordData = false;
                        }

                        // Container Content
                        document = Documents.FirstOrDefault(d => d.Name == sheetArray[i][colDoc]);

                        var index = int.TryParse(sheetArray[i][colCargoIndex], out int _indx) ? _indx : 0;
                        documentRecord = document!.Records.FirstOrDefault(r => r.Seq == index);

                        counterContent ++;

                        record.Contents.Add(new()
                        {
                            Id = counterContent,
                            Quantity = int.TryParse(sheetArray[i][colPackageQty], out int _pkgQty) ? _pkgQty : 0,
                            NetWt = double.TryParse(sheetArray[i][colNet], out double _nwt) ? _nwt : 0,
                            GrossWt = double.TryParse(sheetArray[i][colGross], out double _gwt) ? _gwt : 0,
                            DocumentRecord = documentRecord!,
                            ExportOrderRecord = record,
                        });
                    }
                }
                records.Add(record);
            }
        }
        catch (Exception ex)
        {
            var msg = ex.Message;
        }

        return records;
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
                        if (null != obj)
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
