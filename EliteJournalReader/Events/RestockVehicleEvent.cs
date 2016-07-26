using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written: when purchasing an SRV or Fighter
    //Parameters:
    //•	Type: type of vehicle being purchased (SRV or fighter model)
    //•	Loadout: variant
    //•	Cost: purchase cost
    public class RestockVehicleEvent : JournalEvent<RestockVehicleEvent.RestockVehicleEventArgs>
    {
        public RestockVehicleEvent() : base("RestockVehicle") { }

        public class RestockVehicleEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Type = evt.Value<string>("Type");
                Loadout = evt.Value<string>("Loadout");
                Cost = evt.Value<int>("Cost");
            }

            public string Type { get; set; }
            public string Loadout { get; set; }
            public int Cost { get; set; }
        }
    }
}
