using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written: when selling a ship stored in the shipyard
    //Parameters:
    //•	ShipType: type of ship being sold
    //•	ShipPrice: sale price
    //•	System: (if ship is in another system) name of system
    public class ShipyardSellEvent : JournalEvent<ShipyardSellEvent.ShipyardSellEventArgs>
    {
        public ShipyardSellEvent() : base("ShipyardSell") { }

        public class ShipyardSellEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                ShipType = evt.Value<string>("ShipType");
                ShipPrice = evt.Value<int>("ShipPrice");
                System = evt.Value<string>("System");
            }

            public string ShipType { get; set; }
            public int ShipPrice { get; set; }
            public string System { get; set; }
        }
    }
}
