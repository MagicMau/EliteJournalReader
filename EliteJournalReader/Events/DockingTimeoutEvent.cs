using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when a docking request has timed out
    //Parameters:
    //•	StationName: name of station
    public class DockingTimeoutEvent : JournalEvent<DockingTimeoutEvent.DockingTimeoutEventArgs>
    {
        public DockingTimeoutEvent() : base("DockingTimeout") { }

        public class DockingTimeoutEventArgs : JournalEventArgs
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
