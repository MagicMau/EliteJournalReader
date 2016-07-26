using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when landing at landing pad in a space station, outpost, or surface settlement
    //Parameters:
    //•	StationName: name of station
    //•	StationType: type of station
    //•	CockpitBreach:true (only if landing with breached cockpit)
    public class DockedEvent : JournalEvent<DockedEvent.DockedEventArgs>
    {
        public DockedEvent() : base("Docked") { }

        public class DockedEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                StationName = evt.Value<string>("StationName");
                StationType = evt.Value<string>("StationType");
                CockpitBreach = evt.Value<bool?>("CockpitBreach");
            }

            public string StationName { get; set; }
            public string StationType { get; set; }
            public bool? CockpitBreach { get; set; }
        }
    }
}
