using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace EliteJournalReader
{
    public class SystemPosition
    {
        public decimal X, Y, Z;

        public bool IsZero() => X == 0 && Y == 0 && Z == 0;

        public override bool Equals(object obj) => obj is SystemPosition that && Equals(that);

        public bool Equals(SystemPosition that) => X == that.X && Y == that.Y && Z == that.Z;

        public override int GetHashCode()
        {
            return System.HashCode.Combine(X, Y, Z);
        }

        public decimal[] ToArray() => new[] { X, Y, Z };

        public override string ToString() => FormattableString.Invariant($"{X},{Y},{Z}");

        public static bool operator ==(SystemPosition left, SystemPosition right) => left.Equals(right);

        public static bool operator !=(SystemPosition left, SystemPosition right) => !(left == right);
    }

    public class SystemPositionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(SystemPosition);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var pos = (SystemPosition)existingValue;
            if (JToken.ReadFrom(reader) is JArray jarr)
            {
                decimal[] array = jarr.ToObject<decimal[]>();
                pos = new SystemPosition
                {
                    X = Math.Round(array[0], 3),
                    Y = Math.Round(array[1], 3),
                    Z = Math.Round(array[2], 3),
                };
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
