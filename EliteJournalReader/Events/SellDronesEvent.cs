using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written: when selling unwanted drones back to the market
    //Parameters:
    //•	Type
    //•	Count
    //•	SellPrice
    //•	TotalSale
    public class SellDronesEvent : JournalEvent<SellDronesEvent.SellDronesEventArgs>
    {
        public SellDronesEvent() : base("SellDrones") { }

        public class SellDronesEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Type = evt.Value<string>("Type");
                Count = evt.Value<int>("Count");
                SellPrice = evt.Value<int>("SellPrice");
                TotalSale = evt.Value<int>("TotalSale");
            }

            public string Type { get; set; }
            public int Count { get; set; }
            public int SellPrice { get; set; }
            public int TotalSale { get; set; }
        }
    }
}
