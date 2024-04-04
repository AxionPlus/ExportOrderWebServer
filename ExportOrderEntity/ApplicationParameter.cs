namespace ExportOrderEntites;

public static class ApplicationParameter
{
    public static readonly string AdminUser = "sdanilov@axionplus.ru";
    public static string ApplicationUser = "sdanilov@axionplus.ru";

#if DEBUG
    public static readonly string UploadFileUrl = "http://localhost:5050/file/UploadFile/";
    public static readonly string ViewReportUrl = "http://localhost:5050/view/ViewReport/";
#else
    public static readonly string UploadFileUrl = "http://localhost:7070/file/UploadFile/";
    public static readonly string ViewReportUrl = "http://exportorder.axionplus.ru/view/ViewReport/";    
#endif
}
