using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: landing on a planet surface
    //Parameters:
    //•	Latitude
    //•	Longitude
    //•	PlayerControlled: (bool) false if ship was recalled from SRV, true if player is landing
    public class TouchdownEvent : JournalEvent<TouchdownEvent.TouchdownEventArgs>
    {
        public TouchdownEvent() : base("Touchdown") { }

        public class TouchdownEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Latitude = evt.Value<double>("Latitude");
                Longitude = evt.Value<double>("Longitude");
                IsPlayerControlled = evt.Value<bool>("PlayerControlled");
            }

            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public bool IsPlayerControlled { get; set; }
        }
    }
}
