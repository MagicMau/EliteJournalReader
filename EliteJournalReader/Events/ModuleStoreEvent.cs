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
    //•	StoredItem
    //•	EngineerModifications: name of modification blueprint, if any
    //•	ReplacementItem (if a core module)
    //•	Cost (if any)
    public class ModuleStoreEvent : JournalEvent<ModuleStoreEvent.ModuleStoreEventArgs>
    {
        public ModuleStoreEvent() : base("ModuleStore") { }

        public class ModuleStoreEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Slot = evt.Value<string>("FromSlot");
                Ship = evt.Value<string>("Ship");
                ShipId = evt.Value<int>("ShipId");
                StoredItem = evt.Value<string>("StoredItem");
                EngineerModifications = evt.Value<string>("EngineerModifications");
                ReplacementItem = evt.Value<string>("ReplacementItem");
                Cost = evt.Value<decimal?>("Cost");
            }

            public string Slot { get; set; }
            public string Ship { get; set; }
            public int ShipId { get; set; }
            public string StoredItem { get; set; }
            public string EngineerModifications { get; set; }
            public string ReplacementItem { get; set; }
            public decimal? Cost { get; set; }
        }
    }
}
