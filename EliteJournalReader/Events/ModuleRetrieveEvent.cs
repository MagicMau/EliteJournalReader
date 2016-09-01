using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written: when fetching a previously stored module
    //Parameters:
    //•	Slot
    //•	Ship
    //•	ShipID
    //•	RetrievedItem
    //•	EngineerModifications: name of modification blueprint, if any
    //•	SwapOutItem (if slot was not empty)
    //•	Cost
    public class ModuleRetrieveEvent : JournalEvent<ModuleRetrieveEvent.ModuleRetrieveEventArgs>
    {
        public ModuleRetrieveEvent() : base("ModuleRetrieve") { }

        public class ModuleRetrieveEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Slot = evt.Value<string>("FromSlot");
                Ship = evt.Value<string>("Ship");
                ShipId = evt.Value<int>("ShipId");
                RetrievedItem = evt.Value<string>("RetrievedItem");
                EngineerModifications = evt.Value<string>("EngineerModifications");
                SwapOutItem = evt.Value<string>("SwapOutItem");
                Cost = evt.Value<decimal?>("Cost");
            }

            public string Slot { get; set; }
            public string Ship { get; set; }
            public int ShipId { get; set; }
            public string RetrievedItem { get; set; }
            public string EngineerModifications { get; set; }
            public string SwapOutItem { get; set; }
            public decimal? Cost { get; set; }
        }
    }
}
