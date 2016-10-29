using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written: when moving a module to a different slot on the ship
    //Parameters:
    //•	FromSlot
    //•	ToSlot
    //•	FromItem
    //•	ToItem
    //•	Ship
    //•	ShipID
    public class ModuleSwapEvent : JournalEvent<ModuleSwapEvent.ModuleSwapEventArgs>
    {
        public ModuleSwapEvent() : base("ModuleSwap") { }

        public class ModuleSwapEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                FromSlot = evt.Value<string>("FromSlot");
                ToSlot = evt.Value<string>("ToSlot");
                FromItem = evt.Value<string>("FromItem");
                ToItem = evt.Value<string>("ToItem");
                Ship = evt.Value<string>("Ship");
                ShipId = evt.Value<int>("ShipId");
            }

            public string FromSlot { get; set; }
            public string ToSlot { get; set; }
            public string FromItem { get; set; }
            public string ToItem { get; set; }
            public string Ship { get; set; }
            public int ShipId { get; set; }
        }
    }
}
