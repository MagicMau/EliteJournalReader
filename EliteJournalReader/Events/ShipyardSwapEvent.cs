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
    //•	ShipID
    //•	StoreOldShip: (if storing old ship) type of ship being stored
    //•	StoreShipID
    //•	SellOldShip: (if selling old ship) type of ship being sold
    //•	SellShipID
    public class ShipyardSwapEvent : JournalEvent<ShipyardSwapEvent.ShipyardSwapEventArgs>
    {
        public ShipyardSwapEvent() : base("ShipyardSwap") { }

        public class ShipyardSwapEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                ShipType = evt.Value<string>("ShipType");
                ShipId = evt.Value<int>("ShipID");
                StoreOldShip = evt.Value<string>("StoreOldShip");
                StoreShipId = evt.Value<int?>("StoreShipID");
                SellOldShip = evt.Value<string>("SellOldShip");
                SellShipId = evt.Value<int?>("SellShipID");
                SellPrice = evt.Value<int?>("SellPrice");
            }

            public string ShipType { get; set; }
            public int ShipId { get; set; }
            public string StoreOldShip { get; set; }
            public int? StoreShipId { get; set; }
            public string SellOldShip { get; set; }
            public int? SellShipId { get; set; }
            public int? SellPrice { get; set; }
                
        }
    }
}
