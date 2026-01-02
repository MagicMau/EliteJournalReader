using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace EliteJournalReader
{
    public static class EnumHelpers
    {
        private static readonly Dictionary<Type, Dictionary<string, object>> enumDescriptionCache = new Dictionary<Type, Dictionary<string, object>>();
        private static readonly Dictionary<Type, Dictionary<object, string>> enumToDescriptionCache = new Dictionary<Type, Dictionary<object, string>>();

        public static T ToEnum<T>(this string value, T defaultValue) where T : struct
        {
            if (value == null)
                return defaultValue;

            var type = typeof(T);
            if (!enumDescriptionCache.TryGetValue(type, out var cache))
            {
                cache = new Dictionary<string, object>();
                var attrs = type.GetFields().SelectMany(f => f.GetCustomAttributes<DescriptionAttribute>().Select(d => new { field = f, desc = d }));
                foreach(var attr in attrs)
                {
                    cache[attr.desc.Description] = attr.field.GetValue(null);
                }
                enumDescriptionCache[type] = cache;
            }

            // check if it's in the cache
            if (cache.TryGetValue(value, out object resultFromCache) && resultFromCache is T resultTyped)
            {
                return resultTyped;
            }

            // so it's not in the cache, maybe we can parse it the usual way?
            if (Enum.TryParse(value, true, out T resultDirect))
            {
                cache[value] = resultDirect; // store for next time!
                return resultDirect;
            }

            // if this all fails, try it again, but now case insensitive
            var descs = enumDescriptionCache[type];
            FieldInfo fieldInfo = null;
            if (descs == null)
            {
                fieldInfo = typeof(T)
                    .GetFields()
                    .FirstOrDefault(f => f.GetCustomAttributes<DescriptionAttribute>()
                                 .Any(a => a.Description.Equals(value, StringComparison.OrdinalIgnoreCase))
                    );

            }
            else
            {
                foreach (var kv in descs)
                {
                    if (kv.Key.Equals(value, StringComparison.OrdinalIgnoreCase))
                    {
                        cache[value] = kv.Value; // store for next time!
                        return (T)kv.Value;
                    }
                }
            }

            // not in the descriptions, perhaps in the 'regular' values?
            if (fieldInfo == null)
                fieldInfo = typeof(T)
                    .GetFields()
                    .FirstOrDefault(f => f.Name.Equals(value, StringComparison.OrdinalIgnoreCase));

            if (fieldInfo != null)
            {
                var resultInsensitive = (T)fieldInfo.GetValue(null);
                cache[value] = resultInsensitive; // remember for next time
                return resultInsensitive;
            }

            // we've tried everything, return the default value, but remember this for next time
            cache[value] = defaultValue;
            return defaultValue;
        }

        public static string StringValue(this Enum enumItem)
        {
            if (enumToDescriptionCache.TryGetValue(enumItem.GetType(), out var cache))
            {
                if (cache.TryGetValue(enumItem, out string resultFromCache))
                {
                    return resultFromCache;
                }
            }
            if (cache == null)
            {
                cache = new Dictionary<object, string>();
                enumToDescriptionCache[enumItem.GetType()] = cache;
            }
            string value = enumItem
                .GetType()
                .GetField(enumItem.ToString())
                .GetCustomAttributes<DescriptionAttribute>()
                .Select(a => a.Description)
                .FirstOrDefault() ?? enumItem.ToString();

            cache[enumItem] = value;
            return value;
        }

        public static IEnumerable<Enum> GetFlags(this Enum value)
        {
            return GetFlags(value, Enum.GetValues(value.GetType()).Cast<Enum>().ToArray());
        }

        public static IEnumerable<Enum> GetIndividualFlags(this Enum value)
        {
            return GetFlags(value, GetFlagValues(value.GetType()).ToArray());
        }

        private static IEnumerable<Enum> GetFlags(Enum value, Enum[] values)
        {
            ulong bits = Convert.ToUInt64(value);
            List<Enum> results = new List<Enum>();
            for (int i = values.Length - 1; i >= 0; i--)
            {
                ulong mask = Convert.ToUInt64(values[i]);
                if (i == 0 && mask == 0L)
                    break;
                if ((bits & mask) == mask)
                {
                    results.Add(values[i]);
                    bits -= mask;
                }
            }
            if (bits != 0L)
                return Enumerable.Empty<Enum>();
            if (Convert.ToUInt64(value) != 0L)
                return results.Reverse<Enum>();
            if (bits == Convert.ToUInt64(value) && values.Length > 0 && Convert.ToUInt64(values[0]) == 0L)
                return values.Take(1);
            return Enumerable.Empty<Enum>();
        }

        private static IEnumerable<Enum> GetFlagValues(Type enumType)
        {
            ulong flag = 0x1;
            foreach (var value in Enum.GetValues(enumType).Cast<Enum>())
            {
                ulong bits = Convert.ToUInt64(value);
                if (bits == 0L)
                    //yield return value;
                    continue; // skip the zero value
                while (flag < bits) flag <<= 1;
                if (flag == bits)
                    yield return value;
            }
        }
    }


    public class ExtendedStringEnumConverter<T> : StringEnumConverter where T : struct
    {
        private readonly T defaultValue;

        public ExtendedStringEnumConverter()
        {
            defaultValue = default;
        }

        public ExtendedStringEnumConverter(T defaultValue)
        {
            this.defaultValue = defaultValue;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                string enumText = reader.Value.ToString();
                return enumText.ToEnum(defaultValue);
            }

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
    }
}
