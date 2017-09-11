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

        public SystemPosition(JArray array)
        {
            X = Math.Round(array[0].Value<double>(), 3);
            Y = Math.Round(array[1].Value<double>(), 3);
            Z = Math.Round(array[2].Value<double>(), 3);
        }

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
}
