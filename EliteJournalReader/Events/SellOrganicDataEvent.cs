using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    public class SellOrganicDataEvent : JournalEvent<SellOrganicDataEvent.SellOrganicDataEventArgs>
    {
        public SellOrganicDataEvent() : base("SellOrganicData") { }

        public class SellOrganicDataEventArgs : JournalEventArgs
        {
            public long MarketID { get; set; }
            public List<BioData> BioData { get; set; }
        }
    }
}