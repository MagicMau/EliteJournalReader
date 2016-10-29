using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteJournalReader
{
    public struct Position
    {
        public double X, Y, Z;

        public Position(JArray array)
        {
            X = array[0].Value<double>();
            Y = array[1].Value<double>();
            Z = array[2].Value<double>();
        }

        public bool IsZero()
        {
            return X == 0 && Y == 0 && Z == 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is Position)
                return Equals((Position)obj);
            return false;
        }

        public bool Equals(Position that)
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
    }
}
