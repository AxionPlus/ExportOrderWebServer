using ExportOrderEntites.ExportOrder;
using ExportOrderWebServer.UploadFile;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExportOrderWebServer.Service;

public class ExcelService : IDisposable
{
    private string? FilePath { get; set; }
    private readonly uint ExcelAppPid;
    private readonly IDocumentProvider _documentProvider;

    //private ExportOrderEntity ExportOder = new ExportOrderEntity();
    private IEnumerable<CntrTpSz> CntrTypes = new List<CntrTpSz>();
    //private IEnumerable<DocumentEntity> Documents = new List<DocumentEntity>();
    
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

    //public async Task<UploadResult> ReadUploadingFile()
    //public async Task<(List<ExportOrderRecord>, List<ExportOrderRecord>)> _ReadUploadingFile() { return (null, null); }
    public async Task<IEnumerable<ExportOrderRecord>> ReadUploadingFile()
    {
        if (!File.Exists(FilePath)) return (null);
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
                
        var records = new List<ExportOrderRecord>();
        //var documents = new List<DocumentEntity>();

        try
        {
            uint columns = 9;
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

            uint counter = 0;

            foreach (var itemCntrNum in cntrNumGroup)
            {
                var newRecord = new ExportOrderRecord()
                {
                    Id = counter++,
                    CntrNum = itemCntrNum.FirstOrDefault()![colCntrNum],
                    CntrType = CntrTypes.FirstOrDefault(x => x.Normolize == itemCntrNum.FirstOrDefault()![colCntrType].ToUpper())!,
                    CntrTareWt = double.TryParse(itemCntrNum.FirstOrDefault()![colCntrTareWt], out double _Twt) ? _Twt : 0,
                    Seal = itemCntrNum.FirstOrDefault()![colSeal],
                };

                //var _Document = await _documentProvider.GetDocumentAsync(itemCntrNum.FirstOrDefault()![colDoc]);

                foreach (var record in itemCntrNum)
                {
                    int cgoIndex = int.TryParse(itemCntrNum.FirstOrDefault()![colCargoIndex], out int _cIndex) ? _cIndex : 0;

                    var content = new ContainerContent()
                    {
                        Quantity = int.TryParse(itemCntrNum.FirstOrDefault()![colPackageQty], out int _pkgQty) ? _pkgQty : 0,
                        NetWt = double.TryParse(itemCntrNum.FirstOrDefault()![colNet], out double _nwt) ? _nwt : 0,
                        GrossWt = double.TryParse(itemCntrNum.FirstOrDefault()![colGross], out double _gwt) ? _gwt : 0,
                        DocumentRecord = await _documentProvider.GetDocumentRecordAsync(itemCntrNum.FirstOrDefault()![colDoc],
                                                                                        cgoIndex),
                    };

                    newRecord.Contents.Add(content);
                }

                records.Add(newRecord);
            }

            #region Docs & Records
            //records = records.ToList();
            //var document = new DocumentEntity() { Name = ""};
            //var documentRecord = new DocumentRecord();
            //int CargoIndex = 0;

            

            //foreach (var cntrNum in cntrNums)
            //{                
            //    var record = new ExportOrderRecord();                

            //    bool IsRecordData = true;

            //    counter ++;

            //    uint counterContent = 0;

            //    for (int i = 0; i < sheetArray.Length; i++)
            //    {
            //        if (sheetArray[i][colCntrNum].Contains(cntrNum))
            //        {
            //            // Document
            //            var _Document = await _documentProvider.GetDocumentAsync(sheetArray[i][colDoc]);

            //            if (!documents.Any(s => s.Name == sheetArray[i][colDoc]))
            //            {
            //                CargoIndex = int.TryParse(sheetArray[i][colCargoIndex], out int _indx) ? _indx : 0;
            //                documentRecord = _Document!.Records.FirstOrDefault(r => r.Seq == CargoIndex);

            //                document = _Document;
            //                document.Records.Clear();
            //                documents.Add(document);
            //            }
            //            else
            //            {
            //                CargoIndex = int.TryParse(sheetArray[i][colCargoIndex], out int _indx) ? _indx : 0;
            //                document = documents.FirstOrDefault(s => s.Name == sheetArray[i][colDoc]);

            //                if (!document!.Records.Any(s => s.Seq == CargoIndex))
            //                    documentRecord = _Document!.Records.FirstOrDefault(r => r.Seq == CargoIndex);
            //                else
            //                    documentRecord = null;
            //            }

            //            if (documentRecord != null)
            //                document.Records!.Add(documentRecord!);

            //            // Container record
            //            if (IsRecordData)
            //            {
            //                record.Id = counter;
            //                record.CntrNum = sheetArray[i][colCntrNum];
            //                record.CntrType = CntrTypes.FirstOrDefault(x => x.Normolize == sheetArray[i][colCntrType].ToUpper())!;
            //                record.CntrTareWt = double.TryParse(sheetArray[i][colCntrTareWt], out double _Twt) ? _Twt : 0;
            //                record.Seal = sheetArray[i][colSeal];

            //                IsRecordData = false;
            //            }

            //            // Container Content
            //            counterContent++;

            //            record.Contents.Add(new()
            //            {
            //                Id = counterContent,
            //                Quantity = int.TryParse(sheetArray[i][colPackageQty], out int _pkgQty) ? _pkgQty : 0,
            //                NetWt = double.TryParse(sheetArray[i][colNet], out double _nwt) ? _nwt : 0,
            //                GrossWt = double.TryParse(sheetArray[i][colGross], out double _gwt) ? _gwt : 0,
            //                DocumentRecord = documentRecord!,
            //                ExportOrderRecord = record,
            //            });
            //        }
            //    }
            //    records.Add(record);
            //}
            #endregion
        }
        catch (Exception ex)
        {
            var msg = ex.Message;
        }

        return (records);
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
