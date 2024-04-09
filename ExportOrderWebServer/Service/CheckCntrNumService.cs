namespace ExportOrderWebServer.Service;

public class CheckCntrNumService
{
    public readonly IVesselCallProvider vesselCallProvider;

    public CheckCntrNumService(IVesselCallProvider _vesselCallProvider)
    {
        vesselCallProvider = _vesselCallProvider;
    }

    public async Task<int> ControlDigit(string cntrNum)
    {
        // Chars in CntrNum
        char[] chars = new char[26]
        {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
        };

        // digital equivalents:
        int[] ControlEquivalents = new int[26]
        {
            10, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 23, 24, 25, 26, 27, 28, 29,30, 31, 32, 34, 35, 36, 37, 38
        };

        int DigitalEquivalent1 = ControlEquivalents[Array.IndexOf(chars, Convert.ToChar(cntrNum.Substring(0, 1)))] * (int)Math.Pow(2, 0);
        int DigitalEquivalent2 = ControlEquivalents[Array.IndexOf(chars, Convert.ToChar(cntrNum.Substring(1, 1)))] * (int)Math.Pow(2, 1);
        int DigitalEquivalent3 = ControlEquivalents[Array.IndexOf(chars, Convert.ToChar(cntrNum.Substring(2, 1)))] * (int)Math.Pow(2, 2);
        int DigitalEquivalent4 = ControlEquivalents[Array.IndexOf(chars, Convert.ToChar(cntrNum.Substring(3, 1)))] * (int)Math.Pow(2, 3);

        // Digits in CntrNum
        int digit1 = Convert.ToInt32(cntrNum.Substring(4, 1)) * (int)Math.Pow(2, 4);
        int digit2 = Convert.ToInt32(cntrNum.Substring(5, 1)) * (int)Math.Pow(2, 5);
        int digit3 = Convert.ToInt32(cntrNum.Substring(6, 1)) * (int)Math.Pow(2, 6);
        int digit4 = Convert.ToInt32(cntrNum.Substring(7, 1)) * (int)Math.Pow(2, 7);
        int digit5 = Convert.ToInt32(cntrNum.Substring(8, 1)) * (int)Math.Pow(2, 8);
        int digit6 = Convert.ToInt32(cntrNum.Substring(9, 1)) * (int)Math.Pow(2, 9);

        // sum of ControlEquivalents multiplications (Characters only)
        int multiplicationOfChars = DigitalEquivalent1 + DigitalEquivalent2 + DigitalEquivalent3 + DigitalEquivalent4;

        // sum of Serial numbers multiplications (Digits only)
        int multiplicationOfDigits = digit1 + digit2 + digit3 + digit4 + digit5 + digit6;

        // Остаток от деления на 11 по модулю
        int Remainder = (multiplicationOfChars + multiplicationOfDigits) % 11;

        if (Remainder == 10) Remainder = 0;
        //int Remainder;
        //int div = Math.DivRem((multiplicationOfChars + multiplicationOfDigits), 11, out Remainder);

        await Task.Delay(5);

        return Remainder;
    }

    public async Task<bool> IsCntrNumDuplicates(string cntrNum, long voyageId)
    {
        try
        {     
            var responseVoyage = await vesselCallProvider.GetItemAsync(voyageId);

            if (!responseVoyage!.HasError)
            { 
                VesselCallEntity Voyage = (VesselCallEntity)responseVoyage.Object!;

                return Voyage.Details.SelectMany(vcd => vcd.ExportOrders)
                                     .SelectMany(eo => eo.Records)
                                     .Select(eor => eor.CntrNum)
                                     .Any(n => n.Equals(cntrNum));
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }

        return false;
    }
}
