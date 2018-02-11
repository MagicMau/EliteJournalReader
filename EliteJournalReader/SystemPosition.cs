using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteJournalReader
{
    public struct SystemPosition
    {
        public double X, Y, Z;

        public bool IsZero()
        {
            return X == 0 && Y == 0 && Z == 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is SystemPosition)
                return Equals((SystemPosition)obj);
            return false;
        }

        public bool Equals(SystemPosition that)
        {
            return X == that.X && Y == that.Y && Z == that.Z;
        }

        public override int GetHashCode()
        {
            return (int)((X * 10000 + Y * 10000 + Z * 10000) % int.MaxValue);
        }

        public double[] ToArray()
        {
            return new[] { X, Y, Z };
        }

        public override string ToString()
        {
            return System.FormattableString.Invariant($"{X}, {Y}, {Z}");
        }
    }

    public class SystemPositionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SystemPosition);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var pos = (SystemPosition)existingValue;
            if (JToken.ReadFrom(reader) is JArray jarr)
            {
                double[] array = jarr.ToObject<double[]>();
                pos.X = Math.Round(array[0], 3);
                pos.Y = Math.Round(array[1], 3);
                pos.Z = Math.Round(array[2], 3);
            }
            return pos;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var pos = (SystemPosition)value;
            new JArray(pos.X, pos.Y, pos.Z).WriteTo(writer);
        }
    }
}
