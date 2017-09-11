using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: When selling a stored ship to raise funds when on insurance/rebuy screen
    //Parameters:
    //•	ShipType
    //•	System
    //•	SellShipId
    //•	ShipPrice
    public class SellShipOnRebuyEvent : JournalEvent<SellShipOnRebuyEvent.SellShipOnRebuyEventArgs>
    {
        public SellShipOnRebuyEvent() : base("SellShipOnRebuy") { }

        public class SellShipOnRebuyEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                ShipType = evt.Value<string>("ShipType");
                SellShipId = evt.Value<int>("SellShipID");
                ShipPrice = evt.Value<int>("ShipPrice");
                System = evt.Value<string>("System");
            }

            public string ShipType { get; set; }
            public int SellShipId { get; set; }
            public int ShipPrice { get; set; }
            public string System { get; set; }
        }
    }
}
