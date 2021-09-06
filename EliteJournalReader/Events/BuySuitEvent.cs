using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    public class BuySuitEvent : JournalEvent<BuySuitEvent.BuySuitEventArgs>
    {
        public BuySuitEvent() : base("BuySuit") { }

        public class BuySuitEventArgs : JournalEventArgs
        {
            public string Name { get; set; }
            public string Name_Localised { get; set; }
            public int Price { get; set; }
            public long SuitID { get; set; }

            public string[] SuitMods { get; set; }
        }
    }
}