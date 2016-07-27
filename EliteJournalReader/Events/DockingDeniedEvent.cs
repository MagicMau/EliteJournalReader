using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when the station denies a docking request
    //Parameters:
    //•	StationName: name of station
    //•	Reason: reason for denial
    //
    //Reasons include: NoSpace, TooLarge, Hostile, Offences, Distance, ActiveFighter, NoReason
    public class DockingDeniedEvent : JournalEvent<DockingDeniedEvent.DockingDeniedEventArgs>
    {
        public DockingDeniedEvent() : base("DockingDenied") { }

        public class DockingDeniedEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                StationName = evt.Value<string>("StationName");
                Reason = evt.Value<string>("Reason");
            }

            public string StationName { get; set; }
            public string Reason { get; set; }
        }
    }
}
