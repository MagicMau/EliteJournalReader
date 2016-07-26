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
        public decimal X, Y, Z;

        public Position(JArray array)
        {
            X = array[0].Value<decimal>();
            Y = array[1].Value<decimal>();
            Z = array[2].Value<decimal>();
        }
    }
}
