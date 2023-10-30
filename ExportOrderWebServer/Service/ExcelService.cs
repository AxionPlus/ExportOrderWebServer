using ExportOrderWebServer.UploadFile;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExportOrderWebServer.Service;

public class ExcelService : IDisposable
{
    private string? FilePath { get; set; }
    private readonly uint ExcelAppPid;
    private readonly IDocumentProvider _documentProvider;
    private IEnumerable<CntrTpSz> CntrTypes = new List<CntrTpSz>();
    
    private Excel.Application? ExcelApp;
    private Excel.Workbooks? Workbooks;
    private Excel.Workbook? Workbook;
    private Excel.Sheets? WorkSheets;
    private Excel.Worksheet? WorkSheet;
    private Excel.Worksheet? WorkSheetData;
    private Excel.Range? Range;
    private Excel.Range? FilterRange;


    public ExcelService(string filePath, IEnumerable<CntrTpSz> cntrTypes, IDocumentProvider documentProvider)
    {
        FilePath = filePath;
        CntrTypes = cntrTypes;
        _documentProvider = documentProvider;

        ExcelApp = new Excel.Application();
        Workbooks = ExcelApp.Workbooks;        

        var tid = GetWindowThreadProcessId(ExcelApp.Hwnd, out ExcelAppPid);
    }


    public async Task<(List<ExportOrderRecord>, List<DocumentEntity>)> ReadUploadingFile()
    {
        if (!File.Exists(FilePath)) return (new List<ExportOrderRecord>(), new List<DocumentEntity>());
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
        int colPackageName = 7;
        int colNet = 8;
        int colGross = 9;        
        
        #endregion
                
        var records = new List<ExportOrderRecord>();
        var documents = new List<DocumentEntity>();

        UploadResult uploadResult = new UploadResult();

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

            var cntrNumGroup = sheetArray.GroupBy(s => s[colCntrNum]).ToList();

            foreach (var itemCntrNum in cntrNumGroup)
            {
                // Container Records
                var newRecord = new ExportOrderRecord()
                {
                    CntrNum = itemCntrNum.FirstOrDefault()![colCntrNum],
                    CntrType = CntrTypes.FirstOrDefault(x => x.Normolize == itemCntrNum.FirstOrDefault()![colCntrType].ToUpper())!,
                    CntrTareWt = double.TryParse(itemCntrNum.FirstOrDefault()![colCntrTareWt], out double _Twt) ? _Twt : 0,
                    Seal = itemCntrNum.FirstOrDefault()![colSeal],
                };

                foreach (var record in itemCntrNum)
                {
                    int cargoIndex = int.TryParse(record[colCargoIndex], out int _cIndex) ? _cIndex : 0;

                    var content = new ContainerContent()
                    {
                        PackageQty = uint.TryParse(record[colPackageQty], out uint _pkgQty) ? _pkgQty : 0,
                        PackageName = record[colPackageName],
                        NetWt = double.TryParse(record[colNet], out double _nwt) ? _nwt : 0,
                        GrossWt = double.TryParse(record[colGross], out double _gwt) ? _gwt : 0,
                        DocumentRecord = await _documentProvider.GetDocumentRecordAsync(record[colDoc], cargoIndex)
                    };

                    newRecord.Contents.Add(content);


                    // Documents
                    var _Document = await _documentProvider.GetDocumentAsync(record[colDoc]);

                    if (_Document is null)
                    {
                        return (null, null);
                    }   

                    var document = new DocumentEntity() { Name = "" };
                    var documentRecord = new DocumentRecord();

                    if (!documents.Any(s => s.Name == record[colDoc]))
                    {
                        documentRecord = _Document!.Records.FirstOrDefault(r => r.Seq == cargoIndex);

                        document = _Document;
                        document.Records.Clear();

                        documents.Add(document);
                    }
                    else
                    {
                        document = documents.FirstOrDefault(s => s.Name == record[colDoc]);

                        if (!document!.Records.Any(s => s.Seq == cargoIndex))
                            documentRecord = _Document!.Records.FirstOrDefault(r => r.Seq == cargoIndex);
                        else
                            documentRecord = null;
                    }

                    if (documentRecord != null)
                        document.Records!.Add(documentRecord!);
                }

                records.Add(newRecord);
            }

        }
        catch (Exception ex)
        {
            var msg = ex.Message;
        }

        return (records, documents);
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
