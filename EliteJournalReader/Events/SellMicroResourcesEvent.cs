using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    public class SellMicroResourcesEvent : JournalEvent<SellMicroResourcesEvent.SellMicroResourcesEventArgs>
    {
        public SellMicroResourcesEvent() : base("SellMicroResources") { }

        public class SellMicroResourcesEventArgs : JournalEventArgs
        {
            public List<MicroResource> MicroResources { get; set; }
            public int Price { get; set; }
            public long MarketID { get; set; }
        }
    }
}