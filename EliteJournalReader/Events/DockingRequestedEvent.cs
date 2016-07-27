using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when the player requests docking at a station
    //Parameters:
    //•	StationName: name of station
    public class DockingRequestedEvent : JournalEvent<DockingRequestedEvent.DockingRequestedEventArgs>
    {
        public DockingRequestedEvent() : base("DockingRequested") { }

        public class DockingRequestedEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                StationName = evt.Value<string>("StationName");
            }

            public string StationName { get; set; }
        }
    }
}
