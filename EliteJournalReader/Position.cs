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
    }
}
