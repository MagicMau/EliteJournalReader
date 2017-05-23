using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when taking off from planet surface
    //Parameters:
    //•	Latitude
    //•	Longitude
    //•	PlayerControlled: (bool) false if ship dismissed when player is in SRV, true if player is taking off
    public class LiftoffEvent : JournalEvent<LiftoffEvent.LiftoffEventArgs>
    {
        public LiftoffEvent() : base("Liftoff") { }

        public class LiftoffEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Latitude = evt.Value<double>("Latitude");
                Longitude = evt.Value<double>("Longitude");
                IsPlayerControlled = evt.Value<bool>("IsPlayerControlled");
            }

            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public bool IsPlayerControlled { get; set; }
        }
    }
}
