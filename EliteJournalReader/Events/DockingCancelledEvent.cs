using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when the player cancels a docking request
    //Parameters:
    //•	StationName: name of station
    public class DockingCancelledEvent : JournalEvent<DockingCancelledEvent.DockingCancelledEventArgs>
    {
        public DockingCancelledEvent() : base("DockingCancelled") { }

        public class DockingCancelledEventArgs : JournalEventArgs
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
