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
    public class MassModuleStoreEvent : JournalEvent<MassModuleStoreEvent.MassModuleStoreEventArgs>
    {
        public MassModuleStoreEvent() : base("MassModuleStore") { }

        public class MassModuleStoreEventArgs : JournalEventArgs
        {
            public struct ModuleItems
            {
                public string Slot;
                public string Name;
                public string EngineerModifications;
            }

            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Ship = evt.Value<string>("Ship");
                ShipId = evt.Value<int>("ShipId");
                Items = evt["Items"].ToObject<ModuleItems[]>();
            }

            public string Ship { get; set; }
            public int ShipId { get; set; }
            public ModuleItems[] Items { get; set; }
        }
    }
}
