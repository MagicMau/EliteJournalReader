using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when a mission is updated with a new destination
    //Parameters
    //•	MissionID
    //•	MissionName
    //•	NewDestinationStation
    //•	OldDestinationStation
    //•	NewDestinationSystem
    //•	OldDestinationSystem

    public class MissionRedirectedEvent : JournalEvent<MissionRedirectedEvent.MissionRedirectedEventArgs>
    {
        public MissionRedirectedEvent() : base("MissionRedirected") { }

        public class MissionRedirectedEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                MissionId = evt.Value<int>("MissionID");
                MissionName = evt.Value<string>("MissionName");
                NewDestinationStation = evt.Value<string>("NewDestinationStation");
                OldDestinationStation = evt.Value<string>("OldDestinationStation");
                NewDestinationSystem = evt.Value<string>("NewDestinationSystem");
                OldDestinationSystem = evt.Value<string>("OldDestinationSystem");
            }

            public int MissionId { get; set; }
            public string MissionName { get; set; }
            public string NewDestinationStation { get; set; }
            public string OldDestinationStation { get; set; }
            public string NewDestinationSystem { get; set; }
            public string OldDestinationSystem { get; set; }
        }
    }
}
