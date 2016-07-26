using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written: when repairing the ship
    //Parameters:
    //•	Item: all, wear, hull, paint, or name of module
    //•	Cost: cost of repair
    public class RepairEvent : JournalEvent<RepairEvent.RepairEventArgs>
    {
        public RepairEvent() : base("Repair") { }

        public class RepairEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Item = evt.Value<string>("Item");
                Cost = evt.Value<int>("Cost");
            }

            public string Item { get; set; }
            public int Cost { get; set; }
        }
    }
}
