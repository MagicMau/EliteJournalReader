using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: at startup, or when being resurrected at a station
    //Parameters:
    //•	StarSystem: name of destination starsystem
    //•	StarPos: star position, as a Json array [x, y, z], in light years
    //•	Body: star’s body name
    public class LocationEvent : JournalEvent<LocationEvent.LocationEventArgs>
    {
        public LocationEvent() : base("Location") { }

        public class LocationEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                StarSystem = evt.Value<string>("StarSystem");
                StarPos = new Position(evt.Value<JArray>("StarPos"));
                Body = evt.Value<string>("Body");
            }

            public string StarSystem { get; set; }
            public Position StarPos { get; set; }
            public string Body { get; set; }
        }
    }
}
