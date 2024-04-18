namespace ExportOrderWebServer.Service;

public interface ICheckCntrNumService
{
    Task<int> ControlDigit(string cntrNum);
    Task<bool> IsCntrNumDuplicates(string cntrNum, long voyageId);
}
