namespace ExportOrderEntites;

public static class ApplicationParameter
{
    public static readonly string AdminUser = "sdanilov@axionplus.ru";
    public static string ApplicationUser = "sdanilov@axionplus.ru";

#if DEBUG
    public static readonly string MainPageUrl = "http://localhost:7070/";
    public static readonly string PostUploadFileUrl = "http://localhost:7070/file/UploadFile/";
    public static readonly string PostDownloadFileUrl = "http://localhost:7070/file/DownloadFile/";
    public static readonly string UploadFileUrl = "http://localhost:7070/file/UploadFile/";
    public static readonly string DownloadFileUrl = "http://localhost:7070/file/DownloadFile/";
    public static readonly string ViewReportUrl = "http://localhost:7070/view/ViewReport/";
    public static readonly string ImportManifestController = "http://localhost:7070/file/ImportManifest/";
#else
    public static readonly string MainPageUrl = "http://exportorder.axionplus.ru/";
    public static readonly string PostUploadFileUrl = "http://localhost:7070/file/UploadFile/";
    public static readonly string PostDownloadFileUrl = "http://localhost:7070/file/DownloadFile/";
    public static readonly string UploadFileUrl = "http://exportorder.axionplus.ru/file/UploadFile/";
    public static readonly string DownloadFileUrl = "http://exportorder.axionplus.ru/file/DownloadFile/";
    public static readonly string ViewReportUrl = "http://exportorder.axionplus.ru/view/ViewReport/";
     public static readonly string ImportManifestController = "http://localhost:7070/file/ImportManifest/";
#endif
}
