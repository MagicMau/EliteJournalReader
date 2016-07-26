using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written: when buying a new ship in the shipyard
    //Parameters:
    //•	ShipType: ship being purchased
    //•	ShipPrice: purchase cost 
    //•	StoreOldShip: (if storing old ship) ship type being stored
    //•	SellOldShip: (if selling current ship) ship type being sold
    //•	SellPrice: (if selling current ship) ship sale price
    public class ShipyardBuyEvent : JournalEvent<ShipyardBuyEvent.ShipyardBuyEventArgs>
    {
        public ShipyardBuyEvent() : base("ShipyardBuy") { }

        public class ShipyardBuyEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                ShipType = evt.Value<string>("ShipType");
                ShipPrice = evt.Value<int>("ShipPrice");
                StoreOldShip = evt.Value<bool?>("StoreOldShip");
                SellOldShip = evt.Value<bool?>("SellOldShip");
                SellPrice = evt.Value<int?>("SellPrice");
            }

            public string ShipType { get; set; }
            public int ShipPrice { get; set; }
            public bool? StoreOldShip { get; set; }
            public bool? SellOldShip { get; set; }
            public int? SellPrice { get; set; }
        }
    }
}
