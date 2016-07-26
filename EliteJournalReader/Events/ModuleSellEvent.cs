using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written: when selling a module in outfitting
    //Parameters:
    //•	Slot
    //•	SellItem
    //•	SellPrice
    //•	Ship
    public class ModuleSellEvent : JournalEvent<ModuleSellEvent.ModuleSellEventArgs>
    {
        public ModuleSellEvent() : base("ModuleSell") { }

        public class ModuleSellEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Slot = evt.Value<string>("Slot");
                SellItem = evt.Value<string>("SellItem");
                SellPrice = evt.Value<int>("SellPrice");
                Ship = evt.Value<string>("Ship");
                ShipId = evt.Value<int>("ShipId");
            }

            public string Slot { get; set; }
            public string SellItem { get; set; }
            public int SellPrice { get; set; }
            public string Ship { get; set; }
            public int ShipId { get; set; }
        }
    }
}
