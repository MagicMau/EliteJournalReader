using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written: when purchasing goods in the market
    //Parameters:
    //•	Type: cargo type
    //•	Count: number of units
    //•	BuyPrice: cost per unit
    //•	TotalCost: total cost
    public class MarketBuyEvent : JournalEvent<MarketBuyEvent.MarketBuyEventArgs>
    {
        public MarketBuyEvent() : base("MarketBuy") { }

        public class MarketBuyEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Type = evt.Value<string>("Type");
                Count = evt.Value<int>("Count");
                BuyPrice = evt.Value<int>("BuyPrice");
                TotalCost = evt.Value<int>("TotalCost");
            }

            public string Type { get; set; }
            public int Count { get; set; }
            public int BuyPrice { get; set; }
            public int TotalCost { get; set; }
        }
    }
}
