using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written: when starting a mission
    //Parameters:
    //•	Name: name of mission
    //•	Faction: faction offering mission
    //Optional Parameters (depending on mission type)
    //•	Commodity: commodity type
    //•	Count: number required / to deliver
    //•	Target: name of target
    //•	TargetType: type of target
    //•	TargetFaction: target’s faction
    public class MissionAcceptedEvent : JournalEvent<MissionAcceptedEvent.MissionAcceptedEventArgs>
    {
        public MissionAcceptedEvent() : base("MissionAccepted") { }

        public class MissionAcceptedEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Name = evt.Value<string>("Name");
                Faction = evt.Value<string>("Faction");
                Commodity = evt.Value<string>("Commodity");
                Count = evt.Value<int?>("Count");
                Target = evt.Value<string>("Target");
                TargetType = evt.Value<string>("TargetType");
                TargetFaction = evt.Value<string>("TargetFaction");
            }

            public string Name { get; set; }
            public string Faction { get; set; }
            public string Commodity { get; set; }
            public int? Count { get; set; }
            public string Target { get; set; }
            public string TargetType { get; set; }
            public string TargetFaction { get; set; }
        }
    }
}
