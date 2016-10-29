using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace EliteJournalReader
{
    public static class EnumHelpers
    {
        public static T ToEnum<T>(this string value, T defaultValue) where T : struct
        {
            if (value == null)
                return defaultValue;

            var e = typeof(T)
                .GetFields()
                .FirstOrDefault(f => f.GetCustomAttributes<DescriptionAttribute>()
                             .Any(a => a.Description.Equals(value))
                );

            if (e != null)
                return (T)e.GetValue(null);

            T result;
            if (Enum.TryParse(value, out result))
                return result;

            return defaultValue;
        }

        public static string StringValue(this Enum enumItem)
        {
            return enumItem
            .GetType()
            .GetField(enumItem.ToString())
            .GetCustomAttributes<DescriptionAttribute>()
            .Select(a => a.Description)
            .FirstOrDefault() ?? enumItem.ToString();
        }
    }
}
