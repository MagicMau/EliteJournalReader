using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when a docking request is granted
    //Parameters:
    //•	StationName: name of station
    //•	LandingPad: pad number
    public class DockingGrantedEvent : JournalEvent<DockingGrantedEvent.DockingGrantedEventArgs>
    {
        public DockingGrantedEvent() : base("DockingGranted") { }

        public class DockingGrantedEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                StationName = evt.Value<string>("StationName");
                LandingPad = evt.Value<int>("LandingPad");
            }

            public string StationName { get; set; }
            public int LandingPad { get; set; }
        }
    }
}
