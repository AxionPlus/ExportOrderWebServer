using System.Text.RegularExpressions;

namespace ExportOrderEntites;

public class ValidatePageFields
{
    public static bool IsValid_HSCode(string? text)
    {
        if (string.IsNullOrEmpty(text)) return false;

        string pattern = @"\b\d{10}\b";

        if (Regex.IsMatch(text, pattern))
            return true;
        else
            return false;
    }

    public static bool IsValid_UNNO(string? text)
    {
        if (string.IsNullOrEmpty(text)) return false;

        string pattern = @"\b\d{4}\b";

        if (Regex.IsMatch(text, pattern))
            return true;
        else
            return false;
    }

    public static bool IsValid_DeclarationNum(string? text, DocumentType docType)
    {
        if (string.IsNullOrEmpty(text)) return false;

        bool response = false;
        string pattern = string.Empty;

        //string pattern = @"^\d{8}/[0-3]{1}\d{1}(0|1){1}\d{3}/\d{7}$";

        if (docType == DocumentType.ПД)
            pattern = @"\d{8}/[0-3]{1}\d{1}(0|1){1}\d{3}/([0-9]|[A-Z]){7}";
        else
            pattern = @"\d{8}/[0-3]{1}\d{1}(0|1){1}\d{3}/\d{7}";

        if (Regex.IsMatch(text, pattern))
        {
            string dateText = text.Substring(9, 2);
            string monthText = text.Substring(11, 2);
            string yearText = text.Substring(13, 2);

            uint date = uint.TryParse(dateText, out uint _Date) ? _Date : 0;
            uint month = uint.TryParse(monthText, out uint _Month) ? _Month : 0;
            uint year = uint.TryParse(yearText, out uint _Year) ? _Year : 0;

            uint[] month_31 = new uint[] { 1, 3, 5, 7, 8, 10, 12 };
            uint[] month_30 = new uint[] { 4, 6, 9, 11 };
            
            if (date > 0 && month > 0 && year > 0 && month < 13 &&
                (month_31.Any(s => s.Equals(month)) ? date < 32 :
                    month_30.Any(s => s.Equals(month)) ? date < 31 : date < 30))
            {
                response = true;
            }                
        }

        return response;
    }
}
