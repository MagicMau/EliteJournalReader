using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written: when switching to another ship already stored at this station
    //Parameters:
    //•	ShipType: type of ship being switched to
    //•	StoreOldShip: (if storing old ship) type of ship being stored
    //•	SellOldShip: (if selling old ship) type of ship being sold
    //
    public class ShipyardSwapEvent : JournalEvent<ShipyardSwapEvent.ShipyardSwapEventArgs>
    {
        public ShipyardSwapEvent() : base("ShipyardSwap") { }

        public class ShipyardSwapEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                ShipType = evt.Value<string>("ShipType");
                StoreOldShip = evt.Value<bool?>("StoreOldShip");
                SellOldShip = evt.Value<bool?>("SellOldShip");
                SellPrice = evt.Value<int?>("SellPrice");
            }

            public string ShipType { get; set; }
            public bool? StoreOldShip { get; set; }
            public bool? SellOldShip { get; set; }
            public int? SellPrice { get; set; }
                
        }
    }
}
