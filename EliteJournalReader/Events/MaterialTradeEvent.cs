using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written: when mining fragments are converted unto a unit of cargo by refinery
    //Parameters:
    //•	Type: cargo type
    public class MaterialTradeEvent : JournalEvent<MaterialTradeEvent.MaterialTradeEventArgs>
    {
        public MaterialTradeEvent() : base("MaterialTrade") { }

        public class MaterialTradeEventArgs : JournalEventArgs
        {
            public struct MaterialTraded
            {
                public string Material;
                public string Material_Localised;
                public int Quantity;
            }

            public long MarketID { get; set; }
            public string TraderType { get; set; }
            public List<MaterialTraded> Paid { get; set; }
            public List<MaterialTraded> Received { get; set; }
        }
    }
}
