using System.Text.RegularExpressions;

namespace ExportOrderWebServer.Service;

public interface IValidationService_New : IDisposable
{
    public string? ValidateCntrNums(string[] editedCntrNums, string[]? dbVoyageCntrNums);
    public int ControlDigit(string cntrNum);
}

public class ValidationService_New : IValidationService_New
{
    public ValidationService_New() { }

    /// <summary>
    /// Проверка номера контейнера в коллекции:
    /// 1. editedCntrNums   - Подгружаемой из файла Uploaded Excel Result;
    /// 2. editedCntrNums   - Со страницы ExportOrder_ItemComponent;
    /// 3. singleCntrNum    - Из текстового поля Номер контейнера на странице ExportOrder_ItemComponent
    /// </summary>
    /// <param name="editedCntrNums"></param>
    /// <param name="dbVoyageCntrNums"></param>
    /// <returns></returns>

    public string? ValidateCntrNums(string[] editedCntrNums, string[]? dbVoyageCntrNums)
    {
        StringBuilder errMessages = new();

        try
        {
            foreach (string num in editedCntrNums)
            {
                string errPrefix = $"{num}: ";
                ///LENGTH
                if (num.Length == 11)
                {
                    /// FORMAT
                    if (!Regex.IsMatch(num, "[a-zA-Z]{4}[0-9]{7}"))
                        errMessages.Append($"{errPrefix} Формат номера Контейнера не соответствует маске 'ABCD1234567'.<br>");
                    else
                    {
                        /// CONTROL DIGIT
                        int controlDigit = ControlDigit(num.ToUpper());

                        if (Convert.ToInt32(num.Substring(10, 1)) != controlDigit)
                            errMessages.Append($"{errPrefix} Контрольная цифра в номере Контейнера не верна. Правильно - {controlDigit}.<br>");
                    }
                }
                else
                    errMessages.Append($"{errPrefix} Количество символов в номере Контейнера не верно.<br>");

                /// DUPPLICATES
                ///
                /// <edited pageFound>Поиск в редактируемой коллекции</edited>
                bool editedFound = false;
                bool dbFound = false;

                editedFound = editedCntrNums.GroupBy(n => n).Where(g => g.Key == num && g.Count() > 1).Any();

                if (editedFound)
                    errMessages.Append($"{errPrefix} Контейнер повторяется в этом Поручении.<br>");
                else
                {
                    /// <db dbFound>Поиск в БД, исключая редактируемую коллекцию</db> 
                    if (dbVoyageCntrNums is not null && dbVoyageCntrNums.Any())
                        dbFound = dbVoyageCntrNums.Any(n => n == num);

                    if (dbFound)
                        errMessages.Append($"{errPrefix} Контейнер повторяется в этом рейсе.<br>");
                }
            }

            if (errMessages.Length == 0) return null;

            return errMessages.ToString().TrimEnd();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            //return Enumerable.Empty<string>();
            return null;
        }
    }

    public int ControlDigit(string cntrNum)
    {
        /// Chars in CntrNum
        char[] chars = new char[26]
        {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
        };

        /// digital equivalents:
        int[] ControlEquivalents = new int[26]
        {
            10, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 23, 24, 25, 26, 27, 28, 29,30, 31, 32, 34, 35, 36, 37, 38
        };

        int DigitalEquivalent1 = ControlEquivalents[Array.IndexOf(chars, Convert.ToChar(cntrNum.Substring(0, 1)))] * (int)Math.Pow(2, 0);
        int DigitalEquivalent2 = ControlEquivalents[Array.IndexOf(chars, Convert.ToChar(cntrNum.Substring(1, 1)))] * (int)Math.Pow(2, 1);
        int DigitalEquivalent3 = ControlEquivalents[Array.IndexOf(chars, Convert.ToChar(cntrNum.Substring(2, 1)))] * (int)Math.Pow(2, 2);
        int DigitalEquivalent4 = ControlEquivalents[Array.IndexOf(chars, Convert.ToChar(cntrNum.Substring(3, 1)))] * (int)Math.Pow(2, 3);

        /// Digits in CntrNum
        int digit1 = Convert.ToInt32(cntrNum.Substring(4, 1)) * (int)Math.Pow(2, 4);
        int digit2 = Convert.ToInt32(cntrNum.Substring(5, 1)) * (int)Math.Pow(2, 5);
        int digit3 = Convert.ToInt32(cntrNum.Substring(6, 1)) * (int)Math.Pow(2, 6);
        int digit4 = Convert.ToInt32(cntrNum.Substring(7, 1)) * (int)Math.Pow(2, 7);
        int digit5 = Convert.ToInt32(cntrNum.Substring(8, 1)) * (int)Math.Pow(2, 8);
        int digit6 = Convert.ToInt32(cntrNum.Substring(9, 1)) * (int)Math.Pow(2, 9);

        /// sum of ControlEquivalents multiplications (Characters only)
        int multiplicationOfChars = DigitalEquivalent1 + DigitalEquivalent2 + DigitalEquivalent3 + DigitalEquivalent4;

        /// sum of Serial numbers multiplications (Digits only)
        int multiplicationOfDigits = digit1 + digit2 + digit3 + digit4 + digit5 + digit6;

        /// Остаток от деления на 11 по модулю
        int Remainder = (multiplicationOfChars + multiplicationOfDigits) % 11;

        if (Remainder == 10) Remainder = 0;
        //int Remainder;
        //int div = Math.DivRem((multiplicationOfChars + multiplicationOfDigits), 11, out Remainder);

        return Remainder;
    }

    public void Dispose()
    {

    }
}
