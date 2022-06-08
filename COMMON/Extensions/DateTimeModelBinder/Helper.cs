using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace CloudArchive.Models
{
    public class Helper
    {
        public static DateTime? ParseDateTime(
            string dateToParse,
            string[] formats = null,
            IFormatProvider provider = null,
            DateTimeStyles styles = DateTimeStyles.AssumeLocal)
        {
            DateTime validDate;

            var CUSTOM_DATE_FORMATS = new string[]
                {
                "yyyy-MM-dd",
                "MM-dd-yyyy",
                "dd.MM.yyyy HH:mm:ss",
                "dd/MM/yyyy, HH:mm:ss",
                "dd.MM.yyyy"
                };

            if (formats == null || !formats.Any())
            {
                formats = CUSTOM_DATE_FORMATS;
            }

            foreach (var format in formats)
            {
                if (format.EndsWith("Z"))
                {
                    if (DateTime.TryParseExact(dateToParse, format,
                             provider,
                             DateTimeStyles.AssumeUniversal,
                             out validDate))
                    {
                        return validDate;
                    }
                }

                if (DateTime.TryParseExact(dateToParse, format,
                         provider, styles, out validDate))
                {
                    if (validDate.Minute == 0 && validDate.Second == 0)
                    {
                        validDate = validDate.AddHours(12 - validDate.Hour);
                        return validDate;
                    }
                    else
                        return validDate;
                }
            }
            bool parsed = DateTime.TryParse(dateToParse, out validDate);
            if (parsed)
            {
                if (validDate.Minute == 0 && validDate.Second == 0)
                {
                    validDate = validDate.AddHours(12 - validDate.Hour);
                    return validDate;
                }
                else
                    return validDate;
            }
            return null;
        }

        public static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }
    }
}
