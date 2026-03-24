namespace ExportOrderWebServer.Areas.ImportDocument.Extensions
{
    public static class DateTimeParser
    {
        public static DateTime? ParseDate(this object dateValue)
        {
            if (dateValue == null)
                return null;

            try
            {
                // Если значение уже DateTime
                if (dateValue is DateTime dateTime)
                {
                    return dateTime;
                }

                var dateString = dateValue.ToString().Trim();

                if (string.IsNullOrWhiteSpace(dateString))
                    return null;

                // Пробуем разные форматы дат
                string[] formats = new[]
                {
                    "yyyy-MM-dd HH:mm:ss",
                    "yyyy-MM-dd",
                    "dd.MM.yyyy",
                    "dd/MM/yyyy",
                    "MM/dd/yyyy",
                    "dd.MM.yy",
                    "dd/MM/yy"
                };

                if (DateTime.TryParseExact(dateString, formats,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out DateTime result))
                {
                    return result;
                }

                // Если парсинг с форматами не удался, пробуем обычный парсинг
                if (DateTime.TryParse(dateString, out result))
                {
                    return result;
                }
            }
            catch
            {
                // Игнорируем ошибки парсинга даты
            }

            return null;
        }
    }
}
